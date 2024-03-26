
using System;
using System.Collections.Generic;
using System.Threading;

using Unity.SaveData.PS5.Initialization;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// A request that is currently in the pending queue and waiting to be processed.
    /// </summary>
    public class PendingRequest
    {
        internal Int32 requestId;
        internal RequestBase request;
        internal ResponseBase response;
        internal bool abortPending = false;

        /// <summary>
        /// The unique request ID.
        /// </summary>
        public Int32 NpRequestId { get { return requestId; } }

        /// <summary>
        /// The request object that contains the request settings.
        /// </summary>
        public RequestBase Request { get { return request; } }

        /// <summary>
        /// True if the request has been aborted but is still in the pending list awaiting removal, false otherwise.
        /// </summary>
        public bool AbortPending { get { return abortPending; } }
    };


    static class ProcessQueueThread
    {
        static Thread thread;
        static bool stopThread = false;
        static Semaphore workLoad = new Semaphore(0, 1000);
        static object syncQueue = new object();
        static int nextRequestId = 1;
        static ThreadSettingsNative threadSettings;

        // This is always in a blocked state. Any thread adding a sync request will call WaitOne on this
        // event. 
        // Ocne the processing queue has finished executing a sync request it will set this event and ALL waiting
        // threads will wake up. Each of those will check if the request that was just completed is theirs and if it is
        // that thread will continue. That thread will also reset this event to block again. All other threads will then go back
        // to sleep on this event 
        static ManualResetEvent syncTaskCompleted = new ManualResetEvent(false);

        static List<PendingRequest> completedSyncRequests = new List<PendingRequest>();
        static object completedLock = new object();

        static Queue<PendingRequest> pendingQueue = new Queue<PendingRequest>();

        // List of pending request that have started processing but need to be polled.
        static List<PendingRequest> pollingList = new List<PendingRequest>();

        // This is called by the ProcessQueueThread when a synchronous request has finished. 
        // If add the request to a list and then wakes up all pending threads that are waiting for their request to finish.
        // Each of those thread will then test if it is their request that has completed.
        static void AddCompletedSyncRequest(PendingRequest pe)
        {
            lock (completedLock)
            {
                completedSyncRequests.Add(pe);
                syncTaskCompleted.Set(); // Release all theads waiting for a synchronous task to complete
            }
        }

        static bool RemoveCompletedSyncRequest(PendingRequest pe)
        {
            lock (completedLock)
            {
                bool removed = completedSyncRequests.Remove(pe);
                if (removed == true)
                {
                    // This request has been completed. If the list is empty it can be signal so all other threads will go back to sleep
                    if (completedSyncRequests.Count == 0)
                    {
                        syncTaskCompleted.Reset();
                    }
                }
                return removed;
            }
        }

        internal static List<PendingRequest> PendingRequests
        {
            get
            {
                lock (syncQueue)
                {
                    List<PendingRequest> copy = new List<PendingRequest>(pendingQueue);
                    return copy;
                }
            }
        }


        public static void Start(ThreadAffinity affinity)
        {
            stopThread = false;
            thread = new Thread(new ThreadStart(RunProc));
            // thread.Name = "SaveDataProcessQueue";

            threadSettings = new ThreadSettingsNative(affinity, thread.Name);

            thread.Start();
        }

        private static void RunProc()
        {
            // Call into the PRX and change this threads affinity
            Init.SetThreadAffinity(threadSettings);
            thread.Name = "SaveDataProcessQueue";

            workLoad.WaitOne();

            while (!stopThread)
            {
                PendingRequest pendingRequest = null;

                for (int i = 0; i < pollingList.Count; i++)
                {
                    pendingRequest = pollingList[i];

                    if (pendingRequest.request.ExecutePolling(pendingRequest) == false)
                    {
                        // Stop polling this request and complete it
                        // Remove this from the pollin list
                        pollingList.RemoveAt(i);
                        i--;

                        if (pendingRequest.request.async == true)
                        {
                            if (pendingRequest.request.IgnoreCallback == false)
                            {
                                DispatchQueueThread.AddRequest(pendingRequest);
                            }
                            else
                            {
                                if (pendingRequest.response != null)
                                {
                                    pendingRequest.response.locked = false;
                                }

                                if (pendingRequest.request != null)
                                {
                                    pendingRequest.request.locked = false;
                                }

                                Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
                            }
                        }
                        else
                        {
                            if (pendingRequest.response != null)
                            {
                                pendingRequest.response.locked = false;
                            }

                            if (pendingRequest.request != null)
                            {
                                pendingRequest.request.locked = false;
                            }

                            Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
                            AddCompletedSyncRequest(pendingRequest);
                        }
                    }
                }

                pendingRequest = null;

                // Lock the queue breifly to get the next event from it
                lock (syncQueue)
                {
                    if (pendingQueue.Count > 0)
                    {
                        pendingRequest = pendingQueue.Peek();
                    }
                }

                // If a pending event was dequeued then start processing it.
                // This will block this thread until it is complete, hence the reason why the pendingQueue must never be locked for a long time.
                if (pendingRequest != null)
                {
                    if (pendingRequest.abortPending == true)
                    {
                        lock (syncQueue)
                        {
                            // Now remove from the queue once it's processed. 
                            pendingQueue.Dequeue();
                        }

                        // Need to abort this request and not process it.
                        // Only async requests can be aborted. No need to handle sync requests.
                        EmptyResponse notification = new EmptyResponse();
                        notification.returnCode = 0;

                        int userId = -1;

                        if (pendingRequest.request != null)
                        {
                            userId = pendingRequest.request.userId;
                        }

                        DispatchQueueThread.AddNotificationRequest(notification, FunctionTypes.NotificationAborted, userId, pendingRequest.requestId, pendingRequest.request);
                    }
                    else
                    {
                        try
                        {
                            // Execute the pendingEvent
                            pendingRequest.request.Execute(pendingRequest);
                        }
                        catch (Exception e)
                        {
                            if (pendingRequest.response != null)
                            {
                                pendingRequest.response.exception = e;
                            }
                        }

                        lock (syncQueue)
                        {
                            // Now remove from the queue once it's processed. 
                            pendingQueue.Dequeue();
                        }

                        if (pendingRequest.request.IsDeferred == false || pendingRequest.response.IsErrorCodeWithoutLockCheck == true)
                        {
                            // Once complete pass on the result of the request to the dispatch queue.
                            // This way this thread can never be blocked by game script or interferred with by external code.
                            if (pendingRequest.request.async == true)
                            {
                                if (pendingRequest.request.IgnoreCallback == false)
                                {
                                    DispatchQueueThread.AddRequest(pendingRequest);
                                }
                                else
                                {
                                    if (pendingRequest.response != null)
                                    {
                                        pendingRequest.response.locked = false;
                                    }

                                    if (pendingRequest.request != null)
                                    {
                                        pendingRequest.request.locked = false;
                                    }

                                    Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
                                }
                            }
                            else
                            {
                                if (pendingRequest.response != null)
                                {
                                    pendingRequest.response.locked = false;
                                }

                                if (pendingRequest.request != null)
                                {
                                    pendingRequest.request.locked = false;
                                }

                                Main.ProcessInternalResponses(pendingRequest.request, pendingRequest.response);
                                AddCompletedSyncRequest(pendingRequest);
                            }
                        }
                        else
                        {
                            // Add this to the deferred queue for polling, until polling is complete
                            pollingList.Add(pendingRequest);
                        }
                    }
                }

                int timeOutMilliseconds = Timeout.Infinite;

                if (pollingList.Count > 0)
                {
                    timeOutMilliseconds = 16; // 16ms polling time
                }

                // Process the pending queue here
                workLoad.WaitOne(timeOutMilliseconds);
            }
        }

        internal static int GenerateNotificationRequestId()
        {
            int requestId = 0;

            lock (syncQueue)
            {
                requestId = nextRequestId;
                nextRequestId++;
            }

            return requestId;
        }

        internal static PendingRequest AddEvent(RequestBase request, ResponseBase response)
        {
            PendingRequest newEvent = new PendingRequest();
            newEvent.request = request;
            newEvent.response = response;
            newEvent.requestId = 0;
            response.locked = true;
            request.locked = true;

            lock (syncQueue)
            {
                newEvent.requestId = nextRequestId;
                nextRequestId++;
                pendingQueue.Enqueue(newEvent);
            }

            // Increase the Semaphore so the thread runs
            workLoad.Release();

            return newEvent;
        }

        internal static int WaitIfSyncRequest(PendingRequest pendingEvent)
        {
            if (pendingEvent.request.async == false)
            {
                while (RemoveCompletedSyncRequest(pendingEvent) == false)
                {
                    syncTaskCompleted.WaitOne();
                }
                return pendingEvent.response.returnCode;
            }
            else
            {
                return pendingEvent.requestId;
            }
        }

        internal static bool AbortRequest(Int32 requestId)
        {
            // Can't abort the request at the top of the queue. This might be in the middle of being processed.
            lock (syncQueue)
            {
                PendingRequest[] requests = pendingQueue.ToArray();
                for (int i = 1; i < requests.Length; i++)
                {
                    if (requests[i].requestId == requestId)
                    {
                        if (requests[i].request != null && requests[i].request.async == true)
                        {
                            requests[i].abortPending = true;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        internal static void Stop()
        {
            stopThread = true;
            workLoad.Release();
        }

    }
}
