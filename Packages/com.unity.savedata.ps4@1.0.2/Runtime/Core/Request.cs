

using System;
using System.Runtime.InteropServices;

namespace Sony
{
    namespace PS4
    {
        namespace SaveData
        {
            /// <summary>
            /// Defines the different APIs provided by the SaveData library.
            /// It is set automatically when a request object is created, and identifies the function it belongs to.
            /// </summary>
            public enum FunctionTypes
            {
                /// <summary>Non-valid function. It should never be returned</summary>
                Invalid = 0,

                /// <summary>Mount a savedata</summary>
                Mount,
                /// <summary>Unmount a savedata</summary>
                Unmount,
                /// <summary>Get mounted savedata size info</summary>
                GetMountInfo,
                /// <summary>Get mounted savedata parameters</summary>
                GetMountParams,
                /// <summary>Set mounted savedata parameters</summary>
                SetMountParams,
                /// <summary>Save icon to mounted savedata</summary>
                SaveIcon,
                /// <summary>Load icon from mounted savedata</summary>
                LoadIcon,

                /// <summary>Delete a savedata directory</summary>
                Delete,

                /// <summary>Search for a users savedata directories</summary>
                DirNameSearch,

                /// <summary>Backup a savedata directory</summary>
                Backup,
                /// <summary>Check if a savedata directory has a backup</summary>
                CheckBackup,
                /// <summary>Restore a savedata directory from a backup</summary>
                RestoreBackup,

                /// <summary>Custom request to perform file operations on a mounted savedata directory</summary>
                FileOps,

                /// <summary>Open a savedata dialogh</summary>
                OpenDialog,

                /// <summary>Notification once a backup while unmounting a savedata directory has completed</summary>
                NotificationUnmountWithBackup,
                /// <summary>Notification once a backup has completed</summary>
                NotificationBackup,
                /// <summary>Notification when a request has been aborted.</summary>
                NotificationAborted,

                /// <summary>Notification when a dialog has been opened</summary>
                NotificationDialogOpened,
                /// <summary>Notification when a dialog has been closed</summary>
                NotificationDialogClosed,
            }

            /// <summary>
            /// The base class contain common settings for all request classes
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
                /// Returns a value representing the function that uses the request.
                /// </summary>
                public FunctionTypes FunctionType { get { return functionType; } }

                /// <summary>
                /// Calling user Id performing the request
                /// </summary>
                public Int32 UserId
                {
                    get { return userId; }
                    set { ThrowExceptionIfLocked(); userId = value; }
                }

                /// <summary>
                /// Way the request will be performed: asynchronous or synchronous
                /// </summary>
                public bool Async
                {
                    get { return async; }
                    set { ThrowExceptionIfLocked(); async = value; }
                }

                /// <summary>
                /// Indicates if a Request object is locked as it's currently in the queue to be processed. 
                /// </summary>
                public bool Locked { get { return locked; } }

                /// <summary>
                /// Don't call the async callback when request has completed. Useful for polling an async response, for example inside a Coroutine, instead of receiving a callback.
                /// </summary>
                public bool IgnoreCallback
                {
                    get { return ignoreCallback; }
                    set { ThrowExceptionIfLocked(); ignoreCallback = value; }
                }

                internal virtual bool IsDeferred { get { return false; } }

                /// <summary>
                /// Initialise the class with its service type and function type.
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

                // Return true is polling should continue or false if polling can stop
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
    }
}
