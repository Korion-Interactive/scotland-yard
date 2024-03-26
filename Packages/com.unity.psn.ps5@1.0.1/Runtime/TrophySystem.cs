using System;
using System.IO;

using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using UnityEngine;

#if UNITY_PS5
namespace Unity.PSN.PS5.Trophies
{
    /// <summary>
    /// The TrophySystem APIs are used to obtain information, such as trophy configuration data and trophy records, from the trophy system.
    /// Trophy unlocking is done using the Universal Data System. <see cref="UDS.UniversalDataSystem.UnlockTrophyRequest"/>
    /// </summary>
    public class TrophySystem
    {
        enum NativeMethods : UInt32
        {
            StartSystem = 0x0300001u,
            StopSystem = 0x0300002u,
            GetGameInfo = 0x0300003u,
            GetGroupInfo = 0x0300004u,
            GetTrophyInfo = 0x0300005u,
            GetGameIcon = 0x0300006u,
            GeGroupIcon = 0x0300007u,
            GetTrophyIcon = 0x0300008u,
            GetRewardIcon = 0x0300009u,
            ShowTrophyList = 0x030000Au,
            FetchUnlockEvent = 0x030000Bu,
        }

        static WorkerThread workerQueue = new WorkerThread();

        /// <summary>
        /// Delegate for notifications about trophy unlocks.
        /// </summary>
        /// <param name="reqEvent">The event data</param>
        public delegate void UnlockNotification(Int32 trophyId);

        /// <summary>
        /// Event called when a trophy is unlocked
        /// </summary>
        public static event UnlockNotification OnUnlockNotification;

        internal static void Start()
        {
            workerQueue.Start("Trophy System");
            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            workerQueue.Stop();
            Main.OnSystemUpdate -= Update;
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Trophy queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Return the next trophy id unlocked
        /// </summary>
        /// <returns></returns>
        internal static Int32 FetchNextUnlockNotification()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchUnlockEvent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            Int32 trophyId = -1;

            if (nativeMethod.ResultsSize > 0)
            {
                trophyId = nativeMethod.Reader.ReadInt32();
            }

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            return trophyId;
        }

        private static void Update()
        {
            Int32 trophyId = FetchNextUnlockNotification();

            if (trophyId >= 0)
            {
                if (OnUnlockNotification != null)
                {
                    OnUnlockNotification(trophyId);
                }
            }
        }

        /// <summary>
        /// Has the Trophy system been initialised. <see cref="StartSystemRequest"/> and <see cref="StopSystemRequest"/>
        /// </summary>
        public static bool IsInitialized { get; internal set; } = false;

        /// <summary>
        /// Start the trophy system. To test is the system is initialized <see cref="IsInitialized"/>
        /// </summary>
        public class StartSystemRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartSystem);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Stop the trophy system. To test is the system is initialized <see cref="IsInitialized"/>
        /// </summary>
        public class StopSystemRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StopSystem);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);

                IsInitialized = false;
            }
        }

        /// <summary>
        /// Trophy configuration data of a trophy set
        /// </summary>
        public class TrophyGameDetails
        {
            /// <summary>
            /// Defined total number of trophy groups
            /// </summary>
            public UInt32 NumGroups { get; internal set; }

            /// <summary>
            /// Defined total number of trophies
            /// </summary>
            public UInt32 NumTrophies { get; internal set; }

            /// <summary>
            /// Defined total number of platinum trophies
            /// </summary>
            public UInt32 NumPlatinum { get; internal set; }

            /// <summary>
            /// Defined total number of gold trophies
            /// </summary>
            public UInt32 NumGold { get; internal set; }

            /// <summary>
            /// Defined total number of silver trophies
            /// </summary>
            public UInt32 NumSilver { get; internal set; }

            /// <summary>
            /// Defined total number of bronze trophies
            /// </summary>
            public UInt32 NumBronze { get; internal set; }

            /// <summary>
            /// Name of the trophy set
            /// </summary>
            public string Title { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                NumGroups = reader.ReadUInt32();
                NumTrophies = reader.ReadUInt32();
                NumPlatinum = reader.ReadUInt32();
                NumGold = reader.ReadUInt32();
                NumSilver = reader.ReadUInt32();
                NumBronze = reader.ReadUInt32();

                Title = reader.ReadPrxString();
            }
        }

        /// <summary>
        /// Trophy record of a trophy set
        /// </summary>
        public class TrophyGameData
        {
            /// <summary>
            /// Number of unlocked trophies
            /// </summary>
            public UInt32 UnlockedTrophies { get; internal set; }

            /// <summary>
            /// Number of unlocked platinum trophies
            /// </summary>
            public UInt32 UnlockedPlatinum { get; internal set; }

            /// <summary>
            /// Number of unlocked gold trophies
            /// </summary>
            public UInt32 UnlockedGold { get; internal set; }

            /// <summary>
            /// Number of unlocked silver trophies
            /// </summary>
            public UInt32 UnlockedSilver { get; internal set; }

            /// <summary>
            /// Number of unlocked bronze trophies
            /// </summary>
            public UInt32 UnlockedBronze { get; internal set; }

            /// <summary>
            /// Degree of progress toward unlocking the trophy (%)
            /// </summary>
            public UInt32 ProgressPercentage { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                UnlockedTrophies = reader.ReadUInt32();
                UnlockedPlatinum = reader.ReadUInt32();
                UnlockedGold = reader.ReadUInt32();
                UnlockedSilver = reader.ReadUInt32();
                UnlockedBronze = reader.ReadUInt32();
                ProgressPercentage = reader.ReadUInt32();
            }
        }

        /// <summary>
        /// Get trophy set information
        /// </summary>
        public class GetGameInfoRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Obtained trophy configuration
            /// </summary>
            public TrophyGameDetails GameDetails { get; set; }

            /// <summary>
            /// Obtained trophy records
            /// </summary>
            public TrophyGameData GameData { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetGameInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                GameDetails.Deserialise(nativeMethod.Reader);
                GameData.Deserialise(nativeMethod.Reader);

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);

                //System.Threading.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Trophy configuration data of a trophy group
        /// </summary>
        public class TrophyGroupDetails
        {
            /// <summary>
            /// Trophy group ID
            /// </summary>
            public Int32 GroupId { get; internal set; }

            /// <summary>
            /// Defined total number of trophies
            /// </summary>
            public UInt32 NumTrophies { get; internal set; }

            /// <summary>
            /// Defined total number of platinum trophies
            /// </summary>
            public UInt32 NumPlatinum { get; internal set; }

            /// <summary>
            /// Defined total number of gold trophies
            /// </summary>
            public UInt32 NumGold { get; internal set; }

            /// <summary>
            /// Defined total number of silver trophies
            /// </summary>
            public UInt32 NumSilver { get; internal set; }

            /// <summary>
            /// Defined total number of bronze trophies
            /// </summary>
            public UInt32 NumBronze { get; internal set; }

            /// <summary>
            /// Name of the trophy group
            /// </summary>
            public string Title { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                GroupId = reader.ReadInt32();
                NumTrophies = reader.ReadUInt32();
                NumPlatinum = reader.ReadUInt32();
                NumGold = reader.ReadUInt32();
                NumSilver = reader.ReadUInt32();
                NumBronze = reader.ReadUInt32();

                Title = reader.ReadPrxString();
            }
        }

        /// <summary>
        /// Trophy record of a trophy group
        /// </summary>
        public class TrophyGroupData
        {
            /// <summary>
            /// rophy group ID
            /// </summary>
            public Int32 GroupId { get; internal set; }

            /// <summary>
            /// Number of unlocked trophies
            /// </summary>
            public UInt32 UnlockedTrophies { get; internal set; }

            /// <summary>
            /// Number of unlocked platinum trophies
            /// </summary>
            public UInt32 UnlockedPlatinum { get; internal set; }

            /// <summary>
            /// Number of unlocked gold trophies
            /// </summary>
            public UInt32 UnlockedGold { get; internal set; }

            /// <summary>
            /// Number of unlocked silver trophies
            /// </summary>
            public UInt32 UnlockedSilver { get; internal set; }

            /// <summary>
            /// Number of unlocked bronze trophies
            /// </summary>
            public UInt32 UnlockedBronze { get; internal set; }

            /// <summary>
            /// Degree of progress toward unlocking the trophy (%)
            /// </summary>
            public UInt32 ProgressPercentage { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                GroupId = reader.ReadInt32();
                UnlockedTrophies = reader.ReadUInt32();
                UnlockedPlatinum = reader.ReadUInt32();
                UnlockedGold = reader.ReadUInt32();
                UnlockedSilver = reader.ReadUInt32();
                UnlockedBronze = reader.ReadUInt32();
                ProgressPercentage = reader.ReadUInt32();
            }
        }

        /// <summary>
        /// Get trophy group information
        /// </summary>
        public class GetGroupInfoRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Group Id
            /// </summary>
            public Int32 GroupId { get; set; }

            /// <summary>
            /// Obtained trophy configuration
            /// </summary>
            public TrophyGroupDetails GroupDetails { get; set; }

            /// <summary>
            /// Obtained trophy records
            /// </summary>
            public TrophyGroupData GroupData { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetGroupInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(GroupId);

                nativeMethod.Call();

                GroupDetails.Deserialise(nativeMethod.Reader);
                GroupData.Deserialise(nativeMethod.Reader);

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Trophy configuration data of a trophy
        /// </summary>
        public class TrophyDetails
        {
            /// <summary>
            /// Trophy ID
            /// </summary>
            public Int32 TrophyId { get; internal set; }

            /// <summary>
            /// Grade of the trophy. unknown (0), platinum (1), gold (2), silver (3), bronze (4)
            /// </summary>
            public Int32 TrophyGrade { get; internal set; }

            /// <summary>
            /// Trophy group ID to which this trophy belongs
            /// </summary>
            public Int32 GroupId { get; internal set; }

            /// <summary>
            /// Is trophy hidden
            /// </summary>
            public bool Hidden { get; internal set; }

            /// <summary>
            /// Whether reward data is included. If true <see cref="Reward"/>
            /// </summary>
            public bool HasReward { get; internal set; }

            /// <summary>
            /// Name of the trophy
            /// </summary>
            public string Title { get; internal set; }

            /// <summary>
            /// Description of the trophy
            /// </summary>
            public string Description { get; internal set; }

            /// <summary>
            /// Reward data. Only contains data if <see cref="HasReward"/> is true
            /// </summary>
            public string Reward { get; internal set; }

            /// <summary>
            /// Is the trophy a progress type
            /// </summary>
            public bool IsProgress { get; internal set; }

            /// <summary>
            /// Target value (progress value at which the trophy will be unlocked) for progressive trophies
            /// </summary>
            public Int64 TargetValue { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                TrophyId = reader.ReadInt32();
                TrophyGrade = reader.ReadInt32();
                GroupId = reader.ReadInt32();

                Hidden = reader.ReadBoolean();
                HasReward = reader.ReadBoolean();

                IsProgress = reader.ReadBoolean();

                if (IsProgress)
                {
                    TargetValue = reader.ReadInt64();
                }
                else TargetValue = 0;

                Title = reader.ReadPrxString();
                Description = reader.ReadPrxString();
                Reward = reader.ReadPrxString();
            }
        }

        /// <summary>
        /// Trophy record of a trophy
        /// </summary>
        public class TrophyData
        {
            /// <summary>
            /// Trophy ID
            /// </summary>
            public Int32 TrophyId { get; internal set; }

            /// <summary>
            /// Whether or not the trophy is unlocked
            /// </summary>
            public bool Unlocked { get; internal set; }

            /// <summary>
            /// Is the trophy a progress type
            /// </summary>
            public bool IsProgress { get; internal set; }

            /// <summary>
            /// The current progress value for progressive trophies
            /// </summary>
            public Int64 ProgressValue { get; internal set; }

            /// <summary>
            /// The time stamp of when the trophy was first unlocked, or 0 if the trophy has not been unlocked
            /// </summary>
            public DateTime TimeStamp { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                TrophyId = reader.ReadInt32();
                Unlocked = reader.ReadBoolean();

                IsProgress = reader.ReadBoolean();

                if (IsProgress)
                {
                    ProgressValue = reader.ReadInt64();
                }
                else ProgressValue = 0;

                TimeStamp = reader.ReadRtcTicks();
            }
        }

        /// <summary>
        /// Get trophy information
        /// </summary>
        public class GetTrophyInfoRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy ID
            /// </summary>
            public Int32 TrophyId { get; set; }

            /// <summary>
            /// Obtained trophy configuration data
            /// </summary>
            public TrophyDetails TrophyDetails { get; set; }

            /// <summary>
            /// Obtained trophy records
            /// </summary>
            public TrophyData TrophyData { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetTrophyInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);

                nativeMethod.Call();

                TrophyDetails.Deserialise(nativeMethod.Reader);
                TrophyData.Deserialise(nativeMethod.Reader);

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get trophy set still-image icon
        /// </summary>
        public class GetTrophyGameIconRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Obtained icon data
            /// </summary>
            public Icon Icon { get; private set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetGameIcon);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Icon = new Icon();

                Icon.RawImage = nativeMethod.Reader.ReadData();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get trophy group still-image icon
        /// </summary>
        public class GetTrophyGroupIconRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Group Id
            /// </summary>
            public Int32 GroupId { get; set; }

            /// <summary>
            /// Obtained icon data
            /// </summary>
            public Icon Icon { get; private set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GeGroupIcon);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(GroupId);

                nativeMethod.Call();

                Icon = new Icon();

                Icon.RawImage = nativeMethod.Reader.ReadData();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get trophy still-image icon
        /// </summary>
        public class GetTrophyIconRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy Id
            /// </summary>
            public Int32 TrophyId { get; set; }

            /// <summary>
            /// Obtained icon data
            /// </summary>
            public Icon Icon { get; private set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetTrophyIcon);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);

                nativeMethod.Call();

                Icon = new Icon();

                Icon.RawImage = nativeMethod.Reader.ReadData();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtain a reward icon
        /// </summary>
        public class GetTrophyRewardIconRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Trophy Id
            /// </summary>
            public Int32 TrophyId { get; set; }

            /// <summary>
            /// Obtained icon data
            /// </summary>
            public Icon Icon { get; private set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetRewardIcon);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(TrophyId);

                nativeMethod.Call();

                Icon = new Icon();

                Icon.RawImage = nativeMethod.Reader.ReadData();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Display the trophy list screen
        /// </summary>
        public class ShowTrophyListRequest : Request
        {
            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.ShowTrophyList);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Contains raw PNG icon data
        /// </summary>
        public class Icon
        {
            byte[] internalData = null;
            Texture2D internalIcon = null;
            bool imageCreated = false;

            /// <summary>
            /// Has a <see cref="Texture2D"/> already been created for this icon
            /// </summary>
            public bool HasTexture2D { get { return internalIcon != null; } }

            /// <summary>
            /// Return or create <see cref="Texture2D"/> from PNG data.
            /// </summary>
            /// <remarks>
            /// This will create a new <see cref="Texture2D"/> is one isn't already created.
            /// If a texture hasn't been created then this should only be called from the main Unity thread.
            /// Use <see cref="HasTexture2D"/> to test if a texture has already been created
            /// </remarks>
            public Texture2D Image
            {
                get
                {
                    if (imageCreated == false && RawImage != null)
                    {
                        if (internalIcon == null)
                        {
                            internalIcon = new Texture2D(2, 2);
                        }

                        internalIcon.LoadImage(RawImage);
                        imageCreated = true;
                    }

                    return internalIcon;
                }
            }

            /// <summary>
            /// Return the raw PNG bytes for the icon
            /// </summary>
            public byte[] RawImage
            {
                get { return internalData; }
                internal set
                {
                    internalData = value;
                    imageCreated = false;
                }
            }
        }
    }
}
#endif
