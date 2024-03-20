

using System;
using System.Runtime.InteropServices;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// Defines the different APIs provided by the SaveData library.
    /// This is set automatically when a request object is created, and it identifies the function it belongs to.
    /// </summary>
    public enum FunctionTypes
    {
        /// <summary>Non-valid function. It should never be returned</summary>
        Invalid = 0,

        /// <summary>Mount a save data.</summary>
        Mount,
        /// <summary>Unmount a save data.</summary>
        Unmount,
        /// <summary>Get mounted save data size info.</summary>
        GetMountInfo,
        /// <summary>Get mounted save data parameters.</summary>
        GetMountParams,
        /// <summary>Set mounted save data parameters.</summary>
        SetMountParams,
        /// <summary>Save icon to mounted save data.</summary>
        SaveIcon,
        /// <summary>Load icon from mounted save data.</summary>
        LoadIcon,

        /// <summary>Delete a save data folder.</summary>
        Delete,

        /// <summary>Search for a user's save data folders.</summary>
        DirNameSearch,

        /// <summary>Back up a save data folder.</summary>
        Backup,

        /// <summary>Custom request to perform file operations on a mounted save data folder.</summary>
        FileOps,

        /// <summary>Open a save data dialog.</summary>
        OpenDialog,

        /// <summary>Notification that triggers when a backup while unmounting a save data folder has completed.</summary>
        NotificationUnmountWithBackup,
        /// <summary>Notification that triggers when a backup has completed.</summary>
        NotificationBackup,
        /// <summary>Notification that triggers when a request has been aborted.</summary>
        NotificationAborted,

        /// <summary>Notification that triggers when a dialog has been opened.</summary>
        NotificationDialogOpened,
        /// <summary>Notification that triggers when a dialog has been closed.</summary>
        NotificationDialogClosed,
    }

    /// <summary>
    /// The base class that contains common settings for all request classes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public class RequestBase
    {
        internal FunctionTypes functionType;
        internal Int32 userId;

        [MarshalAs(UnmanagedType.I1)]
        internal bool async = true;

        [MarshalAs(UnmanagedType.I1)]
        internal bool locked;

        [MarshalAs(UnmanagedType.I1)]
        bool ignoreCallback;

        internal UInt32 padding = 1234;

        /// <summary>
        /// Returns the function that uses the request.
        /// </summary>
        public FunctionTypes FunctionType { get { return functionType; } }

        /// <summary>
        /// Gets the ID of the user performing the request.
        /// </summary>
        public Int32 UserId
        {
            get { return userId; }
            set { ThrowExceptionIfLocked(); userId = value; }
        }

        /// <summary>
        /// True if the request will be performed asynchronously, false otherwise.
        /// </summary>
        public bool Async
        {
            get { return async; }
            set { ThrowExceptionIfLocked(); async = value; }
        }

        /// <summary>
        /// Indicates if a Request object is locked because it is currently in the queue to be processed. 
        /// </summary>
        public bool Locked { get { return locked; } }

        /// <summary>
        /// True if you want to ignore the async callback when the request has completed, false otherwise. This is useful for polling an async response, for example inside a Coroutine, instead of receiving a callback.
        /// </summary>
        public bool IgnoreCallback
        {
            get { return ignoreCallback; }
            set { ThrowExceptionIfLocked(); ignoreCallback = value; }
        }

        internal virtual bool IsDeferred { get { return false; } }

        /// <summary>
        /// Initializes the class with its service type and function type.
        /// </summary>
        /// <param name="functionType">The function type.</param>
        internal RequestBase(FunctionTypes functionType)
        {
            userId = -1;
            this.functionType = functionType;
        }

        internal virtual void Execute(PendingRequest pendingRequest)
        {

        }

        // Returns true is polling should continue or false if polling can stop.
        internal virtual bool ExecutePolling(PendingRequest completedRequest)
        {
            return false;
        }

        internal void ThrowExceptionIfLocked()
        {
            if (locked == true)
            {
                throw new SaveDataException("This request object can't be changed while it is waiting to be processed.");
            }
        }
    }
}
