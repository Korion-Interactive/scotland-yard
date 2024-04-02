

using System;
using System.Collections.Generic;
using System.Threading;
using Unity.SaveData.PS5.Initialization;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// Callback class that contains the results of an asynchronous request or notification.
    /// </summary>
    public class SaveDataCallbackEvent
    {
        internal FunctionTypes apiCalled;
        internal Int32 requestId;
        internal Int32 userId;
        internal RequestBase request;
        internal ResponseBase response;

        /// <summary>
        /// Function called to perform the request. For the notification service, provides the notification type.
        /// </summary>
        public FunctionTypes ApiCalled { get { return apiCalled; } }

        /// <summary>
        /// The request Id returned when the async request was made.
        /// </summary>
        public Int32 RequestId { get { return requestId; } }

        /// <summary>
        /// The response passed when the request was made. For notifications, the plug-in creates this.
        /// </summary>
        public ResponseBase Response { get { return response; } }

        /// <summary>
        /// The request instance that started the async request. Will be null for any Notification responses.
        /// </summary>
        public RequestBase Request { get { return request; } }

        /// <summary>
        /// The ID of the user who performed the request.
        /// </summary>
        public Int32 UserId { get { return userId; } }

    };

    static class DispatchQueueThread
    {
        static Thread thread;
        static bool stopThread = false;
        static Semaphore workLoad = new Semaphore(0, 1000);
        static object syncQueue = new object();
        static ThreadSettingsNative threadSettings;

        static Queue<SaveDataCallbackEvent> pendingQueue = new Queue<SaveDataCallbackEvent>();

        public static void Start(ThreadAffinity affinity)
        {
            stopThread = false;
            thread = new Thread(new ThreadStart(RunProc));
            thread.Name = "SaveDataDispatchQueue";

            threadSettings = new ThreadSettingsNative(affinity, thread.Name);

            thread.Start();
        }

        private static void RunProc()
        {
            // Call into the PRX and change this threads affinity
            Init.SetThreadAffinity(threadSettings);

            workLoad.WaitOne();

            while (!stopThread)
            {
                SaveDataCallbackEvent callbackEvent = null;

                // Lock the queue breifly to get the next event from it
                lock (syncQueue)
                {
                    if (pendingQueue.Count > 0)
                    {
                        callbackEvent = pendingQueue.Dequeue();
                    }
                }

                // If a pending event was dequeued then start processing it.
                if (callbackEvent != null)
                {
                    if (callbackEvent.response != null)
                    {
                        callbackEvent.response.locked = false;
                    }

                    if (callbackEvent.request != null)
                    {
                        callbackEvent.request.locked = false;
                    }

                    Main.ProcessInternalResponses(callbackEvent.request, callbackEvent.response);

                    // Send event back to the game script code
                    Main.CallOnAsyncEvent(callbackEvent);
                }

                // Process the pending queue here
                workLoad.WaitOne();
            }
        }

        internal static void AddRequest(PendingRequest finishedRequest)
        {
            SaveDataCallbackEvent newEvent = new SaveDataCallbackEvent();

            newEvent.apiCalled = finishedRequest.request.functionType;
            newEvent.requestId = finishedRequest.requestId;
            newEvent.userId = finishedRequest.request.userId;
            newEvent.request = finishedRequest.request;
            newEvent.response = finishedRequest.response;

            lock (syncQueue)
            {
                pendingQueue.Enqueue(newEvent);
            }

            // Increase the Semaphore so the thread runs
            workLoad.Release();
        }

        internal static void AddNotificationRequest(ResponseBase response, FunctionTypes apiCalled, Int32 userId, int requestId = -1, RequestBase request = null)
        {
            if (requestId == -1)
            {
                requestId = ProcessQueueThread.GenerateNotificationRequestId();
            }

            SaveDataCallbackEvent newEvent = new SaveDataCallbackEvent();

            newEvent.apiCalled = apiCalled;
            newEvent.requestId = requestId;
            newEvent.userId = userId;
            newEvent.request = request;
            newEvent.response = response;

            lock (syncQueue)
            {
                pendingQueue.Enqueue(newEvent);
            }

            // Increase the Semaphore so the thread runs
            workLoad.Release();
        }

        internal static void Stop()
        {
            stopThread = true;
            workLoad.Release();
        }

        internal static bool IsSameThread()
        {
            return Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId;
        }

        internal static void ThrowExceptionIfSameThread(bool asyncRequest)
        {
            if (asyncRequest == false && IsSameThread() == true)
            {
                throw new SaveDataException("A synchronous (blocking) request can't be made on this thread.");
            }
        }
    }
}
