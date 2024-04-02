using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;

namespace Unity.SaveData.PS5.Delete
{
    /// <summary>
    /// Deletes a save data folder.
    /// </summary>
    public class Deleting
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern void PrxSaveDataDelete(DeleteRequest request, out APIResult result);

        #endregion

        #region Requests

        /// <summary>
        /// Requests to to delete a save data folder identified by its name.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class DeleteRequest : RequestBase
        {
            internal DirName dirName;

            /// <summary>
            /// The name of the folder to delete.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
                set { ThrowExceptionIfLocked(); dirName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DeleteRequest"/> class.
            /// </summary>
            public DeleteRequest()
                : base(FunctionTypes.Delete)
            {
            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                PrxSaveDataDelete(this, out result);

                EmptyResponse response = pendingRequest.response as EmptyResponse;

                if (response != null)
                {
                    response.Populate(result);
                }
            }
        }

        #endregion

        /// <summary>
        /// This method is used to delete a save data folder. For more information, see Sony developer documentation on sceSaveDataDelete for PS5.
        /// </summary>
        /// <param name="request">The save data folder to delete.</param>
        /// <param name="response">This response contains a return code and doesn't contain any actual data.</param>
        /// <returns>If the operation is asynchronous, the function provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or if an internal error has occured inside the plug-in.</exception>
        public static int Delete(DeleteRequest request, EmptyResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }
    }
}
