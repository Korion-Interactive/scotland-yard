
using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Matches
{
    /// <summary>
    ///
    /// </summary>
    public enum PlayerType
    {
        /// <summary> Unset player type </summary>
        NotSet,
        /// <summary> Indicates a PlayStation Network player </summary>
        PSNPlayer,
        /// <summary> A player other than a PlayStation Network player </summary>
        NonPSNPlayer,
        /// <summary> A non-player character </summary>
        NPC,
    }

    /// <summary>
    /// Match status
    /// </summary>
    public enum MatchStatus
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> The match has yet to begin, indicating a status in which it will begin at the specified time. (Currently, matches with this status cannot be created; it is anticipated that the status will be used in the future.) </summary>
        Scheduled,  // "SCHEDULED"
        /// <summary> The match is waiting for players to participate </summary>
        Waiting,    // "WAITING"
        /// <summary> The match has begun and is currently underway  </summary>
        Playing,    // "PLAYING"
        /// <summary> The match is temporarily paused </summary>
        OnHold,     // "ONHOLD"
        /// <summary> The match has been terminated without it reporting results </summary>
        Cancelled,  // "CANCELLED"
        /// <summary> The match has concluded with results reported </summary>
        Completed,  // "COMPLETED"
    }

    /// <summary>
    /// Whether the match is a team match
    /// </summary>
    public enum MatchGroupType
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> Team match </summary>
        TeamMatch,  // "TEAM_MATCH"
        /// <summary> Not team match </summary>
        NonTeamMatch, // "NON_TEAM_MATCH"
    }

    /// <summary>
    /// Whether a match is a competitive match
    /// </summary>

    public enum MatchCompetitionType
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> Competitive </summary>
        Competitive, // "COMPETITIVE"
        /// <summary> Cooperative </summary>
        Cooperative, // "COOPERATIVE"
    }

    /// <summary>
    /// Whether there are scores in the match results
    /// </summary>
    public enum MatchResultsType
    {
        /// <summary> Not set </summary>
        NotSet = 0,
        /// <summary> If the activity has score statistics </summary>
        Score, // "SCORE"
        /// <summary> Only results are available </summary>
        Result, // "RESULT"
    }

    /// <summary>
    /// Results of cooperative play
    /// </summary>
    public enum CooperativeResults
    {
        /// <summary> Unset player type </summary>
        NotSet,
        /// <summary> Indicates a PlayStation Network player </summary>
        Success, // "SUCCESS"
        /// <summary> A player other than a PlayStation Network player </summary>
        Unfinished, // "UNFINISHED"
        /// <summary> A non-player character </summary>
        Failed, // "FAILED"
    }

    /// <summary>
    /// Details of a player in a match
    /// </summary>
    public class MatchPlayer
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// The type of player
        /// </summary>
        public PlayerType PlayerType { get; set; }

        /// <summary>
        /// Application-defined name of player
        /// </summary>
        /// <remarks>
        /// Used when displaying match results on the platform. If nothing is specified, the default name will be displayed by the system software.
        /// </remarks>
        public string PlayerName { get; set; }

        /// <summary>
        /// Account ID of the player
        /// </summary>
        public UInt64 AccountId { get; set; }

        /// <summary>
        /// Online ID of the player
        /// </summary>
        /// <remarks>
        /// If the account does not exist because it has already been deleted, all players' online IDs will be fixed as "_anonymous_user".
        /// </remarks>
        public string OnlineId { get; set; }

        /// <summary>
        /// Whether the player is currently participating in a match
        /// </summary>
        public bool Joined { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();

            PlayerType = (PlayerType)reader.ReadInt32();

            bool isPlayerNameSet = reader.ReadBoolean();
            if (isPlayerNameSet)
            {
                PlayerName = reader.ReadPrxString();
            }

            bool isAccountIdSet = reader.ReadBoolean();
            if (isAccountIdSet)
            {
                string accIdStr = reader.ReadPrxString();
                UInt64 tempId;
                UInt64.TryParse(accIdStr, out tempId);
                AccountId = tempId;
            }

            bool isOnlineIdSet = reader.ReadBoolean();
            if (isOnlineIdSet)
            {
                OnlineId = reader.ReadPrxString();
            }

            Joined = reader.ReadBoolean();
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
            writer.Write((Int32)PlayerType);

            if(PlayerName != null && PlayerName.Length > 0)
            {
                writer.Write(true);
                writer.WritePrxString(PlayerName);
            }
            else
            {
                writer.Write(false);
            }

            if(PlayerType == PlayerType.PSNPlayer)
            {
                writer.Write(true);
                writer.Write(AccountId);
            }
            else
            {
                writer.Write(false);
            }
        }
    }

    /// <summary>
    /// Details of a match team member
    /// </summary>
    public class MatchTeamMember
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Whether the player has joined a team.
        /// </summary>
        public bool Joined { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();
            Joined = reader.ReadBoolean();
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
        }
    }

    /// <summary>
    /// Details for a team in a match
    /// </summary>
    public class MatchTeam
    {
        /// <summary>
        /// Application-defined team ID
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Application-defined name of team
        /// </summary>
        /// <remarks>
        /// Used when displaying match results on the platform. If nothing is specified, the system default name will be used.
        /// </remarks>
        public string TeamName { get; set; }

        /// <summary>
        /// List of players belonging to the team
        /// </summary>
        public List<MatchTeamMember> Members { get; set; } = new List<MatchTeamMember>();

        internal void Deserialise(BinaryReader reader)
        {
            TeamId = reader.ReadPrxString();

            bool isTeamNameSet = reader.ReadBoolean();
            if (isTeamNameSet)
            {
                TeamName = reader.ReadPrxString();
            }

            bool isTeamMembersSet = reader.ReadBoolean();
            if (isTeamMembersSet)
            {
                int numMembers = reader.ReadInt32();

                for (int i = 0; i < numMembers; i++)
                {
                    MatchTeamMember tm = new MatchTeamMember();
                    tm.Deserialise(reader);
                    Members.Add(tm);
                }
            }
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(TeamId);

            if (TeamName != null && TeamName.Length > 0)
            {
                writer.Write(true);
                writer.WritePrxString(TeamName);
            }
            else
            {
                writer.Write(false);
            }

            Int32 numMembers = Members != null ? Members.Count : 0;

            writer.Write(numMembers);

            for (int i = 0; i < numMembers; i++)
            {
                Members[i].Serialise(writer);
            }
        }
    }

    /// <summary>
    /// Individual results for a player in a match
    /// </summary>
    public class MatchPlayerResult
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Value indicating the ranking of the player
        /// </summary>
        public Int32 Rank { get; set; }

        /// <summary>
        /// Score of the player. Provided if the match's <see cref="Match.ResultsType"/> is <see cref="MatchResultsType.Score"/>.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Is the score value valid
        /// </summary>
        public bool IsScoreSet { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();

            Rank = reader.ReadInt32();

            IsScoreSet = reader.ReadBoolean();
            if (IsScoreSet)
            {
                Score = reader.ReadDouble();
            }
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);

            writer.Write(Rank);

            writer.Write(IsScoreSet);
            if (IsScoreSet)
            {
                writer.Write(Score);
            }
        }
    }

    /// <summary>
    /// Individual results for a player in a team
    /// </summary>
    public class MatchTeamMemberResult
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Scores of each member who contributed to the team's overall score
        /// </summary>
        public double Score { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();
            Score = reader.ReadDouble();
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
            writer.Write(Score);
        }
    }

    /// <summary>
    /// Results for each team
    /// </summary>
    public class MatchTeamResult
    {
        /// <summary>
        /// Application-defined team ID
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Value indicating the ranking of the team
        /// </summary>
        public Int32 Rank { get; set; }

        /// <summary>
        /// Score of the team. Provided if the match's <see cref="Match.ResultsType"/> is <see cref="MatchResultsType.Score"/>.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Is the score value valid
        /// </summary>
        public bool IsScoreSet { get; set; }

        /// <summary>
        /// Results for each member of the team
        /// Used to display the scores (degree of contribution to the team) of each member of the team.
        /// </summary>
        public List<MatchTeamMemberResult> TeamMemberResults { get; set; } = new List<MatchTeamMemberResult>();

        internal void Deserialise(BinaryReader reader)
        {
            TeamId = reader.ReadPrxString();

            Rank = reader.ReadInt32();

            IsScoreSet = reader.ReadBoolean();
            if (IsScoreSet)
            {
                Score = reader.ReadDouble();
            }

            bool isTeamMemberResultsSet = reader.ReadBoolean();
            if (isTeamMemberResultsSet)
            {
                int numMembers = reader.ReadInt32();

                for (int i = 0; i < numMembers; i++)
                {
                    MatchTeamMemberResult res = new MatchTeamMemberResult();
                    res.Deserialise(reader);
                    TeamMemberResults.Add(res);
                }
            }
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(TeamId);

            writer.Write(Rank);

            writer.Write(IsScoreSet);
            if (IsScoreSet)
            {
                writer.Write(Score);
            }

            int numMembers = TeamMemberResults != null ? TeamMemberResults.Count : 0;

            writer.Write(numMembers);

            for (int i = 0; i < numMembers; i++)
            {
                TeamMemberResults[i].Serialise(writer);
            }
        }
    }

    /// <summary>
    /// The results for a match
    /// </summary>
    public class MatchResults
    {
        /// <summary>
        /// Version of the results
        /// </summary>
        public Int32 Version { get; set; } = 1;

        /// <summary>
        /// Results of cooperative play
        /// </summary>
        public CooperativeResults CooperativeResult { get; set; }

        /// <summary>
        /// Results for each player who competed
        /// Only set when the match's <see cref="Match.GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>.
        /// </summary>
        public List<MatchPlayerResult> PlayerResults { get; set; } = new List<MatchPlayerResult>();

        /// <summary>
        /// Results for each team that competed
        /// Only set when the match's <see cref="Match.GroupType"/> is <see cref="MatchGroupType.TeamMatch"/>.
        /// </summary>
        public List<MatchTeamResult> TeamResults { get; set; } = new List<MatchTeamResult>();

        /// <summary>
        /// Used by GetMatchDetailRequest as part of the RetrievedMatchDetail
        /// </summary>
        internal void Deserialise(BinaryReader reader)
        {
            Version = reader.ReadInt32();

            bool isCooperativeResultSet = reader.ReadBoolean();
            if (isCooperativeResultSet)
            {
                CooperativeResult = (CooperativeResults)reader.ReadInt32();
            }

            bool isCompetitiveResultSet = reader.ReadBoolean();
            if (isCompetitiveResultSet)
            {
                bool isPlayerResultsSet = reader.ReadBoolean();
                if (isPlayerResultsSet)
                {
                    int numPlayers = reader.ReadInt32();

                    for (int i = 0; i < numPlayers; i++)
                    {
                        MatchPlayerResult res = new MatchPlayerResult();
                        res.Deserialise(reader);
                        PlayerResults.Add(res);
                    }
                }

                bool isTeamResultsSet = reader.ReadBoolean();
                if (isTeamResultsSet)
                {
                    int numTeams = reader.ReadInt32();

                    for (int i = 0; i < numTeams; i++)
                    {
                        MatchTeamResult res = new MatchTeamResult();
                        res.Deserialise(reader);
                        TeamResults.Add(res);
                    }
                }
            }

        }

        /// <summary>
        /// Used by ReportResultsRequest
        /// </summary>
        internal void Serialise(BinaryWriter writer, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultType)
        {
            writer.Write(Version);

            if (competitionType == MatchCompetitionType.Cooperative)
            {
                writer.Write((Int32)CooperativeResult);
            }
            else if (competitionType == MatchCompetitionType.Competitive)
            {
                if (groupType == MatchGroupType.NonTeamMatch)
                {
                    int numPlayers = PlayerResults != null ? PlayerResults.Count : 0;

                    writer.Write(numPlayers);

                    for (int i = 0; i < numPlayers; i++)
                    {
                        PlayerResults[i].Serialise(writer);
                    }
                }
                else if (groupType == MatchGroupType.TeamMatch)
                {
                    int numTeams = TeamResults != null ? TeamResults.Count : 0;

                    writer.Write(numTeams);

                    for (int i = 0; i < numTeams; i++)
                    {
                        TeamResults[i].Serialise(writer);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Key/Value pair additional stats
    /// </summary>
    public class AdditonalStats
    {
        /// <summary>
        /// Additional stats of a player
        /// </summary>
        public Dictionary<string, string> StatsData { get; set; } = new Dictionary<string, string>();

        internal void Deserialise(BinaryReader reader)
        {
            int numStats = reader.ReadInt32();

            for (int i = 0; i < numStats; i++)
            {
                string key = reader.ReadPrxString();
                string value = reader.ReadPrxString();

                StatsData.Add(key, value);
            }
        }

        internal void Serialise(BinaryWriter writer)
        {
            int numStats = StatsData != null ? StatsData.Count : 0;

            writer.Write(numStats);

            foreach(var pair in StatsData)
            {
                writer.WritePrxString(pair.Key);
                writer.WritePrxString(pair.Value);
            }
        }
    }

    /// <summary>
    /// Additional stats per player
    /// </summary>
    public class MatchPlayerStats
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Additional stats of a player
        /// </summary>
        public AdditonalStats Stats { get; set; } = new AdditonalStats();

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();
            Stats.Deserialise(reader);
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
            Stats.Serialise(writer);
        }
    }

    /// <summary>
    /// Stats of each member of the team
    /// </summary>
    public class MatchTeamMemberStats
    {
        /// <summary>
        /// Application-defined player ID
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// Additional stats of a player
        /// </summary>
        public AdditonalStats Stats { get; set; } = new AdditonalStats();

        internal void Deserialise(BinaryReader reader)
        {
            PlayerId = reader.ReadPrxString();
            Stats.Deserialise(reader);
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(PlayerId);
            Stats.Serialise(writer);
        }
    }

    /// <summary>
    /// Additional stats per team
    /// </summary>
    public class MatchTeamStats
    {
        /// <summary>
        /// Application-defined team ID
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Additional stats of a team
        /// </summary>
        public AdditonalStats Stats { get; set; } = new AdditonalStats();

        /// <summary>
        /// Stats of each member of the team
        /// </summary>
        public List<MatchTeamMemberStats> TeamMemberStats { get; set; } = new List<MatchTeamMemberStats>();

        internal void Deserialise(BinaryReader reader)
        {
            TeamId = reader.ReadPrxString();
            Stats.Deserialise(reader);

            TeamMemberStats.Clear();
            int numMembers = reader.ReadInt32();

            for (int i = 0; i < numMembers; i++)
            {
                MatchTeamMemberStats stats = new MatchTeamMemberStats();
                stats.Deserialise(reader);
                TeamMemberStats.Add(stats);
            }
        }

        internal void Serialise(BinaryWriter writer, MatchGroupType groupType)
        {
            writer.WritePrxString(TeamId);

            Stats.Serialise(writer);

            int numMembers = TeamMemberStats != null ? TeamMemberStats.Count : 0;

            writer.Write(numMembers);

            for (int i = 0; i < numMembers; i++)
            {
                TeamMemberStats[i].Serialise(writer);
            }
        }
    }

    /// <summary>
    /// Additional stats of a match
    /// </summary>
    public class MatchStats
    {
        /// <summary>
        /// Additional stats per player
        /// </summary>
        public List<MatchPlayerStats> PlayerStats { get; set; } = new List<MatchPlayerStats>();

        /// <summary>
        /// Additional stats per team
        /// </summary>
        public List<MatchTeamStats> TeamStats { get; set; } = new List<MatchTeamStats>();


        internal void Deserialise(BinaryReader reader)
        {
            bool isPlayerStatsSet = reader.ReadBoolean();
            if (isPlayerStatsSet)
            {
                PlayerStats.Clear();

                // Deserialise the players list
                int numPlayers = reader.ReadInt32();

                for (int i = 0; i < numPlayers; i++)
                {
                    MatchPlayerStats stats = new MatchPlayerStats();
                    stats.Deserialise(reader);
                    PlayerStats.Add(stats);
                }
            }

            bool isTeamStatsSet = reader.ReadBoolean();
            if (isTeamStatsSet)
            {
                TeamStats.Clear();

                // Deserialise the players list
                int numTeam = reader.ReadInt32();

                for (int i = 0; i < numTeam; i++)
                {
                    MatchTeamStats stats = new MatchTeamStats();
                    stats.Deserialise(reader);
                    TeamStats.Add(stats);
                }
            }
        }

        /// <summary>
        /// Used by ReportResultsRequest
        /// </summary>
        internal void Serialise(BinaryWriter writer, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultType)
        {
            if (competitionType == MatchCompetitionType.Cooperative || (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.NonTeamMatch) )
            {
                // writer.Write((Int32)CooperativeResult);
                int numPlayers = PlayerStats != null ? PlayerStats.Count : 0;

                writer.Write(numPlayers);

                for (int i = 0; i < numPlayers; i++)
                {
                    PlayerStats[i].Serialise(writer);
                }
            }
            else if (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.TeamMatch)
            {
                int numTeams = TeamStats != null ? TeamStats.Count : 0;

                writer.Write(numTeams);

                for (int i = 0; i < numTeams; i++)
                {
                    TeamStats[i].Serialise(writer, groupType);
                }
            }
        }
    }

    /// <summary>
    /// Match instance
    /// </summary>
    public partial class Match
    {
        /// <summary>
        /// PSN Match id
        /// </summary>
        public string MatchId { get; internal set; } = "";

        /// <summary>
        /// Activity relating to a match
        /// </summary>
        public string ActivityId { get; internal set; } = "";

        /// <summary>
        /// NP service label
        /// </summary>
        public Int32 ServiceLabel { get; set; }

        /// <summary>
        /// Match status
        /// </summary>
        public MatchStatus Status { get; internal set; }

        /// <summary>
        /// ID of the zone where the match takes place
        /// </summary>
        public string ZoneId { get; internal set; }

        /// <summary>
        /// Has the player session been created and is currently valid
        /// </summary>
        public bool IsActive { get { return MatchId != null && MatchId.Length > 0; } }

        /// <summary>
        /// The match is a team match or other
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
        /// Grace period until a match is canceled
        /// </summary>
        /// <remarks>
        /// Starting when the status of the match becomes either <see cref="MatchStatus.Waiting"/> or <see cref="MatchStatus.OnHold"/>,
        /// if neither the Status nor ExpirationTime are updated by the period specified for <see cref="MatchRequests.RetrievedMatchDetail.ExpirationTime"/>, the match is automatically canceled.
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
        /// <remarks>
        /// Each <see cref="MatchPlayer.PlayerId"/> in the request must be unique.
        /// If a player's <see cref="MatchPlayer.PlayerType"/> is <see cref="PlayerType.PSNPlayer"/>, an <see cref="MatchPlayer.AccountId"/> is required (its existence will be checked for). In all other cases, specifying an accountId results in an error.
        /// More than one <see cref="MatchPlayer.PlayerId"/> can be linked to a single <see cref="MatchPlayer.AccountId"/>.
        /// </remarks>
        public List<MatchPlayer> Players { get; set; }

        /// <summary>
        /// List of teams participating in the match
        /// If the match's <see cref="GroupType"/> is <see cref="MatchGroupType.NonTeamMatch"/>, this value won't be specified.
        /// </summary>
        /// <remarks>
        /// Each <see cref="MatchTeam.TeamId"/> within the match must be unique.
        /// </remarks>
        public List<MatchTeam> Teams { get; set; }

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
        /// Only  <see cref="MatchStats.TeamStats"/> is specified if the match's <see cref="CompetitionType"/> is <see cref="MatchCompetitionType.Competitive"/> and <see cref="GroupType"/> is <see cref="MatchGroupType.TeamMatch"/>.
        /// </remarks>
        public MatchStats Stats { get; internal set; }


        internal void InitialiseCreationParams(MatchCreationParams initParams)
        {
            if (initParams == null) return;

            ActivityId = initParams.ActivityId;
            ServiceLabel = initParams.ServiceLabel;
            ZoneId = initParams.ZoneId;

            if (initParams.Players != null)
            {
                if (Players == null)
                {
                    Players = new List<MatchPlayer>();
                }
                else
                {
                    Players.Clear();
                }

                for (int i = 0; i < initParams.Players.Count; i++)
                {
                    AddPlayerFrom(initParams.Players[i]);
                }
            }

            if (initParams.Teams != null)
            {
                if (Teams == null)
                {
                    Teams = new List<MatchTeam>();
                }
                else
                {
                    Teams.Clear();
                }

                for (int i = 0; i < initParams.Teams.Count; i++)
                {
                    AddTeamFrom(initParams.Teams[i]);
                }
            }
        }

        /// <summary>
        /// Update the match details using the result retrieved about the match
        /// </summary>
        /// <param name="details">The details returned by calling <see cref="MatchRequests.GetMatchDetailRequest"/></param>

        public void UpdateDetails(MatchRequests.RetrievedMatchDetail details)
        {
            if(MatchId != details.MatchId)
            {
                return;
            }

            Status = details.Status;

            ActivityId = details.ActivityId;

            GroupType = details.GroupType;

            CompetitionType = details.CompetitionType;

            ResultsType = details.ResultsType;

            ExpirationTime = details.ExpirationTime;

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.ZoneId) != 0)
            {
                ZoneId = details.ZoneId;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.ZoneId) != 0)
            {
                ZoneId = details.ZoneId;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.StartTimeStamp) != 0)
            {
                StartTimeStamp = details.StartTimeStamp;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.EndTimeStamp) != 0)
            {
                EndTimeStamp = details.EndTimeStamp;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.LastPausedTimeStamp) != 0)
            {
                LastPausedTimeStamp = details.LastPausedTimeStamp;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.Players) != 0)
            {
                Players = details.Players;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.Teams) != 0)
            {
                Teams = details.Teams;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.Results) != 0)
            {
                Results = details.Results;
            }

            if ((details.SetFlags & MatchRequests.RetrievedMatchDetail.ParamTypes.Stats) != 0)
            {
                Stats = details.Stats;
            }
        }

        internal void AddPlayerFrom(MatchPlayerCreateParams playerParams)
        {
            MatchPlayer player = new MatchPlayer();
            Players.Add(player);

            player.PlayerId = playerParams.PlayerId;
            player.PlayerName = playerParams.PlayerName;
            player.PlayerType = playerParams.PlayerType;
            player.AccountId = playerParams.AccountId;
        }

        internal void AddTeamFrom(MatchTeamCreateParams teamParams)
        {
            MatchTeam team = new MatchTeam();
            Teams.Add(team);

            team.TeamId = teamParams.TeamId;
            team.TeamName = teamParams.TeamName;

            if (teamParams.TeamMembers != null)
            {
                for(int i = 0; i < teamParams.TeamMembers.Count; i++)
                {
                    MatchPlayerCreateParams playerParams = teamParams.TeamMembers[i];

                    MatchTeamMember member = new MatchTeamMember();
                    team.Members.Add(member);

                    member.PlayerId = playerParams.PlayerId;
                }
            }
        }

        /// <summary>
        /// Find a team using the team id
        /// </summary>
        /// <param name="teamId">The team id to find</param>
        /// <returns>The found team or null</returns>
        public MatchTeam FindTeam(string teamId)
        {
            if (Teams == null) return null;

            for (int i = 0; i < Teams.Count; i++)
            {
                if (Teams[i].TeamId == teamId)
                {
                    return Teams[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a player using the player id
        /// </summary>
        /// <param name="playerId">The player id to find</param>
        /// <returns>The found player or null</returns>

        public MatchPlayer FindPlayer(string playerId)
        {
            if (Players == null) return null;

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].PlayerId == playerId)
                {
                    return Players[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find a player using their account id
        /// </summary>
        /// <param name="accountId">The account id to find</param>
        /// <returns>The found player or null</returns>
        public MatchPlayer FindPlayer(UInt64 accountId)
        {
            if (Players == null) return null;

            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].AccountId == accountId)
                {
                    return Players[i];
                }
            }

            return null;
        }

        static internal MatchPlayer FindInList(string playerId, List<MatchPlayer> players)
        {
            if (players == null) return null;

            for(int i = 0; i < players.Count; i++)
            {
                if(players[i].PlayerId == playerId)
                {
                    return players[i];
                }
            }

            return null;
        }

        static internal Match CreateMatch(string matchId, out bool isNew)
        {
            return activeMatches.CreateMatch(matchId, out isNew);
        }

        /// <summary>
        /// Find a session instance from a session id
        /// </summary>
        /// <param name="matchId">The match id to find</param>
        /// <returns></returns>
        static public Match FindPlayerSessionFromSessionId(string matchId)
        {
            return activeMatches.FindMatchFromMatchId(matchId);
        }

        /// <summary>
        /// Find match from an account id.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        static public Match FindMatchFromAccountId(UInt64 accountId)
        {
            return activeMatches.FindMatchFromAccountId(accountId);
        }

        /// <summary>
        /// Return a copy of the list of active matches
        /// </summary>
        static public List<Match> MatchesList
        {
            get { return activeMatches.CopyOfMatchesList; }
        }

        /// <summary>
        /// Removes a match from the system so active matches are no longer tracked.
        /// </summary>
        /// <param name="match">The match to remove</param>
        static public void RemoveMatch(Match match)
        {
            activeMatches.RemoveMatch(match);
        }

        static MatchList<Match> activeMatches = new MatchList<Match>();
    }

    internal class MatchList<T> where T : Match, new()
    {
        object MatchCreationSyncObj = new object();
        List<T> activeMatches = new List<T>();

        internal T CreateMatch(string matchId, out bool isNew)
        {
            isNew = false;

            T ps = null;

            lock (MatchCreationSyncObj)
            {
                // must do this because there may have been a "psn:sessionManager:playerSession:created" event come in that may have already created the session
                ps = FindMatchFromMatchId(matchId);

                if (ps == null)
                {
                    isNew = true;
                    ps = new T();
                    ps.MatchId = matchId;

                    TrackMatch(ps);
                }
            }

            return ps;
        }

        internal void RemoveMatch(T match)
        {
            lock (MatchCreationSyncObj)
            {
                if (match != null)
                {
                    UntrackMatch(match);
                }
            }
        }

        // Only called while under the SessionCreationSyncObj lock
        private void TrackMatch(T match)
        {
            if (activeMatches.Contains(match)) return;

            activeMatches.Add(match);
        }

        // Only called while under the SessionCreationSyncObj lock
        private void UntrackMatch(T match)
        {
            if(activeMatches.Contains(match) == false) return;

            activeMatches.Remove(match);
        }

        internal T FindMatchFromMatchId(string matchId)
        {
            lock (MatchCreationSyncObj)
            {
                foreach (var match in activeMatches)
                {
                    if (match.MatchId == matchId)
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        internal T FindMatchFromAccountId(UInt64 accountId)
        {
            lock (MatchCreationSyncObj)
            {
                foreach (var match in activeMatches)
                {
                    int count = match.Players != null ? match.Players.Count : 0;

                    for(int i = 0; i < count; i++)
                    {
                        if(match.Players[i].AccountId == accountId)
                        {
                            return match;
                        }
                    }
                }
            }

            return null;
        }

        internal List<T> CopyOfMatchesList
        {
            get
            {
                lock (MatchCreationSyncObj)
                {
                    List<T> copy = new List<T>(activeMatches);
                    return copy;
                }
            }
        }
    }
}
