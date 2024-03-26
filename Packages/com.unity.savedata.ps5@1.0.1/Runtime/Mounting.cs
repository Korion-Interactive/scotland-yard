using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Internal;

namespace Unity.SaveData.PS5.Mount
{
    /// <summary>
    /// Mount save data directory.
    /// </summary>
    public class Mounting
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern void PrxSaveDataMount(MountRequest request, out MemoryBufferNative data, out APIResult result);
        [DllImport("__Internal")]
        private static extern void PrxSaveDataMountPS4(MountPS4Request request, out MemoryBufferNative data, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataUnmount(UnmountRequest request, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataGetMountInfo(GetMountInfoRequest request, out MemoryBufferNative data, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataGetMountParams(GetMountParamsRequest request, out MemoryBufferNative data, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataSetMountParams(SetMountParamsRequest request, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataSaveIcon(SaveIconRequest request, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataLoadIcon(LoadIconRequest request, out MemoryBufferNative data, out APIResult result);

        [DllImport("__Internal")]
        private static extern void PrxSaveDataGetIconSize(byte[] pngData, out Int32 width, out Int32 height);

        #endregion

        #region Structures

        /// <summary>
        /// Save data mount point name.
        /// </summary>
        /// <remarks>
        /// The save data mount point name is a character string called "/savedata0", "/savedata1", […] or "/savedata15".
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MountPointName
        {
            /// <summary>
            /// Maximum size of mount point name.
            /// </summary>
            public const Int32 MOUNT_POINT_DATA_MAXSIZE = 15;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MOUNT_POINT_DATA_MAXSIZE + 1)]
            internal string data;

            /// <summary>
            /// Mount point name.
            /// </summary>
            public string Data
            {
                get { return data; }
            }

            internal void Read(MemoryBuffer buffer)
            {
                buffer.ReadString(ref data);
            }

            /// <summary>
            /// The mount point name string.
            /// </summary>
            /// <returns>The mount point name</returns>
            public override string ToString()
            {
                return data;
            }
        }

        /// <summary>
        /// Mount mode
        /// </summary>
        [Flags]
        public enum MountModeFlags : uint
        {
            /// <summary>Non-valid. It should never be used.</summary>
            Invalid = 0,
            /// <summary>Read-only. See SCE_SAVE_DATA_MOUNT_MODE_RDONLY.</summary>
            ReadOnly = 1,
            /// <summary>Read/write. See SCE_SAVE_DATA_MOUNT_MODE_RDWR.</summary>
            ReadWrite = 2,
            /// <summary>Create new (error if save data folder already exists). See SCE_SAVE_DATA_MOUNT_MODE_CREATE.</summary>
            Create = 4,
            /// <summary>Destruct off (not recommended). See SCE_SAVE_DATA_MOUNT_MODE_DESTRUCT_OFF.</summary>
            DestructOff = 8,
            /// <summary>Copy save_data.png in package as icon when creating a new save data. See SCE_SAVE_DATA_MOUNT_MODE_COPY_ICON.</summary>
            CopyIcon = 16,
            /// <summary>Create new (mount save data folder if it already exists). See SCE_SAVE_DATA_MOUNT_MODE_CREATE2.</summary>
            Create2 = 32
        }

        /// <summary>
        /// Details for a mounted save data folder.
        /// </summary>
        public class MountPoint
        {
            internal MountPointName name;
            internal DateTime openTime;
            internal Int32 userId;
            internal MountModeFlags mountMode;
            internal DirName dirName;
            internal bool isMounted;

            /// <summary>
            /// The pathname for the mounted save data. This is a character string called "/savedata0", "/savedata1", […] or "/savedata15".
            /// This is the root path name when doing file operations.
            /// </summary>
            public MountPointName PathName
            {
                get { return name; }
            }

            /// <summary>
            /// The time the save data folder was mounted
            /// </summary>
            public DateTime OpenTime
            {
                get { return openTime; }
            }

            /// <summary>
            /// The estimated time the save data folder has been mounted for. See TRC R4098 for details on how long a Read/Write save data folder can remain mounted. Only use this as a guide.
            /// </summary>
            public double TimeMountedEstimate
            {
                get
                {
                    return (DateTime.UtcNow - openTime).TotalSeconds;
                }
            }

            /// <summary>
            /// Gets the ID of the user who opened the mount point.
            /// </summary>
            public Int32 UserId
            {
                get { return userId; }
            }

            /// <summary>
            /// Flags used to mount the save data folder.
            /// </summary>
            public MountModeFlags MountMode
            {
                get { return mountMode; }
            }

            /// <summary>
            /// Folder name associated with the mount point. When doing file operations, use the mount point name. Do not use this folder name.
            public DirName DirName
            {
                get { return dirName; }
            }

            /// <summary>
            /// Flag to indicate if the mount point is still mounted. Will be set to false when an unmount request is pending in the request queue. Don't use any mount points
            /// that are going to be unmounted on the background thread.
            /// </summary>
            public bool IsMounted
            {
                get { return isMounted; }
            }

            /// <summary>
            /// Mount point name.
            /// </summary>
            /// <returns>Mount point name.</returns>
            public override string ToString()
            {
                return name.Data;
            }
        }

        #endregion

        #region Requests

        /// <summary>
        /// Request parameters to mount a save data folder.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class MountRequest : RequestBase
        {


            /// <summary>
            /// Number of bytes in a single block.
            /// </summary>
            public const Int32 BLOCK_SIZE = 65536; // SCE_SAVE_DATA_BLOCK_SIZE2

            /// <summary>
            /// The minimum reserved size of a save data folder.
            /// </summary>
            public const Int32 BLOCKS_MIN = 48; // SCE_SAVE_DATA_BLOCKS_MIN3

            /// <summary>
            /// The maximum reserved size of a save data folder.
            /// </summary>
            public const Int32 BLOCKS_MAX = 16384; // SCE_SAVE_DATA_BLOCKS_MAX2

            /// <summary>
            /// Valud for system blocks to create a save data that supported the rollback feature.
            /// </summary>
            public const UInt64 SYSTEM_BLOCKS_EQUAL_TO_BLOCKS = 0xFFFFFFFFFFFFFFFF; // SCE_SAVE_DATA_SYSTEM_BLOCKS_EQUAL_TO_BLOCKS


            internal DirName dirName;
            internal UInt64 blocks;
            internal UInt64 systemBlocks;
            internal MountModeFlags mountMode;

            /// <summary>
            /// Name of the folder to mount.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
                set { ThrowExceptionIfLocked(); dirName = value; }
            }

            /// <summary>
            /// Maximum number of save data blocks during new creation (number of blocks, 96 to 32768).
            /// </summary>
            public UInt64 Blocks
            {
                get { return blocks; }
                set
                {
                    ThrowExceptionIfLocked();
                    if (value < BLOCKS_MIN)
                    {
                        throw new SaveDataException("The block size can't be less than " + BLOCKS_MIN + " blocks (BLOCKS_MIN)");
                    }
                    if (value > BLOCKS_MAX)
                    {
                        throw new SaveDataException("The block size can't be greater than " + BLOCKS_MAX + " blocks (BLOCKS_MAX)");
                    }

                    blocks = value;
                }
            }

            /// <summary>
            /// Number of blocks for save data system area.
            /// </summary>
            public UInt64 SystemBlocks
            {
                get { return systemBlocks; }
                set
                {
                    ThrowExceptionIfLocked();
                    systemBlocks = value;
                }
            }

            /// <summary>
            /// Mount mode.
            /// </summary>
            public MountModeFlags MountMode
            {
                get { return mountMode; }
                set { ThrowExceptionIfLocked(); mountMode = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MountRequest"/> class.
            /// </summary>
            public MountRequest()
                : base(FunctionTypes.Mount)
            {
                blocks = BLOCKS_MIN;
#if UNITY_PS5
                systemBlocks = (Main.initResult.sceSDKVersion >= 0x05000000) ? SYSTEM_BLOCKS_EQUAL_TO_BLOCKS : 0; // for runtime >= sdk 5.00 we default to the rollback, otherwise defaulting to zero is correct.
#endif
            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataMount(this, out data, out result);

                MountResponse response = pendingRequest.response as MountResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }


       /// <summary>
        /// Request parameters to mount a PS4 save data folder.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class MountPS4Request : RequestBase
        {


            internal DirName dirName;
            internal TitleId titleId;
            internal Fingerprint fingerprint;

            /// <summary>
            /// Name of the folder to mount.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
                set { ThrowExceptionIfLocked(); dirName = value; }
            }

            /// <summary>
            /// TitleId to mount.
            /// </summary>
            public TitleId TitleId
            {
                get { return titleId; }
                set { ThrowExceptionIfLocked(); titleId = value; }
            }

           /// <summary>
            /// Fingerprint of savedata to mount.
            /// </summary>
            public Fingerprint Fingerprint
            {
                get { return fingerprint; }
                set { ThrowExceptionIfLocked(); fingerprint = value; }
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="MountRequest"/> class.
            /// </summary>
            public MountPS4Request()
                : base(FunctionTypes.Mount)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataMountPS4(this, out data, out result);

                MountResponse response = pendingRequest.response as MountResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }


        /// <summary>
        /// Unmounts a save data mount point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class UnmountRequest : RequestBase
        {
            internal MountPointName mountPointName;

            /// <summary>
            /// Name of mount point to unmount.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UnmountRequest"/> class.
            /// </summary>
            public UnmountRequest()
                : base(FunctionTypes.Unmount)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                PrxSaveDataUnmount(this, out result);

                EmptyResponse response = pendingRequest.response as EmptyResponse;

                if (response != null)
                {
                    response.Populate(result);
                }
            }
        }

        /// <summary>
        /// Gets mount information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class GetMountInfoRequest : RequestBase
        {
            internal MountPointName mountPointName;

            /// <summary>
            /// Mount point name to retieve the info.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GetMountInfoRequest"/> class.
            /// </summary>
            public GetMountInfoRequest()
                : base(FunctionTypes.GetMountInfo)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataGetMountInfo(this, out data, out result);

                MountInfoResponse response = pendingRequest.response as MountInfoResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }

        /// <summary>
        /// Get save data parameters.

        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class GetMountParamsRequest : RequestBase
        {
            internal MountPointName mountPointName;

            /// <summary>
            /// Moint point name to retrieve the parameters.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GetMountParamsRequest"/> class.
            /// </summary>
            public GetMountParamsRequest()
                : base(FunctionTypes.GetMountParams)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataGetMountParams(this, out data, out result);

                MountParamsResponse response = pendingRequest.response as MountParamsResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }

        /// <summary>
        /// Sets parameters for a mount point name.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class SetMountParamsRequest : RequestBase
        {
            internal MountPointName mountPointName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaveDataParams.TITLE_MAXSIZE + 1)]
            internal string title;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaveDataParams.SUBTITLE_MAXSIZE + 1)]
            internal string subTitle;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SaveDataParams.DETAIL_MAXSIZE + 1)]
            internal string detail;

            internal UInt32 userParam;

            /// <summary>
            /// Mount point name to set the parameters.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Parameters to set.
            /// </summary>
            public SaveDataParams Params
            {
                get
                {
                    SaveDataParams temp = new SaveDataParams();

                    temp.title = title;
                    temp.subTitle = subTitle;
                    temp.detail = detail;
                    temp.userParam = userParam;

                    return temp;
                }
                set
                {
                    ThrowExceptionIfLocked();
                    title = value.title;
                    subTitle = value.subTitle;
                    detail = value.detail;
                    userParam = value.userParam;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SetMountParamsRequest"/> class.
            /// </summary>
            public SetMountParamsRequest()
                : base(FunctionTypes.SetMountParams)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                PrxSaveDataSetMountParams(this, out result);

                EmptyResponse response = pendingRequest.response as EmptyResponse;

                if (response != null)
                {
                    response.Populate(result);
                }
            }
        }

        /// <summary>
        /// Request class to save icon to a mount point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class SaveIconRequest : RequestBase
        {
            /// <summary>
            /// The maximum length of the pathname to an icon file
            /// </summary>
            public const Int32 FILEPATH_LENGTH = 127;

            /// <summary>
            /// The maximum number of bytes in the icon PNG file.
            /// </summary>
            public const Int32 ICON_FILE_MAXSIZE = (DATA_ICON_WIDTH_FULL * DATA_ICON_HEIGHT_FULL * 4);

            /// <summary>
            /// The required width of the full icon.
            /// </summary>
            public const Int32 DATA_ICON_WIDTH_FULL = 776;

            /// <summary>
            /// The required height of the full icon.
            /// </summary>
            public const Int32 DATA_ICON_HEIGHT_FULL = 436;

            /// <summary>
            /// The required width of the small icon.
            /// </summary>
            public const Int32 DATA_ICON_WIDTH_SMALL = 688;

            /// <summary>
            /// The required height of the icon.
            /// </summary>
            public const Int32 DATA_ICON_HEIGHT_SMALL = 388;

            internal MountPointName mountPointName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = FILEPATH_LENGTH + 1)]
            internal string iconPath;

            [MarshalAs(UnmanagedType.LPArray)]
            internal byte[] rawPNG;

            internal UInt64 pngDataSize;

            /// <summary>
            /// Mount name to save the icon.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Path to a PNG file.
            /// </summary>
            public string IconPath
            {
                get { return iconPath; }
                set { ThrowExceptionIfLocked(); iconPath = value; }
            }

            /// <summary>
            /// An array of bytes containing the PNG data.
            /// </summary>
            public byte[] RawPNG
            {
                get { return rawPNG; }
                set
                {
                    ThrowExceptionIfLocked();
                    //if (value.Length > MAX_SIZE_FIXED_DATA)
                    //{
                    //    throw new NpToolkitException("The size of the fixed data array is more than " + MAX_SIZE_FIXED_DATA + " bytes.");
                    //}
                    rawPNG = value;
                    pngDataSize = (value != null) ? (UInt64)value.Length : 0;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SaveIconRequest"/> class.
            /// </summary>
            public SaveIconRequest()
                : base(FunctionTypes.SaveIcon)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                if (pngDataSize == 0)
                {
                    rawPNG = System.IO.File.ReadAllBytes(iconPath);
                    pngDataSize = (UInt64)rawPNG.Length;
                }

                PrxSaveDataSaveIcon(this, out result);

                EmptyResponse response = pendingRequest.response as EmptyResponse;

                if (response != null)
                {
                    response.Populate(result);
                }
            }
        }

        /// <summary>
        /// Load an icon from a mount point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class LoadIconRequest : RequestBase
        {
            internal MountPointName mountPointName;

            /// <summary>
            /// The mount point name to load the icon.
            /// </summary>
            public MountPointName MountPointName
            {
                get { return mountPointName; }
                set { ThrowExceptionIfLocked(); mountPointName = value; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LoadIconRequest"/> class.
            /// </summary>
            public LoadIconRequest()
                : base(FunctionTypes.LoadIcon)
            {

            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataLoadIcon(this, out data, out result);

                LoadIconResponse response = pendingRequest.response as LoadIconResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }

        #endregion

        #region Responses

        /// <summary>
        /// Response class that contains the mount result.
        /// </summary>
        public class MountResponse : ResponseBase
        {
            internal MountPoint mountPoint = new MountPoint();

            internal UInt64 requiredBlocks;
            internal bool wasCreated;

            /// <summary>
            /// The mount point name.
            /// </summary>
            public MountPoint MountPoint
            {
                get { ThrowExceptionIfLocked(); return mountPoint; }
            }

            /// <summary>
            /// Number of free file system blocks required for mounting.
            /// </summary>
            public UInt64 RequiredBlocks
            {
                get { ThrowExceptionIfLocked(); return requiredBlocks; }
            }

            /// <summary>
            /// True if the save data folder was created as part of the mount operation, false if the folder already existed before the operation.
            /// </summary>
            public bool WasCreated
            {
                get { ThrowExceptionIfLocked(); return wasCreated; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MountResponse"/> class.
            /// </summary>
            public MountResponse()
            {

            }

            internal void Populate(APIResult result, MemoryBufferNative data)
            {
                base.Populate(result);

                MemoryBuffer readBuffer = new MemoryBuffer(data);
                readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                mountPoint.name.Read(readBuffer);

                requiredBlocks = readBuffer.ReadUInt64();
                UInt32 mountStatus = readBuffer.ReadUInt32();

                wasCreated = false;

                if (mountStatus == 1)
                {
                    wasCreated = true;
                }

                readBuffer.CheckEndMarker();

                mountPoint.openTime = DateTime.UtcNow;
                mountPoint.isMounted = true;
            }
        }

        /// <summary>
        /// Response class that contains size info for a mountpoint.
        /// </summary>
        public class MountInfoResponse : ResponseBase
        {
            internal SaveDataInfo sdInfo;

            /// <summary>
            /// The size info.
            /// </summary>
            public SaveDataInfo Info
            {
                get { ThrowExceptionIfLocked(); return sdInfo; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MountInfoResponse"/> class.
            /// </summary>
            public MountInfoResponse()
            {

            }

            internal void Populate(APIResult result, MemoryBufferNative data)
            {
                base.Populate(result);

                sdInfo = new SaveDataInfo();

                MemoryBuffer readBuffer = new MemoryBuffer(data);
                readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                sdInfo.Read(readBuffer);

                readBuffer.CheckEndMarker();
            }
        }

        /// <summary>
        /// Response class that contains the parameters of the mount point.
        /// </summary>
        public class MountParamsResponse : ResponseBase
        {
            internal SaveDataParams sdParams;

            /// <summary>
            /// The mount point parameters.
            /// </summary>
            public SaveDataParams Params
            {
                get { ThrowExceptionIfLocked(); return sdParams; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MountParamsResponse"/> class.
            /// </summary>
            public MountParamsResponse()
            {

            }

            internal void Populate(APIResult result, MemoryBufferNative data)
            {
                base.Populate(result);

                sdParams = new SaveDataParams();

                MemoryBuffer readBuffer = new MemoryBuffer(data);
                readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                sdParams.Read(readBuffer);

                readBuffer.CheckEndMarker();
            }
        }

        /// <summary>
        /// Response class that contains the save data mount point icon.
        /// </summary>
        public class LoadIconResponse : ResponseBase
        {
            internal Icon icon = null;

            /// <summary>
            /// The save data icon retrieved, if there is one. This will be null if no icon is available.
            /// </summary>
            public Icon Icon { get { ThrowExceptionIfLocked(); return icon; } }

            /// <summary>
            /// Initializes a new instance of the <see cref="LoadIconResponse"/> class.
            /// </summary>
            public LoadIconResponse()
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
                    icon = Icon.ReadAndCreate(readBuffer);
                }
                else
                {
                    icon = null;
                }

                readBuffer.CheckEndMarker();
            }
        }

        #endregion

        #region Mount Point Tracking
        internal static List<MountPoint> activeMountPoints = new List<MountPoint>();

        internal static object activeMPSync = new object();

        internal static void AddMountPoint(MountPoint mountPoint)
        {
            lock (activeMPSync)
            {
                activeMountPoints.Add(mountPoint);
            }
        }

        internal static bool RemoveMountPoint(MountPoint mountPoint)
        {
            lock (activeMPSync)
            {
                for (int i = 0; i < activeMountPoints.Count; i++)
                {
                    if (activeMountPoints[i] == mountPoint)
                    {
                        activeMountPoints.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool RemoveMountPoint(MountPointName mountPointName)
        {
            lock (activeMPSync)
            {
                for (int i = 0; i < activeMountPoints.Count; i++)
                {
                    if (activeMountPoints[i].name.data == mountPointName.data)
                    {
                        activeMountPoints.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        internal static MountPoint FindMountPoint(MountPointName mountPointName)
        {
            lock (activeMPSync)
            {
                for (int i = 0; i < activeMountPoints.Count; i++)
                {
                    if (activeMountPoints[i].name.data == mountPointName.data)
                    {
                        return activeMountPoints[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a list of active mount points. This takes a snapshot copy of the current list, so you can safely iterate the list.
        /// </summary>
        public static List<MountPoint> ActiveMountPoints
        {
            get
            {
                lock (activeMPSync)
                {
                    List<MountPoint> copy = new List<MountPoint>(activeMountPoints);
                    return copy;
                }
            }
        }

        #endregion

        /// <summary>
        /// Mounts a save data folder. See Sony's documentation on sceSaveDataMount2 for the PS5 for more information.
        /// </summary>
        /// <param name="request">The save data mount settings.</param>
        /// <param name="response">The results of the mount operation.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int Mount(MountRequest request, MountResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            response.mountPoint.mountMode = request.mountMode;
            response.mountPoint.userId = request.userId;
            response.mountPoint.dirName = request.dirName;

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            int result = ProcessQueueThread.WaitIfSyncRequest(pe);

            //if (request.async == false && response.ReturnCode == ReturnCodes.SUCCESS)
            //{
            //    AddMountPoint(response.MountPoint);
            //}

            return result;
        }

       /// <summary>
        /// Mounts a PS4 save data folder. See Sony's documentation on sceSaveDataTransferringMountPs4 for the PS5 for more information.
        /// </summary>
        /// <param name="request">The save data mount settings.</param>
        /// <param name="response">The results of the mount operation.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int MountPS4(MountPS4Request request, MountResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            response.mountPoint.mountMode = MountModeFlags.ReadOnly;
            response.mountPoint.userId = request.userId;
            response.mountPoint.dirName = request.dirName;
            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            int result = ProcessQueueThread.WaitIfSyncRequest(pe);

            //if (request.async == false && response.ReturnCode == ReturnCodes.SUCCESS)
            //{
            //    AddMountPoint(response.MountPoint);
            //}

            return result;
        }

        /// <summary>
        /// Unmounts a save data directory.  See Sony's documentation on sceSaveDataUmount for the PS5 for more information.
        /// </summary>
        /// <param name="request">The save data mount point to unmount.</param>
        /// <param name="response">This response contains a return code and doesn't contain actual data.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int Unmount(UnmountRequest request, EmptyResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            MountPoint mp = FindMountPoint(request.mountPointName);

            if (mp == null)
            {
                throw new SaveDataException("The mount point name provided isn't a currently active mount point. " + request.mountPointName);
            }

            if (mp.isMounted == false)
            {
                throw new SaveDataException("The mount point name provided is already unmounted. " + request.mountPointName);
            }

            mp.isMounted = false;

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            int result = ProcessQueueThread.WaitIfSyncRequest(pe);

            if (request.async == false)
            {
                //if (response.ReturnCode == ReturnCodes.SUCCESS)
                //{
                //    RemoveMountPoint(mp);
                //}
                //else

                if (response.ReturnCode != ReturnCodes.SUCCESS)
                {
                    // Somehow the mount point didn't get unmounted. Re-enable the mount point as being mounted.
                    // TODO - Maybe the error code will show the save data is already been unmounted in some other unknown way.
                    mp.isMounted = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves size information for a save data folder. For more information, see Sony's documentation on sceSaveDataGetMountInfo for the PS5.
        /// </summary>
        /// <param name="request">The save data mount point.</param>
        /// <param name="response">The size info for the mount point.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int GetMountInfo(GetMountInfoRequest request, MountInfoResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }

        /// <summary>
        /// Gets the parameters of a save data mount point. For more information, see Sony's documentation on sceSaveDataGetParam for the PS5.
        /// </summary>
        /// <param name="request">The save data mount.</param>
        /// <param name="response">The parameters of the save data mount point.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int GetMountParams(GetMountParamsRequest request, MountParamsResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }

        /// <summary>
        /// Sets the mountpoint parameters. For more information, see Sony's documentation on sceSaveDataSetParam for the PS5.
        /// </summary>
        /// <param name="request">The save data mount.</param>
        /// <param name="response">This response contains a return code and doesn't contain actual data.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int SetMountParams(SetMountParamsRequest request, EmptyResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }

        /// <summary>
        /// Saves an icon to the save data mount point. For more information, see Sony's documentation on sceSaveDataSaveIcon for the PS5.
        /// </summary>
        /// <param name="request">The save data mount settings.</param>
        /// <param name="response">This response contains a return code and doesn't contain actual data.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int SaveIcon(SaveIconRequest request, EmptyResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            if (request.pngDataSize > 0)
            {
                if (request.pngDataSize > SaveIconRequest.ICON_FILE_MAXSIZE)
                {
                    throw new SaveDataException("The number of bytes in the PNG icon is " + request.pngDataSize + " which is greater than the maximum" + SaveIconRequest.ICON_FILE_MAXSIZE + " allowed.\nSee SaveIconRequest.ICON_FILE_MAXSIZE.");
                }

                Int32 width = 0;
                Int32 height = 0;

                PrxSaveDataGetIconSize(request.rawPNG, out width, out height);

                if ((width == SaveIconRequest.DATA_ICON_WIDTH_SMALL && height == SaveIconRequest.DATA_ICON_HEIGHT_SMALL) == false &&
                    (width == SaveIconRequest.DATA_ICON_WIDTH_FULL && height == SaveIconRequest.DATA_ICON_HEIGHT_FULL) == false)
                {
                    throw new SaveDataException("The PNG icon size is incorrect. The current size is " + width + " x " + height + ".\nThe size must be " +
                        SaveIconRequest.DATA_ICON_WIDTH_SMALL + " x " + SaveIconRequest.DATA_ICON_HEIGHT_SMALL + " or " + SaveIconRequest.DATA_ICON_WIDTH_FULL + " x " + SaveIconRequest.DATA_ICON_HEIGHT_FULL + ". See SaveIconRequest.DATA_ICON_WIDTH_SMALL and SaveIconRequest.DATA_ICON_HEIGHT_SMALL etc.");
                }
            }

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }

        /// <summary>
        /// Loads a savedata icon. For more information, see Sony's documentation on sceSaveDataLoadIcon for the PS5.
        /// </summary>
        /// <param name="request">The save data mount settings.</param>
        /// <param name="response">The response contains the PNG image data.</param>
        /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int LoadIcon(LoadIconRequest request, LoadIconResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }
    }
}
