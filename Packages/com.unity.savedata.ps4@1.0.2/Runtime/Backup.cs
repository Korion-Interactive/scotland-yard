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
            /// Backup savedata requests
            /// </summary>
            public class Backups
            {
                #region DLL Imports

                [DllImport("__Internal")]
                private static extern void PrxSaveDataBackup(BackupRequest request, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataCheckBackup(CheckBackupRequest request, out MemoryBufferNative data, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataRestoreBackup(RestoreBackupRequest request, out APIResult result);

                #endregion

                #region Requests

                /// <summary>
                /// Request passed to backup a savedata directory name
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class BackupRequest : RequestBase
                {
                    internal DirName dirName;

                    /// <summary>
                    /// The directory name to backup
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

                /// <summary>
                /// Request passed to check is a savedata directory has a backup
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class CheckBackupRequest : RequestBase
                {
                    internal DirName dirName;

                    [MarshalAs(UnmanagedType.I1)]
                    internal bool includeParams;

                    [MarshalAs(UnmanagedType.I1)]
                    internal bool includeIcon;

                    /// <summary>
                    /// The directory name to check
                    /// </summary>
                    public DirName DirName
                    {
                        get { return dirName; }
                        set { ThrowExceptionIfLocked(); dirName = value; }
                    }

                    /// <summary>
                    /// Select true if the parameters should be included in the returned results. False by default
                    /// </summary>
                    public bool IncludeParams
                    {
                        get { return includeParams; }
                        set { ThrowExceptionIfLocked(); includeParams = value; }
                    }

                    /// <summary>
                    /// Select true if icon details should be included in the returned results. False by default
                    /// </summary>
                    public bool IncludeIcon
                    {
                        get { return includeIcon; }
                        set { ThrowExceptionIfLocked(); includeIcon = value; }
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="CheckBackupRequest"/> class.
                    /// </summary>
                    public CheckBackupRequest()
                        : base(FunctionTypes.CheckBackup)
                    {
                    }

                    internal override void Execute(PendingRequest pendingRequest)
                    {
                        APIResult result;

                        MemoryBufferNative data = new MemoryBufferNative();

                        PrxSaveDataCheckBackup(this, out data, out result);

                        CheckBackupResponse response = pendingRequest.response as CheckBackupResponse;

                        if (response != null)
                        {
                            response.Populate(result, data);
                        }
                    }
                }

                /// <summary>
                /// Request passed to restore a savedata directory from its backup
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class RestoreBackupRequest : RequestBase
                {
                    internal DirName dirName;

                    /// <summary>
                    /// The directory name to restore
                    /// </summary>
                    public DirName DirName
                    {
                        get { return dirName; }
                        set { ThrowExceptionIfLocked(); dirName = value; }
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="RestoreBackupRequest"/> class.
                    /// </summary>
                    public RestoreBackupRequest()
                        : base(FunctionTypes.RestoreBackup)
                    {
                    }

                    internal override void Execute(PendingRequest pendingRequest)
                    {
                        APIResult result;

                        PrxSaveDataRestoreBackup(this, out result);

                        EmptyResponse response = pendingRequest.response as EmptyResponse;

                        if (response != null)
                        {
                            response.Populate(result);
                        }
                    }
                }

                #endregion

                #region Response

                /// <summary>
                /// Response class containing data for a savedata directory backup
                /// </summary>
                public class CheckBackupResponse : ResponseBase
                {
                    internal bool hasParams;

                    internal SaveDataParams sdParams;
                    internal Icon icon = null;

                    /// <summary>
                    /// The savedata icon retrieved if there is one. This will be null is no icon available. Only valid is <see cref="HasIcon"/> is true. 
                    /// </summary>
                    public Icon Icon { get { ThrowExceptionIfLocked(); return icon; } }

                    /// <summary>
                    /// True if paramater info was returned in the request. 
                    /// </summary>
                    public bool HasParams
                    {
                        get { ThrowExceptionIfLocked(); return hasParams; }
                    }

                    /// <summary>
                    /// True if icon data was returned in the request. 
                    /// </summary>
                    public bool HasIcon
                    {
                        get { ThrowExceptionIfLocked(); return icon != null; }
                    }

                    /// <summary>
                    /// Savedata parameter info. Only valid if <see cref="HasParams"/> is true.
                    /// </summary>
                    public SaveDataParams Params
                    {
                        get { ThrowExceptionIfLocked(); return sdParams; }
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="CheckBackupResponse"/> class.
                    /// </summary>
                    public CheckBackupResponse()
                    {

                    }

                    internal void Populate(APIResult result, MemoryBufferNative data)
                    {
                        base.Populate(result);

                        MemoryBuffer readBuffer = new MemoryBuffer(data);
                        readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                        bool valid = readBuffer.ReadBool();

                        if (valid == true)
                        {
                            hasParams = readBuffer.ReadBool();
                            bool wasIconFound = readBuffer.ReadBool();

                            if (hasParams == true)
                            {
                                sdParams.Read(readBuffer);
                            }

                            if (wasIconFound == true)
                            {
                                icon = Icon.ReadAndCreate(readBuffer);
                            }
                            else
                            {
                                icon = null;
                            }
                        }

                        readBuffer.CheckEndMarker();
                    }
                }

                #endregion

                /// <summary>
                /// This method is used to backup a savedata directory. See sceSaveDataBackup PS4 documention for further details. 
                /// </summary>
                /// <param name="request">The savedata directory to backup.</param>
                /// <param name="response">This response does not have data, only return code.</param>
                /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
                public static int Backup(BackupRequest request, EmptyResponse response)
                {
                    DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

                    PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

                    return ProcessQueueThread.WaitIfSyncRequest(pe);
                }

                /// <summary>
                /// This method is used to check is a savedata directory has a backup. See sceSaveDataCheckBackupData PS4 documention for further details. 
                /// </summary>
                /// <param name="request">The savedata directory to check.</param>
                /// <param name="response">The backup details if any exist.</param>
                /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
                public static int CheckBackup(CheckBackupRequest request, CheckBackupResponse response)
                {
                    DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

                    PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

                    return ProcessQueueThread.WaitIfSyncRequest(pe);
                }

                /// <summary>
                /// This method is used to restore a savedata directory from a backup,it is exists. See sceSaveDataRestoreBackupData PS4 documention for further details. 
                /// </summary>
                /// <param name="request">The savedata directory to restore.</param>
                /// <param name="response">This response does not have data, only return code.</param>
                /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
                public static int RestoreBackup(RestoreBackupRequest request, EmptyResponse response)
                {
                    DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

                    PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

                    return ProcessQueueThread.WaitIfSyncRequest(pe);
                }

            }
        } // SavedGames
    } // PS4
} // Sony