
using System;
using System.Runtime.InteropServices;
using System.Threading;

using Unity.SaveData.PS5.Internal;
using Unity.SaveData.PS5.Initialization;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// Notification called after the backup which was triggered by unmounting a mount point has completed. See <see cref="Mounting.Unmount"/>
    /// </summary>
    public class UnmountWithBackupNotification : ResponseBase
    {
        internal Int32 userId;
        internal DirName dirName;

        /// <summary>
        /// ID of the user who made the unmount request.
        /// </summary>
        public Int32 UserId
        {
            get { ThrowExceptionIfLocked(); return userId; }
        }

        /// <summary>
        /// Name of the save data folder that was backed up.
        /// </summary>
        public DirName DirName
        {
            get { ThrowExceptionIfLocked(); return dirName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmountWithBackupNotification"/> class.
        /// </summary>
        public UnmountWithBackupNotification()
        {

        }
    }

    /// <summary>
    /// Notification when a save data folder has been backed up. See <see cref="Backups.Backup"/>
    /// </summary>
    public class BackupNotification : ResponseBase
    {
        internal Int32 userId;
        internal DirName dirName;

        /// <summary>
        /// ID of the user who made the backup request.
        /// </summary>
        public Int32 UserId
        {
            get { ThrowExceptionIfLocked(); return userId; }
        }

        /// <summary>
        /// Name of the save data folder that was backed up.
        /// </summary>
        public DirName DirName
        {
            get { ThrowExceptionIfLocked(); return dirName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupNotification"/> class.
        /// </summary>
        public BackupNotification()
        {

        }
    }

    internal class Notifications
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern int PrxNotificationPoll(out MemoryBufferNative data, out APIResult result);

        #endregion

        static Thread thread;
        static bool stopThread = false;
        static ThreadSettingsNative threadSettings;
        static bool isBusy = false;

        static Semaphore workLoad = new Semaphore(0, 1000);

        public static void Start(ThreadAffinity affinity)
        {
            stopThread = false;
            thread = new Thread(new ThreadStart(RunProc));
            thread.Name = "SaveDataNotifications";

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
                int timeOutMilliseconds = Timeout.Infinite;

                try
                {
                    bool continuePolling = ReadAndCreateNotification();

                    if (continuePolling == true)
                    {
                        // In this case the notification failed so need to do a timeout and after a while poll the notification again as there is a notification expected
                        // but it hasn't completed yet.
                        timeOutMilliseconds = 1000;
                        isBusy = true;
                    }
                    else
                    {
                        isBusy = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception Occured in Notifications handler : " + e.Message);
                    Console.WriteLine(e.StackTrace);
                    throw;  // Throw the expection again as this shouldn't really hide the exception has occured.
                }

                workLoad.WaitOne(timeOutMilliseconds);
            }
        }

        internal static void Stop()
        {
            stopThread = true;
            workLoad.Release();
        }

        internal static void ExpectingNotification()
        {
            workLoad.Release();
        }

        internal static bool IsBusy()
        {
            return isBusy;
        }

        private static bool ReadAndCreateNotification()
        {
            bool continuePolling = true;

            APIResult result;

            MemoryBufferNative data = new MemoryBufferNative();

            PrxNotificationPoll(out data, out result);

            MemoryBuffer readBuffer = new MemoryBuffer(data);
            readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

            Int32 returnCode = readBuffer.ReadInt32();

            if (returnCode == 0)
            {
                DirName dirName = new DirName();
                // A notification has been found.
                // Therefore read the rest of the data
                Int32 type = readBuffer.ReadInt32();
                Int32 userId = readBuffer.ReadInt32();
                Int32 errorCode = readBuffer.ReadInt32();
                dirName.Read(readBuffer);

                // SCE_SAVE_DATA_EVENT_TYPE_UMOUNT_BACKUP_END           1  Backup processing in sceSaveDataUmountWithBackup() ended
                // SCE_SAVE_DATA_EVENT_TYPE_BACKUP_END                  2  Backup processing in sceSaveDataBackup() ended
                // SCE_SAVE_DATA_EVENT_TYPE_SAVE_DATA_MEMORY_SYNC_END   3   Synchronous processing in sceSaveDataSyncSaveDataMemory() ended
                if (type == 1)
                {
                    UnmountWithBackupNotification notification = new UnmountWithBackupNotification();
                    notification.returnCode = errorCode;
                    notification.userId = userId;
                    notification.dirName = dirName;

                    DispatchQueueThread.AddNotificationRequest(notification, FunctionTypes.NotificationUnmountWithBackup, userId);

                    continuePolling = true;
                }
                else if (type == 2)
                {
                    BackupNotification notification = new BackupNotification();
                    notification.returnCode = errorCode;
                    notification.userId = userId;
                    notification.dirName = dirName;

                    DispatchQueueThread.AddNotificationRequest(notification, FunctionTypes.NotificationBackup, userId);

                    continuePolling = true;
                }
            }
            else
            {
                // An error has occured
                if ((uint)returnCode == 0x809f0018) //SCE_SAVE_DATA_ERROR_EVENT_BUSY  Event is being processed and results have not been output
                {
                    continuePolling = true;
                }
                else if ((uint)returnCode == 0x809f0008) //SCE_SAVE_DATA_ERROR_NOT_FOUND  No events and no events being processed
                {
                    continuePolling = false;
                }
            }

            readBuffer.CheckEndMarker();

            return continuePolling;
        }
    }
}
