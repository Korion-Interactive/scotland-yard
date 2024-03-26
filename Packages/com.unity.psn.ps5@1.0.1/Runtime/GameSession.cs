
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
    /// Game Session representative
    /// </summary>
    public class GameSessionRepresentative
    {
        /// <summary>
        /// The account id of the session representative or <see cref="SessionMember.InvalidAccountId"/> if not set
        /// </summary>
        public UInt64 AccountId { get; internal set; } = SessionMember.InvalidAccountId;

        /// <summary>
        /// The online id of the member
        /// </summary>
        public string OnlineId { get; internal set; } = "";

        /// <summary>
        /// The platform of the member.
        /// </summary>
        public SessionPlatforms Platform { get; internal set; } = 0;

        internal void Deserialise(BinaryReader reader)
        {
            AccountId = reader.ReadUInt64();
            OnlineId = reader.ReadPrxString();
            Platform = (SessionPlatforms)reader.ReadUInt32();
        }
    }

    /// <summary>
    /// Game session instance
    /// </summary>
    public partial class GameSession : Session  // see also GameSessionNotifcations.cs
    {
        /// <summary>
        /// Param types used when retrieving game session data or certain types of session notifications
        /// </summary>
        [Flags]
        public enum ParamTypes
        {
            /// <summary> Not set </summary>
            NotSet = 0,
            /// <summary> The session id is used </summary>
            SessionId = 1,                          // bit 1
            /// <summary> Date and time of creation of the Player Session </summary>
            CreatedTimeStamp = 2,                   // bit 2
            /// <summary> Maximum number of members who can join a Player Session as players </summary>
            MaxPlayers = 4,                         // bit 3
            /// <summary> Maximum number of members who can join a Player Session as spectators </summary>
            MaxSpectators = 8,                      // bit 4
            /// <summary> Participating members (including players and spectators) </summary>
            Member = 16,                            // bit 5
            /// <summary> Members participating as players </summary>
            MemberPlayers = 32,                     // bit 6
            /// <summary> Members participating as spectators </summary>
            MemberSpectators = 64,                  // bit 7
            /// <summary> Join states of the members who have joined the Game Session as players.</summary>
            MemberPlayersJoinState = 128,           // bit 8
            /// <summary> Join states of the members who have joined the Game Session as spectators (JOINED only).</summary>
            MemberSpectatorsJoinState = 256,        // bit 9
            /// <summary> Custom data 1 for a members participating as players.</summary>
            MemberPlayersCustomData1 = 512,         // bit 10
            /// <summary> Custom data 1 for members participating as spectators.</summary>
            MemberSpectatorsCustomData1 = 1024,      // bit 11
            /// <summary> Flag for temporarily prohibiting joining </summary>
            JoinDisabled = 2048,                    // bit 12
            /// <summary> Platforms that can join </summary>
            SupportedPlatforms = 4096,              // bit 13
            /// <summary> Game Session representative </summary>
            Representative = 8192,                  // bit 14
            /// <summary> Custom data 1 </summary>
            CustomData1 = 16384,                    // bit 15
            /// <summary> Custom data 2 </summary>
            CustomData2 = 32768,                    // bit 16
            /// <summary> Flag that indicates Game Session and Player Session dependency </summary>
            UsePlayerSession = 65536,               // bit 17
            /// <summary> Information added by the Matchmaking service </summary>
            Matchmaking = 131072,                   // bit 18
            /// <summary> Information added by the Matchmaking service </summary>
            ReservationTimeoutSeconds = 262144,     // bit 19
            /// <summary> session searchable with Game Session Search </summary>
            Searchable = 1<<(20-1),                 // bit 20
            /// <summary> Nat Type </summary>
            NatType = 1<<(21-1),                 // bit 21
            /// <summary>Search Index </summary>
            SearchIndex = 1<<(22-1),                 // bit 22
            /// <summary>Attributes for searching</summary>
            SearchAttributes = 1<<(23-1),                 // bit 23



            /// <summary> A typical set of flags when retrieving player sesssion data </summary>
            Default = SessionId | CreatedTimeStamp | MaxPlayers | MaxSpectators | Member | SupportedPlatforms,
            /// <summary> All the flag set </summary>
            All = Default | MemberPlayers | MemberSpectators | MemberPlayersJoinState | MemberSpectatorsJoinState | MemberPlayersCustomData1 | MemberSpectatorsCustomData1 | JoinDisabled | Representative | CustomData1 | CustomData2 | UsePlayerSession | Matchmaking | ReservationTimeoutSeconds |  Searchable | NatType | SearchIndex | SearchAttributes
        }


        /// <summary>
        /// Test is a flag is set
        /// </summary>
        /// <param name="flags">The session flags to check</param>
        /// <param name="flagToCheck">The session flag to test</param>
        /// <returns>Returns true if the flag to check is set</returns>

        public static bool IsParamFlagSet(ParamTypes flags, ParamTypes flagToCheck)
        {
            if ((flags & flagToCheck) != 0) return true;

            return false;
        }

        static internal string[] FlagText = new string[]
        {
                "sessionId",                        // SessionId                    // bit 1
                "createdTimestamp",                 // CreatedTimeStamp             // bit 2
                "maxPlayers",                       // MaxPlayers                   // bit 3
                "maxSpectators",                    // MaxSpectators                // bit 4
                "member",                           // Member                       // bit 5
                "member(players)",                  // MemberPlayers                // bit 6
                "member(spectators)",               // MemberSpectators             // bit 7
                "member(players(joinState))",       // MemberPlayersJoinState       // bit 8
                "member(spectators(joinState))",    // MemberSpectatorsJoinState    // bit 9
                "member(players(customData1))",     // MemberPlayersCustomData1     // bit 10
                "member(spectators(customData1))",  // MemberSpectatorsCustomData1  // bit 11
                "joinDisabled",                     // JoinDisabled                 // bit 12
                "supportedPlatforms",               // SupportedPlatforms           // bit 13
                "representative",                   // Representative               // bit 14
                "customData1",                      // CustomData1                  // bit 15
                "customData2",                      // CustomData2                  // bit 16
                "usePlayerSession",                 // UsePlayerSession             // bit 17
                "matchmaking",                      // Matchmaking                  // bit 18
                "reservationTimeoutSeconds",        // ReservationTimeoutSeconds    // bit 19
                "searchable",                       // Searchable                   // bit 20
                "natType",                          // NatType                      // bit 21
                "searchIndex",                      // SearchIndex                  // bit 22
                "searchAttributes",                 // SearchAttributes             // bit 23
        };

        /// <summary>
        /// Flag that indicates Game Session and Player Session dependency true indicates an association with a Player Session, and false indicates a standalone Game Session. Defaults true
        /// </summary>
        public bool UsePlayerSession { get; internal set; } = true;

        /// <summary>
        /// Information added by the Matchmaking service
        /// </summary>
        public string MatchmakingOfferId { get; internal set; }

        /// <summary>
        /// Game Session representative
        /// </summary>
        public GameSessionRepresentative Representative { get; internal set; }

        /// <summary>
        /// Period of validity for a reservation by a member to join a Game Session
        /// </summary>
        public Int32 ReservationTimeoutSeconds { get; internal set; } = 300;

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

        internal void Initialise(GameSessionCallbacks callbacks, WebApiPushEvent pushEvent)
        {
            if (pushEvent != null)
            {
                UserPushEvents.AddPushEvent(pushEvent);
            }

            if (callbacks != null)
            {
                if (callbacks.WebApiNotificationCallback != null)
                {
                    OnRawEvent = callbacks.WebApiNotificationCallback;
                }

                if (callbacks.OnSessionUpdated != null)
                {
                    OnSessionUpdated = callbacks.OnSessionUpdated;
                }
            }
        }

        internal void InitialiseCreationParams(GameSessionCreationParams initParams)
        {
            if (initParams == null) return;

            MaxPlayers = initParams.MaxPlayers;
            MaxSpectators = initParams.MaxSpectators;

            SupportedPlatforms = initParams.SupportedPlatforms;

            JoinDisabled = initParams.JoinDisabled;

            UsePlayerSession = initParams.UsePlayerSession;

            ReservationTimeoutSeconds = initParams.ReservationTimeoutSeconds;

            CustomData1 = initParams.CustomData1;

            CustomData2 = initParams.CustomData2;

            Searchable= initParams.Searchable;
            SearchIndex = initParams.SearchIndex;
            SearchAttributes = initParams.SearchAttributes;
        }

        internal void SessionDeleted()
        {
            //   Debug.LogError("SessionDeleted");
            CleanupGameSessionRequest request = new CleanupGameSessionRequest() { Session = this };
            var requestOp = new AsyncRequest<CleanupGameSessionRequest>(request);
            SessionsManager.Schedule(requestOp);

            // Mark up everything that needs to happen to cleanup the session
            SessionsManager.DestroyGameSession(this);
        }

        internal class CleanupGameSessionRequest : Request
        {
            public GameSession Session { get; set; }

            protected internal override void Run()
            {
                Session.CleanupSessionBlocking();
            }
        }

        /// <summary>
        ///  Update a game session from data retrieved from <see cref="GameSessionRequests.GetGameSessionsRequest"/>
        /// </summary>
        /// <param name="sessionData">The data to update</param>
        public void UpdateFrom(GameSessionRequests.RetrievedSessionData sessionData)
        {
            if ((sessionData.SetFlags & ParamTypes.SessionId) != 0)
            {
                SessionId = sessionData.SessionId;
            }

            if ((sessionData.SetFlags & ParamTypes.CreatedTimeStamp) != 0)
            {
                CreatedTimestamp = sessionData.CreatedTimeStamp;
            }

            if ((sessionData.SetFlags & ParamTypes.MaxPlayers) != 0)
            {
                MaxPlayers = sessionData.MaxPlayers;
            }

            if ((sessionData.SetFlags & ParamTypes.MaxSpectators) != 0)
            {
                MaxSpectators = sessionData.MaxSpectators;
            }

            if ((sessionData.SetFlags & ParamTypes.Member) != 0)
            {
                if ((sessionData.SetFlags & ParamTypes.MemberPlayers) != 0)
                {
                    SessionMember[] players = sessionData.Players;

                    for (int i = 0; i < players.Length; i++)
                    {
                        AddorUpdateMember(players[i]);
                    }
                }

                if ((sessionData.SetFlags & ParamTypes.MemberSpectators) != 0)
                {
                    SessionMember[] spectators = sessionData.Spectators;

                    for (int i = 0; i < spectators.Length; i++)
                    {
                        AddorUpdateMember(spectators[i]);
                    }
                }
            }

            if ((sessionData.SetFlags & ParamTypes.SupportedPlatforms) != 0)
            {
                SupportedPlatforms = sessionData.SupportedPlatforms;
            }

            if ((sessionData.SetFlags & ParamTypes.Representative) != 0)
            {
                Representative = sessionData.Representative;
            }

            if ((sessionData.SetFlags & ParamTypes.JoinDisabled) != 0)
            {
                JoinDisabled = sessionData.JoinDisabled;
            }

            if ((sessionData.SetFlags & ParamTypes.CustomData1) != 0)
            {
                CustomData1 = sessionData.CustomData1;
            }

            if ((sessionData.SetFlags & ParamTypes.CustomData2) != 0)
            {
                CustomData2 = sessionData.CustomData2;
            }

            if ((sessionData.SetFlags & ParamTypes.UsePlayerSession) != 0)
            {
                UsePlayerSession = sessionData.UsePlayerSession;
            }

            if ((sessionData.SetFlags & ParamTypes.Matchmaking) != 0)
            {
                MatchmakingOfferId = sessionData.MatchmakingOfferId;
            }

            if ((sessionData.SetFlags & ParamTypes.ReservationTimeoutSeconds) != 0)
            {
                ReservationTimeoutSeconds = sessionData.ReservationTimeoutSeconds;
            }

            if ((sessionData.SetFlags & ParamTypes.Searchable) != 0)
            {
                Searchable = sessionData.Searchable;
            }

            if ((sessionData.SetFlags & ParamTypes.NatType) != 0)
            {
                NatType = sessionData.NatType;
            }

            if ((sessionData.SetFlags & ParamTypes.SearchIndex) != 0)
            {
                SearchIndex = sessionData.SearchIndex;
            }
        }
    }

}
