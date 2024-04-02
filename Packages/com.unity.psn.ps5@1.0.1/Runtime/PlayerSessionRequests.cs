
using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// The parameters used when creating a player session
    /// </summary>
    public class PlayerSessionCreationParams
    {
        /// <summary>
        /// Maximum number of supported players. Min 1, Max 100
        /// </summary>
        public UInt32 MaxPlayers { get; set; } = 1; // Min 1, Max 100

        /// <summary>
        /// Maximum number of supported spectators. Min 0, Max 50
        /// </summary>
        public UInt32 MaxSpectators { get; set; } = 0; // Min , Max 50

        /// <summary>
        /// Flag indicating whether members who have joined can switch from players to spectators, or from spectators to players, without leaving.
        /// When true, swapping is allowed. (Users can swap using the system software UI.)When false, swapping is not allowed
        /// </summary>
        public bool SwapSupported { get; set; } = false;

        /// <summary>
        /// Flag for temporarily prohibiting joining a Player Session.
        /// When true, the Player Session cannot be joined. When false, users who have received invitation notifications or users who can join without such notifications because of the value set for <see cref="JoinableUserType"/> can join the Player Session.
        /// </summary>
        public bool JoinDisabled { get; set; } = false;

        /// <summary>
        /// Users who can join Player Sessions without invitations
        /// </summary>
        /// <remarks>
        /// <see cref="JoinableUserTypes.NoOne"/> : No user can join without an invitation
        /// <see cref="JoinableUserTypes.Friends"/> : Friends of the leader of the Player Session can join
        /// <see cref="JoinableUserTypes.FriendsOfFriends"/> : Friends of friends of the leader of the Player Session can join
        /// <see cref="JoinableUserTypes.Anyone"/> : Anyone can join
        /// <see cref="JoinableUserTypes.SpecifiedUsers"/> : Users who have been registered on the joinableSpecifiedUsers list can join without invitations
        /// </remarks>
        public JoinableUserTypes JoinableUserType { get; set; } = JoinableUserTypes.NoOne;

        /// <summary>
        /// Information about who can send invitations to a Player Session
        /// </summary>
        /// <remarks>
        /// <see cref="InvitableUserTypes.NoOne"/> : No user can send an invite
        /// <see cref="InvitableUserTypes.Leader"/> : Only the leader can send invites
        /// <see cref="InvitableUserTypes.Member"/> : Any member can send invites
        /// </remarks>
        public InvitableUserTypes InvitableUserType { get; set; } = InvitableUserTypes.NoOne;

        /// <summary>
        /// Information about the platforms with which users can join a Player Session
        /// </summary>
        public SessionPlatforms SupportedPlatforms { get; set; } = SessionPlatforms.PS5;

        /// <summary>
        /// Localisaed session names
        /// </summary>
        public LocalisedSessionNames LocalisedNames { get; set; } = new LocalisedSessionNames();

        /// <summary>
        /// The set of filters to use for the initial user who creates the session. Default to standard set of WebApi notification types. <see cref="PlayerSessionFilters"/>
        /// </summary>
        public WebApiFilters SessionFilters { get; set; } = PlayerSessionFilters.DefaultFilters;

        /// <summary>
        /// If a specification is made with this parameter, the leader will have the specified privileges. If nothing is specified,
        /// the leader will have <see cref="LeaderPrivilegeFlags.Kick"/> and <see cref="LeaderPrivilegeFlags.UpdateJoinableUserType"/> privileges.
        /// However, it is recommended that <see cref="ExclusiveLeaderPrivileges"/>, rather than this parameter, be used to specify the leader's privileges.
        /// <see cref="LeaderPrivilegeFlags.UpdateInvitableUerType"/> flag will be ingorned for these privileges.
        /// </summary>
        public LeaderPrivilegeFlags LeaderPrivileges { get; set; } = LeaderPrivilegeFlags.NotSet;

        /// <summary>
        /// Information about the items to exclude from the leader privileges of a Player Session
        /// If a specification is made with this parameter, the leader will not have the specified privileges.If nothing is specified, the leader will have all leader privileges
        /// All <see cref="LeaderPrivilegeFlags"/> are valid here.
        /// </summary>
        public LeaderPrivilegeFlags ExclusiveLeaderPrivileges { get; set; } = LeaderPrivilegeFlags.NotSet;

        /// <summary>
        /// Information about items to exclude from the System UI for the leader of a Player Session
        /// If a specification is made with this parameter, the leader will not be able to use the System UI for the specified privileges.
        /// If nothing is specified, the leader will have all leader privileges
        /// </summary>
        public LeaderPrivilegeFlags DisableSystemUiMenu { get; set; } = LeaderPrivilegeFlags.NotSet;

        /// <summary>
        /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
        /// </summary>
        public byte[] CustomData1 { get; set; }

        /// <summary>
        /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
        /// </summary>
        public byte[] CustomData2 { get; set; }

        /// <summary>
        /// Callbacks triggered when sWebApi notifications are recieved and processed by the session
        /// </summary>
        public PlayerSessionCallbacks Callbacks { get; set; } = new PlayerSessionCallbacks();

        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(MaxPlayers);
            writer.Write(MaxSpectators);
            writer.Write(SwapSupported);
            writer.Write(JoinDisabled);
            writer.Write((UInt32)JoinableUserType);
            writer.Write((UInt32)InvitableUserType);
            writer.Write((UInt32)SupportedPlatforms);

            LocalisedNames.Serialise(writer);

            PlayerSession.SerialisePrivliges(writer, LeaderPrivileges, false);
            PlayerSession.SerialisePrivliges(writer, ExclusiveLeaderPrivileges, true);
            PlayerSession.SerialisePrivliges(writer, DisableSystemUiMenu, true);

            int customData1Size = CustomData1 != null ? CustomData1.Length : 0;
            writer.Write(customData1Size);
            if (customData1Size > 0)
            {
                writer.Write(CustomData1);
            }

            int customData2Size = CustomData2 != null ? CustomData2.Length : 0;
            writer.Write(customData2Size);
            if (customData2Size > 0)
            {
                writer.Write(CustomData2);
            }
        }
    }

    /// <summary>
    /// Requests used for player sessions
    /// </summary>
    public class PlayerSessionRequests
    {
        internal enum NativeMethods : UInt32
        {
            CreatePlayerSession = 0x0A00001u,
            LeavePlayerSession = 0x0A00002u,
            JoinPlayerSession = 0x0A00003u,
            GetPlayerSessions = 0x0A00004u,
            SendPlayerSessionsInvitation = 0x0A00005u,
            GetPlayerSessionInvitations = 0x0A00006u,
            SetPlayerSessionProperties = 0x0A00007u,
            ChangePlayerSessionLeader = 0x0A00008u,
            AddPlayerSessionJoinableSpecifiedUsers = 0x0A00009u,
            DeletePlayerSessionJoinableSpecifiedUsers = 0x0A0000Au,
            SetPlayerSessionMemberSystemProperties = 0x0A0000Bu,
            SendPlayerSessionMessage = 0x0A0000Cu,
            GetJoinedPlayerSessionsByUser = 0x0A0000Du,
        }

        /// <summary>
        /// Create a new player session
        /// </summary>
        public class CreatePlayerSessionRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Custom binary member data for game session creator
            /// </summary>
            public byte[] CreatorsCustomData1 { get; set; }

            /// <summary>
            /// The parameters used to create the session
            /// </summary>
            public PlayerSessionCreationParams CreationParams { get; set; }

            /// <summary>
            /// The created player session instance
            /// </summary>
            public PlayerSession Session { get; internal set; }

            protected internal override void Run()
            {
                WebApiPushEvent sessionPushEvent = null;

                Result = WebApiNotifications.CreatePushEventBlocking(UserId, CreationParams.SessionFilters, PlayerSession.SessionWebApiEventHandler, true, out sessionPushEvent);

                if (sessionPushEvent == null)
                {
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)PlayerSessionRequests.NativeMethods.CreatePlayerSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(sessionPushEvent.PushCallbackId);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                CreationParams.Serialise(nativeMethod.Writer);

                Int32 dataSize = CreatorsCustomData1 != null ? CreatorsCustomData1.Length : 0;
                nativeMethod.Writer.Write(dataSize);
                if (dataSize > 0)
                {
                    nativeMethod.Writer.Write(CreatorsCustomData1);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    string sessionId = nativeMethod.Reader.ReadPrxString();

                    bool isNewSession = false;
                    Session = SessionsManager.CreatePlayerSession(sessionId, out isNewSession);

                    Session.InitialiseCreationParams(CreationParams);
                    Session.Initialise(CreationParams.Callbacks, sessionPushEvent);

                    // Deserialise the players
                    int numPlayers = nativeMethod.Reader.ReadInt32();

                    for (int i = 0; i < numPlayers; i++)
                    {
                        SessionMember newMember = new SessionMember();
                        newMember.DeserialiseBasicInfo(nativeMethod.Reader, false);

                        Session.AddorUpdateMember(newMember);
                    }

                    UInt64 creatorAccountId = nativeMethod.Reader.ReadUInt64();

                    Session.UpdateUserId(SessionMemberIdentifier.WithCurrentPlatform(creatorAccountId), UserId);
                }
                else
                {
                    // If session failed unregister the user push event
                    WebApiNotifications.UnregisterPushEventCall(sessionPushEvent);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get a local user to leave the player session
        /// </summary>
        public class LeavePlayerSessionRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session to leave
            /// </summary>
            public string SessionId { get; set; }

            protected internal override void Run()
            {
               // Result = Session.LeavePlayerSessionBlocking(UserId);

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)PlayerSessionRequests.NativeMethods.LeavePlayerSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    UInt64 accountId = nativeMethod.Reader.ReadUInt64();

                    PlayerSession session = SessionsManager.FindPlayerSessionFromSessionId(SessionId);

                    if (session != null)
                    {
                        session.RemoveMember(SessionMemberIdentifier.WithCurrentPlatform(accountId));

                        session.DeletePushEvent(UserId);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get a user to join the player session
        /// </summary>
        public class JoinPlayerSessionRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session ID to join
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Join the player session as a spectator
            /// </summary>
            public bool JoinAsSpectator { get; set; } = false;

            /// <summary>
            /// The joined session
            /// </summary>
            public PlayerSession Session { get; internal set; }

            /// <summary>
            /// The session filters to use when creating the users WebApi push events
            /// </summary>
            public WebApiFilters SessionFilters { get; set; } = PlayerSessionFilters.DefaultFilters;

            /// <summary>
            /// The callback used when session is updated
            /// </summary>
            public PlayerSessionCallbacks Callbacks { get; set; } = new PlayerSessionCallbacks();

            protected internal override void Run()
            {
                WebApiPushEvent sessionPushEvent = null;

                Result = WebApiNotifications.CreatePushEventBlocking(UserId, SessionFilters, PlayerSession.SessionWebApiEventHandler, true, out sessionPushEvent);

                if (sessionPushEvent == null)
                {
                    return;
                }

                // Find a local session. If one already exists then it means the player is join a local session
                // Must also handle if the session id doesn't exist because the the user might be joining a player session just using the session id.
                bool isNewSession = false;
                Session = SessionsManager.CreatePlayerSession(SessionId, out isNewSession);

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)PlayerSessionRequests.NativeMethods.JoinPlayerSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(sessionPushEvent.PushCallbackId);
                nativeMethod.Writer.Write(JoinAsSpectator);
                nativeMethod.Writer.Write(false);
                nativeMethod.Writer.WritePrxString(SessionId);

                // Debug.LogError("nativeMethod Call");
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Session.Initialise(Callbacks, sessionPushEvent);

                    // Deserialise the players
                    int numPlayers = nativeMethod.Reader.ReadInt32();

                    //    Debug.LogError("numPlayers " + numPlayers);
                    for (int i = 0; i < numPlayers; i++)
                    {
                        SessionMember newMember = new SessionMember();
                        newMember.DeserialiseBasicInfo(nativeMethod.Reader, JoinAsSpectator);

                        Session.AddorUpdateMember(newMember);
                    }

                    UInt64 joinerAccountId = nativeMethod.Reader.ReadUInt64();

                    Session.UpdateUserId(SessionMemberIdentifier.WithCurrentPlatform(joinerAccountId), UserId);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Swap a user to a player or spectator
        /// </summary>
        public class SwapPlayerSessionMemberRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session ID to join
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Swap the member to a spectator
            /// </summary>
            public bool JoinAsSpectator { get; set; } = false;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)PlayerSessionRequests.NativeMethods.JoinPlayerSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write((int)0);
                nativeMethod.Writer.Write(JoinAsSpectator);
                nativeMethod.Writer.Write(true);
                nativeMethod.Writer.WritePrxString(SessionId);

                // Debug.LogError("nativeMethod Call");
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    PlayerSession session = SessionsManager.FindPlayerSessionFromSessionId(SessionId);

                    // Deserialise the players
                    int numPlayers = nativeMethod.Reader.ReadInt32();

                    //    Debug.LogError("numPlayers " + numPlayers);
                    for (int i = 0; i < numPlayers; i++)
                    {
                        SessionMember newMember = new SessionMember();
                        newMember.DeserialiseBasicInfo(nativeMethod.Reader, JoinAsSpectator);

                        session.AddorUpdateMember(newMember);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Retrieved sessions data when using <see cref="GetPlayerSessionsRequest"/>
        /// </summary>
        public class RetrievedSessionsData
        {

            /// <summary>
            /// True is the calling user is in the session
            /// </summary>
            public bool IsUserInSession { get; internal set; }

            public List<RetrievedSessionData> Sessions;

            // must match native code in GameSessionCommands::SerialiseSessionInfo() GameSession.cpp
            internal void Deserialise(BinaryReader reader)
            {
                Sessions = new List<RetrievedSessionData>();

                IsUserInSession = reader.ReadBoolean();

                if (IsUserInSession == false) return;

                int numSessions = reader.ReadInt32();
                for (int session=0; session<numSessions; session++)
                {
                    var sessiondata = new RetrievedSessionData();
                    sessiondata.Deserialise(reader);
                    Sessions.Add(sessiondata);
                }
            }
        }

        /// <summary>
        /// Retrieved session data when using <see cref="GetPlayerSessionsRequest"/>
        /// </summary>
        public class RetrievedSessionData
        {
            /// <summary>
            /// The flags represent which parts of the data have been returned based on the <see cref="GetPlayerSessionsRequest.RequiredFields"/>
            /// </summary>
            public PlayerSession.ParamTypes SetFlags { get; internal set; }

            /// <summary>
            /// True is the calling user is in the session
            /// </summary>
            public bool IsUserInSession { get; internal set; }

            /// <summary>
            /// The session id
            /// </summary>
            public string SessionId { get; internal set; }

            /// <summary>
            /// Date and time of creation of the Player Session
            /// </summary>
            public DateTime CreatedTimeStamp { get; internal set; }

            /// <summary>
            /// Maximum number of members who can join as players
            /// </summary>
            public UInt32 MaxPlayers { get; internal set; }

            /// <summary>
            /// Maximum number of members who can join as spectators
            /// </summary>
            public UInt32 MaxSpectators { get; internal set; }

            /// <summary>
            /// Members participating as players. This will only contain a partial set of session member data
            /// </summary>
            public SessionMember[] Players { get; internal set; }

            /// <summary>
            /// Members participating as spectators. This will only contain a partial set of session member data
            /// </summary>
            public SessionMember[] Spectators { get; internal set; }

            /// <summary>
            /// Flag for temporarily prohibiting joining
            /// </summary>
            public bool JoinDisabled { get; internal set; }

            /// <summary>
            /// Platforms that can join
            /// </summary>
            public SessionPlatforms SupportedPlatforms { get; internal set; }

            /// <summary>
            /// Name of the Player Session
            /// </summary>
            public string SessionName { get; internal set; }

            /// <summary>
            /// Name of the Player Session (in all languages)
            /// </summary>
            public LocalisedSessionNames LocalisedNames { get; internal set; }

            /// <summary>
            /// The leader account id
            /// </summary>
            public UInt64 LeaderAccountId { get; internal set; }

            /// <summary>
            /// The leaders platform
            /// </summary>
            public SessionPlatforms LeaderPlatform { get; internal set; }

            /// <summary>
            /// Users who can join without invitations
            /// </summary>
            public JoinableUserTypes JoinableUserType { get; internal set; }

            /// <summary>
            /// List of the users who can join without invitations
            /// </summary>
            public UInt64[] JoinableSpecifiedUsers { get; internal set; }

            /// <summary>
            /// Members who can send invitations
            /// </summary>
            public InvitableUserTypes InvitableUserType { get; internal set; }

            /// <summary>
            /// Information about the items to include the leader privileges of a Player Session
            /// </summary>
            public LeaderPrivilegeFlags LeaderPrivileges { get; internal set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Information about the items to exclude from the leader privileges of a Player Session
            /// </summary>
            public LeaderPrivilegeFlags ExclusiveLeaderPrivileges { get; internal set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Information about items to exclude from the System UI for the leader of a Player Session
            /// </summary>
            public LeaderPrivilegeFlags DisableSystemUiMenu { get; internal set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
            /// </summary>
            public byte[] CustomData1 { get; internal set; }

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
            /// </summary>
            public byte[] CustomData2 { get; internal set; }

            /// <summary>
            /// Flag indicating whether swapping from player to spectator, or from spectator to player, is supported
            /// When true, it is supported.When this is the case, it is also possible to swap from the system software UI.When false, it is not supported.
            /// </summary>
            public bool SwapSupported { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                SetFlags = 0;

                bool isValid = reader.ReadBoolean();

                if (isValid == false) return;

                bool isSessionSet = reader.ReadBoolean();
                if (isSessionSet)
                {
                    SessionId = reader.ReadPrxString();
                    SetFlags |= PlayerSession.ParamTypes.SessionId;
                }

                bool isCreatedTimeStamp = reader.ReadBoolean();
                if (isCreatedTimeStamp)
                {
                    CreatedTimeStamp = reader.ReadUnixTimestampString();
                    SetFlags |= PlayerSession.ParamTypes.CreatedTimeStamp;
                }

                bool isMaxPlayers = reader.ReadBoolean();
                if (isMaxPlayers)
                {
                    MaxPlayers = reader.ReadUInt32();
                    SetFlags |= PlayerSession.ParamTypes.MaxPlayers;
                }

                bool isMaxSpectators = reader.ReadBoolean();
                if (isMaxSpectators)
                {
                    MaxSpectators = reader.ReadUInt32();
                    SetFlags |= PlayerSession.ParamTypes.MaxSpectators;
                }

                bool memberSet = reader.ReadBoolean();

                if (memberSet == true)
                {
                    SetFlags |= PlayerSession.ParamTypes.Member;

                    bool playersSet = reader.ReadBoolean();

                    if (playersSet == true)
                    {
                        SetFlags |= PlayerSession.ParamTypes.MemberPlayers;

                        int numPlayers = reader.ReadInt32();

                        Players = new SessionMember[numPlayers];

                        for (int i = 0; i < numPlayers; i++)
                        {
                            Players[i] = new SessionMember();
                            Players[i].Deserialise(reader);

                            bool isCustomData1Set = reader.ReadBoolean();
                            if (isCustomData1Set)
                            {
                                SetFlags |= PlayerSession.ParamTypes.MemberPlayersCustomData1;
                                Players[i].DeserialiseCustomData(reader);
                            }
                        }
                    }

                    bool spectatorsSet = reader.ReadBoolean();

                    if (spectatorsSet == true)
                    {
                        SetFlags |= PlayerSession.ParamTypes.MemberSpectators;

                        int numSpectators = reader.ReadInt32();

                        Spectators = new SessionMember[numSpectators];

                        for (int i = 0; i < numSpectators; i++)
                        {
                            Spectators[i] = new SessionMember();
                            Spectators[i].Deserialise(reader);

                            bool isCustomData1Set = reader.ReadBoolean();
                            if (isCustomData1Set)
                            {
                                SetFlags |= PlayerSession.ParamTypes.MemberSpectatorsCustomData1;
                                Spectators[i].DeserialiseCustomData(reader);
                            }
                        }
                    }
                }

                bool isJoinDisabled = reader.ReadBoolean();
                if (isJoinDisabled)
                {
                    SetFlags |= PlayerSession.ParamTypes.JoinDisabled;
                    JoinDisabled = reader.ReadBoolean();
                }

                bool isSupportedPlatforms = reader.ReadBoolean();
                if (isSupportedPlatforms)
                {
                    SetFlags |= PlayerSession.ParamTypes.SupportedPlatforms;
                    SupportedPlatforms = (SessionPlatforms)reader.ReadUInt32();
                }

                bool isSessionName = reader.ReadBoolean();
                if (isSessionName)
                {
                    SetFlags |= PlayerSession.ParamTypes.SessionName;
                    SessionName = reader.ReadPrxString();
                }

                bool isLocalizedSessionName = reader.ReadBoolean();
                if (isLocalizedSessionName)
                {
                    SetFlags |= PlayerSession.ParamTypes.LocalizedSessionName;
                    LocalisedNames = new LocalisedSessionNames();
                    LocalisedNames.Deserialise(reader);
                }

                bool isLeader = reader.ReadBoolean();
                if (isLeader)
                {
                    LeaderAccountId = reader.ReadUInt64();
                    LeaderPlatform = (SessionPlatforms)reader.ReadUInt32();
                    SetFlags |= PlayerSession.ParamTypes.Leader;
                }

                bool isJoinableUserType = reader.ReadBoolean();
                if (isJoinableUserType)
                {
                    SetFlags |= PlayerSession.ParamTypes.JoinableUserType;
                    JoinableUserType = (JoinableUserTypes)reader.ReadUInt32();
                }

                bool isjoinableSpecifiedUsers = reader.ReadBoolean();
                if (isjoinableSpecifiedUsers)
                {
                    SetFlags |= PlayerSession.ParamTypes.JoinableSpecifiedUsers;

                    uint count = reader.ReadUInt32();

                    JoinableSpecifiedUsers = new UInt64[count];
                    for(int i = 0; i < count; i++)
                    {
                        JoinableSpecifiedUsers[i] = reader.ReadUInt64();
                    }
                }

                bool isInvitableUserType = reader.ReadBoolean();
                if (isInvitableUserType)
                {
                    SetFlags |= PlayerSession.ParamTypes.InvitableUserType;
                    InvitableUserType = (InvitableUserTypes)reader.ReadUInt32();
                }

                bool isLeaderPrivileges = reader.ReadBoolean();
                if (isLeaderPrivileges)
                {
                    SetFlags |= PlayerSession.ParamTypes.LeaderPrivileges;
                    LeaderPrivileges = PlayerSession.DeserialisePrivliges(reader);
                }

                bool isExclusiveLeaderPrivileges = reader.ReadBoolean();
                if (isExclusiveLeaderPrivileges)
                {
                    SetFlags |= PlayerSession.ParamTypes.ExclusiveLeaderPrivileges;
                    ExclusiveLeaderPrivileges = PlayerSession.DeserialisePrivliges(reader);
                }

                bool isDisableSystemUIMenu = reader.ReadBoolean();
                if (isDisableSystemUIMenu)
                {
                    SetFlags |= PlayerSession.ParamTypes.DisableSystemUiMenu;
                    DisableSystemUiMenu = PlayerSession.DeserialisePrivliges(reader);
                }

                bool isCustomData1 = reader.ReadBoolean();
                if (isCustomData1)
                {
                    CustomData1 = reader.ReadData();
                    SetFlags |= PlayerSession.ParamTypes.CustomData1;
                }

                bool isCustomData2 = reader.ReadBoolean();
                if (isCustomData2)
                {
                    CustomData2 = reader.ReadData();
                    SetFlags |= PlayerSession.ParamTypes.CustomData2;
                }

                bool isSwapSupported = reader.ReadBoolean();
                if (isSwapSupported)
                {
                    SwapSupported = reader.ReadBoolean();
                    SetFlags |= PlayerSession.ParamTypes.SwapSupported;
                }

            }
        }

        /// <summary>
        /// Get info for a session
        /// </summary>
        public class GetPlayerSessionsRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session ID to retrieve info
            /// </summary>
            public string SessionIds { get; set; }

            /// <summary>
            /// Which fields should be retrieved for the session
            /// </summary>
            public PlayerSession.ParamTypes RequiredFields { get; set; } = PlayerSession.ParamTypes.Default;

            /// <summary>
            /// The retieved session data
            /// </summary>
            public RetrievedSessionsData SessionsData { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetPlayerSessions);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionIds);

                // Must add SessionId flag so when the data is returned the session id is included
                PlayerSession.ParamTypes required = RequiredFields | PlayerSession.ParamTypes.SessionId;

                SessionFieldFlagsSerialiser.SerialiseFieldFlags((UInt32)required, PlayerSession.FlagText, nativeMethod.Writer);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    SessionsData = new RetrievedSessionsData();

                    SessionsData.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Send player session invitiations
        /// </summary>
        public class SendPlayerSessionInvitationsRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session ID to send invite
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// The account Ids to recieve the invitation
            /// </summary>
            public List<UInt64> AccountIds { get; set; } = null;

            /// <summary>
            /// The returned invitiation ids
            /// </summary>
            public List<string> InvitationIds { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SendPlayerSessionsInvitation);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Writer.Write((Int32)AccountIds.Count);

                for (int i = 0; i < AccountIds.Count; i++)
                {
                    nativeMethod.Writer.Write(AccountIds[i]);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Int32 numInvitationIds = nativeMethod.Reader.ReadInt32();

                    InvitationIds = new List<string>(numInvitationIds);

                    for (int i = 0; i < numInvitationIds; i++)
                    {
                        InvitationIds.Add(nativeMethod.Reader.ReadPrxString());
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Retrieved invitiation settings returned from <see cref="GetPlayerSessionInvitationsRequest"/>
        /// </summary>
        public class RetrievedInvitation
        {
            /// <summary>
            /// Param types for a session
            /// </summary>
            [Flags]
            public enum ParamTypes
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary> Invitation ID </summary>
                InvitationId = 1,           // bit 1
                /// <summary> Sender information </summary>
                From = 2,                   // bit 2
                /// <summary> Session ID belonging to the invitation </summary>
                SessionId = 4,              // bit 3
                /// <summary> The platforms that can use the invitation </summary>
                SupportedPlatforms = 8,     // bit 4
                /// <summary>  The date and time of receiving </summary>
                ReceivedTimestamp = 16,     // bit 5
                /// <summary>  Flag for whether the invitation is valid or invalid </summary>
                InvitationInvalid = 32,     // bit 6
                /// <summary>  Default flag set </summary>
                Default = InvitationId | From | ReceivedTimestamp | InvitationInvalid,
                /// <summary>  All flags </summary>
                All = Default | SessionId | SupportedPlatforms
            }

            static internal string[] FlagText = new string[]
            {
                "invitationId",             // InvitationId         // bit 1
                "from",                     // From                 // bit 2
                "sessionId",                // SessionId            // bit 3
                "supportedPlatforms",       // SupportedPlatforms   // bit 4
                "receivedTimestamp",        // ReceivedTimestamp    // bit 5
                "invitationInvalid",        // InvitationInvalid    // bit 6
            };

            /// <summary>
            /// Flags
            /// </summary>
            public ParamTypes SetFlags { get; internal set; }

            /// <summary>
            /// Invitation Id
            /// </summary>
            public string InvitationId { get; internal set; }

            /// <summary>
            /// Sender account id
            /// </summary>
            public UInt64 FromAccountId { get; internal set; }

            /// <summary>
            /// Sender online id
            /// </summary>
            public string FromOnLineId { get; internal set; }

            /// <summary>
            /// Sender platform
            /// </summary>
            public SessionPlatforms FromPlatform { get; internal set; }

            /// <summary>
            /// Player Session ID.
            /// </summary>
            public string SessionId { get; internal set; }

            /// <summary>
            /// The platforms that can use the invitation
            /// </summary>
            public SessionPlatforms SupportedPlatforms { get; internal set; }

            /// <summary>
            /// Date and time of receiving
            /// </summary>
            public DateTime ReceivedTimestamp { get; internal set; }

            /// <summary>
            /// Flag for whether the invitation is valid or invalid
            /// </summary>
            /// <remarks>
            /// If the session to which the invitation belongs does not exist, or if the sender of the invitation has left the Player Session, the invitation will be invalid.
            /// </remarks>
            public bool InvitationInvalid { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                SetFlags = 0;

                bool isInvitationIdSet = reader.ReadBoolean();
                if (isInvitationIdSet)
                {
                    InvitationId = reader.ReadPrxString();
                    SetFlags |= ParamTypes.InvitationId;
                }

                bool isFromSet = reader.ReadBoolean();
                if (isFromSet)
                {
                    FromAccountId = reader.ReadUInt64();
                    FromOnLineId = reader.ReadPrxString();
                    FromPlatform = (SessionPlatforms)reader.ReadUInt32();

                    SetFlags |= ParamTypes.From;
                }

                bool isSessionIdSet = reader.ReadBoolean();
                if (isSessionIdSet)
                {
                    SessionId = reader.ReadPrxString();
                    SetFlags |= ParamTypes.SessionId;
                }

                bool isSupportedPlatformsSet = reader.ReadBoolean();
                if (isSupportedPlatformsSet)
                {
                    SupportedPlatforms = (SessionPlatforms)reader.ReadUInt32();
                    SetFlags |= ParamTypes.SupportedPlatforms;
                }

                bool isReceivedTimestampSet = reader.ReadBoolean();
                if (isReceivedTimestampSet)
                {
                    ReceivedTimestamp = reader.ReadUnixTimestampString();
                    SetFlags |= ParamTypes.ReceivedTimestamp;
                }

                bool isInvitationInvalidSet = reader.ReadBoolean();
                if (isInvitationInvalidSet)
                {
                    InvitationInvalid = reader.ReadBoolean();
                    SetFlags |= ParamTypes.InvitationInvalid;
                }
            }
        }

        /// <summary>
        /// Get player session invitiations
        /// </summary>
        public class GetPlayerSessionInvitationsRequest : Request
        {
            /// <summary>
            /// Filter for the type of invite to retrieve
            /// </summary>
            public enum RetrievalFilters
            {
                /// <summary> Only retrieve valid invitations</summary>
                ValidOnly = 0,
                /// <summary> Only retrieve invalid invitations </summary>
                InvalidOnly = 1,
                /// <summary> Retrieve both valid and invalid invitations </summary>
                All = 2
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Fields to retrieve
            /// </summary>
            public RetrievedInvitation.ParamTypes RequiredFields { get; set; } = RetrievedInvitation.ParamTypes.Default;

            /// <summary>
            /// Does the request get valid, invalid or all types of invites
            /// </summary>
            public RetrievalFilters Filter { get; set; } = RetrievalFilters.All;

            /// <summary>
            /// List of invitations obtained
            /// </summary>
            public List<RetrievedInvitation> Invitations { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetPlayerSessionInvitations);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                SessionFieldFlagsSerialiser.SerialiseFieldFlags((UInt32)RequiredFields, RetrievedInvitation.FlagText, nativeMethod.Writer);

                nativeMethod.Writer.Write((Int32)Filter);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    bool anyData = nativeMethod.Reader.ReadBoolean();

                    if(anyData == true)
                    {
                        int count = nativeMethod.Reader.ReadInt32();

                        if(count > 0)
                        {
                            Invitations = new List<RetrievedInvitation>(count);

                            for(int i = 0; i < count; i++)
                            {
                                RetrievedInvitation invite = new RetrievedInvitation();
                                invite.Deserialise(nativeMethod.Reader);
                                Invitations.Add(invite);
                            }
                        }
                        else
                        {
                            Invitations = null;
                        }
                    }
                    else
                    {
                        Invitations = null;
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Update Player Session information
        /// </summary>
        /// <remarks>
        /// Modify one item of information specified, out of all the information concerning the Player Session (it is not possible to update multiple items, and specifying multiple such items will result in an error).
        /// If <see cref="MaxPlayers"/> is being updated, it cannot be modified to a value smaller than the number of players already currently participating.
        /// If <see cref="MaxSpectators"/> is being updated, it cannot be modified to a value smaller than the number of spectators already currently participating.
        /// If updating <see cref="JoinableUserType"/> or <see cref="InvitableUserType"/>, the user who is the leader of the Player Session must be specified.
        /// </remarks>
        public class SetPlayerSessionPropertiesRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Session id to update
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Which parameters should be set. Only one item can be set at a time, specifying multiple such items will result in an error
            /// </summary>
            public PlayerSession.ParamTypes ParamToSet { get; set; }

            /// <summary>
            /// Maximum number of supported players. Min 1, Max 100
            /// </summary>
            public UInt32 MaxPlayers { get; set; } = 1; // Min 1, Max 100

            /// <summary>
            /// Maximum number of supported spectators. Min 0, Max 50
            /// </summary>
            public UInt32 MaxSpectators { get; set; } = 0; // Min , Max 50

            /// <summary>
            /// Flag that temporarily prohibits joining a Player Session.
            /// When true, joining is prohibited. When false, joining is allowed (however, limited to the users allowed to join without invitations based on the set value of <see cref="JoinableUserType"/>).
            /// </summary>
            public bool JoinDisabled { get; set; } = false;

            /// <summary>
            /// Users who can join Player Sessions without invitations
            /// </summary>
            /// <remarks>
            /// <see cref="JoinableUserTypes.NoOne"/> : No user can join without an invitation
            /// <see cref="JoinableUserTypes.Friends"/> : Friends of the leader of the Player Session can join
            /// <see cref="JoinableUserTypes.FriendsOfFriends"/> : Friends of friends of the leader of the Player Session can join
            /// <see cref="JoinableUserTypes.Anyone"/> : Anyone can join
            /// <see cref="JoinableUserTypes.SpecifiedUsers"/> : Users who have been registered on the joinableSpecifiedUsers list can join without invitations
            /// </remarks>
            public JoinableUserTypes JoinableUserType { get; set; } = JoinableUserTypes.NoOne;

            /// <summary>
            /// Information about who can send invitations to a Player Session
            /// </summary>
            /// <remarks>
            /// <see cref="InvitableUserTypes.NoOne"/> : No user can send an invite
            /// <see cref="InvitableUserTypes.Leader"/> : Only the leader can send invites
            /// <see cref="InvitableUserTypes.Member"/> : Any member can send invites
            /// </remarks>
            public InvitableUserTypes InvitableUserType { get; set; } = InvitableUserTypes.NoOne;

            /// <summary>
            /// Localisaed session names
            /// </summary>
            public LocalisedSessionNames LocalisedNames { get; set; } = new LocalisedSessionNames();

            /// <summary>
            /// If a specification is made with this parameter, the leader will have the specified privileges. If nothing is specified,
            /// the leader will have <see cref="LeaderPrivilegeFlags.Kick"/> and <see cref="LeaderPrivilegeFlags.UpdateJoinableUserType"/> privileges.
            /// However, it is recommended that <see cref="ExclusiveLeaderPrivileges"/>, rather than this parameter, be used to specify the leader's privileges.
            /// <see cref="LeaderPrivilegeFlags.UpdateInvitableUerType"/> flag will be ingorned for these privileges.
            /// </summary>
            public LeaderPrivilegeFlags LeaderPrivileges { get; set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Information about the items to exclude from the leader privileges of a Player Session
            /// If a specification is made with this parameter, the leader will not have the specified privileges.If nothing is specified, the leader will have all leader privileges
            /// All <see cref="LeaderPrivilegeFlags"/> are valid here.
            /// </summary>
            public LeaderPrivilegeFlags ExclusiveLeaderPrivileges { get; set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Information about items to exclude from the System UI for the leader of a Player Session
            /// If a specification is made with this parameter, the leader will not see the specified privileges in the system UI.
            /// If nothing is specified, the leader will have all leader privileges
            /// All <see cref="LeaderPrivilegeFlags"/> are valid here.
            /// </summary>
            public LeaderPrivilegeFlags DisableSystemUiMenu { get; set; } = LeaderPrivilegeFlags.NotSet;

            /// <summary>
            /// Custom binary session data
            /// </summary>
            public byte[] CustomData1 { get; set; }

            /// <summary>
            /// Custom binary session data
            /// </summary>
            public byte[] CustomData2 { get; set; }

            /// <summary>
            /// Flag indicating whether members who have joined can switch from players to spectators, or from spectators to players, without leaving.
            /// When true, swapping is allowed. (Users can swap using the system software UI.)When false, swapping is not allowed
            /// </summary>
            public bool SwapSupported { get; set; } = false;



            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetPlayerSessionProperties);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                if ((ParamToSet & PlayerSession.ParamTypes.MaxPlayers) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(MaxPlayers);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.MaxSpectators) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(MaxSpectators);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.JoinDisabled) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(JoinDisabled);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.JoinableUserType) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((UInt32)JoinableUserType);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.InvitableUserType) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((UInt32)InvitableUserType);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.LocalizedSessionName) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    LocalisedNames.Serialise(nativeMethod.Writer);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.LeaderPrivileges) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    PlayerSession.SerialisePrivliges(nativeMethod.Writer, LeaderPrivileges, false);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.ExclusiveLeaderPrivileges) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    PlayerSession.SerialisePrivliges(nativeMethod.Writer, ExclusiveLeaderPrivileges, true);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.DisableSystemUiMenu) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    PlayerSession.SerialisePrivliges(nativeMethod.Writer, DisableSystemUiMenu, true);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.CustomData1) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(CustomData1.Length);
                    nativeMethod.Writer.Write(CustomData1);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.CustomData2) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(CustomData2.Length);
                    nativeMethod.Writer.Write(CustomData2);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & PlayerSession.ParamTypes.SwapSupported) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(SwapSupported);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Change player session leader
        /// </summary>
        public class ChangePlayerSessionLeaderRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Player Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Account ID of new leader
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Platform of new leader
            /// </summary>
            public string Platform { get; set; } = "PS5";

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.ChangePlayerSessionLeader);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Writer.Write(AccountId);

                nativeMethod.Writer.WritePrxString(Platform);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Add list of joinable specified users to a session
        /// </summary>
        public class AddPlayerSessionJoinableSpecifiedUsersRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Player Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Add the specified users to the <see cref="PlayerSession.JoinableSpecifiedUsers"/>
            /// </summary>
            public List<UInt64> JoinableAccountIds { get; set; }

            /// <summary>
            /// Recieved set of joinable account ids
            /// </summary>
            public List<UInt64> RetrievedAccountIds { get; internal set; } = new List<UInt64>();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.AddPlayerSessionJoinableSpecifiedUsers);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                int count = JoinableAccountIds != null ? JoinableAccountIds.Count : 0;

                nativeMethod.Writer.Write(count);

                for(int i = 0; i < count; i++)
                {
                    nativeMethod.Writer.Write(JoinableAccountIds[i]);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    int size = nativeMethod.Reader.ReadInt32();

                    if (size > 0)
                    {
                        RetrievedAccountIds.Clear();

                        for (int i = 0; i < size; i++)
                        {
                            RetrievedAccountIds.Add(nativeMethod.Reader.ReadUInt64());
                        }
                    }
                    else
                    {
                        RetrievedAccountIds = null;
                    }

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Delete list of joinable specified users from a session
        /// </summary>
        public class DeletePlayerSessionJoinableSpecifiedUsersRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Player Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Delete the specified users from the <see cref="PlayerSession.JoinableSpecifiedUsers"/>
            /// </summary>
            public List<UInt64> JoinableAccountIds { get; set; }


            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeletePlayerSessionJoinableSpecifiedUsers);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                string accountIdsStr = "";

                int count = JoinableAccountIds != null ? JoinableAccountIds.Count : 0;

                for (int i = 0; i < count; i++)
                {
                    accountIdsStr += JoinableAccountIds[i].ToString();

                    if(i < count-1)
                    {
                        accountIdsStr += ",";
                    }
                }

                nativeMethod.Writer.WritePrxString(accountIdsStr);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Set member system properties for a player session
        /// </summary>
        public class SetPlayerSessionMemberSystemPropertiesRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Player Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64
            /// </summary>
            public byte[] CustomData1 { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetPlayerSessionMemberSystemProperties);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Writer.Write(CustomData1.Length);
                nativeMethod.Writer.Write(CustomData1);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Send player session message
        /// </summary>
        public class SendPlayerSessionMessageRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of recipients' Account IDs
            /// </summary>
            public List<SessionMemberIdentifier> Recipients { get; set; }

            /// <summary>
            /// Player Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Specify an arbitrary string (e.g., in Base64 or JSON). The amount of data must be at least 1 byte and no greater than 8032 bytes
            /// </summary>
            public string Payload { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SendPlayerSessionMessage);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Writer.WritePrxString(Payload);

                int count = Recipients != null ? Recipients.Count : 0;

                nativeMethod.Writer.Write(count);

                for (int i = 0; i < count; i++)
                {
                    SessionMemberIdentifier recipient = Recipients[i];
                    nativeMethod.Writer.Write(recipient.accountId);
                    nativeMethod.Writer.Write((uint)recipient.platform);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Joined player session info
        /// </summary>
        public class JoinedPlayerSession
        {
            /// <summary>
            /// The session id joined by the user
            /// </summary>
            public string SessionId { get; internal set; }

            /// <summary>
            /// The platform for the session
            /// </summary>
            public SessionPlatforms Platform { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                SessionId = reader.ReadPrxString();
                Platform = (SessionPlatforms)reader.ReadUInt32();
            }
        }

        /// <summary>
        /// Obtain a list of Player Sessions in which a user is participating
        /// </summary>
        public class GetJoinedPlayerSessionsByUserRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of Player Sessions obtained
            /// </summary>
            public List<JoinedPlayerSession> FoundPlayerSessions { get; internal set; } = new List<JoinedPlayerSession>();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetJoinedPlayerSessionsByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                FoundPlayerSessions.Clear();

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Int32 count = nativeMethod.Reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        JoinedPlayerSession jps = new JoinedPlayerSession();
                        jps.Deserialise(nativeMethod.Reader);
                        FoundPlayerSessions.Add(jps);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
