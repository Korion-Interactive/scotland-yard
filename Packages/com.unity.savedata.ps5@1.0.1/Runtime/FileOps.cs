using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Mount;

namespace Unity.SaveData.PS5.Info
{
    /// <summary>
    /// Perform file operations on mounted save data.
    /// </summary>
    public class FileOps
    {
        #region Requests

        /// <summary>
        /// Base request class for custom file operations. Override the DoFileOperations method to customize file handling inside a save data folder. 
        /// Use System.IO .NET methods for file handling.
        /// Prefix any folder or file names with the save data mount point name as the root path.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public abstract class FileOperationRequest : RequestBase
        {
            internal Mounting.MountPointName mountPointName;

            /// <summary>
            /// The mount point name to perform the file operations.
            /// </summary>
            public Mounting.MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileOperationRequest"/> class.
            /// </summary>
            public FileOperationRequest()
                : base(FunctionTypes.FileOps)
            {
            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                FileOperationResponse response = pendingRequest.response as FileOperationResponse;

                Mounting.MountPoint mp = Mounting.FindMountPoint(mountPointName);

                if (mp == null || mp.IsMounted == false)
                {
                    response.returnCode = unchecked((int)ReturnCodes.InvalidMountPointName);
                    return;
                }

                DoFileOperations(mp, response);
            }

            /// <summary>
            /// Override this method to perform file operations.
            /// </summary>
            /// <param name="mp">The mount point root path to use.</param>
            /// <param name="response">The custom response object used to store results from any file operations.</param>
            public abstract void DoFileOperations(Mounting.MountPoint mp, FileOperationResponse response);

        }

        #endregion

        /// <summary>
        /// Base response class for custom file operations. Inherit from this class to provide custom file handling results.
        /// </summary>
        public abstract class FileOperationResponse : ResponseBase
        {
            internal float progress;

            /// <summary>
            /// Gets the current progress of the file operation. The value can be in the 0.0 to 1.0 range.
            /// </summary>
            public float Progress
            {
                get { return progress; }
            }

            /// <summary>
            /// Update the progress value of the file operation. Used by the dialog system when displaying a progress bar.
            /// </summary>
            /// <param name="progress">Progress value (0.0 to 1.0)</param>
            public void UpdateProgress(float progress)
            {
                this.progress = progress;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileOperationResponse"/> class.
            /// </summary>
            public FileOperationResponse()
            {
                progress = 0.0f;
            }
        }

        /// <summary>
        /// Use this method to perform file operations on a save data folder.
        /// </summary>
        /// <param name="request">The custom request object to perform the file operations.</param>
        /// <param name="response">The custom response object to store any results from file operations.</param>
        /// <returns>If the operation is asynchronous, the function provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int CustomFileOp(FileOperationRequest request, FileOperationResponse response)
        {
            response.progress = 0.0f;

            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }
    }
}
