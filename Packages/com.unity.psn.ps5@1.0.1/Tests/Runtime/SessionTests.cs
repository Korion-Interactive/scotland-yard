#if UNITY_PS5
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Matches;
using Unity.PSN.PS5.Sessions;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Session tests")]
    public class SessionTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");
            return op.IsCompleted;
        }

        public void OnPlayerSessionUpdated(PlayerSession.Notification notificationData)
        {

        }

        public void OnGameSessionUpdated(GameSession.Notification notificationData)
        {

        }

        PlayerSession currentPlayerSession = null;
        GameSession currentGameSession = null;
        Match currentMatch = null;

        [UnityTest, Order(10), Description("Create Player Session")]
        public IEnumerator CreatePlayerSession()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = CreatePlayerSessionTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            var psr = request as PlayerSessionRequests.CreatePlayerSessionRequest;

            Assert.IsNotNull(psr, "Request is not a CreatePlayerSessionRequest type");

            currentPlayerSession = psr.Session;

            Assert.IsNotNull(currentPlayerSession, "Player session object is null");
        }

        [UnityTest, Order(20), Description("Join Player Session")]
        public IEnumerator JoinPlayerSession()
        {
            Assert.IsNotNull(currentPlayerSession, "Player session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = JoinPlayerSessionTest(secondUserId, currentPlayerSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentPlayerSession.Players.Count == 2, "There must be 2 players in the session");
        }

        [UnityTest, Order(30), Description("Leave Player Session")]
        public IEnumerator LeavePlayerSession()
        {
            Assert.IsNotNull(currentPlayerSession, "Player session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = LeavePlayerSessionTest(secondUserId, currentPlayerSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentPlayerSession.Players.Count == 1, "There must be 1 players in the session");
        }

        [UnityTest, Order(40), Description("Send Player Session Invitation")]
        public IEnumerator SendPlayerSessionInvitation()
        {
            Assert.IsNotNull(currentPlayerSession, "Player session object is null");

            var mainUserId = GetMainUserId();
            var secondAccountId = GetAccountId(1);

            Request request;

            AsyncOp op = SendPlayerSessionInviteTest(mainUserId, secondAccountId, currentPlayerSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(50), Description("Get Player Session Invitations")]
        public IEnumerator GetPlayerSessionInvitations()
        {
            Assert.IsNotNull(currentPlayerSession, "Player session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = GetInvitationsRequest(secondUserId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            var gpsir = request as PlayerSessionRequests.GetPlayerSessionInvitationsRequest;

            Assert.IsNotNull(gpsir, "Request is not a SendPlayerSessionInvitationsRequest type");

            Assert.IsNotNull(gpsir.Invitations, "Request does not contain any invitations");

            Assert.IsNotNull(gpsir.Invitations.Count > 0, "Request does not contain at least one invitation");
        }

        [UnityTest, Order(60), Description("Rejoin Player Session")]
        public IEnumerator RejoinPlayerSession()
        {
            Assert.IsNotNull(currentPlayerSession, "Player session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = JoinPlayerSessionTest(secondUserId, currentPlayerSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentPlayerSession.Players.Count == 2, "There must be 2 players in the session");
        }

        [UnityTest, Order(110), Description("Create Game Session")]
        public IEnumerator CreateGameSession()
        {
            var userId = GetMainUserId();
            var secondUserId = GetUserId(1);

            Request request;
            AsyncOp op = CreateGameSessionTest(userId, secondUserId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            var gsr = request as GameSessionRequests.CreateGameSessionRequest;

            Assert.IsNotNull(gsr, "Request is not a CreateGameSessionRequest type");

            currentGameSession = gsr.Session;

            Assert.IsNotNull(currentGameSession, "Game session object is null");

            Debug.Log("Number players = " + currentGameSession.Players.Count);
            Debug.Log("Number spectators = " + currentGameSession.Spectators.Count);
        }

        [UnityTest, Order(120), Description("Join Game Session")]
        public IEnumerator JoinGameSession()
        {
            Assert.IsNotNull(currentGameSession, "Game session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = JoinGameSessionTest(secondUserId, currentGameSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentGameSession.Players.Count == 2, "There must be 2 players in the session");
        }

        [UnityTest, Order(130), Description("Leave Game Session")]
        public IEnumerator LeaveGameSession()
        {
            Assert.IsNotNull(currentPlayerSession, "Game session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = LeaveGameSessionTest(secondUserId, currentGameSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentGameSession.Players.Count == 1, "There must be 1 players in the session");
        }

        [UnityTest, Order(140), Description("Join Game Session")]
        public IEnumerator RejoinGameSession()
        {
            Assert.IsNotNull(currentGameSession, "Game session object is null");

            var secondUserId = GetUserId(1);

            Request request;

            AsyncOp op = JoinGameSessionTest(secondUserId, currentGameSession.SessionId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            Assert.IsTrue(currentGameSession.Players.Count == 2, "There must be 2 players in the session");
        }

        [UnityTest, Order(210), Description("Create Match")]
        public IEnumerator CreateMatch()
        {
            Assert.IsNotNull(currentGameSession, "Game session object is null");

            var userId = GetMainUserId();

            Request request;

            AsyncOp op = CreateMatchTest(userId, currentGameSession, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            var mr = request as MatchRequests.CreateMatchRequest;

            Assert.IsNotNull(mr, "Request is not a CreateMatchRequest type");

            currentMatch = mr.Match;

            Assert.IsNotNull(currentMatch, "Match object is null");

            Debug.Log("Number players = " + currentMatch.Players.Count);

            Assert.IsTrue(currentMatch.Players.Count == 2, "There must be 2 players in the session");
        }

        [UnityTest, Order(220), Description("Update Match Status")]
        public IEnumerator UpdateMatchStatus()
        {
            Assert.IsNotNull(currentMatch, "Match object is null");

            var userId = GetMainUserId();

            Request request;

            AsyncOp op = UpdateMatchStatusTest(userId, currentMatch.MatchId, MatchRequests.UpdateMatchStatus.Playing, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");


            op = GetMatchDetailTest(userId, currentMatch.MatchId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            var matchDetails = request as MatchRequests.GetMatchDetailRequest;

            Assert.IsNotNull(matchDetails, "Request is not a GetMatchDetailRequest type");

            Assert.IsTrue(matchDetails.MatchDetail.Status == MatchStatus.Playing, "Match status should be 'Playing'");

            // This must be done to update all the values in the Match object so Match stats can be reported correctly.
            // Otherwise the system won't know the type or group of match that has been created.
            currentMatch.UpdateDetails(matchDetails.MatchDetail);
        }

        [UnityTest, Order(230), Description("Update Match Scores")]
        public IEnumerator UpdateMatchScores()
        {
            Assert.IsNotNull(currentMatch, "Match object is null");

            var userId = GetMainUserId();

            Request request;

            AsyncOp op = UpdateMatchResultsTest(userId, currentMatch, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(240), Description("Report Match Scores")]
        public IEnumerator ReportFinalMatchScores()
        {
            Assert.IsNotNull(currentMatch, "Match object is null");

            var userId = GetMainUserId();

            Request request;

            AsyncOp op = ReportMatchResultsTest(userId, currentMatch, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        AsyncOp UpdateMatchStatusTest(Int32 userId, string matchId, MatchRequests.UpdateMatchStatus status, out Request testRequest)
        {
            MatchRequests.UpdateMatchStatusRequest request = new MatchRequests.UpdateMatchStatusRequest()
            {
                UserId = userId,
                MatchID = matchId,
                UpdateStatus = status
            };

            var requestOp = new AsyncRequest<MatchRequests.UpdateMatchStatusRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp GetMatchDetailTest(Int32 userId, string matchId, out Request testRequest)
        {
            MatchRequests.GetMatchDetailRequest request = new MatchRequests.GetMatchDetailRequest()
            {
                UserId = userId,
                MatchID = matchId,
            };

            var requestOp = new AsyncRequest<MatchRequests.GetMatchDetailRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp UpdateMatchResultsTest(Int32 userId, Match match, out Request testRequest)
        {
            var results = GenerateRandomResults(match, match.CompetitionType, match.GroupType, match.ResultsType);

            var stats = GenerateRandomStats(match, match.CompetitionType, match.GroupType, match.ResultsType);

            MatchRequests.UpdateMatchDetailRequest request = new MatchRequests.UpdateMatchDetailRequest()
            {
                UserId = userId,
                MatchID = match.MatchId,
                SetFlags = MatchRequests.UpdateMatchDetailRequest.ParamTypes.Results | MatchRequests.UpdateMatchDetailRequest.ParamTypes.Stats,
                GroupType = match.GroupType,
                ResultType = match.ResultsType,
                Results = results,
                Stats = stats
            };

            var requestOp = new AsyncRequest<MatchRequests.UpdateMatchDetailRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp ReportMatchResultsTest(Int32 userId, Match match, out Request testRequest)
        {
            var results = GenerateRandomResults(match, match.CompetitionType, match.GroupType, match.ResultsType);

            var stats = GenerateRandomStats(match, match.CompetitionType, match.GroupType, match.ResultsType);

            MatchRequests.ReportResultsRequest request = new MatchRequests.ReportResultsRequest()
            {
                UserId = userId,
                MatchID = match.MatchId,
                ReviewEligibility = MatchRequests.PlayerReviewEligibility.Enabled, // Not used for SDK 7.0 and above
                CompetitionType = match.CompetitionType,
                GroupType = match.GroupType,
                ResultType = match.ResultsType,
                Results = results,
                Stats = stats
            };

            var requestOp = new AsyncRequest<MatchRequests.ReportResultsRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp CreatePlayerSessionTest(int userId, out Request testRequest)
        {
            LocalisedSessionNames sessionNames = new LocalisedSessionNames()
            {
                DefaultLocale = "en-US",
                LocalisedNames = new List<LocalisedText>()
                    {
                        new LocalisedText() { Locale = "en-US", Text = "Unity Session Name" },
                        new LocalisedText() { Locale = "ja-JP", Text = "Japanese のセッション名" },
                    }
            };

            PlayerSessionCreationParams sessionParams = new PlayerSessionCreationParams()
            {
                MaxPlayers = 16,
                MaxSpectators = 5,
                SwapSupported = false,
                //  JoinableUserType = JoinableUserTypes.SpecifiedUsers,
                JoinableUserType = JoinableUserTypes.Anyone,
                InvitableUserType = InvitableUserTypes.Member,
                SupportedPlatforms = SessionPlatforms.PS5 | SessionPlatforms.PS4,
                LocalisedNames = sessionNames,
                //LeaderPrivileges = LeaderPrivilegeFlags.Kick,
                //ExclusiveLeaderPrivileges = LeaderPrivilegeFlags.UpdateJoinableUserType | LeaderPrivilegeFlags.UpdateInvitableUerType,
                //DisableSystemUiMenu = LeaderPrivilegeFlags.PromoteToLeader | LeaderPrivilegeFlags.Kick,
                Callbacks = new PlayerSessionCallbacks()
                {
                    OnSessionUpdated = OnPlayerSessionUpdated//,
                   // WebApiNotificationCallback = RawSessionEventHandler
                }
            };

            PlayerSessionRequests.CreatePlayerSessionRequest request = new PlayerSessionRequests.CreatePlayerSessionRequest()
            {
                UserId = userId,
                CreationParams = sessionParams
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.CreatePlayerSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }
  
        public AsyncOp JoinPlayerSessionTest(int userId, string sessionId, out Request testRequest)
        {
            PlayerSessionRequests.JoinPlayerSessionRequest request = new PlayerSessionRequests.JoinPlayerSessionRequest()
            {
                UserId = userId,
                SessionId = sessionId,
                JoinAsSpectator = false,
                Callbacks = new PlayerSessionCallbacks()
                {
                    OnSessionUpdated = OnPlayerSessionUpdated
                }
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.JoinPlayerSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp LeavePlayerSessionTest(Int32 userId, string sessionId, out Request testRequest)
        {
            PlayerSessionRequests.LeavePlayerSessionRequest request = new PlayerSessionRequests.LeavePlayerSessionRequest()
            {
                UserId = userId,
                SessionId = sessionId,
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.LeavePlayerSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp SendPlayerSessionInviteTest(Int32 userId, UInt64 toInviteAccountId, string sessionId, out Request testRequest)
        {
            List<UInt64> inviteIds = new List<UInt64>();
            inviteIds.Add(toInviteAccountId);

            PlayerSessionRequests.SendPlayerSessionInvitationsRequest request = new PlayerSessionRequests.SendPlayerSessionInvitationsRequest()
            {
                UserId = userId,
                SessionId = sessionId,
                AccountIds = inviteIds
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.SendPlayerSessionInvitationsRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp GetInvitationsRequest(Int32 userId, out Request testRequest)
        {
            PlayerSessionRequests.GetPlayerSessionInvitationsRequest request = new PlayerSessionRequests.GetPlayerSessionInvitationsRequest()
            {
                UserId = userId,
                RequiredFields = PlayerSessionRequests.RetrievedInvitation.ParamTypes.Default,
                Filter = PlayerSessionRequests.GetPlayerSessionInvitationsRequest.RetrievalFilters.ValidOnly
            };

            var requestOp = new AsyncRequest<PlayerSessionRequests.GetPlayerSessionInvitationsRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp CreateGameSessionTest(int userId, int secondUserId, out Request testRequest)
        {
            GameSessionCreationParams sessionParams = new GameSessionCreationParams()
            {
                MaxPlayers = 32,
                MaxSpectators = 10,
                SupportedPlatforms = SessionPlatforms.PS5 | SessionPlatforms.PS4,
                JoinDisabled = false,
                UsePlayerSession = true,
                ReservationTimeoutSeconds = 400,

                SearchIndex = "GameSessionSearchIndex",
                Searchable = true,
                Callbacks = new GameSessionCallbacks()
                {
                    OnSessionUpdated = OnGameSessionUpdated
                }

            };

            sessionParams.SearchAttributes = new SearchAttributesType();

            sessionParams.SearchAttributes.ints[0] = 9;
            sessionParams.SearchAttributes.bools[0] = true;
            sessionParams.SearchAttributes.strings[0] = "myfirststring";

            GameSessionRequests.CreateGameSessionRequest request = new GameSessionRequests.CreateGameSessionRequest()
            {
                UserId = userId,
                CreationParams = sessionParams
            };

            var requestOp = new AsyncRequest<GameSessionRequests.CreateGameSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp JoinGameSessionTest(Int32 userId, string sessionId, out Request testRequest)
        {
            GameSessionRequests.JoinGameSessionRequest request = new GameSessionRequests.JoinGameSessionRequest()
            {
                UserId = userId,
                SessionId = sessionId,
                JoinAsSpectator = false,
                Callbacks = new GameSessionCallbacks()
                {
                    OnSessionUpdated = OnGameSessionUpdated,
                }
            };

            var requestOp = new AsyncRequest<GameSessionRequests.JoinGameSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        AsyncOp LeaveGameSessionTest(Int32 userId, string sessionId, out Request testRequest)
        {
            GameSessionRequests.LeaveGameSessionRequest request = new GameSessionRequests.LeaveGameSessionRequest()
            {
                UserId = userId,
                SessionId = sessionId,
            };

            var requestOp = new AsyncRequest<GameSessionRequests.LeaveGameSessionRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp CreateMatchTest(int userId, GameSession gs, out Request testRequest)
        {
            List<MatchPlayerCreateParams> players = new List<MatchPlayerCreateParams>();

            for (int i = 0; i < gs.Players.Count; i++)
            {
                var gsPlayer = gs.Players[i];

                MatchPlayerCreateParams newPlayer = new MatchPlayerCreateParams(i.ToString());

                newPlayer.PlayerName = gsPlayer.OnlineId;
                newPlayer.PlayerType = PlayerType.PSNPlayer;
                newPlayer.AccountId = gsPlayer.AccountId;

                players.Add(newPlayer);
            }

            MatchCreationParams matchParams = new MatchCreationParams()
            {
                ActivityId = "MyPVPMatch",
                Players = players
            };

            MatchRequests.CreateMatchRequest request = new MatchRequests.CreateMatchRequest()
            {
                UserId = userId,
                CreationParams = matchParams
            };

            var requestOp = new AsyncRequest<MatchRequests.CreateMatchRequest>(request);

            SessionsManager.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }




        List<MatchPlayerResult> GenerateRandomPlayerResults(Match match, MatchResultsType resultsType)
        {
            List<MatchPlayerResult> playerResults = new List<MatchPlayerResult>();

            int count = match.Players != null ? match.Players.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var player = match.Players[i];

                MatchPlayerResult playerResult = new MatchPlayerResult();
                playerResult.PlayerId = player.PlayerId;
                playerResult.Rank = i + 1;

                if (resultsType == MatchResultsType.Score)
                {
                    playerResult.Score = 100 - i;
                    playerResult.IsScoreSet = true;
                }

                playerResults.Add(playerResult);
            }

            return playerResults;
        }

        List<MatchTeamMemberResult> GenerateRandomMatchTeamMemberResults(MatchTeam team, MatchResultsType resultsType, out double teamTotalScore)
        {
            teamTotalScore = 0.0;

            List<MatchTeamMemberResult> teamMemberResults = new List<MatchTeamMemberResult>();

            int count = team.Members != null ? team.Members.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var teamMember = team.Members[i];

                MatchTeamMemberResult memberResult = new MatchTeamMemberResult();

                memberResult.PlayerId = teamMember.PlayerId;
                memberResult.Score = 100 - i;

                teamMemberResults.Add(memberResult);

                teamTotalScore += memberResult.Score;
            }

            return teamMemberResults;
        }

        List<MatchTeamResult> GenerateRandomMatchTeamResults(Match match, MatchResultsType resultsType)
        {
            List<MatchTeamResult> teamResults = new List<MatchTeamResult>();

            int count = match.Teams != null ? match.Teams.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var team = match.Teams[i];

                MatchTeamResult teamResult = new MatchTeamResult();

                teamResult.TeamId = team.TeamId;
                teamResult.Rank = i + 1;

                double teamTotalScore = 0.0;
                teamResult.TeamMemberResults = GenerateRandomMatchTeamMemberResults(team, resultsType, out teamTotalScore);

                if (resultsType == MatchResultsType.Score)
                {
                    teamResult.Score = teamTotalScore;
                    teamResult.IsScoreSet = true;
                }

                teamResults.Add(teamResult);
            }

            return teamResults;
        }

        MatchResults GenerateRandomResults(Match match, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultsType)
        {
            MatchResults newResults = new MatchResults();

            if (competitionType == MatchCompetitionType.Cooperative)
            {
                newResults.CooperativeResult = CooperativeResults.Success;
            }
            else if (competitionType == MatchCompetitionType.Competitive)
            {
                if (groupType == MatchGroupType.NonTeamMatch)
                {
                    newResults.PlayerResults = GenerateRandomPlayerResults(match, resultsType);
                }
                else if (groupType == MatchGroupType.TeamMatch)
                {
                    newResults.TeamResults = GenerateRandomMatchTeamResults(match, resultsType);
                }
            }

            return newResults;
        }

        List<MatchPlayerStats> GeneratePlayerStats(Match match, MatchResultsType resultsType)
        {
            List<MatchPlayerStats> playerStats = new List<MatchPlayerStats>();

            int count = match.Players != null ? match.Players.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var player = match.Players[i];

                MatchPlayerStats playerStat = new MatchPlayerStats();
                playerStat.PlayerId = player.PlayerId;

                int kills = i * 2;
                int deaths = i;

                playerStat.Stats.StatsData.Add("Kills", kills.ToString());
                playerStat.Stats.StatsData.Add("Deaths", deaths.ToString());

                playerStats.Add(playerStat);
            }

            return playerStats;
        }

        List<MatchTeamMemberStats> GenerateRandomMatchTeamMemberStats(MatchTeam team)
        {
            List<MatchTeamMemberStats> teamMemberStats = new List<MatchTeamMemberStats>();

            int count = team.Members != null ? team.Members.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var teamMember = team.Members[i];

                MatchTeamMemberStats memberstats = new MatchTeamMemberStats();

                memberstats.PlayerId = teamMember.PlayerId;

                int kills = i * 2;
                int deaths = i;

                memberstats.Stats.StatsData.Add("Kills", kills.ToString());
                memberstats.Stats.StatsData.Add("Deaths", deaths.ToString());

                teamMemberStats.Add(memberstats);
            }

            return teamMemberStats;
        }

        List<MatchTeamStats> GenerateTeamStats(Match match, MatchResultsType resultsType)
        {
            List<MatchTeamStats> teamStats = new List<MatchTeamStats>();

            int count = match.Teams != null ? match.Teams.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var team = match.Teams[i];

                MatchTeamStats teamStat = new MatchTeamStats();

                teamStat.TeamId = team.TeamId;

                teamStat.Stats.StatsData.Add("TeamFlags", i.ToString());

                teamStat.TeamMemberStats = GenerateRandomMatchTeamMemberStats(team);

                teamStats.Add(teamStat);
            }

            return teamStats;
        }

        MatchStats GenerateRandomStats(Match match, MatchCompetitionType competitionType, MatchGroupType groupType, MatchResultsType resultsType)
        {
            MatchStats newStats = new MatchStats();

            if (competitionType == MatchCompetitionType.Cooperative || (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.NonTeamMatch))
            {
                newStats.PlayerStats = GeneratePlayerStats(match, resultsType);
            }
            else if (competitionType == MatchCompetitionType.Competitive && groupType == MatchGroupType.TeamMatch)
            {
                newStats.TeamStats = GenerateTeamStats(match, resultsType);
            }

            return newStats;
        }
    }
}
#endif
