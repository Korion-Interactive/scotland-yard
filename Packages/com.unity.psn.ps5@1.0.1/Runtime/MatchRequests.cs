
using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Matches
{
    /// <summary>
    /// Parameters for creating a match.
    /// </summary>
    public class MatchPlayerCreateParams
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The type of player
        /// </summary>
        public PlayerType PlayerType { get; set; } = PlayerType.PSNPlayer;

        /// <summary>
        /// Application-defined name of player.
        /// Used when displaying match results on the platform. If nothing is specified, the default name will be displayed by the system software.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Account ID of the player
        /// </summary>
        public UInt64 AccountId { get; set; }

        /// <summary>
        /// MatchPlayer Constructor. Must provide a unique player id
        /// </summary>
        /// <param name="playerId">Unique player id</param>
        public MatchPlayerCreateParams(string playerId)
        {
            PlayerId = playerId;
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
            writer.Write((Int32)PlayerType);
            writer.WritePrxString(PlayerName);
            writer.Write(AccountId);
        }

        internal void SerialiseForTeam(BinaryWriter writer)
        {
            // Only write out player id for team.
            writer.WritePrxString(PlayerId);
        }
    }

    /// <summary>
    /// Parameters for creating a team in a match.
    /// </summary>
    public class MatchTeamCreateParams
    {
        /// <summary>
        /// Application-defined team ID
        /// </summary>
        public string TeamId { get; set; } = "";

        /// <summary>
        /// Application-defined name of team
        /// Used when displaying match results on the platform. If nothing is specified, the system default name will be used
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// List of players belonging to the team
        /// A player can only belong to one team at a time
        /// </summary>
        public List<MatchPlayerCreateParams> TeamMembers { get; set; }

        /// <summary>
        /// MatchTeam Constructor. Must provide a unique team id
        /// </summary>
        /// <param name="teamId">Unique team id</param>
        public MatchTeamCreateParams(string teamId)
        {
            TeamId = teamId;
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(TeamId);
            writer.WritePrxString(TeamName);

            Int32 numPlayers = TeamMembers != null ? TeamMembers.Count : 0;
            writer.Write(numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                TeamMembers[i].SerialiseForTeam(writer);
            }
        }
    }

    /// <summary>
    /// The parameters used when creating a player session
    /// </summary>
    public class MatchCreationParams
    {
        /// <summary>
        /// Activity relating to a match
        /// </summary>
        public string ActivityId { get; set; }

        /// <summary>
        /// NP service label
        /// </summary>
        public Int32 ServiceLabel { get; set; }

        /// <summary>
        /// ID of the zone where the match takes place
        /// </summary>
        public string ZoneId { get; set; }

        /// <summary>
        /// List of players participating in the match
        /// </summary>
        /// <remarks>
        /// Each <see cref="MatchPlayerCreateParams.PlayerId"/> in the request must be unique.
        /// If a player's <see cref="MatchPlayerCreateParams.PlayerType"/> is <see cref="PlayerType.PSNPlayer"/>, an <see cref="MatchPlayerCreateParams.AccountId"/> is required (its existence will be checked for). In all other cases, specifying an accountId results in an error.
        /// More than one <see cref="MatchPlayerCreateParams.PlayerId"/> can be linked to a single <see cref="MatchPlayerCreateParams.AccountId"/>.
        /// </remarks>
        public List<MatchPlayerCreateParams> Players { get; set; }

        /// <summary>
        /// List of teams participating in the match
        /// If the match's groupingType is NON_TEAM_MATCH, this value cannot be specified.
        /// </summary>
        /// <remarks>
        /// Each teamId within the request must be unique.
        /// </remarks>
        public List<MatchTeamCreateParams> Teams { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(ActivityId);
            writer.Write(ServiceLabel);
            writer.WritePrxString(ZoneId);

            Int32 numPlayers = Players != null ? Players.Count : 0;
            writer.Write(numPlayers);
            for (int i = 0; i < numPlayers; i++)
            {
                Players[i].Serialise(writer);
            }

            Int32 numTeams = Teams != null ? Teams.Count : 0;
            writer.Write(numTeams);
            for (int i = 0; i < numTeams; i++)
            {
                Teams[i].Serialise(writer);
            }
        }
    }

    /// <summary>
    /// Requests used for player sessions
    /// </summary>
    public class MatchRequests
    {
        internal enum NativeMethods : UInt32
        {
            CreateMatch = 0x0C00001u,
            GetMatchDetails = 0x0C00002u,
            UpdateMatchStatus = 0x0C00003u,
            JoinMatch = 0x0C00004u,
            LeaveMatch = 0x0C00005u,
            ReportResults = 0x0C00006u,
            UpdateDetails = 0x0C00007u,
        }

        /// <summary>
        /// Create a new player session
        /// </summary>
        public class CreateMatchRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The parameters used to create the session
            /// </summary>
            public MatchCreationParams CreationParams { get; set; }

            /// <summary>
            /// The created player session instance
            /// </summary>
            public Match Match { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.CreateMatch);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                CreationParams.Serialise(nativeMethod.Writer);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method

                    string matchId = nativeMethod.Reader.ReadPrxString();

                    bool isNewMatch = false;
                    Match = Match.CreateMatch(matchId, out isNewMatch);

                    Match.InitialiseCreationParams(CreationParams);
                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Retrieved match data when using <see cref="GetMatchDetailRequest"/>
        /// </summary>
        public class RetrievedMatchDetail
        {
            /// <summary>
            /// Which parameters should be retrieved
            /// </summary>
            [Flags]
            public enum ParamTypes
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary>  </summary>
                ZoneId = 1,                     // bit 1
                /// <summary>  </summary>
                StartTimeStamp = 2,             // bit 2
                /// <summary>  </summary>
                EndTimeStamp = 4,               // bit 3
                /// <summary>  </summary>
                LastPausedTimeStamp = 8,        // bit 4
                /// <summary>  </summary>
                Players = 16,                   // bit 5
                /// <summary>  </summary>
                Teams = 32,                     // bit 6
                /// <summary>  </summary>
                Results = 64,                   // bit 7
                /// <summary>  </summary>
                Stats = 128                     // bit 8
            }

            /// <summary>
            /// Test is a flag is set
            /// </summary>
            /// <param name="flags">The match flags to check</param>
            /// <param name="flagToCheck">The match flag to test</param>
            /// <returns>Returns true if the flag to check is set</returns>

            public static bool IsParamFlagSet(ParamTypes flags, ParamTypes flagToCheck)
            {
                if ((flags & flagToCheck) != 0) return true;

                return false;
            }

            /// <summary>
            /// The flags represent which parts of the data have been returned
            /// </summary>
            public ParamTypes SetFlags { get; internal set; }

            /// <summary>
            /// The match id
            /// </summary>
            public string MatchId { get; internal set; }

            /// <summary>
            /// Match status
            /// </summary>
            public MatchStatus Status { get; internal set; }

            /// <summary>
            /// The match Activity Id
            /// </summary>
            public string ActivityId { get; internal set; }

            /// <summary>
            /// Whether the match is a team match
            /// </summary>
            public MatchGroupType GroupType { get; internal set; }

            /// <summary>
            /// Whether a match is a competitive match
            /// </summary>
            public MatchCompetitionType CompetitionType { get; internal set; }

            /// <summary>
            /// Whether there are scores in the match results
            /// </summary>
            public MatchResultsType ResultsType { get; internal set; }

            /// <summary>
            /// ID of the zone where the match takes place
            /// </summary>
            public string ZoneId { get; internal set; }

            /// <summary>
            /// Grace period until a match is canceled
            /// </summary>
            /// <remarks>
            /// Starting when the status of the match becomes either <see cref="MatchStatus.Waiting"/> or <see cref="MatchStatus.OnHold"/>,
            /// if neither the Status nor ExpirationTime are updated by the period specified for <see cref="RetrievedMatchDetail.ExpirationTime"/>, the match is automatically canceled.
            /// Currently, matches are not automatically canceled. It is anticipated that the value of expirationTime will be used in the future.
            /// </remarks>
            public Int32 ExpirationTime { get; internal set; }

            /// <summary>
            /// Date and time the match was started (the date and time status first became <see cref="MatchStatus.Playing"/>)
            /// </summary>
            public DateTime StartTimeStamp { get; internal set; }

            /// <summary>
            /// Date and time the match concluded (the date and time status became either <see cref="MatchStatus.Cancelled"/> or <see cref="MatchStatus.Completed"/>)
            /// </summary>
            public DateTime EndTimeStamp { get; internal set; }

            /// <summary>
            /// Date and time the match was last temporarily paused (the date and time status last became <see cref="MatchStatus.OnHold"/>)
            /// </summary>
            public DateTime LastPausedTimeStamp { get; internal set; }

            /// <summary>
            /// List of players participating in the match
            /// </summary>
            public List<MatchPlayer> Players { get; internal set; } = new List<MatchPlayer>();

            /// <summary>
            /// List of teams participating in the match
            /// </summary>
            public List<MatchTeam> Teams { get; internal set; } = new List<MatchTeam>();

            /// <summary>
            /// Match results
            /// </summary>
            public MatchResults Results { get; internal set; }

            /// <summary>
            /// Additional stats of a match
            /// </summary>
            /// <remarks>
            /// Only <see cref="MatchStats.PlayerStats"/> is specified if the match's <see cref="CompetitionType"/> is <see cref="MatchCompetitionType.Cooperative"/>.
            /// Only <see cref="MatchStats.PlayerStats"/>  is specified if the match's <see cref="CompetitionType"/> is <see cref="MatchCompetitionType.Competitive"/> and <see cref="GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>.
            /// Only <see cref="MatchStats.TeamStats"/> is specified if the match's <see cref="CompetitionType"/> is <see cref="MatchCompetitionType.Competitive"/> and <see cref="GroupType"/> is <see cref="MatchGroupType.TeamMatch"/>.
            /// </remarks>
            public MatchStats Stats { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                MatchId = reader.ReadPrxString();

                Status = (MatchStatus)reader.ReadInt32();

                ActivityId = reader.ReadPrxString();

                GroupType = (MatchGroupType)reader.ReadInt32();

                CompetitionType = (MatchCompetitionType)reader.ReadInt32();

                ResultsType = (MatchResultsType)reader.ReadInt32();

                bool isZoneSet = reader.ReadBoolean();
                if (isZoneSet)
                {
                    ZoneId = reader.ReadPrxString();
                    SetFlags |= ParamTypes.ZoneId;
                }

                ExpirationTime = reader.ReadInt32();

                bool isStartTimeSet = reader.ReadBoolean();
                if (isStartTimeSet)
                {
                    StartTimeStamp = reader.ReadUnixTimestampString();
                    SetFlags |= ParamTypes.StartTimeStamp;
                }

                bool isEndTimeSet = reader.ReadBoolean();
                if (isEndTimeSet)
                {
                    EndTimeStamp = reader.ReadUnixTimestampString();
                    SetFlags |= ParamTypes.EndTimeStamp;
                }

                bool isPauseTimeSet = reader.ReadBoolean();
                if (isPauseTimeSet)
                {
                    LastPausedTimeStamp = reader.ReadUnixTimestampString();
                    SetFlags |= ParamTypes.LastPausedTimeStamp;
                }

                // Ingame roster
                bool isRosterSet = reader.ReadBoolean();
                if (isRosterSet)
                {
                    Players.Clear();
                    Teams.Clear();

                    bool isPlayersSet = reader.ReadBoolean();
                    if (isPlayersSet)
                    {
                        SetFlags |= ParamTypes.Players;

                        // Deserialise the players list
                        int numPlayers = reader.ReadInt32();

                        for (int i = 0; i < numPlayers; i++)
                        {
                            MatchPlayer mp = new MatchPlayer();
                            mp.Deserialise(reader);
                            Players.Add(mp);
                        }
                    }

                    bool isTeamsSet = reader.ReadBoolean();
                    if (isTeamsSet)
                    {
                        SetFlags |= ParamTypes.Teams;

                        // Deserialise the teams list
                        int numTeams = reader.ReadInt32();

                        for (int i = 0; i < numTeams; i++)
                        {
                            MatchTeam mt = new MatchTeam();
                            mt.Deserialise(reader);
                            Teams.Add(mt);
                        }
                    }
                }
                else
                {
                    Players.Clear();
                    Teams.Clear();
                }

                bool isMatchResultsSet = reader.ReadBoolean();
                if (isMatchResultsSet)
                {
                    SetFlags |= ParamTypes.Results;

                    Results = new MatchResults();
                    Results.Deserialise(reader);
                }

                bool isMatchStatsSet = reader.ReadBoolean();
                if (isMatchStatsSet)
                {
                    SetFlags |= ParamTypes.Stats;

                    Stats = new MatchStats();
                    Stats.Deserialise(reader);
                }

            }
        }

        /// <summary>
        /// Obtain the details of the match
        /// </summary>
        public class GetMatchDetailRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public Int32 ServiceLabel { get; set; } = 0;

            /// <summary>
            /// The retieved match data
            /// </summary>
            public RetrievedMatchDetail MatchDetail { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.GetMatchDetails);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);
                nativeMethod.Writer.Write(ServiceLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MatchDetail = new RetrievedMatchDetail();

                    MatchDetail.Deserialise(nativeMethod.Reader);
                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// The status of a match
        /// </summary>
        public enum UpdateMatchStatus
        {
            /// <summary> </summary>
            NotSet = 0,
            /// <summary> </summary>
            Playing, // "PLAYING"
            /// <summary> </summary>
            OnHold, // "ONHOLD"
            /// <summary> </summary>
            Cancelled, // "CANCELLED"
        }

        /// <summary>
        /// Request to update the status of a match
        /// </summary>
        public class UpdateMatchStatusRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// The status of a match can be updated using this request. If a match whose status is already <see cref="MatchStatus.Completed"/> or <see cref="MatchStatus.Cancelled"/> is updated, an error will occur.
            /// </summary>
            /// <remarks>
            /// The status of a match can be updated to one of the follow: <see cref="MatchStatus.Playing"/>, <see cref="MatchStatus.OnHold"/> or <see cref="MatchStatus.Cancelled"/>
            ///
            /// <see cref="UpdateMatchStatus.Playing"/>
            /// Because the status of a match immediately after it is created is <see cref="MatchStatus.Waiting"/>, update it to <see cref="MatchStatus.Playing"/> status when gameplay commences.
            /// Changing to <see cref="MatchStatus.Playing"/> status indicates that the match has started or resumed. Therefore, the following checks are performed.
            ///    - At least one player must be in the match
            ///    - If the <see cref="Match.GroupType"/> is <see cref="MatchGroupType.TeamMatch"/>, at least one team must be in the match
            /// <see cref="Match.StartTimeStamp"/> is recorded the first time the status of a match becomes <see cref="MatchStatus.Playing"/>.
            ///
            /// <see cref="UpdateMatchStatus.OnHold"/>
            /// Changing to <see cref="MatchStatus.OnHold"/> status means temporarily pausing the match. Matches in <see cref="MatchStatus.OnHold"/> status may be automatically cancelled.
            /// <see cref="Match.LastPausedTimeStamp"/> is recorded every time the status of a match becomes <see cref="MatchStatus.OnHold"/>.
            ///
            /// <see cref="UpdateMatchStatus.Cancelled"/>
            /// Changing to <see cref="MatchStatus.Cancelled"/> status means canceling the match. <see cref="Match.EndTimeStamp"/> is recorded the first time the status of a match becomes <see cref="MatchStatus.Cancelled"/>.
            ///
            /// Match Validity Period
            /// If a match is left alone for more than a certain amount of time, it will be automatically transitioned to <see cref="MatchStatus.Cancelled"/> status.
            /// (*Currently, matches are not automatically canceled. A validity period feature for matches is expected to be implemented in the future.)
            ///     If <see cref="MatchStatus.Playing"/>: If left for 24 hours from the date and time when the match last entered <see cref="MatchStatus.Playing"/> status
            ///     If <see cref="MatchStatus.Waiting"/>: If left from the date and time when the match first entered <see cref="MatchStatus.Waiting"/> status until the <see cref="Match.ExpirationTime"/> has elapsed
            ///     If <see cref="MatchStatus.OnHold"/>: If left from the date and time when the match last entered  <see cref="MatchStatus.OnHold"/> status until the <see cref="Match.ExpirationTime"/> has elapsed
            /// However, if the status changes or expirationTime is updated while <see cref="MatchStatus.Waiting"/> or <see cref="MatchStatus.OnHold"/>, the timer will be reset, and a new countdown will begin.
            ///
            /// </remarks>
            public UpdateMatchStatus UpdateStatus { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.UpdateMatchStatus);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);

                nativeMethod.Writer.Write((Int32)UpdateStatus);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Update match details
        /// </summary>
        public class UpdateMatchDetailRequest : Request
        {
            /// <summary>
            /// Which parameters need to be updated
            /// </summary>
            [Flags]
            public enum ParamTypes
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary>  </summary>
                ZoneId = 1,                     // bit 1
                /// <summary>  </summary>
                ServiceLabel = 2,               // bit 2
                /// <summary>  </summary>
                ExpirationTime = 4,             // bit 3
                /// <summary>  </summary>
                Players = 16,                   // bit 5
                /// <summary>  </summary>
                Teams = 32,                     // bit 6
                /// <summary>  </summary>
                Results = 64,                   // bit 7
                /// <summary>  </summary>
                Stats = 128                     // bit 8
            }

            /// <summary>
            /// The flags represent which parts of the data have been set
            /// </summary>
            public ParamTypes SetFlags { get; set; }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public Int32 ServiceLabel { get; set; }

            /// <summary>
            /// ID of the zone where the match takes place
            /// </summary>
            public string ZoneId { get; set; }

            /// <summary>
            /// Grace period until a match is canceled
            /// </summary>
            /// <remarks>
            /// Starting when the status of the match becomes either <see cref="MatchStatus.Waiting"/> or <see cref="MatchStatus.OnHold"/>,
            /// If neither the Status nor ExpirationTime are updated by the period specified for <see cref="Match.ExpirationTime"/>, the match is automatically canceled.
            /// Currently, matches are not automatically canceled. It is anticipated that the value of expirationTime will be used in the future.
            /// </remarks>
            public Int32 ExpirationTime { get; set; }

            /// <summary>
            /// List of players participating in the match
            /// </summary>
            public List<MatchPlayer> Players { get; set; }

            /// <summary>
            /// List of teams participating in the match
            /// </summary>
            public List<MatchTeam> Teams { get; set; }

            /// <summary>
            /// Whether the match is a team match
            /// </summary>
            public MatchGroupType GroupType { get; set; }

            /// <summary>
            /// Whether there are scores in the match results
            /// </summary>
            public MatchResultsType ResultType { get; set; }

            /// <summary>
            /// Interim Match Results. Can only be set if <see cref="Match.CompetitionType"/> is <see cref="MatchCompetitionType.Competitive"/>
            /// </summary>
            public MatchResults Results { get; set; }

            /// <summary>
            /// Additional stats of a match. All additional stats reported previously will be overwritten
            /// </summary>
            /// <remarks>
            /// Only <see cref="MatchStats.PlayerStats"/> is specified if the match's <see cref="Match.CompetitionType"/> is is <see cref="MatchCompetitionType.Cooperative"/>.
            /// Only <see cref="MatchStats.PlayerStats"/> is specified if the match's <see cref="Match.CompetitionType"/> is is <see cref="MatchCompetitionType.Competitive"/> and <see cref="Match.GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>.
            /// Only <see cref="MatchStats.TeamStats"/> is specified if the match's <see cref="Match.CompetitionType"/> is is <see cref="MatchCompetitionType.Competitive"/> and <see cref="Match.GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>.
            /// </remarks>
            public MatchStats Stats { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.UpdateDetails);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);

                if ((SetFlags & ParamTypes.ServiceLabel) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((SetFlags & ParamTypes.ZoneId) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(ZoneId);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((SetFlags & ParamTypes.ExpirationTime) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ExpirationTime);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((SetFlags & ParamTypes.Players) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    Int32 numPlayers = Players != null ? Players.Count : 0;

                    nativeMethod.Writer.Write(numPlayers);

                    for (int i = 0; i < numPlayers; i++)
                    {
                        Players[i].Serialise(nativeMethod.Writer);
                    }
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((SetFlags & ParamTypes.Teams) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    Int32 numTeams = Teams != null ? Teams.Count : 0;

                    nativeMethod.Writer.Write(numTeams);

                    for (int i = 0; i < numTeams; i++)
                    {
                        Teams[i].Serialise(nativeMethod.Writer);
                    }
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Writer.Write((Int32)GroupType);
                nativeMethod.Writer.Write((Int32)ResultType);

                if ((SetFlags & ParamTypes.Results) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    Results.Serialise(nativeMethod.Writer, MatchCompetitionType.Competitive, GroupType, ResultType);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if ((SetFlags & ParamTypes.Stats) != 0)
                {
                    nativeMethod.Writer.Write(true);
                    Stats.Serialise(nativeMethod.Writer, MatchCompetitionType.Competitive, GroupType, ResultType);
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
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Player details to make join a match
        /// </summary>
        public class JoinPlayer
        {
            /// <summary>
            /// Set which parameters are valid
            /// </summary>
            [Flags]
            public enum ParamTypes
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary> The session id is used </summary>
                PlayerName = 1,                          // bit 1
                /// <summary> Date and time of creation of the Player Session </summary>
                AccountId = 2,                   // bit 2
                /// <summary> Maximum number of members who can join a Player Session as players </summary>
                TeamId = 4,                         // bit 3
            }

            /// <summary>
            /// Application-defined player ID
            /// </summary>
            public string PlayerId { get; set; }

            /// <summary>
            /// The type of player
            /// </summary>
            public PlayerType PlayerType { get; set; } = PlayerType.PSNPlayer;

            /// <summary>
            /// Application-defined name of player.
            /// Used when displaying match results on the platform. If nothing is specified, the default name will be displayed by the system software.
            /// </summary>
            public string PlayerName { get; set; }

            /// <summary>
            /// Account ID of the player
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Application-defined team ID
            /// </summary>
            public string TeamId { get; set; }

            /// <summary>
            /// Which of the parameters are valid
            /// </summary>
            public ParamTypes ValidParams { get; set; }

            internal void Serialise(BinaryWriter writer)
            {
                writer.WritePrxString(PlayerId);
                writer.Write((Int32)PlayerType);

                if ((ValidParams & ParamTypes.PlayerName) != 0)
                {
                    writer.Write(true);
                    writer.WritePrxString(PlayerName);
                }
                else
                {
                    writer.Write(false);
                }

                if ((ValidParams & ParamTypes.AccountId) != 0)
                {
                    writer.Write(true);
                    writer.Write(AccountId);
                }
                else
                {
                    writer.Write(false);
                }

                if ((ValidParams & ParamTypes.TeamId) != 0)
                {
                    writer.Write(true);
                    writer.WritePrxString(TeamId);
                }
                else
                {
                    writer.Write(false);
                }
            }
        }


        /// <summary>
        /// Make players join a match
        /// </summary>
        public class JoinMatchRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// List of players to make join
            /// </summary>
            /// <remarks>
            /// Each playerId in the request must be unique.
            /// If a player's <see cref="JoinPlayer.PlayerType"/> is <see cref="PlayerType.PSNPlayer"/>, an <see cref="JoinPlayer.AccountId"/> is required (its existence will be checked for).
            /// In all other cases, specifying an <see cref="JoinPlayer.AccountId"/> results in an error. More than one <see cref="JoinPlayer.PlayerId"/> can be linked to a single <see cref="JoinPlayer.AccountId"/>.
            /// If a <see cref="JoinPlayer.TeamId"/> is specified, the player will move to that team. If a <see cref="JoinPlayer.TeamId"/> that does not exist is specified, an error will occur.
            /// If the match's <see cref="Match.GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>, a <see cref="JoinPlayer.TeamId"/> cannot be specified.
            /// A player can only belong to a single team at once
            /// </remarks>
            public List<JoinPlayer> Players { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.JoinMatch);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);

                Int32 numPlayers = Players != null ? Players.Count : 0;

                nativeMethod.Writer.Write(numPlayers);

                for (int i = 0; i < numPlayers; i++)
                {
                    Players[i].Serialise(nativeMethod.Writer);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Player details to leave a match
        /// </summary>
        public class LeavePlayer
        {
            /// <summary>
            /// Reasons a player leaves a match
            /// </summary>
            public enum Reasons
            {
                /// <summary> </summary>
                NotSet = 0,
                /// <summary> Use if a network problem resulted in the player leaving </summary>
                Disconnected, // "DISCONNECTED"
                /// <summary> Use if the player left in accordance with the rules of the game </summary>
                Finished, // "FINISHED"
                /// <summary> Use if the player left the game in the middle of a match by his or her choice </summary>
                Quit, // "QUIT"
            }

            /// <summary>
            /// Application-defined player ID
            /// </summary>
            public string PlayerId { get; set; }

            /// <summary>
            /// Reason player leaving match
            /// </summary>
            public Reasons Reason { get; set; } = Reasons.NotSet;

            internal void Serialise(BinaryWriter writer)
            {
                writer.WritePrxString(PlayerId);
                writer.Write((Int32)Reason);
            }
        }

        /// <summary>
        /// Make a player leave a match
        /// </summary>
        public class LeaveMatchRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// List of players to make leave the match
            /// </summary>
            /// <remarks>
            /// You can make up to 100 players leave a match at once
            /// </remarks>
            public List<LeavePlayer> Players { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.LeaveMatch);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);

                Int32 numPlayers = Players != null ? Players.Count : 0;

                nativeMethod.Writer.Write(numPlayers);

                for (int i = 0; i < numPlayers; i++)
                {
                    Players[i].Serialise(nativeMethod.Writer);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Setting of whether a match participant can write a review of other participants
        /// </summary>
        public enum PlayerReviewEligibility
        {
            /// <summary> Invalid </summary>
            NotSet = 0,
            /// <summary> Enable reviews to be written about all the participants </summary>
            Enabled = 1,
            /// <summary> Disable reviews to be written about all the participants </summary>
            Disabled = 2,
        }

        /// <summary>
        /// Report the results of a match
        /// </summary>
        public class ReportResultsRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Match ID
            /// </summary>
            public string MatchID { get; set; }

            /// <summary>
            /// Match Competition type
            /// </summary>
            public MatchCompetitionType CompetitionType { get; set; }

            /// <summary>
            /// Whether the match is a team match
            /// </summary>
            public MatchGroupType GroupType { get; set; }

            /// <summary>
            /// Whether there are scores in the match results
            /// </summary>
            public MatchResultsType ResultType { get; set; }

            /// <summary>
            /// Setting of whether a match participant can write a review of other participants
            /// </summary>
            public PlayerReviewEligibility ReviewEligibility { get; set; } = PlayerReviewEligibility.Disabled;

            /// <summary>
            /// Match Results
            /// </summary>
            public MatchResults Results { get; set; }

            /// <summary>
            /// Additional stats of a match
            /// </summary>
            /// <remarks>
            /// Only playerStatistics is specified if the match's <see cref="CompetitionType"/> is COOPERATIVE.
            /// Only playerStatistics is specified if the match's <see cref="CompetitionType"/> is COMPETITIVE and groupingType is NON_TEAM_MATCH.
            /// Only teamStatistics is specified if the match's <see cref="CompetitionType"/> is COMPETITIVE and groupingType is TEAM_MATCH.
            /// </remarks>
            public MatchStats Stats { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)MatchRequests.NativeMethods.ReportResults);

                nativeMethod.Writer.Write(UserId);

                // Write the setup params
                nativeMethod.Writer.WritePrxString(MatchID);

                nativeMethod.Writer.Write((Int32)CompetitionType);
                nativeMethod.Writer.Write((Int32)GroupType);
                nativeMethod.Writer.Write((Int32)ResultType);
                nativeMethod.Writer.Write((Int32)ReviewEligibility);

                // Serialise results
                Results.Serialise(nativeMethod.Writer, CompetitionType, GroupType, ResultType);

                // Serialise stats
                Stats.Serialise(nativeMethod.Writer, CompetitionType, GroupType, ResultType);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }
                else
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }

}
