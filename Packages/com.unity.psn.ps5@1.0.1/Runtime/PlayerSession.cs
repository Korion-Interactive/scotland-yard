
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
    /// Users who can join Player Sessions without invitations
    /// </summary>
    public enum JoinableUserTypes
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> No one can join without an invitation </summary>
        NoOne,
        /// <summary> Friends of the leader can join without invitations </summary>
        Friends,
        /// <summary> Friends of friends of the leader can join without invitations </summary>
        FriendsOfFriends,
        /// <summary> Anyone can join without an invitation </summary>
        Anyone,
        /// <summary> Users who have been registered on the <see cref="PlayerSession.JoinableSpecifiedUsers"/> list can join without invitations </summary>
        SpecifiedUsers,
    }

    /// <summary>
    /// Information about who can send invitations to a Player Session
    /// </summary>
    public enum InvitableUserTypes
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> Noone can send invites </summary>
        NoOne,
        /// <summary> On the leader can send invites </summary>
        Leader,
        /// <summary> Any member can send invites </summary>
        Member,
    }

    /// <summary>
    /// Information about the platforms with which users can join a Player Session
    /// </summary>
    [Flags]
    public enum SessionPlatforms
    {
        /// <summary> PS5 </summary>
        PS5 = 1,
        /// <summary> PS4 </summary>
        PS4 = 2,
    }

    /// <summary>
    /// Localised text for session names
    /// </summary>
    public class LocalisedText
    {
        /// <summary>
        /// The language locale to use
        /// </summary>
        public string Locale { get; set; } = "en-US";

        /// <summary>
        /// The localised test
        /// </summary>
        public string Text { get; set; } = "";

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(Locale);
            writer.WritePrxString(Text);
        }

        internal void Deserialise(BinaryReader reader)
        {
            Locale = reader.ReadPrxString();
            Text = reader.ReadPrxString();
        }
    }

    /// <summary>
    /// Localised session name
    /// </summary>
    public class LocalisedSessionNames
    {
        /// <summary>
        /// The default language locale to use
        /// </summary>
        public string DefaultLocale { get; set; } = "en-US";

        /// <summary>
        /// The set of localised session names
        /// </summary>
        public List<LocalisedText> LocalisedNames { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(DefaultLocale);

            int nameCount = LocalisedNames != null ? LocalisedNames.Count : 0;

            writer.Write(nameCount);

            for(int i = 0; i < nameCount; i++)
            {
                LocalisedNames[i].Serialise(writer);
            }
        }

        internal void Deserialise(BinaryReader reader)
        {
            DefaultLocale = reader.ReadPrxString();

            UInt32 count = reader.ReadUInt32();

            if (count > 0 && LocalisedNames == null) LocalisedNames = new List<LocalisedText>();

            if (LocalisedNames != null) LocalisedNames.Clear();

            for (int i = 0; i < count; i++)
            {
                LocalisedText text = new LocalisedText();
                text.Deserialise(reader);
                LocalisedNames.Add(text);
            }
        }
    }

    /// <summary>
    /// Information about the privileges possessed by the leader of the Player Session
    /// </summary>
    [Flags]
    public enum LeaderPrivilegeFlags
    {
        /// <summary> No privileges set </summary>
        NotSet = 0,
        /// <summary> The leader can kick a member </summary>
        Kick = 1,
        /// <summary> The leader can update who can join Player Sessions without invitations</summary>
        UpdateJoinableUserType = 2,
        /// <summary> The leader can update who can send invitations to a Player Session</summary>
        UpdateInvitableUerType = 4,
        /// <summary> The leader can nominate a new Session leader</summary>
        PromoteToLeader = 8
    }

    /// <summary>
    /// List of the users who can join without invitations when <see cref="PlayerSession.JoinableUserType"/> is <see cref="JoinableUserTypes.SpecifiedUsers"/>
    /// </summary>
    public class JoinableSpecifiedUsers
    {
        /// <summary>
        /// The account ids who can join the session
        /// </summary>
        public List<UInt64> AccountIds { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            int idCount = AccountIds != null ? AccountIds.Count : 0;

            writer.Write(idCount);

            for (int i = 0; i < idCount; i++)
            {
                writer.Write(AccountIds[i]);
            }
        }
    }

    /// <summary>
    /// Player session instance
    /// </summary>
    public partial class PlayerSession : Session
    {
        /// <summary>
        /// Param types used when retrieving player session data or certain types of session notifications
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
            /// <summary> Custom data 1 for a members participating as players.</summary>
            MemberPlayersCustomData1 = 128,         // bit 8
            /// <summary> Custom data 1 for members participating as spectators.</summary>
            MemberSpectatorsCustomData1 = 256,      // bit 9
            /// <summary> Flag for temporarily prohibiting joining </summary>
            JoinDisabled = 512,                     // bit 10
            /// <summary> Platforms that can join </summary>
            SupportedPlatforms = 1024,              // bit 11
            /// <summary> Name of the Player Session </summary>
            SessionName = 2048,                     // bit 12
            /// <summary> Name of the Player Session (in all languages) </summary>
            LocalizedSessionName = 4096,            // bit 13
            /// <summary> The leader </summary>
            Leader = 8192,                          // bit 14
            /// <summary> Users who can join without invitations </summary>
            JoinableUserType = 16384,               // bit 15
            /// <summary> List of the users who can join without invitations when joinableUserType is SPECIFIED_USERS </summary>
            JoinableSpecifiedUsers = 32768,         // bit 16
            /// <summary> Members who can send invitations </summary>
            InvitableUserType = 65536,              // bit 17
            /// <summary> Permissions had by the leader </summary>
            LeaderPrivileges = 131072,              // bit 18
            /// <summary> Privileges that the leader does not have </summary>
            ExclusiveLeaderPrivileges = 262144,     // bit 19
            /// <summary> Privileges that are hidden from the system UI for the leader </summary>
            DisableSystemUiMenu = 524288,           // bit 20
            /// <summary> Custom data 1 </summary>
            CustomData1 = 1048576,                  // bit 21
            /// <summary> Custom data 2 </summary>
            CustomData2 = 2097152,                  // bit 22
            /// <summary> Flag that indicates whether swapping is supported </summary>
            SwapSupported = 4194304,                // bit 23
            /// <summary> A typical set of flags when retrieving player sesssion data </summary>
            Default = SessionId | CreatedTimeStamp | MaxPlayers | MaxSpectators | Member | SupportedPlatforms | Leader | CustomData1 | SwapSupported,
            /// <summary> All the flag set </summary>
            All = Default | MemberPlayers | MemberSpectators | MemberPlayersCustomData1 | MemberSpectatorsCustomData1 | JoinDisabled | SessionName | LocalizedSessionName | JoinableUserType | JoinableSpecifiedUsers | InvitableUserType | LeaderPrivileges | ExclusiveLeaderPrivileges | DisableSystemUiMenu | CustomData2
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
                "member(players(customData1))",     // MemberPlayersCustomData1     // bit 8
                "member(spectators(customData1))",  // MemberSpectatorsCustomData1  // bit 9
                "joinDisabled",                     // JoinDisabled                 // bit 10
                "supportedPlatforms",               // SupportedPlatforms           // bit 11
                "sessionName",                      // SessionName                  // bit 12
                "localizedSessionName",             // LocalizedSessionName         // bit 13
                "leader",                           // Leader                       // bit 14
                "joinableUserType",                 // JoinableUserType             // bit 15
                "joinableSpecifiedUsers",           // JoinableSpecifiedUsers       // bit 16
                "invitableUserType",                // InvitableUserType            // bit 17
                "leaderPrivileges",                 // LeaderPrivileges             // bit 18
                "exclusiveLeaderPrivileges",        // ExclusiveLeaderPrivileges    // bit 19
                "disableSystemUiMenu",              // DisableSystemUiMenu          // bit 20
                "customData1",                      // CustomData1                  // bit 21
                "customData2",                      // CustomData2                  // bit 22
                "swapSupported",                    // SwapSupported                // bit 23
        };

        /// <summary>
        /// Has the player session been created and is currently valid
        /// </summary>
        public bool IsActive { get { return SessionId != null && SessionId.Length > 0; } }

        /// <summary>
        /// Flag indicating whether members who have joined can switch from players to spectators, or from spectators to players, without leaving.
        /// When true, swapping is allowed. (Users can swap using the system software UI.)When false, swapping is not allowed
        /// </summary>
        public bool SwapSupported { get; internal set; } = false;

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
        public JoinableUserTypes JoinableUserType { get; internal set; } = JoinableUserTypes.NoOne;


        /// <summary>
        /// List of account ids allowed to joing the session. Only used when <see cref="JoinableUserType"/> is <see cref="JoinableUserTypes.SpecifiedUsers"/>
        /// </summary>
        public UInt64[] JoinableSpecifiedUsers { get; internal set; }

        /// <summary>
        /// Information about who can send invitations to a Player Session
        /// </summary>
        /// <remarks>
        /// <see cref="InvitableUserTypes.NoOne"/> : No user can send an invite
        /// <see cref="InvitableUserTypes.Leader"/> : Only the leader can send invites
        /// <see cref="InvitableUserTypes.Member"/> : Any member can send invites
        /// </remarks>
        public InvitableUserTypes InvitableUserType { get; internal set; } = InvitableUserTypes.NoOne;

        /// <summary>
        /// The name of the session
        /// </summary>
        public string SessionName { get; internal set; } = "";

        /// <summary>
        /// Localisaed session names
        /// </summary>
        public LocalisedSessionNames LocalisedNames { get; internal set; } = new LocalisedSessionNames();

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
        /// Current leader of the session as a session memeber
        /// </summary>
        public SessionMember Leader
        {
            get
            {
                bool isSpectator;
                SessionMember member = FindFromAccountId(LeaderId, out isSpectator);
                return member;
            }
        }

        /// <summary>
        /// The account id of the leader.
        /// This is provided for backwards compatibility. It is equivalent to <c>LeaderId.accountId</c>.
        /// </summary>
        public UInt64 LeaderAccountId => LeaderId.accountId;

        public SessionMemberIdentifier LeaderId { get; internal set; }

        static internal SessionPlatforms FromString(string platforms)
        {
            SessionPlatforms flags = 0;

            if (platforms.Contains("PS5") || platforms.Contains("PROSPERO"))
            {
                flags |= SessionPlatforms.PS5;
            }
            if (platforms.Contains("PS4"))
            {
                flags |= SessionPlatforms.PS4;
            }

            return flags;
        }

        internal void Initialise(PlayerSessionCallbacks callbacks, WebApiPushEvent pushEvent)
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

        internal void InitialiseCreationParams(PlayerSessionCreationParams initParams)
        {
            if (initParams == null) return;

            MaxPlayers = initParams.MaxPlayers;
            MaxSpectators = initParams.MaxSpectators;
            SwapSupported = initParams.SwapSupported;
            JoinableUserType = initParams.JoinableUserType;
            InvitableUserType = initParams.InvitableUserType;
            SupportedPlatforms = initParams.SupportedPlatforms;
            LocalisedNames = initParams.LocalisedNames;
            LeaderPrivileges = initParams.LeaderPrivileges;
            ExclusiveLeaderPrivileges = initParams.ExclusiveLeaderPrivileges;
            DisableSystemUiMenu = initParams.DisableSystemUiMenu;
            CustomData1 = initParams.CustomData1;
            CustomData2 = initParams.CustomData2;
        }

        internal void SessionDeleted()
        {
         //   Debug.LogError("SessionDeleted");
            CleanupPlayerSessionRequest request = new CleanupPlayerSessionRequest() { Session = this };
            var requestOp = new AsyncRequest<CleanupPlayerSessionRequest>(request);
            SessionsManager.Schedule(requestOp);

            // Mark up everything that needs to happen to cleanup the session
            SessionsManager.DestroyPlayerSession(this);
        }

        internal class CleanupPlayerSessionRequest : Request
        {
            public PlayerSession Session  { get; set; }

            protected internal override void Run()
            {
                Session.CleanupSessionBlocking();
            }
        }

        static internal void SerialisePrivliges(BinaryWriter writer, LeaderPrivilegeFlags flags, bool isExclusive)
        {
            // Count the flags set
            int count = 0;
            if ((flags & LeaderPrivilegeFlags.Kick) != 0) count++;
            if ((flags & LeaderPrivilegeFlags.UpdateJoinableUserType) != 0) count++;
            if (isExclusive == true && (flags & LeaderPrivilegeFlags.UpdateInvitableUerType) != 0) count++;
            if (isExclusive == true && (flags & LeaderPrivilegeFlags.PromoteToLeader) != 0) count++;

            writer.Write(count);

            if ((flags & LeaderPrivilegeFlags.Kick) != 0)
            {
                writer.WritePrxString("KICK");
            }
            if ((flags & LeaderPrivilegeFlags.UpdateJoinableUserType) != 0)
            {
                writer.WritePrxString("UPDATE_JOINABLE_USER_TYPE");
            }
            if (isExclusive == true && (flags & LeaderPrivilegeFlags.UpdateInvitableUerType) != 0)
            {
                writer.WritePrxString("UPDATE_INVITABLE_USER_TYPE");
            }
            if (isExclusive == true && (flags & LeaderPrivilegeFlags.PromoteToLeader) != 0)
            {
                writer.WritePrxString("PROMOTE_TO_LEADER");
            }
        }

        static internal LeaderPrivilegeFlags DeserialisePrivliges(BinaryReader reader)
        {
            LeaderPrivilegeFlags flags = 0;

            UInt32 count = reader.ReadUInt32();

            for(int i = 0; i < count; i++)
            {
                string priv = reader.ReadPrxString();

                if(priv == "KICK")
                {
                    flags |= LeaderPrivilegeFlags.Kick;
                }

                if (priv == "UPDATE_JOINABLE_USER_TYPE")
                {
                    flags |= LeaderPrivilegeFlags.UpdateJoinableUserType;
                }

                if (priv == "UPDATE_INVITABLE_USER_TYPE")
                {
                    flags |= LeaderPrivilegeFlags.UpdateInvitableUerType;
                }

                if (priv == "PROMOTE_TO_LEADER")
                {
                    flags |= LeaderPrivilegeFlags.PromoteToLeader;
                }
            }

            return flags;
        }

        internal void UpdateSessionMemberLeader()
        {
            // Make sure all members have their leader flag reset
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].IsLeader = false;
            }

            for (int i = 0; i < Spectators.Count; i++)
            {
                Spectators[i].IsLeader = false;
            }

            // Get leader, if the account id actually exists in the member lists
            SessionMember leader = Leader;

            // The leader might not exist. If this is the case then the leader flag will be updated if the member
            // if added to the list at some point in the future
            if (leader != null)
            {
                leader.IsLeader = true;
            }
        }

        internal void UpdateLeader(SessionMemberIdentifier id)
        {
            // update internal account id reference to leader
            LeaderId = id;
            UpdateSessionMemberLeader();
        }

        /// <summary>
        ///  Update a player session from data retrieved from <see cref="PlayerSessionRequests.GetPlayerSessionsRequest"/>
        /// </summary>
        /// <param name="sessionData">The data to update</param>
        public void UpdateFrom(PlayerSessionRequests.RetrievedSessionData sessionData)
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
                //TODO
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

            if ((sessionData.SetFlags & ParamTypes.SessionName) != 0)
            {
                SessionName = sessionData.SessionName;
            }

            if ((sessionData.SetFlags & ParamTypes.LocalizedSessionName) != 0)
            {
                LocalisedNames = sessionData.LocalisedNames;
            }

            if ((sessionData.SetFlags & ParamTypes.Leader) != 0)
            {
                UpdateLeader(new SessionMemberIdentifier (sessionData.LeaderAccountId, sessionData.LeaderPlatform));
            }

            if ((sessionData.SetFlags & ParamTypes.JoinDisabled) != 0)
            {
                JoinDisabled = sessionData.JoinDisabled;
            }

            if ((sessionData.SetFlags & ParamTypes.JoinableUserType) != 0)
            {
                JoinableUserType = sessionData.JoinableUserType;
            }

            if ((sessionData.SetFlags & ParamTypes.JoinableSpecifiedUsers) != 0)
            {
                JoinableSpecifiedUsers = sessionData.JoinableSpecifiedUsers;
            }

            if ((sessionData.SetFlags & ParamTypes.InvitableUserType) != 0)
            {
                InvitableUserType = sessionData.InvitableUserType;
            }

            if ((sessionData.SetFlags & ParamTypes.LeaderPrivileges) != 0)
            {
                LeaderPrivileges = sessionData.LeaderPrivileges;
            }

            if ((sessionData.SetFlags & ParamTypes.ExclusiveLeaderPrivileges) != 0)
            {
                ExclusiveLeaderPrivileges = sessionData.ExclusiveLeaderPrivileges;
            }

            if ((sessionData.SetFlags & ParamTypes.CustomData1) != 0)
            {
                CustomData1 = sessionData.CustomData1;
            }

            if ((sessionData.SetFlags & ParamTypes.CustomData2) != 0)
            {
                CustomData2 = sessionData.CustomData2;
            }

            if ((sessionData.SetFlags & ParamTypes.SwapSupported) != 0)
            {
                SwapSupported = sessionData.SwapSupported;
            }
        }

        internal void UpdateFrom(SessionMember[] members)
        {
            for(int i = 0; i < members.Length; i++)
            {
                AddorUpdateMember(members[i]);
            }
        }

        override internal SessionMember AddorUpdateMember(SessionMember member)
        {
            SessionMember addedMember = base.AddorUpdateMember(member);

            if (addedMember.AccountId != SessionMember.InvalidAccountId && addedMember.AccountId == LeaderAccountId && addedMember.IsLeader == false)
            {
                UpdateSessionMemberLeader();
            }

            return addedMember;
        }

    }
}
