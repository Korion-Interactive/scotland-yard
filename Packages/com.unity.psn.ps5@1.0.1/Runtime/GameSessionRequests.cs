
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
    /// Members added to a game session
    /// </summary>
    public class GameSessionInitMember
    {
        /// <summary>
        /// User Id for local member
        /// </summary>
        public Int32 UserId { get; set; }


        internal Int32 PushCallbackId { get; set; } = WebApiPushEvent.InvalidCallbackId;

        /// <summary>
        /// Member Account id
        /// </summary>
        public UInt64 AccountId { get; set; }

        /// <summary>
        /// Members Platform
        /// </summary>
        public SessionPlatforms Platform { get; set; }

        /// <summary>
        /// Session join status of a member.
        /// </summary>
        public GameSessionJoinState JoinState { get; set; } = GameSessionJoinState.NotSet;

        /// <summary>
        /// NAT type of the member.
        /// </summary>
        public Int32 NatType { get; set; } = 0;

        /// <summary>
        /// Custom binary member data
        /// </summary>
        public byte[] CustomData1 { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(UserId);
            writer.Write(PushCallbackId);
            writer.Write(AccountId);
            writer.Write((Int32)Platform);
            writer.Write((Int32)JoinState);
            writer.Write((Int32)NatType);

            Int32 dataSize = CustomData1 != null ? CustomData1.Length : 0;
            writer.Write(dataSize);
            if (dataSize > 0)
            {
                writer.Write(CustomData1);
            }
        }
    }


    public class SearchAttributesType
    {

        public class DataStore<T>
        {
            private T[] store;
            public uint setBits;
            public DataStore()
            {
                store = new T[10];
                setBits=0;
            }


            public T this[int index]
            {
                get { return store[index];  }
                set { setBits |= 1u<<index; store[index] = value; }
            }

        }

        public DataStore<int> ints;
        public  DataStore<bool> bools;
        public  DataStore<string> strings;


        public SearchAttributesType()
        {
            ints = new DataStore<int>();
            bools = new DataStore<bool>();
            strings = new DataStore<string>();
        }
    }


        // gameSessionsSearch conditions attributes
        public enum SearchAttribute //
        {
            _NOT_SET,
            kMaxplayers, // "maxPlayers"
            kNumplayerslots, // "numPlayerSlots"
            kMaxspectators, // "maxSpectators"
            kNumspectatorslots, // "numSpectatorSlots"
            kJoindisabled, // "joinDisabled"
            kNattype, // "natType"
            kSearchattributesInteger1, // "searchAttributes.integer1"
            kSearchattributesInteger2, // "searchAttributes.integer2"
            kSearchattributesInteger3, // "searchAttributes.integer3"
            kSearchattributesInteger4, // "searchAttributes.integer4"
            kSearchattributesInteger5, // "searchAttributes.integer5"
            kSearchattributesInteger6, // "searchAttributes.integer6"
            kSearchattributesInteger7, // "searchAttributes.integer7"
            kSearchattributesInteger8, // "searchAttributes.integer8"
            kSearchattributesInteger9, // "searchAttributes.integer9"
            kSearchattributesInteger10, // "searchAttributes.integer10"
            kSearchattributesBoolean1, // "searchAttributes.boolean1"
            kSearchattributesBoolean2, // "searchAttributes.boolean2"
            kSearchattributesBoolean3, // "searchAttributes.boolean3"
            kSearchattributesBoolean4, // "searchAttributes.boolean4"
            kSearchattributesBoolean5, // "searchAttributes.boolean5"
            kSearchattributesBoolean6, // "searchAttributes.boolean6"
            kSearchattributesBoolean7, // "searchAttributes.boolean7"
            kSearchattributesBoolean8, // "searchAttributes.boolean8"
            kSearchattributesBoolean9, // "searchAttributes.boolean9"
            kSearchattributesBoolean10, // "searchAttributes.boolean10"
            kSearchattributesString1, // "searchAttributes.string1"
            kSearchattributesString2, // "searchAttributes.string2"
            kSearchattributesString3, // "searchAttributes.string3"
            kSearchattributesString4, // "searchAttributes.string4"
            kSearchattributesString5, // "searchAttributes.string5"
            kSearchattributesString6, // "searchAttributes.string6"
            kSearchattributesString7, // "searchAttributes.string7"
            kSearchattributesString8, // "searchAttributes.string8"
            kSearchattributesString9, // "searchAttributes.string9"
            kSearchattributesString10, // "searchAttributes.string10"
        }

        // gameSessionsSearch conditions attributes
        public enum SearchOperator //
        {
            _NOT_SET = 0,
            kEqual, // "EQUAL"
            kNotEqual, // "NOT_EQUAL"
            kGreaterThan, // "GREATER_THAN"
            kLessThan, // "LESS_THAN"
            kGreaterThanOrEqual, // "GREATER_THAN_OR_EQUAL"
            kLessThanOrEqual, // "LESS_THAN_OR_EQUAL"
            kIn, // "IN"
        }

        public class Condition
        {
            public Condition(SearchAttribute attribute, SearchOperator searchOperator, string value)
            {
                Attribute = attribute; Operator=searchOperator; Value=value;
            }
            public Condition() {}

            public SearchAttribute Attribute;
            public SearchOperator Operator;
            public string Value;
            public List<string> Values;
        }

    /// <summary>
    /// The parameters used when creating a game session
    /// </summary>
    public class GameSessionCreationParams
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
        /// Information about the platforms with which users can join a Game Session
        /// </summary>
        public SessionPlatforms SupportedPlatforms { get; set; } = SessionPlatforms.PS5;

        /// <summary>
        /// Flag for temporarily prohibiting joining a Game Session. When true, the Game Session cannot be joined.
        /// </summary>
        public bool JoinDisabled { get; set; } = false;

        /// <summary>
        /// Flag that indicates Game Session and Player Session dependency true indicates an association with a Player Session, and false indicates a standalone Game Session. Defaults true
        /// </summary>
        public bool UsePlayerSession { get; set; } = true;

        /// <summary>
        /// Period of validity for a reservation by a member to join a Game Session
        /// </summary>
        public Int32 ReservationTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Additional members to add to the game session
        /// </summary>
        public List<GameSessionInitMember> AdditionalMembers;

        /// <summary>
        /// The set of filters to use for the initial user who creates the session. Default to standard set of WebApi notification types. <see cref="GameSessionFilters"/>
        /// </summary>
        public WebApiFilters SessionFilters { get; set; } = GameSessionFilters.DefaultFilters;

        /// <summary>
        /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
        /// </summary>
        public byte[] CustomData1 { get; set; }

        /// <summary>
        /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
        /// </summary>
        public byte[] CustomData2 { get; set; }

        /// <summary>
        /// SearchIndex. An array of up to 64 arbitrary alphanumeric characters used for Game Session Search. One title can use a maximum of 100 different searchIndex values
        /// </summary>
        public string SearchIndex { get; set; }


        /// <summary>
        /// SearchAttributes. Attribute values for narrowing down search conditions.
        /// </summary>
        public SearchAttributesType SearchAttributes { get; set; } // todo... this can be up to 10 each of bool, int or string

        /// <summary>
        /// Searchable. Flag indicates the game session searchable.
        /// </summary>
        public bool Searchable { get; set; }

        /// <summary>
        /// Callbacks triggered when WebApi notifications are recieved and processed by the session
        /// </summary>
        public GameSessionCallbacks Callbacks { get; set; } = new GameSessionCallbacks();

        // must match native code at  GameSessionCommands::InitializationParams::Deserialise()
        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(MaxPlayers);
            writer.Write(MaxSpectators);
            writer.Write((UInt32)SupportedPlatforms);
            writer.Write(JoinDisabled);
            writer.Write(UsePlayerSession);
            writer.Write(ReservationTimeoutSeconds);

            Int32 numMembers = AdditionalMembers != null ? AdditionalMembers.Count : 0;
            writer.Write(numMembers);
            for (int i = 0; i < numMembers; i++)
            {
                AdditionalMembers[i].Serialise(writer);
            }

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

            writer.Write(Searchable);
            writer.WritePrxString(SearchIndex);

            writer.Write(SearchAttributes.strings.setBits);
            writer.Write(SearchAttributes.ints.setBits);
            writer.Write(SearchAttributes.bools.setBits);
            for (int i =0; i<10 ; i++)
            {
                writer.WritePrxString(String.IsNullOrEmpty(SearchAttributes.strings[i]) ? "" : SearchAttributes.strings[i]);
                writer.Write(SearchAttributes.ints[i]);
                writer.Write(SearchAttributes.bools[i]);
            }
        }
    }

    /// <summary>
    /// Requests for Game sessions
    /// </summary>
    public class GameSessionRequests
    {
        internal enum NativeMethods : UInt32
        {
            CreateGameSession = 0x0B00001u,
            LeaveGameSession = 0x0B00002u,
            JoinGameSession = 0x0B00003u,
            GetGameSessions = 0x0B00004u,
            SetGameSessionProperties = 0x0B00005u,
            SetGameSessionMemberSystemProperties = 0x0B00006u,
            SendGameSessionMessage = 0x0B00007u,
            GetJoinedGameSessionsByUser = 0x0B00008u,
            DeleteGameSession = 0x0B00009u,
            GameSessionsSearch = 0x0B0000Au,
        }

        /// <summary>
        /// Create a new game session
        /// </summary>
        public class CreateGameSessionRequest : Request
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
            public GameSessionCreationParams CreationParams { get; set; }

            /// <summary>
            /// The created game session instance
            /// </summary>
            public GameSession Session { get; internal set; }

            protected internal override void Run()
            {
                // serialization must match deserialization in native code GameSessionCommands::CreateGameSessionImpl()
                WebApiPushEvent sessionPushEvent = null;

                Result = WebApiNotifications.CreatePushEventBlocking(UserId, CreationParams.SessionFilters, GameSession.SessionWebApiEventHandler, true, out sessionPushEvent);

                if (sessionPushEvent == null)
                {
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)GameSessionRequests.NativeMethods.CreateGameSession);

                GameSessionInitMember startMember = new GameSessionInitMember();
                startMember.UserId = UserId;
                startMember.PushCallbackId = sessionPushEvent.PushCallbackId;
                startMember.CustomData1 = CreatorsCustomData1;
                startMember.NatType = 3;    // value to be generated

                startMember.Serialise(nativeMethod.Writer);

                CreationParams.Serialise(nativeMethod.Writer);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    string sessionId = nativeMethod.Reader.ReadPrxString();

                    bool isNewSession = false;
                    Session = SessionsManager.CreateGameSession(sessionId, out isNewSession);

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
        /// Get a local user to leave the game session
        /// </summary>
        public class LeaveGameSessionRequest : Request
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
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)GameSessionRequests.NativeMethods.LeaveGameSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    UInt64 accountId = nativeMethod.Reader.ReadUInt64();

                    GameSession session = SessionsManager.FindGameSessionFromSessionId(SessionId);

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
        /// Get a user to join the game session
        /// </summary>
        public class JoinGameSessionRequest : Request
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
            public GameSession Session { get; internal set; }

            /// <summary>
            /// The session filters to use when creating the users WebApi push events
            /// </summary>
            public WebApiFilters SessionFilters { get; set; } = GameSessionFilters.DefaultFilters;

            /// <summary>
            /// The callback used when session is updated
            /// </summary>
            public GameSessionCallbacks Callbacks { get; set; } = new GameSessionCallbacks();

            protected internal override void Run()
            {
                WebApiPushEvent sessionPushEvent = null;

                Result = WebApiNotifications.CreatePushEventBlocking(UserId, SessionFilters, GameSession.SessionWebApiEventHandler, true, out sessionPushEvent);

                if (sessionPushEvent == null)
                {
                    return;
                }

                // Find a local session. If one already exists then it means the player is join a local session
                // Must also handle if the session id doesn't exist because the the user might be joining a player session just using the session id.
                bool isNewSession = false;
                Session = SessionsManager.CreateGameSession(SessionId, out isNewSession);

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)GameSessionRequests.NativeMethods.JoinGameSession);

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
                else
                {
                    // If session failed unregister the user push event
                    WebApiNotifications.UnregisterPushEventCall(sessionPushEvent);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Retrieved sessions data when using <see cref="GetGameSessionsRequest"/>
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
        /// Retrieved session data when using <see cref="GetGameSessionsRequest"/>
        /// </summary>
        public class RetrievedSessionData
        {
            /// <summary>
            /// The flags represent which parts of the data have been returned based on the <see cref="GetGameSessionsRequest.RequiredFields"/>
            /// </summary>
            public GameSession.ParamTypes SetFlags { get; internal set; }

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
            /// Game Session representative
            /// </summary>
            public GameSessionRepresentative Representative { get; internal set; }

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
            /// </summary>
            public byte[] CustomData1 { get; internal set; }

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64.
            /// </summary>
            public byte[] CustomData2 { get; internal set; }

            /// <summary>
            /// Flag that indicates Game Session and Player Session dependency.
            /// true indicates an association with a Player Session, and false indicates a standalone Game Session.
            /// </summary>
            public bool UsePlayerSession { get; internal set; }

            /// <summary>
            /// Information added by the Matchmaking service
            /// </summary>
            public string MatchmakingOfferId { get; internal set; }

            /// <summary>
            /// Period of validity for a reservation by a member to join a Game Session
            /// </summary>
            public Int32 ReservationTimeoutSeconds { get; internal set; }


            /// <summary>
            /// NAT type of the member
            /// </summary>
            public UInt32 NatType  { get; internal set; }

            /// <summary>
            /// Index for searching.
            /// </summary>
            public string  SearchIndex  { get; internal set; }

            /// <summary>
            /// Flag that controls whether to make the session searchable with Game Session Search
            /// </summary>
            public bool  Searchable  { get; internal set; }

            /// <summary>
            /// SearchAttributes. Attribute values for narrowing down search conditions.
            /// </summary>
            public SearchAttributesType SearchAttributes { get;internal  set; }

            internal void Deserialise(BinaryReader reader)
            {
                SetFlags = 0;

                bool isValid = reader.ReadBoolean();

                if (isValid == false) return;

                bool isSessionSet = reader.ReadBoolean();
                if (isSessionSet)
                {
                    SessionId = reader.ReadPrxString();
                    SetFlags |= GameSession.ParamTypes.SessionId;
                }

                bool isCreatedTimeStamp = reader.ReadBoolean();
                if (isCreatedTimeStamp)
                {
                    CreatedTimeStamp = reader.ReadUnixTimestampString();
                    SetFlags |= GameSession.ParamTypes.CreatedTimeStamp;
                }

                bool isMaxPlayers = reader.ReadBoolean();
                if (isMaxPlayers)
                {
                    MaxPlayers = reader.ReadUInt32();
                    SetFlags |= GameSession.ParamTypes.MaxPlayers;
                }

                bool isMaxSpectators = reader.ReadBoolean();
                if (isMaxSpectators)
                {
                    MaxSpectators = reader.ReadUInt32();
                    SetFlags |= GameSession.ParamTypes.MaxSpectators;
                }

                bool memberSet = reader.ReadBoolean();

                if (memberSet == true)
                {
                    SetFlags |= GameSession.ParamTypes.Member;

                    bool playersSet = reader.ReadBoolean();

                    if (playersSet == true)
                    {
                        SetFlags |= GameSession.ParamTypes.MemberPlayers;

                        int numPlayers = reader.ReadInt32();

                        Players = new SessionMember[numPlayers];

                        for (int i = 0; i < numPlayers; i++)
                        {
                            Players[i] = new SessionMember();
                            Players[i].Deserialise(reader);

                            bool isMemberPlayersJoinState = reader.ReadBoolean();
                            if (isMemberPlayersJoinState)
                            {
                                SetFlags |= GameSession.ParamTypes.MemberPlayersJoinState;
                                Players[i].DeserialiseJoinState(reader);
                            }

                            bool isCustomData1Set = reader.ReadBoolean();
                            if (isCustomData1Set)
                            {
                                SetFlags |= GameSession.ParamTypes.MemberPlayersCustomData1;
                                Players[i].DeserialiseCustomData(reader);
                            }
                        }
                    }

                    bool spectatorsSet = reader.ReadBoolean();

                    if (spectatorsSet == true)
                    {
                        SetFlags |= GameSession.ParamTypes.MemberSpectators;

                        int numSpectators = reader.ReadInt32();

                        Spectators = new SessionMember[numSpectators];

                        for (int i = 0; i < numSpectators; i++)
                        {
                            Spectators[i] = new SessionMember();
                            Spectators[i].Deserialise(reader);

                            bool isMemberSpectatorsJoinState = reader.ReadBoolean();
                            if (isMemberSpectatorsJoinState)
                            {
                                SetFlags |= GameSession.ParamTypes.MemberSpectatorsJoinState;
                                Spectators[i].DeserialiseJoinState(reader);
                            }

                            bool isCustomData1Set = reader.ReadBoolean();
                            if (isCustomData1Set)
                            {
                                SetFlags |= GameSession.ParamTypes.MemberSpectatorsCustomData1;
                                Spectators[i].DeserialiseCustomData(reader);
                            }
                        }
                    }
                }

                bool isJoinDisabled = reader.ReadBoolean();
                if (isJoinDisabled)
                {
                    SetFlags |= GameSession.ParamTypes.JoinDisabled;
                    JoinDisabled = reader.ReadBoolean();
                }

                bool isSupportedPlatforms = reader.ReadBoolean();
                if (isSupportedPlatforms)
                {
                    SetFlags |= GameSession.ParamTypes.SupportedPlatforms;
                    SupportedPlatforms = (SessionPlatforms)reader.ReadUInt32();
                }

                bool isRepresentativeSet = reader.ReadBoolean();
                if (isRepresentativeSet)
                {
                    SetFlags |= GameSession.ParamTypes.Representative;
                    if (Representative == null)
                    {
                        Representative = new GameSessionRepresentative();
                    }

                    Representative.Deserialise(reader);
                }

                bool isCustomData1 = reader.ReadBoolean();
                if (isCustomData1)
                {
                    CustomData1 = reader.ReadData();
                    SetFlags |= GameSession.ParamTypes.CustomData1;
                }

                bool isCustomData2 = reader.ReadBoolean();
                if (isCustomData2)
                {
                    CustomData2 = reader.ReadData();
                    SetFlags |= GameSession.ParamTypes.CustomData2;
                }

                bool isUsePlayerSession = reader.ReadBoolean();
                if (isUsePlayerSession)
                {
                    UsePlayerSession = reader.ReadBoolean();
                    SetFlags |= GameSession.ParamTypes.UsePlayerSession;
                }

                bool isMatchmakingOfferId = reader.ReadBoolean();
                if (isMatchmakingOfferId)
                {
                    MatchmakingOfferId = reader.ReadPrxString();
                    SetFlags |= GameSession.ParamTypes.Matchmaking;
                }

                bool isReservationTimeoutSeconds = reader.ReadBoolean();
                if (isReservationTimeoutSeconds)
                {
                    ReservationTimeoutSeconds = reader.ReadInt32();
                    SetFlags |= GameSession.ParamTypes.ReservationTimeoutSeconds;
                }

                bool isSearchable = reader.ReadBoolean();
                if (isSearchable)
                {
                    Searchable = reader.ReadBoolean();
                    SetFlags |= GameSession.ParamTypes.Searchable;
                }

            }
        }

        /// <summary>
        /// Get info for a session
        /// </summary>
        public class GetGameSessionsRequest : Request
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
            public GameSession.ParamTypes RequiredFields { get; set; } = GameSession.ParamTypes.Default;

            /// <summary>
            /// The retieved session data
            /// </summary>
            public RetrievedSessionsData SessionsData { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetGameSessions);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionIds);

                // Must add SessionId flag so when the data is returned the session id is included
                GameSession.ParamTypes required = RequiredFields | GameSession.ParamTypes.SessionId;

                // these values are not included in the valid values supported by the web request "GET /v1/gameSessions". see https://p.siedev.net/resources/documents/WebAPI/1/Session_Manager_WebAPI-Reference/0018.html
                GameSession.ParamTypes notSupported = GameSession.ParamTypes.Searchable|GameSession.ParamTypes.NatType|GameSession.ParamTypes.SearchIndex|GameSession.ParamTypes.SearchAttributes;

                SessionFieldFlagsSerialiser.SerialiseFieldFlags(((UInt32)required) & (~(UInt32)notSupported), GameSession.FlagText, nativeMethod.Writer);

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
        /// Update Game Session information
        /// </summary>
        /// <remarks>
        /// Modify one item of information specified, out of all the information concerning the Game Session (it is not possible to update multiple items, and specifying multiple such items will result in an error).
        /// If <see cref="MaxPlayers"/> is being updated, it cannot be modified to a value smaller than the number of players already currently participating.
        /// If <see cref="MaxSpectators"/> is being updated, it cannot be modified to a value smaller than the number of spectators already currently participating.
        /// </remarks>
        public class SetGameSessionPropertiesRequest : Request
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
            public GameSession.ParamTypes ParamToSet { get; set; }

            /// <summary>
            /// Maximum number of supported players. Min 1, Max 100
            /// </summary>
            public UInt32 MaxPlayers { get; set; } = 1; // Min 1, Max 100

            /// <summary>
            /// Maximum number of supported spectators. Min 0, Max 50
            /// </summary>
            public UInt32 MaxSpectators { get; set; } = 0; // Min , Max 50

            /// <summary>
            /// Flag that temporarily prohibits joining a Game Session.
            /// </summary>
            public bool JoinDisabled { get; set; } = false;

            /// <summary>
            /// Custom binary session data
            /// </summary>
            public byte[] CustomData1 { get; set; }

            /// <summary>
            /// Custom binary session data
            /// </summary>
            public byte[] CustomData2 { get; set; }

            /// <summary>
            /// Flag that controls whether to make the session searchable with Game Session Search or to exclude it temporarily from searches
            /// </summary>
            public bool Searchable { get; set; } = false;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetGameSessionProperties);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                if ((ParamToSet & GameSession.ParamTypes.MaxPlayers) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(MaxPlayers);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & GameSession.ParamTypes.MaxSpectators) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(MaxSpectators);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & GameSession.ParamTypes.JoinDisabled) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(JoinDisabled);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & GameSession.ParamTypes.CustomData1) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(CustomData1.Length);
                    nativeMethod.Writer.Write(CustomData1);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & GameSession.ParamTypes.CustomData2) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(CustomData2.Length);
                    nativeMethod.Writer.Write(CustomData2);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((ParamToSet & GameSession.ParamTypes.Searchable) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Searchable);
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
        /// Set member system properties for a game session
        /// </summary>
        public class SetGameSessionMemberSystemPropertiesRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Game Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Custom data. The maximum size is the size yielded from encoding 1024 bytes in Base64
            /// </summary>
            public byte[] CustomData1 { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetGameSessionMemberSystemProperties);

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
        /// Send gamegame session message
        /// </summary>
        public class SendGameSessionMessageRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of recipients.
            /// </summary>
            public List<SessionMemberIdentifier> Recipients { get; set; }

            /// <summary>
            /// Game Session ID
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// Specify an arbitrary string (e.g., in Base64 or JSON). The amount of data must be at least 1 byte and no greater than 8032 bytes
            /// </summary>
            public string Payload { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SendGameSessionMessage);

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
                    nativeMethod.Writer.Write((uint) recipient.platform);
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
        /// Joined game session info
        /// </summary>
        public class JoinedGameSession
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
        public class GetJoinedGameSessionsByUserRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// List of Player Sessions obtained
            /// </summary>
            public List<JoinedGameSession> FoundPlayerSessions { get; internal set; } = new List<JoinedGameSession>();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetJoinedGameSessionsByUser);

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
                        JoinedGameSession jps = new JoinedGameSession();
                        jps.Deserialise(nativeMethod.Reader);
                        FoundPlayerSessions.Add(jps);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Delete game session
        /// </summary>
        public class DeleteGameSessionRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Game Session ID
            /// </summary>
            public string SessionId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteGameSession);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SessionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// request a "GameSesssionSearch"
        /// </summary>
        public class GameSessionsSearchRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// SearchIndex  ... An array of up to 64 arbitrary alphanumeric characters, set when a game session is created (see RequestGameSession::setSearchIndex() )
            /// </summary>
            public string SearchIndex { get; set; }

            public List<Condition> Conditions;

            /// <summary>
            /// List of Games sessions returned
            /// </summary>
            public List<string> FoundGameSessions { get; internal set; } = new List<string>();

            public GameSessionsSearchRequest()
            {
                Conditions = new List<Condition>();
            }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GameSessionsSearch);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(SearchIndex);


                nativeMethod.Writer.Write(Conditions.Count);
                foreach( var cond in Conditions)
                {
                    nativeMethod.Writer.Write((int)cond.Attribute);
                    nativeMethod.Writer.Write((int)cond.Operator);
                    nativeMethod.Writer.WritePrxString(cond.Value);
                    if (cond.Values == null)
                    {
                        nativeMethod.Writer.Write((int)0);
                    }
                    else
                    {
                        nativeMethod.Writer.Write(cond.Values.Count);
                        foreach( var val in cond.Values)
                        {
                            nativeMethod.Writer.WritePrxString(val);
                        }
                    }
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // these values need to be stored
                    int numSessions = nativeMethod.Reader.ReadInt32();
                    FoundGameSessions = new List<string>();

                    for (int i = 0; i < numSessions; i++)
                    {
                        string sessionId = nativeMethod.Reader.ReadPrxString();
                        FoundGameSessions.Add(sessionId);
                    }
                }
                else
                {
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }

}
