using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Dialogs
{
    /// <summary>
    /// Common dialog system
    /// </summary>
    public class DialogSystem
    {
        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            Main.OnSystemUpdate += Update;
            workerQueue.Start("Dialogs");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
            Main.OnSystemUpdate -= Update;
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Dialog queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Dialog status
        /// </summary>
        public enum DialogStatus
        {
            /// <summary> Not set </summary>
            None,
            /// <summary> The dialog is open and running </summary>
            Running,
            /// <summary> The dialog has finished and the OK/Yes button pressed </summary>
            FinishedOK,
            /// <summary> The dialog has finished and the Cancel/No button pressed </summary>
            FinishedCancel,
            /// <summary> The dialog has finished and the purchased button pressed </summary>
            FinishedPurchased,
            /// <summary> The dialog has was closed by a script method </summary>
            ForceClosed,
        }

        /// <summary>
        /// Update function
        /// </summary>
        private static void Update()
        {

        }

    }
}
