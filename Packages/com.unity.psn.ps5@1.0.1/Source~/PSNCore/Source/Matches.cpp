#include "Matches.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>

namespace psn
{
    void MatchesCommands::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::CreateMatch, MatchesCommands::CreateMatchImpl);
        MsgHandler::AddMethod(Methods::GetMatchDetails, MatchesCommands::GetMatchDetailsImpl);
        MsgHandler::AddMethod(Methods::UpdateMatchStatus, MatchesCommands::UpdateMatchStatusImpl);
        MsgHandler::AddMethod(Methods::JoinMatch, MatchesCommands::JoinMatchImpl);
        MsgHandler::AddMethod(Methods::LeaveMatch, MatchesCommands::LeaveMatchImpl);
        MsgHandler::AddMethod(Methods::ReportResults, MatchesCommands::ReportResultsImpl);
        MsgHandler::AddMethod(Methods::UpdateDetails, MatchesCommands::UpdateDetailsImpl);
    }

    void MatchesCommands::InitializeLib()
    {
    }

    void MatchesCommands::TerminateLib()
    {
    }

    void MatchesCommands::MatchPlayer::Deserialise(BinaryReader& reader)
    {
        m_PlayerId = reader.ReadStringPtr();
        m_PlayerType = (PlayerType)reader.ReadInt32();
        m_PlayerName = reader.ReadStringPtr();
        m_AccountId = reader.ReadUInt64();
    }

    void MatchesCommands::MatchTeamMember::Deserialise(BinaryReader& reader)
    {
        m_PlayerId = reader.ReadStringPtr();
    }

    void MatchesCommands::MatchTeam::Deserialise(BinaryReader& reader)
    {
        m_TeamId = reader.ReadStringPtr();
        m_TeamName = reader.ReadStringPtr();

        int32_t numPlayers = reader.ReadInt32();

        for (int i = 0; i < numPlayers; i++)
        {
            MatchTeamMember member;
            member.Deserialise(reader);

            m_Members.push_back(member);
        }
    }

    MatchesCommands::InitializationParams::InitializationParams()
    {
    }

    MatchesCommands::InitializationParams::~InitializationParams()
    {
    }

    void MatchesCommands::InitializationParams::Deserialise(BinaryReader& reader)
    {
        m_ActivityId = reader.ReadStringPtr();

        m_ServiceLabel = reader.ReadInt32();

        m_ZoneId = reader.ReadStringPtr();

        int32_t numPlayers = reader.ReadInt32();

        for (int i = 0; i < numPlayers; i++)
        {
            MatchPlayer player;
            player.Deserialise(reader);

            m_Players.push_back(player);
        }

        int32_t numTeams = reader.ReadInt32();

        for (int i = 0; i < numTeams; i++)
        {
            MatchTeam team;
            team.Deserialise(reader);

            m_Teams.push_back(team);
        }
    }

    void MatchesCommands::CreateMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        InitializationParams params;
        params.Deserialise(reader);

        BinaryWriter writer(resultsData, resultsMaxSize);

        ret = Create(userId, params, writer);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::GetMatchDetailsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();
        Int32 serviceLabel = reader.ReadInt32();

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        // prepare params
        MatchApi::ParameterToGetMatchDetail param;
        ret = param.initialize(libContextPtr, matchId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.setnpServiceLabel(serviceLabel);

        typedef Common::IntrusivePtr<GetMatchDetailResponse> GetMatchDetailResponseType;
        GetMatchDetailResponseType response;

        Common::Transaction<GetMatchDetailResponseType> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::getMatchDetail(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            param.terminate();
            transaction.finish();
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            param.terminate();
            transaction.finish();
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        SerialiseMatchDetail(writer, response);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::UpdateMatchStatusImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();
        UpdateStatus status = (UpdateStatus)reader.ReadInt32();

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        IntrusivePtr<UpdateMatchStatusRequest> updateMatchStatusRequest;
        ret = UpdateMatchStatusRequestFactory::create(libContextPtr, status, &updateMatchStatusRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        MatchApi::ParameterToUpdateMatchStatus param;
        ret = param.initialize(libContextPtr, matchId, updateMatchStatusRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::updateMatchStatus(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::JoinMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Vector<IntrusivePtr<AddedPlayer> > addedPlayers(libContextPtr);

        int32_t numPlayers = reader.ReadInt32();

        for (int i = 0; i < numPlayers; i++)
        {
            char* playerId = reader.ReadStringPtr();
            PlayerType playerType = (PlayerType)reader.ReadInt32();

            // AddedPlayer
            IntrusivePtr<AddedPlayer> addedPlayer;
            ret = AddedPlayerFactory::create(libContextPtr, playerId, playerType, &addedPlayer);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            // PlayerName
            bool isPlayerNameSet = reader.ReadBool();
            if (isPlayerNameSet == true)
            {
                char* playerName = reader.ReadStringPtr();
                ret = addedPlayer->setPlayerName(playerName);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            bool isAccountIdSet = reader.ReadBool();
            if (isAccountIdSet == true)
            {
                SceNpAccountId accountId = reader.ReadUInt64();

                char accountIdBuf[21];
                GetAccountIdStr(accountId, accountIdBuf, sizeof(accountIdBuf));
                ret = addedPlayer->setAccountId(accountIdBuf);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            bool isTeamIdSet = reader.ReadBool();
            if (isTeamIdSet == true)
            {
                char* teamId = reader.ReadStringPtr();
                ret = addedPlayer->setTeamId(teamId);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            // add to Vector
            ret = addedPlayers.pushBack(addedPlayer);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        // JoinMatchRequest
        IntrusivePtr<JoinMatchRequest> joinMatchRequest;
        ret = JoinMatchRequestFactory::create(libContextPtr, addedPlayers, &joinMatchRequest);

        MatchApi::ParameterToJoinMatch param;
        ret = param.initialize(libContextPtr, matchId, joinMatchRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;
        Common::Transaction<DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::joinMatch(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::LeaveMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Vector<IntrusivePtr<RemovedPlayer> > removedPlayers(libContextPtr);

        int32_t numPlayers = reader.ReadInt32();

        for (int i = 0; i < numPlayers; i++)
        {
            char* playerId = reader.ReadStringPtr();

            LeaveReason reason = (LeaveReason)reader.ReadInt32();

            // RemovedPlayer
            IntrusivePtr<RemovedPlayer> removedPlayer;
            ret = RemovedPlayerFactory::create(libContextPtr, playerId, &removedPlayer);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            if (reason != LeaveReason::_NOT_SET)
            {
                removedPlayer->setReason(reason);
            }

            // add to Vector
            ret = removedPlayers.pushBack(removedPlayer);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        IntrusivePtr<LeaveMatchRequest> leaveMatchRequest;
        ret = LeaveMatchRequestFactory::create(libContextPtr, removedPlayers, &leaveMatchRequest);

        MatchApi::ParameterToLeaveMatch param;
        ret = param.initialize(libContextPtr, matchId, leaveMatchRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;
        Common::Transaction<DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::leaveMatch(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    int MatchesCommands::DeserialiseMatchResults(Common::LibContext* libContextPtr, BinaryReader &reader, CompetitionType competition, GroupingType groupType, ResultType resultType, Common::IntrusivePtr<RequestMatchResults> &requestMatchResults)
    {
        int ret = 0;

        Int32 version = reader.ReadInt32();

        //Common::IntrusivePtr<RequestMatchResults> requestMatchResults;
        ret = RequestMatchResultsFactory::create(libContextPtr, (ResultsVersion)version, &requestMatchResults);
        if (ret < 0)
        {
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (competition == CompetitionType::kCooperative)
#else
        if (competition == CompetitionType::COOPERATIVE)
#endif
        {
            RequestCooperativeResult coopResult = (RequestCooperativeResult)reader.ReadInt32();
            requestMatchResults->setCooperativeResult(coopResult);
        }
        else // CompetitionType::COMPETITIVE
        {
            // RequestCompetitiveResult
            IntrusivePtr<RequestCompetitiveResult> requestCompetitiveResult;

            ret = DeserialiseCompetitiveResults(libContextPtr, reader, groupType, resultType, requestCompetitiveResult);
            if (ret < 0)
            {
                return ret;
            }

            // set competitiveResult
            ret = requestMatchResults->setCompetitiveResult(requestCompetitiveResult);
            if (ret < 0)
            {
                return ret;
            }
        }

        return 0;
    }

    int MatchesCommands::DeserialiseTemporaryMatchResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, Common::IntrusivePtr<RequestTemporaryMatchResults> &requestMatchResults)
    {
        int ret = 0;

        Int32 version = reader.ReadInt32();

        // RequestCompetitiveResult
        IntrusivePtr<RequestTemporaryCompetitiveResult> requestCompetitiveResult;
        ret = DeserialiseTemporaryCompetitiveResults(libContextPtr, reader, groupType, resultType, requestCompetitiveResult);
        if (ret < 0)
        {
            return ret;
        }

        //Common::IntrusivePtr<RequestMatchResults> requestMatchResults;
        ret = RequestTemporaryMatchResultsFactory::create(libContextPtr, (ResultsVersion)version, requestCompetitiveResult, &requestMatchResults);
        if (ret < 0)
        {
            return ret;
        }

        return 0;
    }

    int MatchesCommands::DeserialiseTemporaryCompetitiveResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestTemporaryCompetitiveResult> &requestCompetitiveResult)
    {
        int ret = 0;

        ret = RequestTemporaryCompetitiveResultFactory::create(libContextPtr, &requestCompetitiveResult);
        if (ret < 0)
        {
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (groupType == GroupingType::kNonTeamMatch)
#else
        if (groupType == GroupingType::NON_TEAM_MATCH)
#endif
        {
            // Player results

            int32_t numPlayerResults = reader.ReadInt32();

            if (numPlayerResults > 0)
            {
                Vector<IntrusivePtr<RequestPlayerResults> > requestPlayerResults(libContextPtr);

                for (int i = 0; i < numPlayerResults; i++)
                {
                    char* playerId = reader.ReadStringPtr();
                    int32_t rank = reader.ReadInt32();
                    bool isScoreSet = reader.ReadBool();
                    double playerScore = 0;

                    if (isScoreSet == true)
                    {
                        playerScore = reader.ReadDouble();
                    }

                    IntrusivePtr<RequestPlayerResults> requestPlayerResult;
                    ret = RequestPlayerResultsFactory::create(libContextPtr, playerId, rank, &requestPlayerResult);
                    if (ret < 0)
                    {
                        return ret;
                    }

                    // set score
                    if (isScoreSet == true)
                    {
                        requestPlayerResult->setScore(playerScore);
                    }

                    // add to Vector
                    ret = requestPlayerResults.pushBack(requestPlayerResult);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }

                ret = requestCompetitiveResult->setPlayerResults(requestPlayerResults);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        else if (groupType == GroupingType::kTeamMatch)
#else
        else if (groupType == GroupingType::TEAM_MATCH)
#endif
        {
            // Team results
            int32_t numTeamResults = reader.ReadInt32();

            if (numTeamResults > 0)
            {
                Vector<IntrusivePtr<RequestTemporaryTeamResults> > requestTeamResults(libContextPtr);

                for (int i = 0; i < numTeamResults; i++)
                {
                    char* teamId = reader.ReadStringPtr();
                    int32_t rank = reader.ReadInt32();
                    bool isScoreSet = reader.ReadBool();
                    double teamScore = 0;

                    if (isScoreSet == true)
                    {
                        teamScore = reader.ReadDouble();
                    }

                    int32_t numMemberResults = reader.ReadInt32();

                    IntrusivePtr<RequestTemporaryTeamResults> requestTeamResult;
                    ret = RequestTemporaryTeamResultsFactory::create(libContextPtr, teamId, rank, &requestTeamResult);
                    if (ret < 0)
                    {
                        return ret;
                    }

                    // set score
                    if (isScoreSet == true)
                    {
                        requestTeamResult->setScore(teamScore);
                    }

                    if (numMemberResults > 0)
                    {
                        Vector<IntrusivePtr<RequestTeamMemberResult> > requestTeamMemberResults(libContextPtr);

                        for (int j = 0; j < numMemberResults; j++)
                        {
                            char* playerId = reader.ReadStringPtr();
                            double playerScore = reader.ReadDouble();

                            // RequestTeamMemberResult
                            IntrusivePtr<RequestTeamMemberResult> requestTeamMemberResult;
                            ret = RequestTeamMemberResultFactory::create(libContextPtr, playerId, playerScore, &requestTeamMemberResult);
                            if (ret < 0)
                            {
                                return ret;
                            }

                            // add to Vector
                            ret = requestTeamMemberResults.pushBack(requestTeamMemberResult);
                            if (ret < 0)
                            {
                                return ret;
                            }
                        }
                        // set teamMemberResults
                        ret = requestTeamResult->setTeamMemberResults(requestTeamMemberResults);
                        if (ret < 0)
                        {
                            return ret;
                        }
                    }

                    // add to Vector
                    ret = requestTeamResults.pushBack(requestTeamResult);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }

                // set teamResults
                ret = requestCompetitiveResult->setTeamResults(requestTeamResults);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }

        return ret;
    }

    int MatchesCommands::DeserialiseCompetitiveResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestCompetitiveResult> &requestCompetitiveResult)
    {
        int ret = 0;

        ret = RequestCompetitiveResultFactory::create(libContextPtr, &requestCompetitiveResult);
        if (ret < 0)
        {
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (groupType == GroupingType::kNonTeamMatch)
#else
        if (groupType == GroupingType::NON_TEAM_MATCH)
#endif
        {
            // Player results

            int32_t numPlayerResults = reader.ReadInt32();

            if (numPlayerResults > 0)
            {
                Vector<IntrusivePtr<RequestPlayerResults> > requestPlayerResults(libContextPtr);

                for (int i = 0; i < numPlayerResults; i++)
                {
                    char* playerId = reader.ReadStringPtr();
                    int32_t rank = reader.ReadInt32();
                    bool isScoreSet = reader.ReadBool();
                    double playerScore = 0;

                    if (isScoreSet == true)
                    {
                        playerScore = reader.ReadDouble();
                    }

                    IntrusivePtr<RequestPlayerResults> requestPlayerResult;
                    ret = RequestPlayerResultsFactory::create(libContextPtr, playerId, rank, &requestPlayerResult);
                    if (ret < 0)
                    {
                        return ret;
                    }

                    // set score
                    if (isScoreSet == true)
                    {
                        requestPlayerResult->setScore(playerScore);
                    }

                    // add to Vector
                    ret = requestPlayerResults.pushBack(requestPlayerResult);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }

                ret = requestCompetitiveResult->setPlayerResults(requestPlayerResults);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        else if (groupType == GroupingType::kTeamMatch)
#else
        else if (groupType == GroupingType::TEAM_MATCH)
#endif
        {
            // Team results
            int32_t numTeamResults = reader.ReadInt32();

            if (numTeamResults > 0)
            {
                Vector<IntrusivePtr<RequestTeamResults> > requestTeamResults(libContextPtr);

                for (int i = 0; i < numTeamResults; i++)
                {
                    char* teamId = reader.ReadStringPtr();
                    int32_t rank = reader.ReadInt32();
                    bool isScoreSet = reader.ReadBool();
                    double teamScore = 0;

                    if (isScoreSet == true)
                    {
                        teamScore = reader.ReadDouble();
                    }

                    int32_t numMemberResults = reader.ReadInt32();

                    IntrusivePtr<RequestTeamResults> requestTeamResult;
                    ret = RequestTeamResultsFactory::create(libContextPtr, teamId, rank, &requestTeamResult);
                    if (ret < 0)
                    {
                        return ret;
                    }

                    // set score
                    if (isScoreSet == true)
                    {
                        requestTeamResult->setScore(teamScore);
                    }

                    if (numMemberResults > 0)
                    {
                        Vector<IntrusivePtr<RequestTeamMemberResult> > requestTeamMemberResults(libContextPtr);

                        for (int j = 0; j < numMemberResults; j++)
                        {
                            char* playerId = reader.ReadStringPtr();
                            double playerScore = reader.ReadDouble();

                            // RequestTeamMemberResult
                            IntrusivePtr<RequestTeamMemberResult> requestTeamMemberResult;
                            ret = RequestTeamMemberResultFactory::create(libContextPtr, playerId, playerScore, &requestTeamMemberResult);
                            if (ret < 0)
                            {
                                return ret;
                            }

                            // add to Vector
                            ret = requestTeamMemberResults.pushBack(requestTeamMemberResult);
                            if (ret < 0)
                            {
                                return ret;
                            }
                        }
                        // set teamMemberResults
                        ret = requestTeamResult->setTeamMemberResults(requestTeamMemberResults);
                        if (ret < 0)
                        {
                            return ret;
                        }
                    }

                    // add to Vector
                    ret = requestTeamResults.pushBack(requestTeamResult);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }

                // set teamResults
                ret = requestCompetitiveResult->setTeamResults(requestTeamResults);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }

        return ret;
    }

    int MatchesCommands::DeserialiseMatchStats(Common::LibContext* libContextPtr, BinaryReader &reader, CompetitionType competition, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics)
    {
        int ret = 0;

        //RequestMatchStatistics
        ret = RequestMatchStatisticsFactory::create(libContextPtr, &requestMatchStatistics);
        if (ret < 0)
        {
            return ret;
        }

#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (competition == CompetitionType::kCooperative || (competition == CompetitionType::kCompetitive && groupType == GroupingType::kNonTeamMatch))
        {
            ret = DeserialisePlayerStats(libContextPtr, reader, groupType, resultType, requestMatchStatistics);
            if (ret < 0)
            {
                return ret;
            }
        }
        else if (competition == CompetitionType::kCompetitive && groupType == GroupingType::kTeamMatch)
        {
            ret = DeserialiseTeamStats(libContextPtr, reader, groupType, resultType, requestMatchStatistics);
            if (ret < 0)
            {
                return ret;
            }
        }
#else
        if (competition == CompetitionType::COOPERATIVE || (competition == CompetitionType::COMPETITIVE && groupType == GroupingType::NON_TEAM_MATCH))
        {
            ret = DeserialisePlayerStats(libContextPtr, reader, groupType, resultType, requestMatchStatistics);
            if (ret < 0)
            {
                return ret;
            }
        }
        else if (competition == CompetitionType::COMPETITIVE && groupType == GroupingType::TEAM_MATCH)
        {
            ret = DeserialiseTeamStats(libContextPtr, reader, groupType, resultType, requestMatchStatistics);
            if (ret < 0)
            {
                return ret;
            }
        }
#endif


        return ret;
    }

    int MatchesCommands::DeserialiseAdditionalStats(Common::LibContext* libContextPtr, BinaryReader &reader, Vector<IntrusivePtr<AdditionalStatistic> >& stats)
    {
        int ret = 0;

        int32_t numStats = reader.ReadInt32();

        for (int i = 0; i < numStats; i++)
        {
            char* statsKey = reader.ReadStringPtr();
            char* statsValue = reader.ReadStringPtr();

            IntrusivePtr<AdditionalStatistic> stat;
            ret = AdditionalStatisticFactory::create(libContextPtr, statsKey, statsValue, &stat);
            if (ret < 0)
            {
                return ret;
            }

            ret = stats.pushBack(stat);
            if (ret < 0)
            {
                return ret;
            }
        }

        return 0;
    }

    int MatchesCommands::DeserialisePlayerStats(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics)
    {
        int ret = 0;

        int32_t numPlayerStats = reader.ReadInt32();

        if (numPlayerStats == 0)
        {
            return 0;
        }

        Vector<IntrusivePtr<RequestPlayerStatistic> > requestPlayerStatistics(libContextPtr);

        for (int i = 0; i < numPlayerStats; i++)
        {
            char* playerId = reader.ReadStringPtr();

            Vector<IntrusivePtr<AdditionalStatistic> > playerStats(libContextPtr);

            ret = DeserialiseAdditionalStats(libContextPtr, reader, playerStats);
            if (ret < 0)
            {
                return ret;
            }

            // RequestPlayerStatistic
            IntrusivePtr<RequestPlayerStatistic> requestPlayerStatistic;
            ret = RequestPlayerStatisticFactory::create(libContextPtr, playerId, playerStats, &requestPlayerStatistic);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            ret = requestPlayerStatistics.pushBack(requestPlayerStatistic);
            if (ret < 0)
            {
                return ret;
            }
        }

        // set playerStatistics
        ret = requestMatchStatistics->setPlayerStatistics(requestPlayerStatistics);
        if (ret < 0)
        {
            return ret;
        }

        return 0;
    }

    int MatchesCommands::DeserialiseTeamMemberStats(Common::LibContext* libContextPtr, BinaryReader &reader, IntrusivePtr<RequestTeamStatistic> &requestTeamStatistic)
    {
        int ret = 0;

        int32_t numMemberStats = reader.ReadInt32();

        if (numMemberStats == 0)
        {
            return 0;
        }

        Vector<IntrusivePtr<RequestTeamMemberStatistic> > requestTeamMemberStatistics(libContextPtr);

        for (int i = 0; i < numMemberStats; i++)
        {
            char* playerId = reader.ReadStringPtr();

            // RequestTeamMemberStatistic
            Vector<IntrusivePtr<AdditionalStatistic> > teamMemberStats(libContextPtr);
            ret = DeserialiseAdditionalStats(libContextPtr, reader, teamMemberStats);
            if (ret < 0)
            {
                return ret;
            }

            IntrusivePtr<RequestTeamMemberStatistic> requestTeamMemberStatistic;
            ret = RequestTeamMemberStatisticFactory::create(libContextPtr, playerId, teamMemberStats, &requestTeamMemberStatistic);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            ret = requestTeamMemberStatistics.pushBack(requestTeamMemberStatistic);
            if (ret < 0)
            {
                return ret;
            }
        }

        // set teamMemberStatistics
        ret = requestTeamStatistic->setTeamMemberStatistics(requestTeamMemberStatistics);
        if (ret < 0)
        {
            return ret;
        }

        return 0;
    }

    int MatchesCommands::DeserialiseTeamStats(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics)
    {
        int ret = 0;

        int32_t numTeamStats = reader.ReadInt32();

        if (numTeamStats == 0)
        {
            return 0;
        }

        Vector<IntrusivePtr<RequestTeamStatistic> > requestTeamStatistics(libContextPtr);

        for (int i = 0; i < numTeamStats; i++)
        {
            char* teamId = reader.ReadStringPtr();

            // RequestTeamStatistic
            IntrusivePtr<RequestTeamStatistic> requestTeamStatistic;
            ret = RequestTeamStatisticFactory::create(libContextPtr, teamId, &requestTeamStatistic);
            if (ret < 0)
            {
                return ret;
            }

            // set stats
            Vector<IntrusivePtr<AdditionalStatistic> > teamStats(libContextPtr);
            ret = DeserialiseAdditionalStats(libContextPtr, reader, teamStats);
            if (ret < 0)
            {
                return ret;
            }

            ret = requestTeamStatistic->setStats(teamStats);
            if (ret < 0)
            {
                return ret;
            }

            ret = DeserialiseTeamMemberStats(libContextPtr, reader, requestTeamStatistic);
            if (ret < 0)
            {
                return ret;
            }

            ret = requestTeamStatistics.pushBack(requestTeamStatistic);
            if (ret < 0)
            {
                return ret;
            }
        }

        // set playerStatistics
        ret = requestMatchStatistics->setTeamStatistics(requestTeamStatistics);
        if (ret < 0)
        {
            return ret;
        }

        return 0;
    }

    void MatchesCommands::ReportResultsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();
        CompetitionType competition = (CompetitionType)reader.ReadInt32();
        GroupingType groupType = (GroupingType)reader.ReadInt32();
        ResultType resultType = (ResultType)reader.ReadInt32();

#if (SCE_PROSPERO_SDK_VERSION >= 0x07000000u || SCE_ORBIS_SDK_VERSION >= 0x010500000u)
        // Read and discard eligibility
        reader.ReadInt32();
#else
        PlayerReviewEligibility eligable = (PlayerReviewEligibility)reader.ReadInt32();
#endif

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Common::IntrusivePtr<RequestMatchResults> requestMatchResults;
        DeserialiseMatchResults(libContextPtr, reader, competition, groupType, resultType, requestMatchResults);

        IntrusivePtr<RequestMatchStatistics> requestMatchStatistics;
        DeserialiseMatchStats(libContextPtr, reader, competition, groupType, resultType, requestMatchStatistics);

        // ReportResultsRequest
        IntrusivePtr<ReportResultsRequest> reportResultsRequest;
        ret = ReportResultsRequestFactory::create(libContextPtr, requestMatchResults, &reportResultsRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
#if (defined(SCE_PROSPERO_SDK_VERSION)&& (SCE_PROSPERO_SDK_VERSION < 0x07000000u)) || (defined(SCE_ORBIS_SDK_VERSION)&& (SCE_ORBIS_SDK_VERSION < 0x010500000u))
        reportResultsRequest->setPlayerReviewEligibility(eligable);
#endif

        // set MatchStatistics
        ret = reportResultsRequest->setMatchStatistics(requestMatchStatistics);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // ParameterToReportResults
        MatchApi::ParameterToReportResults param;
        ret = param.initialize(libContextPtr, matchId, reportResultsRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;
        Common::Transaction<DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::reportResults(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::UpdateDetailsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* matchId = reader.ReadStringPtr();

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Common::IntrusivePtr<UpdateMatchDetailRequest> updateMatchDetailRequest;
        ret = UpdateMatchDetailRequestFactory::create(libContextPtr, &updateMatchDetailRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            Int32 npServiceLabel = reader.ReadInt32();
            updateMatchDetailRequest->setNpServiceLabel(npServiceLabel);
        }

        bool isZoneIdSet = reader.ReadBool();
        if (isZoneIdSet)
        {
            char* zoneId = reader.ReadStringPtr();
            ret = updateMatchDetailRequest->setZoneId(zoneId);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        bool isExpirationTimeSet = reader.ReadBool();
        if (isExpirationTimeSet)
        {
            Int32 expirationTime = reader.ReadInt32();
            updateMatchDetailRequest->setExpirationTime(expirationTime);
        }

        IntrusivePtr<RequestInGameRoster> requestInGameRoster;
        ret = RequestInGameRosterFactory::create(libContextPtr, &requestInGameRoster);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // RequestMatchPlayer
        Vector<IntrusivePtr<RequestMatchPlayer> > requestMatchPlayers(libContextPtr);

        bool isPlayersSet = reader.ReadBool();
        if (isPlayersSet)
        {
            int32_t numPlayers = reader.ReadInt32();

            for (int i = 0; i < numPlayers; i++)
            {
                IntrusivePtr<RequestMatchPlayer> requestMatchPlayer;

                char* playerId = reader.ReadStringPtr();
                PlayerType playerType = (PlayerType)reader.ReadInt32();

                ret = RequestMatchPlayerFactory::create(libContextPtr, playerId, playerType, &requestMatchPlayer);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }

                bool isPlayerNameSet = reader.ReadBool();
                if (isPlayerNameSet == true)
                {
                    char* playerName = reader.ReadStringPtr();
                    ret = requestMatchPlayer->setPlayerName(playerName);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }

                bool isAccountIdSet = reader.ReadBool();
                if (isAccountIdSet == true)
                {
                    UInt64 accountId = reader.ReadUInt64();
                    char accountIdBuf[21];
                    GetAccountIdStr(accountId, accountIdBuf, sizeof(accountIdBuf));
                    ret = requestMatchPlayer->setAccountId(accountIdBuf);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }

                ret = requestMatchPlayers.pushBack(requestMatchPlayer);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            ret = requestInGameRoster->setPlayers(requestMatchPlayers);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        bool isTeamsSet = reader.ReadBool();
        if (isTeamsSet)
        {
            int32_t numTeams = reader.ReadInt32();

            Vector<IntrusivePtr<RequestMatchTeam> > requestMatchTeams(libContextPtr);
            for (int i = 0; i < numTeams; i++)
            {
                IntrusivePtr<RequestMatchTeam> requestMatchTeam;

                char* teamId = reader.ReadStringPtr();

                ret = RequestMatchTeamFactory::create(libContextPtr, teamId, &requestMatchTeam);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }

                bool isTeamNameSet = reader.ReadBool();
                if (isTeamNameSet == true)
                {
                    char* teamName = reader.ReadStringPtr();
                    ret = requestMatchTeam->setTeamName(teamName);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }

                int32_t numMembers = reader.ReadInt32();

                if (numMembers > 0)
                {
                    Vector<IntrusivePtr<RequestMember> > requestMembers(libContextPtr);

                    for (int j = 0; j < numMembers; j++)
                    {
                        // RequestMember
                        IntrusivePtr<RequestMember> requestMember;

                        char* playerId = reader.ReadStringPtr();

                        ret = RequestMemberFactory::create(libContextPtr, playerId, &requestMember);
                        if (ret < 0)
                        {
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }

                        // add to Vector
                        ret = requestMembers.pushBack(requestMember);
                        if (ret < 0)
                        {
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }
                    }
                    ret = requestMatchTeam->setMembers(requestMembers);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }
                // add to Vector
                ret = requestMatchTeams.pushBack(requestMatchTeam);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            // set teams
            ret = requestInGameRoster->setTeams(requestMatchTeams);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isPlayersSet || isTeamsSet)
        {
            // set inGameRoster
            ret = updateMatchDetailRequest->setInGameRoster(requestInGameRoster);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        GroupingType groupType = (GroupingType)reader.ReadInt32();
        ResultType resultType = (ResultType)reader.ReadInt32();

        bool isResultsSet = reader.ReadBool();
        if (isResultsSet)
        {
            Common::IntrusivePtr<RequestTemporaryMatchResults> matchResults;
            DeserialiseTemporaryMatchResults(libContextPtr, reader, groupType, resultType, matchResults);

            ret = updateMatchDetailRequest->setMatchResults(matchResults);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        bool isStatsSet = reader.ReadBool();
        if (isStatsSet)
        {
            IntrusivePtr<RequestMatchStatistics> requestMatchStatistics;
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
            DeserialiseMatchStats(libContextPtr, reader, CompetitionType::kCompetitive, groupType, resultType, requestMatchStatistics);
#else
            DeserialiseMatchStats(libContextPtr, reader, CompetitionType::COMPETITIVE, groupType, resultType, requestMatchStatistics);
#endif

            ret = updateMatchDetailRequest->setMatchStatistics(requestMatchStatistics);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        MatchApi::ParameterToUpdateMatchDetail param;
        ret = param.initialize(libContextPtr, matchId, updateMatchDetailRequest);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;
        Common::Transaction<DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::updateMatchDetail(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchesCommands::SerialiseMatchDetail(BinaryWriter& writer, IntrusivePtr<GetMatchDetailResponse> matchDetail)
    {
        writer.WriteString(matchDetail->getMatchId().c_str());

        writer.WriteInt32((Int32)matchDetail->getStatus());

        writer.WriteString(matchDetail->getActivityId().c_str());

        writer.WriteInt32((Int32)matchDetail->getGroupingType());
        writer.WriteInt32((Int32)matchDetail->getCompetitionType());
        writer.WriteInt32((Int32)matchDetail->getResultType());

        if (matchDetail->zoneIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(matchDetail->getZoneId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        writer.WriteInt32(matchDetail->getExpirationTime());

        if (matchDetail->matchStartTimestampIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(matchDetail->getMatchStartTimestamp().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchDetail->matchEndTimestampIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(matchDetail->getMatchEndTimestamp().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchDetail->lastPausedTimestampIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(matchDetail->getLastPausedTimestamp().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchDetail->inGameRosterIsSet())
        {
            IntrusivePtr<ResponseInGameRoster> roster = matchDetail->getInGameRoster();
            writer.WriteBool(true);

            SerialiseGameRoster(writer, roster);
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchDetail->matchResultsIsSet())
        {
            IntrusivePtr<ResponseMatchResults> matchResults = matchDetail->getMatchResults();
            writer.WriteBool(true);

            SerialiseMatchResults(writer, matchResults);
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchDetail->matchStatisticsIsSet())
        {
            IntrusivePtr<ResponseMatchStatistics> matchStats = matchDetail->getMatchStatistics();
            writer.WriteBool(true);

            SerialiseMatchStats(writer, matchStats);
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    void MatchesCommands::SerialisePlayers(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseMatchPlayer> > > players)
    {
        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            writer.WriteString(it->getPlayerId().c_str());

            writer.WriteInt32((Int32)it->getPlayerType());

            if (it->playerNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getPlayerName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->accountIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getAccountId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->onlineIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getOnlineId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            writer.WriteBool(it->getJoinFlag());
        }
    }

    void MatchesCommands::SerialiseTeams(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseMatchTeam> > > teams)
    {
        writer.WriteInt32(teams->size());

        for (auto& it : *teams)
        {
            writer.WriteString(it->getTeamId().c_str());

            if (it->teamNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getTeamName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->membersIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Vector<IntrusivePtr<ResponseMember> > > members = it->getMembers();

                writer.WriteInt32(members->size());

                for (auto& member : *members)
                {
                    writer.WriteString(member->getPlayerId().c_str());
                    writer.WriteBool(member->getJoinFlag());
                }
            }
            else
            {
                writer.WriteBool(false);
            }
        }
    }

    void MatchesCommands::SerialiseGameRoster(BinaryWriter& writer, IntrusivePtr<ResponseInGameRoster> roster)
    {
        if (roster->playersIsSet())
        {
            writer.WriteBool(true);

            SerialisePlayers(writer, roster->getPlayers());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (roster->teamsIsSet())
        {
            writer.WriteBool(true);

            SerialiseTeams(writer, roster->getTeams());
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    void MatchesCommands::SerialiseMatchResults(BinaryWriter& writer, IntrusivePtr<ResponseMatchResults> matchResults)
    {
        writer.WriteInt32((Int32)matchResults->getVersion());

        if (matchResults->cooperativeResultIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)matchResults->getCooperativeResult());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchResults->competitiveResultIsSet())
        {
            writer.WriteBool(true);

            IntrusivePtr<ResponseCompetitiveResult> competitiveResult = matchResults->getCompetitiveResult();

            if (competitiveResult->playerResultsIsSet())
            {
                writer.WriteBool(true);

                SerialisePlayerResults(writer, competitiveResult->getPlayerResults());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (competitiveResult->teamResultsIsSet())
            {
                writer.WriteBool(true);

                SerialiseTeamResults(writer, competitiveResult->getTeamResults());
            }
            else
            {
                writer.WriteBool(false);
            }
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    void MatchesCommands::SerialiseTeamResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseTeamResults> > > teamResults)
    {
        writer.WriteInt32(teamResults->size());

        for (auto& it : *teamResults)
        {
            writer.WriteString(it->getTeamId().c_str());

            writer.WriteInt32(it->getRank());

            if (it->scoreIsSet())
            {
                writer.WriteBool(true);
                writer.WriteDouble(it->getScore());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->teamMemberResultsIsSet())
            {
                writer.WriteBool(true);

                SerialiseTeamMemberResults(writer, it->getTeamMemberResults());
            }
            else
            {
                writer.WriteBool(false);
            }
        }
    }

    void MatchesCommands::SerialiseTeamMemberResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseTeamMemberResult> > > teamMemberResults)
    {
        writer.WriteInt32(teamMemberResults->size());

        for (auto& teamMemberResult : *teamMemberResults)
        {
            writer.WriteString(teamMemberResult->getPlayerId().c_str());

            writer.WriteDouble(teamMemberResult->getScore());
        }
    }

    void MatchesCommands::SerialisePlayerResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponsePlayerResults> > > playerResults)
    {
        writer.WriteInt32(playerResults->size());

        for (auto& it : *playerResults)
        {
            writer.WriteString(it->getPlayerId().c_str());

            writer.WriteInt32(it->getRank());

            if (it->scoreIsSet())
            {
                writer.WriteBool(true);
                writer.WriteDouble(it->getScore());
            }
            else
            {
                writer.WriteBool(false);
            }
        }
    }

    void MatchesCommands::SerialiseAdditionalStats(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<AdditionalStatistic> > > additionalStats)
    {
        writer.WriteInt32(additionalStats->size());

        for (auto& aStats : *additionalStats)
        {
            writer.WriteString(aStats->getStatsKey().c_str());
            writer.WriteString(aStats->getStatsValue().c_str());
        }
    }

    void MatchesCommands::SerialiseMatchStats(BinaryWriter& writer, IntrusivePtr<ResponseMatchStatistics> matchStats)
    {
        if (matchStats->playerStatisticsIsSet())
        {
            writer.WriteBool(true);

            IntrusivePtr<Vector<IntrusivePtr<ResponsePlayerStatistic> > > playerStats = matchStats->getPlayerStatistics();

            writer.WriteInt32(playerStats->size());

            for (auto& it : *playerStats)
            {
                writer.WriteString(it->getPlayerId().c_str());

                SerialiseAdditionalStats(writer, it->getStats());
            }
        }
        else
        {
            writer.WriteBool(false);
        }

        if (matchStats->teamStatisticsIsSet())
        {
            writer.WriteBool(true);

            IntrusivePtr<Vector<IntrusivePtr<ResponseTeamStatistic> > > teamStats = matchStats->getTeamStatistics();

            writer.WriteInt32(teamStats->size());

            for (auto& it : *teamStats)
            {
                writer.WriteString(it->getTeamId().c_str());

                SerialiseAdditionalStats(writer, it->getStats());

                IntrusivePtr<Vector<IntrusivePtr<ResponseTeamMemberStatistic> > > teamMemberStats = it->getTeamMemberStatistics();

                writer.WriteInt32(teamMemberStats->size());

                for (auto& teamMemberState : *teamMemberStats)
                {
                    writer.WriteString(teamMemberState->getPlayerId().c_str());
                    SerialiseAdditionalStats(writer, teamMemberState->getStats());
                }
            }
        }
        else
        {
            writer.WriteBool(false);
        }
    }

    int MatchesCommands::Create(Int32 userId, InitializationParams& initParams, BinaryWriter& writer)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            return -1;
        }

        int ret = 0;

        IntrusivePtr<RequestInGameRoster> requestInGameRoster;
        ret = RequestInGameRosterFactory::create(libContextPtr, &requestInGameRoster);
        if (ret < 0)
        {
            return ret;
        }

        // RequestMatchPlayer
        Vector<IntrusivePtr<RequestMatchPlayer> > requestMatchPlayers(libContextPtr);
        for (auto& player : initParams.m_Players)
        {
            IntrusivePtr<RequestMatchPlayer> requestMatchPlayer;

            ret = RequestMatchPlayerFactory::create(libContextPtr, player.m_PlayerId, player.m_PlayerType, &requestMatchPlayer);
            if (ret < 0)
            {
                return ret;
            }

            if (player.m_PlayerName != NULL && strlen(player.m_PlayerName) > 0)
            {
                ret = requestMatchPlayer->setPlayerName(player.m_PlayerName);
                if (ret < 0)
                {
                    return ret;
                }
            }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
            if (player.m_PlayerType == PlayerType::kPsnPlayer)
#else
            if (player.m_PlayerType == PlayerType::PSN_PLAYER)
#endif
            {
                char accountIdBuf[21];
                GetAccountIdStr(player.m_AccountId, accountIdBuf, sizeof(accountIdBuf));
                ret = requestMatchPlayer->setAccountId(accountIdBuf);
                if (ret < 0)
                {
                    return ret;
                }
            }

            ret = requestMatchPlayers.pushBack(requestMatchPlayer);
            if (ret < 0)
            {
                return ret;
            }
        }

        ret = requestInGameRoster->setPlayers(requestMatchPlayers);
        if (ret < 0)
        {
            return ret;
        }

        if (initParams.m_Teams.size() > 0)
        {
            Vector<IntrusivePtr<RequestMatchTeam> > requestMatchTeams(libContextPtr);
            for (auto& team : initParams.m_Teams)
            {
                IntrusivePtr<RequestMatchTeam> requestMatchTeam;

                ret = RequestMatchTeamFactory::create(libContextPtr, team.m_TeamId, &requestMatchTeam);
                if (ret < 0)
                {
                    return ret;
                }

                if (team.m_TeamName != NULL && strlen(team.m_TeamName) > 0)
                {
                    ret = requestMatchTeam->setTeamName(team.m_TeamName);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }

                if (team.m_Members.size())
                {
                    Vector<IntrusivePtr<RequestMember> > requestMembers(libContextPtr);

                    for (auto& member : team.m_Members)
                    {
                        // RequestMember
                        IntrusivePtr<RequestMember> requestMember;
                        ret = RequestMemberFactory::create(libContextPtr, member.m_PlayerId, &requestMember);
                        if (ret < 0)
                        {
                            return ret;
                        }

                        // add to Vector
                        ret = requestMembers.pushBack(requestMember);
                        if (ret < 0)
                        {
                            return ret;
                        }
                    }
                    ret = requestMatchTeam->setMembers(requestMembers);
                    if (ret < 0)
                    {
                        return ret;
                    }
                }
                // add to Vector
                ret = requestMatchTeams.pushBack(requestMatchTeam);
                if (ret < 0)
                {
                    return ret;
                }
            }

            // set teams
            ret = requestInGameRoster->setTeams(requestMatchTeams);
            if (ret < 0)
            {
                return ret;
            }
        }

        IntrusivePtr<CreateMatchRequest> createMatchRequest;
        ret = CreateMatchRequestFactory::create(libContextPtr, initParams.m_ActivityId, &createMatchRequest);
        if (ret < 0)
        {
            return ret;
        }

        // set inGameRoster
        ret = createMatchRequest->setInGameRoster(requestInGameRoster);
        if (ret < 0)
        {
            return ret;
        }

        createMatchRequest->setNpServiceLabel(initParams.m_ServiceLabel);
        if (ret < 0)
        {
            return ret;
        }

        if (initParams.m_ZoneId != NULL && strlen(initParams.m_ZoneId) > 0)
        {
            ret = createMatchRequest->setZoneId(initParams.m_ZoneId);
            if (ret < 0)
            {
                return ret;
            }
        }

        MatchApi::ParameterToCreateMatch param;
        ret = param.initialize(libContextPtr, createMatchRequest);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<CreateMatchResponse> CreateMatchResponseType;
        CreateMatchResponseType response;

        Common::Transaction<CreateMatchResponseType> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = MatchApi::createMatch(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        if (!(response))
        {
            param.terminate();
            transaction.finish();
            return -1;
        }

        writer.WriteString(response->getMatchId().c_str());

        param.terminate();
        transaction.finish();

        return ret;
    }
}
