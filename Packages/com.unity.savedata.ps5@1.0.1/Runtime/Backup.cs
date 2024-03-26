
using System.Runtime.InteropServices;

using Unity.SaveData.PS5.Core;

namespace Unity.SaveData.PS5.Backup
{
    /// <summary>
    /// Class that contains requests for save data backups.
    /// </summary>
    public class Backups
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern void PrxSaveDataBackup(BackupRequest request, out APIResult result);

        #endregion

        #region Requests

        /// <summary>
        /// Requests to back up a save data folder name.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class BackupRequest : RequestBase
        {
            internal DirName dirName;

            /// <summary>
            /// The name of the folder to back up.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
                set { ThrowExceptionIfLocked(); dirName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BackupRequest"/> class.
            /// </summary>
            public BackupRequest()
                : base(FunctionTypes.Backup)
            {
            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                PrxSaveDataBackup(this, out result);

                // Expect a notification to complete once the backup has been done. This is independent of the Backup operation.
                Notifications.ExpectingNotification();

                EmptyResponse response = pendingRequest.response as EmptyResponse;

                if (response != null)
                {
                    response.Populate(result);
                }
            }
        }

        #endregion

        /// <summary>
        /// This method is used to back up a save data folder. For more information, see Sony's documentation on sceSaveDataBackup for PS5.
        /// </summary>
        /// <param name="request">The save data folder to back up.</param>
        /// <param name="response">This response only contains a return code and doesn't contain any data.</param>
        /// <returns>If the operation is asynchronous, the function provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int Backup(BackupRequest request, EmptyResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }
    }
}
