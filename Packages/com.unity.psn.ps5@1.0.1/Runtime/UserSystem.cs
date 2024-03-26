

using System;
using System.Collections.Generic;
using System.IO;

using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Users
{
    /// <summary>
    /// User profile information, presence information, and friend lists
    /// </summary>
    public class UserSystem
    {
        enum NativeMethods : UInt32
        {
            AddUser = 0x0100001u,
            RemoveUser = 0x0100002u,
            GetFriends = 0x0100003u,
            GetProfiles = 0x0100004u,
            GetBasicPresences = 0x0100005u,
            GetBlockingUsers = 0x0100006u,

#if UNITY_PS5
            StartSignedStateCallback = 0x0100007u,
            StopSignedStateCallback = 0x0100008u,
            FetchSignedStateEvent = 0x0100009u,

            StartReachabilityStateCallback = 0x0100010u,
            StopReachabilityStateCallback = 0x0100011u,
            FetchReachabilityStateEvent = 0x0100012u,
#endif
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("User System");
            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            workerQueue.Stop();
            Main.OnSystemUpdate -= Update;
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal UserSystem queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

#if UNITY_PS5
        /// <summary>
        /// Delegate for notifications about signin events.
        /// </summary>
        /// <param name="signedInEvent">The signin event data containing the notification data</param>
        public delegate void SignedInStateNotification(SignedInEvent signedInEvent);

        /// <summary>
        /// Event called when a users signs into or out of PSN.
        /// To start notifications call <see cref="StartSignedStateCallbackRequest"/>.
        /// To stop notifications call <see cref="StopSignedStateCallbackRequest"/>.
        /// </summary>
        public static event SignedInStateNotification OnSignedInNotification;

        /// <summary>
        /// Sign-in state
        /// </summary>
        public enum SignedInStates
        {
            Unknown = 0,
            SignedOut = 1,
            SignedIn = 2
        }

        /// <summary>
        /// The premium notification data containing the type and user
        /// </summary>
        public class SignedInEvent
        {
            /// <summary> UserId for the event </summary>
            public Int32 UserId { get; set; }

            /// <summary> Premium feature event type </summary>
            public SignedInStates State { get; set; }
        }


        /// <summary>
        /// Delegate for notifications about reachability events.
        /// </summary>
        /// <param name="reachabilityEvent">The reachability event data containing the notification data</param>
        public delegate void ReachabilityNotification(ReachabilityEvent reachabilityEvent);

        /// <summary>
        /// Event called when a PSN reachability state changes.
        /// To start notifications call <see cref="Start"/>.
        /// To stop notifications call <see cref="StopSignedStateCallbackRequest"/>.
        /// </summary>
        public static event ReachabilityNotification OnReachabilityNotification;

        /// <summary>
        /// PSN Reachability states
        /// </summary>
        public enum ReachabilityStates
        {
            /// <summary> PSN features cannot be used or reachability state cannot be confirmed </summary>
            Unavailable = 0,
            /// <summary> PSN features can be used, but servers of PSN cannot be reached  </summary>
            Available = 1,
            /// <summary> Servers of PSN can be reached </summary>
            Reachable = 2
        }

        /// <summary>
        /// Callback that receives notifications for PlayStation™Network reachability state changes
        /// </summary>
        public class ReachabilityEvent
        {
            /// <summary> UserId for the event </summary>
            public Int32 UserId { get; set; }

            /// <summary> Premium feature event type </summary>
            public ReachabilityStates State { get; set; }
        }

#endif

        private static void Update()
        {
#if UNITY_PS5
            SignedInEvent sie = FetchNextSignInEvent();

            if (sie != null)
            {
                OnSignedInNotification(sie);
            }

            ReachabilityEvent re = FetchNextReachabilityEvent();

            if (re != null)
            {
                OnReachabilityNotification(re);
            }
#endif
        }

#if UNITY_PS5
        internal static SignedInEvent FetchNextSignInEvent()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchSignedStateEvent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            if (nativeMethod.ResultsSize == 0)
            {
                return null;
            }

            Int32 userId = nativeMethod.Reader.ReadInt32();
            Int32 state = nativeMethod.Reader.ReadInt32();

            SignedInEvent newEvent = new SignedInEvent();

            newEvent.UserId = userId;
            newEvent.State = (SignedInStates)state;

            return newEvent;
        }

        internal static ReachabilityEvent FetchNextReachabilityEvent()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchReachabilityStateEvent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            if (nativeMethod.ResultsSize == 0)
            {
                return null;
            }

            Int32 userId = nativeMethod.Reader.ReadInt32();
            Int32 state = nativeMethod.Reader.ReadInt32();

            ReachabilityEvent newEvent = new ReachabilityEvent();

            newEvent.UserId = userId;
            newEvent.State = (ReachabilityStates)state;

            return newEvent;
        }
#endif

        /// <summary>
        /// Add a user to the system. This should be called when a user logs onto the system
        /// </summary>
        /// <remarks>
        /// A user Id must first be registered with the system before it can be used elsewhere
        /// This will create all the necessary system contexts in the native PSN library
        /// </remarks>
        public class AddUserRequest : Request
        {
            /// <summary>
            /// User Id to register
            /// </summary>
            public Int32 UserId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.AddUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Remove a user from the system. This should be called when a user logs off.
        /// </summary>
        /// <remarks>
        /// This should be called once the user is no longer required and all of their internal
        /// data can be released
        /// </remarks>
        public class RemoveUserRequest : Request
        {
            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RemoveUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtains a friend list
        /// </summary>
        public class GetFriendsRequest : Request
        {
            /// <summary>
            /// Filter when obtaining a friend list.
            /// </summary>
            public enum Filters
            {
                /// <summary>Return all friends</summary>
                NotSet = 0,
                /// <summary>Only return friends currently online </summary>
                Online,
            };

            /// <summary>
            /// Sort order for friend list
            /// </summary>
            public enum Order
            {
                /// <summary>Arbitrary order</summary>
                NotSet = 0,
                /// <summary>Online IDs will be sorted in the dirctionary order (case-insensitive)</summary>
                OnlineId, // "onlineId"
                /// <summary>Friends sorted by online status first then offline status </summary>
                OnlineStatus, // "onlineStatus"
                /// <summary>Friends sorted by online status first and Online IDs. Combination of <see cref="OnlineId"/> + <see cref="OnlineStatus"/> </summary>
                OnlineStatus_OnlineId, // "onlineStatus+onlineId"
            };

            /// <summary>
            /// User Id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Relative position from start of list
            /// </summary>
            public UInt32 Offset { get; set; }

            /// <summary>
            /// Maximum number of items with which to respond at once.
            /// </summary>
            /// <remarks>
            /// Minimum value 0, Maximum value 2000, Default value 100.
            /// </remarks>
            public UInt32 Limit { get; set; } = 100u;

            /// <summary>
            /// The filter to use
            /// </summary>
            public Filters Filter { get; set; }

            /// <summary>
            /// The sort order
            /// </summary>
            public Order SortOrder { get; set; }

            /// <summary>
            /// The retireved list of friend account ids
            /// </summary>
            public List<UInt64> RetrievedAccountIds { get; internal set; } = new List<UInt64>();

            /// <summary>
            /// The next offset to use if more friends can be fetched
            /// </summary>
            public Int32 NextOffset { get; internal set; }

            /// <summary>
            /// The previous offset used in the request
            /// </summary>
            public Int32 PreviousOffset { get; internal set; }

            protected internal override void Run()
            {
                RetrievedAccountIds.Clear();

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetFriends);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(Offset);
                nativeMethod.Writer.Write(Limit);
                nativeMethod.Writer.Write((UInt32)Filter);
                nativeMethod.Writer.Write((UInt32)SortOrder);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    UInt32 count = nativeMethod.Reader.ReadUInt32();

                    NextOffset = nativeMethod.Reader.ReadInt32();
                    PreviousOffset = nativeMethod.Reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        UInt64 accountId = nativeMethod.Reader.ReadUInt64();
                        RetrievedAccountIds.Add(accountId);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// The user's profile picture
        /// </summary>
        public class ProfilePicture
        {
            /// <summary>
            /// Image size
            /// </summary>
            public enum Sizes
            {
                /// <summary>Not set</summary>
                NotSet = 0,
                /// <summary>Extra large</summary>
                ExtraLarge,
                /// <summary>Large</summary>
                Large,
                /// <summary>Medium</summary>
                Medium,
                /// <summary>Small</summary>
                Small,
            }

            /// <summary>
            /// Image size type
            /// </summary>
            public Sizes Size { get; internal set; }

            /// <summary>
            /// Url to fetch the image
            /// </summary>
            public string Url { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                Size = Sizes.NotSet;
                Url = "";

                if (reader.ReadBoolean() == true)
                {
                    Size = (Sizes)reader.ReadUInt32();
                }

                if (reader.ReadBoolean() == true)
                {
                    Url = reader.ReadPrxString();
                }
            }

        }

        /// <summary>
        /// Personal Details of user
        /// </summary>
        public class UserPersonalDetails
        {
            /// <summary>
            /// User's given name. This is blank if <see cref="DisplayName"/> is provided
            /// </summary>
            public string FirstName { get; internal set; }

            /// <summary>
            /// User's middle name. This is blank if <see cref="DisplayName"/> is provided
            /// </summary>
            public string MiddleName { get; internal set; }

            /// <summary>
            /// User's last name. This is blank if <see cref="DisplayName"/> is provided
            /// </summary>
            public string LastName { get; internal set; }

            /// <summary>
            /// User's public name.
            /// </summary>
            public string DisplayName { get; internal set; }

            /// <summary>
            /// List of user's profile pictures
            /// </summary>
            public List<ProfilePicture> ProfilePictures { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                if (reader.ReadBoolean() == true)
                {
                    FirstName = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true)
                {
                    MiddleName = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true)
                {
                    LastName = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true)
                {
                    DisplayName = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true)
                {
                    UInt32 numPictures = reader.ReadUInt32();

                    ProfilePictures = new List<ProfilePicture>((int)numPictures);

                    for (int i = 0; i < numPictures; i++)
                    {
                        ProfilePicture newPic = new ProfilePicture();

                        newPic.Deserialise(reader);

                        ProfilePictures.Add(newPic);
                    }
                }
            }
        }

        /// <summary>
        /// User's profile
        /// </summary>
        public class UserProfile
        {
            /// <summary>
            /// Is the user's profile verified
            /// </summary>
            public enum VerifiedStates
            {
                /// <summary>Not set</summary>
                NotSet = 0,
                /// <summary>Not verified</summary>
                No,
                /// <summary>Verified</summary>
                Yes,
            }

            /// <summary>
            /// Online Id
            /// </summary>
            public string OnlineId { get; internal set; }

            /// <summary>
            /// Personal Details
            /// </summary>
            public UserPersonalDetails PersonalDetails { get; internal set; }

            /// <summary>
            /// About me text
            /// </summary>
            public string AboutMe { get; internal set; }

            /// <summary>
            /// Avatars pictures
            /// </summary>
            public List<ProfilePicture> Avatars { get; internal set; }

            /// <summary>
            /// User's languages
            /// </summary>
            public List<string> Languages { get; internal set; }

            /// <summary>
            /// User verified account status
            /// </summary>
            public VerifiedStates VerifiedState { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                if (reader.ReadBoolean() == true)
                {
                    OnlineId = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true)
                {
                    PersonalDetails = new UserPersonalDetails();
                    PersonalDetails.Deserialise(reader);
                }

                if (reader.ReadBoolean() == true)
                {
                    AboutMe = reader.ReadPrxString();
                }

                if (reader.ReadBoolean() == true) // avatars
                {
                    UInt32 numAvatars = reader.ReadUInt32();

                    Avatars = new List<ProfilePicture>((int)numAvatars);

                    for (int i = 0; i < numAvatars; i++)
                    {
                        ProfilePicture newPic = new ProfilePicture();

                        newPic.Deserialise(reader);

                        Avatars.Add(newPic);
                    }
                }

                if (reader.ReadBoolean() == true) // languages
                {
                    UInt32 numLanguages = reader.ReadUInt32();

                    Languages = new List<string>((int)numLanguages);

                    for (int i = 0; i < numLanguages; i++)
                    {
                        string language = reader.ReadPrxString();
                        Languages.Add(language);
                    }
                }

                VerifiedState = VerifiedStates.NotSet;
                if (reader.ReadBoolean() == true) // verified
                {
                    bool isVerified = reader.ReadBoolean();
                    if (isVerified == true) VerifiedState = VerifiedStates.Yes;
                    else VerifiedState = VerifiedStates.No;
                }
            }
        }

        /// <summary>
        /// Request a list of user profiles
        /// </summary>
        public class GetProfilesRequest : Request
        {
            /// <summary>
            /// User Id making the request
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of account ids to request
            /// </summary>
            public List<UInt64> AccountIds { get; set; }

            /// <summary>
            /// List of returned user profiles
            /// </summary>
            public List<UserProfile> RetrievedProfiles { get; set; }

            protected internal override void Run()
            {
                if (RetrievedProfiles == null)
                {
                    return;
                }

                RetrievedProfiles.Clear();

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetProfiles);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                if (AccountIds == null)
                {
                    nativeMethod.Writer.Write((UInt32)0);
                }
                else
                {
                    nativeMethod.Writer.Write((UInt32)AccountIds.Count);

                    for (int i = 0; i < AccountIds.Count; i++)
                    {
                        nativeMethod.Writer.Write((UInt64)AccountIds[i]);
                    }
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    UInt32 numProfiles = nativeMethod.Reader.ReadUInt32();

                    for (int i = 0; i < numProfiles; i++)
                    {
                        UserProfile profile = new UserProfile();
                        profile.Deserialise(nativeMethod.Reader);

                        RetrievedProfiles.Add(profile);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Basic user presense
        /// </summary>
        public class BasicPresence
        {
            /// <summary>
            /// Online status of a user
            /// </summary>
            public enum OnlineStates
            {
                /// <summary>Not set</summary>
                NotSet = 0,
                /// <summary>User online</summary>
                Online,
                /// <summary>User offline</summary>
                Offline,
            }

            /// <summary>
            /// Account id of user's PSN account
            /// </summary>
            public UInt64 AccountId { get; internal set; }

            /// <summary>
            /// Online status
            /// </summary>
            public OnlineStates OnlineStatus { get; internal set; }

            /// <summary>
            /// Whether the current user and the target user are playing a game with the same NP communication ID
            /// </summary>
            public bool InContext { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                if (reader.ReadBoolean() == true)
                {
                    AccountId = reader.ReadUInt64();
                }

                if (reader.ReadBoolean() == true)
                {
                    OnlineStatus = (OnlineStates)reader.ReadInt32();
                }

                if (reader.ReadBoolean() == true)
                {
                    InContext = reader.ReadBoolean();
                }
            }
        }

        /// <summary>
        /// Request a list of user presences
        /// </summary>
        public class GetBasicPresencesRequest : Request
        {
            /// <summary>
            /// User Id making the request
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of account ids to fetch
            /// </summary>
            public List<UInt64> AccountIds { get; set; }

            /// <summary>
            /// List of retrieved account presences
            /// </summary>
            public List<BasicPresence> RetrievedPresences { get; set; }

            protected internal override void Run()
            {
                if (RetrievedPresences == null)
                {
                    return;
                }

                if (AccountIds == null)
                {
                    return;
                }

                RetrievedPresences.Clear();

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetBasicPresences);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write((UInt32)AccountIds.Count);

                for (int i = 0; i < AccountIds.Count; i++)
                {
                    nativeMethod.Writer.Write((UInt64)AccountIds[i]);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    UInt32 numPresences = nativeMethod.Reader.ReadUInt32();

                    for (int i = 0; i < numPresences; i++)
                    {
                        BasicPresence presence = new BasicPresence();
                        presence.Deserialise(nativeMethod.Reader);

                        RetrievedPresences.Add(presence);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Requesta list of blocked users
        /// </summary>
        public class GetBlockingUsersRequest : Request
        {
            /// <summary>
            /// User Id making the request
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Start offset of the list
            /// </summary>
            public UInt32 Offset { get; set; }

            /// <summary>
            /// Maximum number of accounts to return in a single request
            /// </summary>
            public UInt32 Limit { get; set; }

            /// <summary>
            /// List of retrieved account id blocked by the user
            /// </summary>
            public List<UInt64> RetrievedAccountIds { get; set; }

            /// <summary>
            /// Next offset
            /// </summary>
            public Int32 NextOffset { get; private set; }

            /// <summary>
            /// Previous Offset
            /// </summary>
            public Int32 PreviousOffset { get; private set; }

            /// <summary>
            /// Number of all entries included in the block list
            /// </summary>
            public Int32 TotalItemCount { get; private set; }

            protected internal override void Run()
            {
                if (RetrievedAccountIds == null)
                {
                    return;
                }

                RetrievedAccountIds.Clear();

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetBlockingUsers);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(Offset);
                nativeMethod.Writer.Write(Limit);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    UInt32 count = nativeMethod.Reader.ReadUInt32();

                    for (int i = 0; i < count; i++)
                    {
                        UInt64 accountId = nativeMethod.Reader.ReadUInt64();
                        RetrievedAccountIds.Add(accountId);
                    }

                    NextOffset = nativeMethod.Reader.ReadInt32();
                    PreviousOffset = nativeMethod.Reader.ReadInt32();
                    TotalItemCount = nativeMethod.Reader.ReadInt32();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


#if UNITY_PS5

        /// <summary>
        /// Start notifying the sign-in state. <see cref="OnSignedInNotification"/>
        /// </summary>
        public class StartSignedStateCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartSignedStateCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Stop notifying the sign-in state
        /// </summary>
        public class StopSignedStateCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StopSignedStateCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Start notifying the PSN reachability state. <see cref="OnReachabilityNotification"/>
        /// </summary>
        public class StartReachabilityStateCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartReachabilityStateCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Stop notifying the PSN reachability state
        /// </summary>
        public class StopReachabilityStateCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StopReachabilityStateCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
#endif
    }
}
