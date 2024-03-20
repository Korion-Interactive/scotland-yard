    
using System;
using System.IO;
using System.Collections.Generic;

using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using UnityEngine;
using Unity.PSN.PS5.WebApi;

namespace Unity.PSN.PS5.Sessions
{
    static class SessionFieldFlagsSerialiser
    {
        static public void SerialiseFieldFlags(UInt32 flags, string[] fieldNames, BinaryWriter writer)
        {
            UInt32 count = 0;

            // Doesn't need to test bits, just count how many times the number can be ANDed with itself minus one.
            // 10100 (2 bits)
            // 10100 & 10011 = 10000
            // 10000 & 01111 = 00000
            uint bits = (uint)flags;
            while (bits != 0)
            {
                bits &= (bits - 1);
                count++;
            }

            writer.Write(count);

            // Now go through each flag and write out the text for it, if it is set
            bits = (uint)flags;
            int index = 0;
            while (bits != 0)
            {
                // Test lowest bit
                if ((bits & 1) != 0)
                {
                    if (index < fieldNames.Length)
                    {
                        writer.WritePrxString(fieldNames[index]);
                    }
                    else
                    {
                        writer.WritePrxString("");
                    }
                }

                bits = bits >> 1;
                index++;
            }
        }
    }

    internal class SessionEventsList
    {
        internal Dictionary<int, WebApiPushEvent> sessionPushEvents = new Dictionary<int, WebApiPushEvent>();

        internal bool AddPushEvent(WebApiPushEvent pushEvent)
        {
            if (sessionPushEvents.ContainsKey(pushEvent.UserId)) return false;

            sessionPushEvents.Add(pushEvent.UserId, pushEvent);

            return true;
        }

        internal APIResult DeletePlayerPushEventBlocking(int userId)
        {
            WebApiPushEvent playerSessionEvent = null;

            APIResult result = new APIResult();

            if (sessionPushEvents.TryGetValue(userId, out playerSessionEvent) == false)
            {
                return result;
            }

            result = WebApiNotifications.UnregisterPushEventCall(playerSessionEvent);

            if (result.apiResult == APIResultTypes.Success)
            {
                sessionPushEvents.Remove(userId);
            }

            return result;
        }

        internal void RemoveAllPushEvents()
        {
            foreach (var pushEvent in sessionPushEvents.Values)
            {
                APIResult result = WebApiNotifications.UnregisterPushEventCall(pushEvent);
            }

            sessionPushEvents.Clear();
        }

        internal WebApiPushEvent FindPushEvent(int userId)
        {
            WebApiPushEvent playerSessionEvent = null;

            sessionPushEvents.TryGetValue(userId, out playerSessionEvent);

            return playerSessionEvent;
        }
    }

    /// <summary>
    /// Join states of the members who have joined the Game Session
    /// </summary>
    public enum GameSessionJoinState   // C++ InitialJoinState
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> Participating in the Game Session </summary>
        Joined = 1,
        /// <summary> Scheduled to join the Game Session </summary>
        Reserved = 2,
    }

    /// <summary>
    /// The information required to uniquely identify a participant in a session,
    /// for example to send them an in-session message.
    ///
    /// This corresponds to the native class MemberWithMultiPlatform from the CppWebApi.
    /// </summary>
    ///
    public struct SessionMemberIdentifier : IEquatable<SessionMemberIdentifier>
    {
        public UInt64 accountId;
        public SessionPlatforms platform;

        // ^ This is the actual data.  Everything from here down is convenience / boilerplate.

        /// <summary>
        /// Convenience property to access the current platform as a SessionPlatforms.
        /// </summary>
        public static SessionPlatforms CurrentPlatform =>
#if UNITY_PS5
            SessionPlatforms.PS5;
#elif UNITY_PS4
            SessionPlatforms.PS4;
#else
            throw new InvalidOperationException("Queried for current platform on neither PS5 nor PS4.");
#endif

        public static SessionMemberIdentifier WithCurrentPlatform (UInt64 accountId)
        {
            return new SessionMemberIdentifier(accountId, CurrentPlatform);
        }

        public SessionMemberIdentifier (UInt64 accountId, SessionPlatforms platform)
        {
            this.accountId = accountId;
            this.platform = platform;
        }

        public bool Equals (SessionMemberIdentifier other) => accountId == other.accountId && platform == other.platform;

        public override bool Equals (object o) => (o is SessionMemberIdentifier id) && this.Equals (id);

        public static bool operator == (SessionMemberIdentifier left, SessionMemberIdentifier right)
        {
            return left.Equals (right);
        }
        public static bool operator != (SessionMemberIdentifier left, SessionMemberIdentifier right)
        {
            return !(left == right);
        }
        public override int GetHashCode() => HashCode.Combine (accountId, platform);
    }

    /// <summary>
    /// Member in a player session
    /// </summary>
    public class SessionMember
    {
        /// <summary>
        /// The default user id representing an invalid user
        /// </summary>
        public const Int32 InvalidUserId = -1;

        /// <summary>
        /// The default account id representing an invalid user
        /// </summary>
        public const UInt64 InvalidAccountId = 0;

        /// <summary>
        /// The user id of the session member if they are local to the PlayStation system or <see cref="InvalidUserId"/>
        /// </summary>
        public Int32 UserId { get; internal set; } = InvalidUserId;

        /// <summary>
        /// The account id of the session member or <see cref="InvalidAccountId"/> if not set.
        /// </summary>
        public UInt64 AccountId { get; internal set; }


        /// <summary>
        /// The combination of the accountId and the platform.
        /// This is unique to any one SessionMember within a session.
        /// </summary>
        public SessionMemberIdentifier AccountIdAndPlatform => new SessionMemberIdentifier () { accountId = AccountId, platform = Platform };

        /// <summary>
        /// The online id of the member
        /// </summary>
        public string OnlineId { get; internal set; } = "";

        /// <summary>
        /// The platform of the member.
        /// </summary>
        public SessionPlatforms Platform { get; internal set; } = 0;

        /// <summary>
        /// Date and time when the member joined the session
        /// </summary>
        public DateTime JoinTimestamp { get; internal set; } = DateTime.MinValue;

        /// <summary>
        /// Is the member a spectator. If false then they are a player
        /// </summary>
        public bool IsSpectator { get; internal set; } = false;

        /// <summary>
        /// Is the member the leader of a session
        /// </summary>
        public bool IsLeader { get; internal set; } = false;

        /// <summary>
        /// Is the member a local player
        /// </summary>
        public bool IsLocal { get { return UserId != InvalidUserId; } }

        /// <summary>
        /// Game Session join status of a member. Joined state or Reserved state. Only valid for members in a game session.
        /// </summary>
        [Obsolete("Use JoinState instead")]
        public bool IsReserved
        {
            get { return JoinState == GameSessionJoinState.Reserved; }
            set
            {
                if (value == true) JoinState = GameSessionJoinState.Reserved;
                else JoinState = GameSessionJoinState.Joined;
            }
        }

        /// <summary>
        /// Join states of the members who have joined the Game Session. Only value for members in a game session.
        /// </summary>
        public GameSessionJoinState JoinState { get; set; } = GameSessionJoinState.NotSet;

        /// <summary>
        /// The custom data for the member
        /// </summary>
        public byte[] CustomData1 { get; internal set; } = null;

        /// <summary>
        /// Has the custom data been set
        /// </summary>
        internal bool HasCustomData { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            IsSpectator = reader.ReadBoolean();
            AccountId = reader.ReadUInt64();
            OnlineId = reader.ReadPrxString();
            Platform = (SessionPlatforms)reader.ReadUInt32();
            JoinTimestamp = reader.ReadUnixTimestampString();
        }

        internal void DeserialiseJoinState(BinaryReader reader)
        {
            JoinState = (GameSessionJoinState)reader.ReadInt32();
        }

        internal void DeserialiseCustomData(BinaryReader reader)
        {
            CustomData1 = reader.ReadData();
            HasCustomData = true;
        }

        internal void DeserialiseBasicInfo(BinaryReader reader, bool isSpectator)
        {
            IsSpectator = isSpectator;
            AccountId = reader.ReadUInt64();
            Platform = (SessionPlatforms)reader.ReadUInt32();
        }

        internal void UpdateFrom(NotificationMember memberUpdate, bool isSpectator)
        {
            IsSpectator = isSpectator;
            AccountId = memberUpdate.accountId;
            Platform = PlayerSession.FromString(memberUpdate.platform);
            OnlineId = memberUpdate.onlineId;

            if (memberUpdate.customProperties.joinTimestamp != null)
            {
                UInt64 unixMs = 0;

                if (UInt64.TryParse(memberUpdate.customProperties.joinTimestamp, out unixMs) == true)
                {
                    System.DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    JoinTimestamp = unixEpoch.AddMilliseconds(unixMs);
                }
            }
        }

        internal void UpdateFrom(SessionMember member)
        {
            // Some parameters of this class may not be set. It depends on what sort of  data
            // was recieved from PSN. Only some of the data might be availble.
            // Therefore only some of the data should be copied.

            // The Spectator flag 'must' be set if they are a spectator.

            // IsLeader flag is controlled by the session itself, don't copy this.

            IsSpectator = member.IsSpectator;

            // Update the existing member.
            if (member.UserId != InvalidUserId) UserId = UserId;
            if (member.AccountId != InvalidAccountId) AccountId = member.AccountId;
            if (member.OnlineId != null && member.OnlineId.Length > 0) OnlineId = member.OnlineId;
            if (member.Platform != 0) Platform = member.Platform;
            if (member.JoinTimestamp != DateTime.MinValue) JoinTimestamp = member.JoinTimestamp;
            if (member.JoinState != GameSessionJoinState.NotSet) JoinState = member.JoinState;

            if (member.HasCustomData == true)
            {
                CustomData1 = member.CustomData1;
                HasCustomData = true;
            }
        }
    }

    /// <summary>
    /// Base session class for Player and Game sessions
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Delegate for notifications about premium events.
        /// </summary>
        /// <param name="eventData">The notification event data</param>
        public delegate void RawSessionEventHandler(Session session, WebApiNotifications.CallbackParams eventData);

        /// <summary>
        /// Callback containing the raw notification data if additional processing of the raw data is required.
        /// </summary>
        public RawSessionEventHandler OnRawEvent { get; set; }

        /// <summary>
        /// PSN session id
        /// </summary>
        public string SessionId { get; internal set; } = "";

        /// <summary>
        /// Time the session was created
        /// </summary>
        public DateTime CreatedTimestamp { get; internal set; }

        /// <summary>
        /// Maximum number of supported players. Min 1, Max 100
        /// </summary>
        public UInt32 MaxPlayers { get; internal set; } = 1; // Min 1, Max 100

        /// <summary>
        /// Maximum number of supported spectators. Min 0, Max 50
        /// </summary>
        public UInt32 MaxSpectators { get; internal set; } = 0; // Min , Max 50

        /// <summary>
        /// Flag for temporarily prohibiting joining a Player Session or Game Session.
        /// </summary>
        /// <remarks>
        /// For a Player session: When true, the Player Session cannot be joined. When false, users who have received invitation notifications or
        /// users who can join without such notifications because of the value set for <see cref="PlayerSession.JoinableUserType"/> can join the Player Session.
        /// For a game session: Flag for temporarily prohibiting joining a Game Session. When true, the Game Session cannot be joined.
        /// </remarks>
        public bool JoinDisabled { get; internal set; } = false;

        /// <summary>
        /// Information about the platforms with which users can join a Player or Game Session
        /// </summary>
        public SessionPlatforms SupportedPlatforms { get; internal set; } = SessionPlatforms.PS5;

        /// <summary>
        /// List of Session players
        /// </summary>
        public List<SessionMember> Players { get; internal set; } = new List<SessionMember>();

        /// <summary>
        /// List of Session spectators
        /// </summary>
        public List<SessionMember> Spectators { get; internal set; } = new List<SessionMember>();

        /// <summary>
        /// Custom binary session data
        /// </summary>
        public byte[] CustomData1 { get; internal set; }

        /// <summary>
        /// Custom binary session data
        /// </summary>
        public byte[] CustomData2 { get; internal set; }

        /// <summary>
        /// Find a session member from a local user id
        /// </summary>
        /// <param name="userId">The local user id</param>
        /// <returns>The found session member or null</returns>
        public SessionMember FindFromUserId(Int32 userId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].UserId == userId)
                {
                    return Players[i];
                }
            }

            for (int i = 0; i < Spectators.Count; i++)
            {
                if (Spectators[i].UserId == userId)
                {
                    return Spectators[i];
                }
            }

            return null;
        }

        // This is called by the the requests to create or join a player session or game session.
        // (i.e. all four of CreatePlayerSessionRequest, JoinPlayerSessionRequest, CreateGameSessionRequest, JoinGameSessionRequest)
        //
        // In each of those cases the code on the native side of the package
        // writes the accountId of the user who made the request, found using sceNpGetAccountIdA.
        //
        // By matching that accountId to one of the session members of the recently created/joined session,
        // we might be able to fill in their userId.
        internal void UpdateUserId(SessionMemberIdentifier id, Int32 userId)
        {
            bool isSpectator = false;
            SessionMember sm = FindFromAccountId(id, out isSpectator);

            if (sm != null)
            {
                sm.UserId = userId;
            }
            else
            {
#if DEBUG
                Debug.LogError($"PlayerSession.UpdateUserId failed to find member with accountId {id.accountId} and platform {id.platform}");
#endif
            }
        }

        /// <summary>
        /// Find a session member from an account id
        /// </summary>
        /// <param name="accountId">The account id to find</param>
        /// <param name="isSpectator">Set true is the member is in the spectators list.</param>
        /// <returns>The found session member or null</returns>
        public SessionMember FindFromAccountId(SessionMemberIdentifier id, out bool isSpectator)
        {
            isSpectator = false;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].AccountIdAndPlatform == id)
                {
                    return Players[i];
                }
            }

            for (int i = 0; i < Spectators.Count; i++)
            {
                if (Spectators[i].AccountIdAndPlatform == id)
                {
                    isSpectator = true;
                    return Spectators[i];
                }
            }

            return null;
        }

        internal void MoveMember(SessionMember member)
        {
            // The flag on the member will show which list the member should be in.
            // Make sure it is removed from the 'other' list and added to the correct one.

            // Check to make sure the member is in the correct list
            if (member.IsSpectator == true)
            {
                // Member is a spectator

                // Remove the member from the players list if it is in there
                if (Players.Contains(member) == true)
                {
                    // Remove it
                    Players.Remove(member);
                }

                // Add to spectators list if its not already in there
                if (Spectators.Contains(member) == false)
                {
                    // Add
                    Spectators.Add(member);
                }
            }
            else
            {
                // Member is a player

                // Remove the member from the spectators list if it is in there
                if (Spectators.Contains(member) == true)
                {
                    // Remove it
                    Spectators.Remove(member);
                }

                // Add to Players list if its not already in there
                if (Players.Contains(member) == false)
                {
                    // Add
                    Players.Add(member);
                }
            }
        }

        virtual internal SessionMember AddorUpdateMember(SessionMember member)
        {
            // Member might already be added to session. Check their account id.
            // Member might be new, or may have been a spectator and is not a player.
            bool isSpectator;
            SessionMember existingMember = FindFromAccountId(member.AccountIdAndPlatform, out isSpectator);

            if (existingMember == null)
            {
                // A new member is being added
                // Add the member to the correct group
                if (member.IsSpectator == true)
                {
                    Spectators.Add(member);
                }
                else
                {
                    Players.Add(member);
                }

                return member;
                //if (member.AccountId != SessionMember.InvalidAccountId && member.AccountId == LeaderAccountId)
                //{
                //    // Leader may have just be added to the list.
                //    UpdateSessionMemberLeader();
                //}
            }
            else
            {
                existingMember.UpdateFrom(member);
                // See if member needs to move from player to spectator or the other way around.
                // The members spectator flag may have changed.
                MoveMember(existingMember);

                //if (existingMember.AccountId != SessionMember.InvalidAccountId && existingMember.AccountId == LeaderAccountId && existingMember.IsLeader == false)
                //{
                //    // Leader may have just be added to the list.
                //    UpdateSessionMemberLeader();
                //}
                return existingMember;
            }
        }

        internal SessionMember RemoveMember(SessionMemberIdentifier id)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].AccountIdAndPlatform == id)
                {
                    SessionMember found = Players[i];
                    Players.RemoveAt(i);
                    return found;
                }
            }

            for (int i = 0; i < Spectators.Count; i++)
            {
                if (Spectators[i].AccountIdAndPlatform == id)
                {
                    SessionMember found = Spectators[i];
                    Spectators.RemoveAt(i);
                    return found;
                }
            }

            return null;
        }

        internal SessionEventsList UserPushEvents = new SessionEventsList();

        public WebApiPushEvent FindPushEvent(int userId)
        {
            if (UserPushEvents != null)
            {
                return UserPushEvents.FindPushEvent(userId);
            }

            return null;
        }

        public void DeletePushEvent(int userId)
        {
            if (UserPushEvents != null)
            {
                UserPushEvents.DeletePlayerPushEventBlocking(userId);
            }
        }

        internal void CleanupSessionBlocking()
        {
            if (UserPushEvents != null)
            {
                UserPushEvents.RemoveAllPushEvents();
            }
        }
    }
}
