using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sony
{
    namespace PS4
    {
        namespace SaveData
        {
            /// <summary>
            /// Delete savedata directory
            /// </summary>
            public class Deleting
            {
                #region DLL Imports

                [DllImport("__Internal")]
                private static extern void PrxSaveDataDelete(DeleteRequest request, out APIResult result);

                #endregion

                #region Requests

                /// <summary>
                /// Request passed to delete a savedata directory name
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class DeleteRequest : RequestBase
                {
                    internal DirName dirName;

                    /// <summary>
                    /// The directory name to delete
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
                /// This method is used to delete a savedata directory. See sceSaveDataDelete PS4 documention for further details. 
                /// </summary>
                /// <param name="request">The savedata directory to delete.</param>
                /// <param name="response">This response does not have data, only return code.</param>
                /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
                public static int Delete(DeleteRequest request, EmptyResponse response)
                {
                    DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

                    PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

                    return ProcessQueueThread.WaitIfSyncRequest(pe);
                }
            }
        } // SavedGames
    } // PS4
} // Sony