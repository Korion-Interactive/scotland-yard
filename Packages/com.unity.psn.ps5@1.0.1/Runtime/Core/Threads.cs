//#define OUTPUT_THREAD_PROCESS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Unity.PSN.PS5.Aysnc
{
    internal class WorkerThread
    {
        Thread workerThread;
        bool stopThread = false;
        Semaphore workLoad = new Semaphore(0, 1000);
        object workerQueueSyncObj = new object();

        Queue<AsyncOp> asyncOps = new Queue<AsyncOp>();

        static List<WorkerThread> allThreads = new List<WorkerThread>();
        static object trackingSyncObj = new object();

        AsyncOp activeOp = null;

        Stopwatch timer;

        // Create event who's initial state is nonsignaled so any thread waiting on it will block.
        // Use a manual reset type so all threads waiting can be released at the same time.
        EventWaitHandle pollingEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

        static void AddThreadTracking(WorkerThread thread)
        {
            lock (trackingSyncObj)
            {
                allThreads.Add(thread);
            }
        }

        static void RemoveThreadTracking(WorkerThread thread)
        {
            lock (trackingSyncObj)
            {
                allThreads.Remove(thread);
            }
        }

        public static string GetThreadsListDebugOutput()
        {
            string output = "Threads List:\n";

            lock (trackingSyncObj)
            {
                for (int i = 0; i < allThreads.Count; i++)
                {
                    WorkerThread thread = allThreads[i];

                    output += "    " + thread.workerThread.Name + "\n";
                }
            }

            return output;
        }

        public static string GetPendingRequestDebugOutput()
        {
            string output = "Requests List:\n";

            lock (trackingSyncObj)
            {
                for (int i = 0; i < allThreads.Count; i++)
                {
                    WorkerThread thread = allThreads[i];

                    // thread name
                    output += "    " + thread.workerThread.Name + " Thread\n";

                    lock (thread.workerQueueSyncObj)
                    {
                        var runningRequest = thread.activeOp;

                        if (runningRequest != null)
                        {
                            string name = runningRequest.GetType().Name;
                            output += "         " + name + "\n";
                        }

                        foreach (var request in thread.asyncOps)
                        {
                            string name = request.GetType().Name;
                            output += "         " + name + "\n";
                        }
                    }
                }
            }

            return output;
        }

        public void Start(string name)
        {
            timer = Stopwatch.StartNew();

            stopThread = false;
            workerThread = new Thread(new ThreadStart(RunProc));
            workerThread.Name = name;
            workerThread.Start();

            AddThreadTracking(this);
        }

        static internal void ReleasePollingThreads()
        {
            lock (trackingSyncObj)
            {
                for (int i = 0; i < allThreads.Count; i++)
                {
                    WorkerThread thread = allThreads[i];
                    thread.pollingEvent.Set();
                }
            }
        }

        private void RunProc()
        {
            activeOp = null;

            workLoad.WaitOne();

            while (!stopThread)
            {
                lock (workerQueueSyncObj)
                {
                    if (asyncOps.Count > 0)
                    {
                        activeOp = asyncOps.Dequeue();
                    }
                }

                if (activeOp != null)
                {
                    ExecuteOp(activeOp);
                }

                activeOp = null;

                // Need to wait here for a certain amount of time. This will throttle back the ammount of work pushed
                // through the queue


                workLoad.WaitOne();
            }

          //  Console.WriteLine("REQUEST THREAD STOPPED");

            RemoveThreadTracking(this);
        }

        [Conditional("OUTPUT_THREAD_PROCESS")]
        public static void DebuOutputExecution(string output)
        {
            UnityEngine.Debug.LogWarning(output);
        }

        public bool ExecuteOp(AsyncOp op)
        {
            if (op == null)
            {
#if DEBUG
                UnityEngine.Debug.LogError("Aysnc Op is null.");
#endif
                return false;
            }

            DebuOutputExecution("ExecuteOp Started");

            long startMs = timer.ElapsedMilliseconds;

            op.WaitDurationMs = startMs - op.queueStartTimeMs;

            op.IsCompleted = false;

            op.AysncState = AsyncOp.State.Running;

            bool success = false;

            try
            {
                DebuOutputExecution("op.Execute() - " + op.GetType());
                op.Execute();

                // The op maight be a special polling type, which needs to be updated on the next frame.
                // If this is the case loop around the update method here, but wait until the main update method signals the beginning of a new frame
                if(op.HasUpdate())
                {
                    DebuOutputExecution("WaitOne Update : Waiting to do first update");
                    pollingEvent.WaitOne();
                    while (op.Update() == false)
                    {
                        DebuOutputExecution("Update Run -> WaitOne");
                        pollingEvent.WaitOne();
                    }
                    DebuOutputExecution("Updating Finished");
                }

                success = true;
            }
#pragma warning disable CS0168
            catch (PSNException e)
            {
#if DEBUG
                UnityEngine.Debug.LogError("Request Exception : " + e.ExtendedMessage + "\n" + e.StackTrace);
                DebuOutputExecution(e.StackTrace);
#endif
            }
            catch (Exception e)
            {
#if DEBUG
                UnityEngine.Debug.LogError("RequestThread Exception : " + e.Message + "\n" + e.StackTrace);
                DebuOutputExecution(e.StackTrace);
#endif
            }
#pragma warning restore CS0168

            op.ExecuteDurationMs = timer.ElapsedMilliseconds - startMs;

            if (success == true)
            {
                op.IsCompleted = true;

                if (op.nextOp != null) //&& request.antecedent.isErrorHandler == false)
                {
                    op.AysncState = AsyncOp.State.WaitingForSequence;

                    //  Console.WriteLine("ExecuteOp Next Started");
                    DebuOutputExecution("ExecuteOp Next Started");

                    success = ExecuteOp(op.nextOp);

                    if (success == true)
                    {
                        op.AysncState = AsyncOp.State.Success;
                    }
                    else
                    {
                        op.AysncState = AsyncOp.State.Error;
                    }

                    DebuOutputExecution("ExecuteOp Next Finished");

                    return success;
                }

                op.AysncState = AsyncOp.State.Success;

                if (op.previousOp == null)
                {
                    // Root op so mark the sequence as complete
                    op.IsSequenceCompleted = true;
                }
            }
            else
            {
                op.AysncState = AsyncOp.State.Error;
            }

            DebuOutputExecution("ExecuteOp Finished");

            return success;
        }

        public void Stop()
        {
            stopThread = true;
            workLoad.Release();
        }

        public void Schedule(AsyncOp op)
        {
            AsyncOp root = op.Root;

            // Debug. Display type of ops.
            //int count = 0;

            //string debugOutput = "WorkerThread.Schedule:";
            //AsyncOp current = root;
            //while (current != null)
            //{
            //    debugOutput += "   - " + current.GetType().ToString() + "\n";
            //    current = current.nextOp;
            //    count++;
            //}
            //Console.WriteLine(debugOutput + "\n Total = " + count);

            lock (workerQueueSyncObj)
            {
                if(asyncOps.Count > 500)
                {
                    throw new ExceededMaximumOperations("Exceed maximum size of workqueue");
                }

                root.ResetInternal(AsyncOp.State.Scheduled);

                asyncOps.Enqueue(root);
            }

            root.SetQueueStartTimeMs(timer.ElapsedMilliseconds);

            workLoad.Release();
        }
    }

    /// <summary>
    /// Asynchronous operation base class
    /// </summary>
    public class AsyncOp
    {
        /// <summary>
        /// The current state of the asynchronous operation
        /// </summary>
        public enum State
        {
            /// <summary>Operation has been created and is inactive</summary>
            Created,
            /// <summary>Operation is scheduled to be run</summary>
            Scheduled,
            /// <summary>Operation is current running</summary>
            Running,
            /// <summary>Operation has completed but it waiting for a dependent operation to complete</summary>
            WaitingForSequence,
            /// <summary>Operation has finished successfully</summary>
            Success,
            /// <summary>Operation has finished with an error</summary>
            Error,
        }

        /// <summary>
        /// Reset the state of the operation so it can be scheduled again
        /// </summary>
        public void Reset()
        {
            ResetInternal(State.Created);
        }

        internal void ResetInternal(State startState)
        {
            IsCompleted = false;
            IsSequenceCompleted = false;
            HasSequenceFailed = false;
            AysncState = startState;

            // Reset each op in the sequence.
            if (nextOp != null)
            {
                nextOp.ResetInternal(startState);
            }
        }

        internal void SetQueueStartTimeMs(long time)
        {
            queueStartTimeMs = time;

            if (nextOp != null)
            {
                nextOp.SetQueueStartTimeMs(time);
            }
        }


        /// <summary>
        /// Has the operation completed. This only refers to this operation and not the full sequence of operations.
        /// </summary>
        public bool IsCompleted { get; internal set; }

        /// <summary>
        /// Has the sequence completed. This is only set on a Root operation when all operations in the sequence have completed
        /// </summary>
        public bool IsSequenceCompleted { get; internal set; }

        /// <summary>
        /// Has the sequence failed
        /// </summary>
        public bool HasSequenceFailed { get; internal set; }

        /// <summary>
        /// Has the root operation completed sucessfully along with the full sequence of operations.
        /// </summary>
        public bool IsCompletedSuccessfully
        {
            get
            {
                return IsSequenceCompleted == true && AysncState == State.Success;
            }
        }


        internal long queueStartTimeMs;

        /// <summary>
        /// How long was the operation waiting in the queue before running
        /// </summary>
        public long WaitDurationMs { get; internal set; }

        /// <summary>
        /// How long did the operation run for
        /// </summary>
        public long ExecuteDurationMs { get; internal set; }

        /// <summary>
        /// Return the async state
        /// </summary>
        public State AysncState { get; internal set; } = State.Created;

        /// <summary>
        /// Get the root operation in a sequence.
        /// </summary>
        public AsyncOp Root
        {
            get
            {
                AsyncOp current = this;
                while (current.previousOp != null)
                {
                    current = current.previousOp;
                }

                return current;
            }
        }

        /// <summary>
        /// Get the last operation in a sequence.
        /// </summary>
        public AsyncOp Last
        {
            get
            {
                AsyncOp current = this;
                while (current.nextOp != null)
                {
                    current = current.nextOp;
                }

                return current;
            }
        }

        internal AsyncOp previousOp;  // previous
        internal AsyncOp nextOp; // next

        internal void AddNext(AsyncOp next)
        {
            nextOp = next;
            next.previousOp = this;
        }

        /// <summary>
        /// Join another operation onto this one, which will execute after this operation has finished.
        /// </summary>
        /// <param name="continueWith">The next operation</param>
        public void ContinueWith(AsyncOp continueWith)
        {
            // Make sure continueWith is already part of another sequence.
            AddNext(continueWith.Root);
        }

        /// <summary>
        /// Join an Action onto this operation, which will execute after this operation has finished.
        /// </summary>
        /// <param name="continueWith">The Action to run</param>
        /// <returns>An operation which will execute the action</returns>
        public AsyncAction ContinueWith(Action continueWith)
        {
            var actionOp = new AsyncAction(continueWith);

            AddNext(actionOp);

            return actionOp;
        }

        /// <summary>
        /// Join an Action, which contains an operation, onto this operation, which will execute after this operation has finished.
        /// </summary>
        /// <param name="continueWith">The action to run</param>
        /// <returns>An async action containing the operation</returns>
        public AsyncAction<AsyncOp> ContinueWith(Action<AsyncOp> continueWith)
        {
            var actionOp = new AsyncAction<AsyncOp>(continueWith);

            AddNext(actionOp);

            return actionOp;
        }

        /// <summary>
        /// Join an Request operation onto this operation, which will execute after this operation has finished.
        /// </summary>
        /// <typeparam name="T">A type of Request. <see cref="Request"/></typeparam>
        /// <param name="continueWith">The request async operation</param>
        /// <returns>An async request operation</returns>
        public AsyncRequest<T> ContinueWith<T>(AsyncRequest<T> continueWith) where T : Request
        {
            AddNext(continueWith.Root);

            return continueWith;
        }

        internal virtual void Execute()
        {

        }

        internal virtual bool HasUpdate()
        {
            return false;
        }

        internal virtual bool Update()
        {
            return true; // Finished update
        }
    }

    /// <summary>
    /// An async operation that contains a type of request. <see cref="Unity.PSN.PS5.Aysnc.Request"/>
    /// </summary>
    /// <typeparam name="T">A type of Request. <see cref="Unity.PSN.PS5.Aysnc.Request"/></typeparam>
    public class AsyncRequest<T> : AsyncOp where T : Request
    {
        /// <summary>
        /// The request data
        /// </summary>
        public T Request { get; internal set; }

        /// <summary>
        /// Contruct an empty async operation
        /// </summary>
        public AsyncRequest()
        {
            Request = null;
        }

        /// <summary>
        /// Construct an async operation containing a request
        /// </summary>
        /// <param name="request">The request object to run</param>
        public AsyncRequest(T request)
        {
            Request = request;
        }

        /// <summary>
        ///  Join an Action, which contains an async Request, onto this operation, which will execute after this operation has finished.
        /// </summary>
        /// <param name="continueWith">Action containing the Async request</param>
        /// <returns>An async action</returns>
        public AsyncAction<AsyncRequest<T>> ContinueWith(Action<AsyncRequest<T>> continueWith)
        {
            var actionOp = new AsyncAction<AsyncRequest<T>>(continueWith);

            AddNext(actionOp);

            return actionOp;
        }

        internal override void Execute()
        {
            if (Request != null)
            {
                Request.Run();
            }
        }

        internal override bool HasUpdate()
        {
            if (Request != null)
            {
                return Request.HasUpdate();
            }
            return false;
        }

        internal override bool Update()
        {
            if (Request != null)
            {
                return Request.Update();
            }
            return false;
        }
    }

    /// <summary>
    /// An async operation that executes an System.Action.
    /// </summary>
    public class AsyncAction : AsyncOp
    {
        Action actionCallback;

        /// <summary>
        /// Create an empty operation
        /// </summary>
        public AsyncAction()
        {
            actionCallback = null;
        }

        /// <summary>
        /// Create an operation with the action to run
        /// </summary>
        /// <param name="action">The action to run</param>
        public AsyncAction(Action action)
        {
            actionCallback = action;
        }

        internal override void Execute()
        {
         //   Console.WriteLine("AsyncAction.Execute()");
            if (actionCallback != null)
            {
                actionCallback();
            }
        }
    }

    /// <summary>
    /// An async operation can executes an Action which take a parameter of type <see cref="AsyncOp"/> />.
    /// </summary>
    /// <typeparam name="T">Any type of AsyncOp</typeparam>
    public class AsyncAction<T> : AsyncOp where T : AsyncOp
    {
        Action<T> actionWithParam;

        /// <summary>
        /// Create an empty async operation
        /// </summary>
        public AsyncAction()
        {
            actionWithParam = null;
        }

        /// <summary>
        /// Create an operation with the action to run
        /// </summary>
        /// <param name="action">The action to run</param>
        public AsyncAction(Action<T> action)
        {
            actionWithParam = action;
        }

        internal override void Execute()
        {
            if (actionWithParam != null)
            {
                actionWithParam((T)this.previousOp);
            }
        }
    }

    /// <summary>
    /// Base class for a Request
    /// </summary>
    public class Request
    {
        /// <summary>
        /// The results of any SDK API's called when the request is executed.
        /// </summary>
        public APIResult Result { get; protected set; }

        protected internal virtual void Run()
        {

        }

        protected internal virtual bool HasUpdate()
        {
            return false;
        }

        protected internal virtual bool Update()
        {
            return true; // finished update
        }
    }
}
