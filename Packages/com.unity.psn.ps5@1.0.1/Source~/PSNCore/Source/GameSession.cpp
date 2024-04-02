#include "GameSession.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>

namespace psn
{
    void GameSessionCommands::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::CreateGameSession, GameSessionCommands::CreateGameSessionImpl);
        MsgHandler::AddMethod(Methods::LeaveGameSession, GameSessionCommands::LeaveGameSessionImpl);
        MsgHandler::AddMethod(Methods::JoinGameSession, GameSessionCommands::JoinGameSessionImpl);
        MsgHandler::AddMethod(Methods::GetGameSessions, GameSessionCommands::GetGameSessionsImpl);
        MsgHandler::AddMethod(Methods::SetGameSessionProperties, GameSessionCommands::SetGameSessionPropertiesImpl);
        MsgHandler::AddMethod(Methods::SetGameSessionMemberSystemProperties, GameSessionCommands::SetGameSessionMemberSystemPropertiesImpl);
        MsgHandler::AddMethod(Methods::SendGameSessionMessage, GameSessionCommands::SendGameSessionMessageImpl);
        MsgHandler::AddMethod(Methods::GetJoinedGameSessionsByUser, GameSessionCommands::GetJoinedGameSessionsByUserImpl);
        MsgHandler::AddMethod(Methods::DeleteGameSession, GameSessionCommands::DeleteGameSessionImpl);
        MsgHandler::AddMethod(Methods::GameSessionsSearch, GameSessionCommands::GameSessionsSearchImpl);
    }

    // must match GameSessionInitMember.Serialise()
    void GameSessionCommands::GSMember::Deserialise(BinaryReader& reader)
    {
        m_UserId = reader.ReadInt32();
        m_PushCallbackId = reader.ReadInt32();
        m_AccountId = reader.ReadUInt64();
        m_Platform = reader.ReadInt32();
        m_JoinState = (InitialJoinState)reader.ReadInt32();
        m_NatType = reader.ReadInt32();

        m_CustomDataSize1 = reader.ReadInt32();
        m_CustomData1 = NULL;

        if (m_CustomDataSize1 > 0)
        {
            m_CustomData1 = reader.ReadDataPtr(m_CustomDataSize1);
        }
    }

    GameSessionCommands::InitializationParams::InitializationParams() :
        m_MaxPlayers(0),
        m_MaxSpectators(0),
        m_PlatformFlags(0),
        m_JoinDisabled(false),
        m_UsePlayerSession(false),
        m_ReservationTimeoutSeconds(0),
        m_NumberMembers(0),
        m_Members(NULL),
        m_CustomDataSize1(0),
        m_CustomData1(NULL),
        m_CustomDataSize2(0),
        m_CustomData2(NULL),
        m_Searchable(true)
    {
    }

    GameSessionCommands::InitializationParams::~InitializationParams()
    {
        if (m_Members != NULL) delete[] m_Members;
    }

    void GameSessionCommands::InitializationParams::Deserialise(BinaryReader& reader)
    {
        m_MaxPlayers = reader.ReadUInt32();
        m_MaxSpectators = reader.ReadUInt32();
        m_PlatformFlags = reader.ReadUInt32();

        m_JoinDisabled = reader.ReadBool();
        m_UsePlayerSession = reader.ReadBool();
        m_ReservationTimeoutSeconds = reader.ReadInt32();

        m_NumberMembers = reader.ReadUInt32();
        if (m_NumberMembers > 0)
        {
            m_Members = new GSMember[m_NumberMembers];

            for (int i = 0; i < m_NumberMembers; i++)
            {
                m_Members[i].Deserialise(reader);
            }
        }

        m_CustomDataSize1 = reader.ReadInt32();
        if (m_CustomDataSize1 > 0)
        {
            m_CustomData1 = reader.ReadDataPtr(m_CustomDataSize1);
        }

        m_CustomDataSize2 = reader.ReadInt32();
        if (m_CustomDataSize2 > 0)
        {
            m_CustomData2 = reader.ReadDataPtr(m_CustomDataSize2);
        }
        m_Searchable = reader.ReadBool();

        m_SearchIndex = reader.ReadStringPtr();


        m_StringsSetBits = reader.ReadUInt32();
        m_IntsSetBits = reader.ReadUInt32();
        m_BoolsSetBits = reader.ReadUInt32();
        for (int i = 0; i < kNumSearchAttributes; i++)
        {
            m_Strings[i] = reader.ReadStringPtr();
            m_Ints[i] = reader.ReadInt32();
            m_Bools[i] = reader.ReadBool();
        }
    }

    // see managed code GameSessions.CreateGameSessionRequest()
    void GameSessionCommands::CreateGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        GSMember creator;
        creator.Deserialise(reader);

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(creator.m_UserId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        ret = userCtx->GetAccountId(&creator.m_AccountId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        creator.m_Platform = Utils::GetThisPlatformFlag();

        InitializationParams params;
        params.Deserialise(reader);

        BinaryWriter writer(resultsData, resultsMaxSize);

        ret = Create(creator, params, writer);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int GameSessionCommands::AddUser(GSMember& member, Vector<IntrusivePtr<RequestGameSessionPlayer> >& requestGameSessionPlayers)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        int ret = 0;

        // RequestGameSessionPlayer
        IntrusivePtr<RequestGameSessionPlayer> requestGameSessionPlayerPtr;

        char accountId[32] = { 0 };
        snprintf(accountId, sizeof(accountId) - 1, "%lu", member.m_AccountId);
        ret = RequestGameSessionPlayerFactory::create(libContextPtr, accountId, Utils::ToPlatformString(member.m_Platform), &requestGameSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        if (member.m_JoinState != InitialJoinState::_NOT_SET)
        {
            // setJoinState "RESERVED" or "JOINED"
            requestGameSessionPlayerPtr->setJoinState(member.m_JoinState);
        }

        if (member.m_NatType != 0)
        {
            requestGameSessionPlayerPtr->setNatType(member.m_NatType);
        }

        int pushCallbackId = member.m_PushCallbackId;

        if (pushCallbackId != 0)
        {
            OrderGuaranteedPushEvent* pushEvent = WebApiNotifications::FindOrderedPushEvent(pushCallbackId);

            if (pushEvent == NULL)
            {
                return -1;
            }

            // PlayerSessionPushContext
            IntrusivePtr<GameSessionPushContext> gameSessionPushContextPtr;
            ret = GameSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            Vector<IntrusivePtr<GameSessionPushContext> > gameSessionPushContexts(libContextPtr);
            ret = gameSessionPushContexts.pushBack(gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            ret = requestGameSessionPlayerPtr->setPushContexts(gameSessionPushContexts);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (member.m_CustomDataSize1 > 0)
        {
            ret = requestGameSessionPlayerPtr->setCustomData1(member.m_CustomData1, member.m_CustomDataSize1);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        ret = requestGameSessionPlayers.pushBack(requestGameSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        return 0;
    }

    void GameSessionCommands::LeaveGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();
        const char* sessionId = reader.ReadStringPtr();

        int ret = Leave(userId, sessionId);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        SceNpAccountId accountId = SCE_NP_INVALID_ACCOUNT_ID;
        sceNpGetAccountIdA(userId, &accountId);

        writer.WriteUInt64(accountId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void GameSessionCommands::JoinGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();
        Int32 pushCallbackId = reader.ReadInt32();
        bool joinAsSpector = reader.ReadBool();
        bool swapping = reader.ReadBool();
        const char* sessionId = reader.ReadStringPtr();

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        if (joinAsSpector == false)
        {
            int ret = JoinAsPlayer(userId, pushCallbackId, sessionId, swapping, writer);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }
        else
        {
            int ret = JoinAsSpectator(userId, pushCallbackId, sessionId, swapping, writer);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int GameSessionCommands::Create(GSMember& creator, InitializationParams& initParams, BinaryWriter& writer)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        OrderGuaranteedPushEvent* pushEvent = WebApiNotifications::FindOrderedPushEvent(creator.m_PushCallbackId);

        if (pushEvent == NULL)
        {
            return -1;
        }

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(creator.m_UserId);

        if (userCtx == NULL)
        {
            return -1;
        }

        int ret = 0;

        // RequestGameSessionPlayers
        Vector<IntrusivePtr<RequestGameSessionPlayer> > requestGameSessionPlayers(libContextPtr);

        AddUser(creator, requestGameSessionPlayers);

        // Members
        for (int i = 0; i < initParams.m_NumberMembers; i++)
        {
            AddUser(initParams.m_Members[i], requestGameSessionPlayers);
        }

        // RequestGameSessionMemberPlayer
        IntrusivePtr<RequestGameSessionMemberPlayer> requestGameSessionMemberPlayer;
        ret = RequestGameSessionMemberPlayerFactory::create(libContextPtr, requestGameSessionPlayers, &requestGameSessionMemberPlayer);
        if (ret < 0)
        {
            return ret;
        }

        Vector<String> gameSessionSupportedPlatforms(libContextPtr);
        ret = Utils::AddPlatformStrings(initParams.m_PlatformFlags, gameSessionSupportedPlatforms);
        if (ret < 0)
        {
            return ret;
        }

        // RequestGameSession
        IntrusivePtr<RequestGameSession> requestGameSessionPtr;
        ret = RequestGameSessionFactory::create(libContextPtr, initParams.m_MaxPlayers, requestGameSessionMemberPlayer, gameSessionSupportedPlatforms, &requestGameSessionPtr);
        if (ret < 0)
        {
            return ret;
        }

        // set maxSpectator
        requestGameSessionPtr->setMaxSpectators(initParams.m_MaxSpectators);

        requestGameSessionPtr->setJoinDisabled(initParams.m_JoinDisabled);

        requestGameSessionPtr->setUsePlayerSession(initParams.m_UsePlayerSession);

        requestGameSessionPtr->setReservationTimeoutSeconds(initParams.m_ReservationTimeoutSeconds);

        if (initParams.m_SearchIndex != NULL)
        {
            ret = requestGameSessionPtr->setSearchIndex(initParams.m_SearchIndex);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (initParams.m_StringsSetBits | initParams.m_IntsSetBits | initParams.m_BoolsSetBits)  // any values set in the search attributes?
        {
            // SearchAttributes
            IntrusivePtr<SearchAttributes> searchAttributes;
            ret = SearchAttributesFactory::create(libContextPtr, &searchAttributes);
            if (ret < 0)
            {
                return ret;
            }

            for (int i = 0; i < 10; i++)
            {
                if (initParams.m_StringsSetBits & (1 << i))
                {
                    switch (i)
                    {
                        case 0:  ret = searchAttributes->setString1(initParams.m_Strings[i]);  break;
                        case 1:  ret = searchAttributes->setString2(initParams.m_Strings[i]);  break;
                        case 2:  ret = searchAttributes->setString3(initParams.m_Strings[i]);  break;
                        case 3:  ret = searchAttributes->setString4(initParams.m_Strings[i]);  break;
                        case 4:  ret = searchAttributes->setString5(initParams.m_Strings[i]);  break;
                        case 5:  ret = searchAttributes->setString6(initParams.m_Strings[i]);  break;
                        case 6:  ret = searchAttributes->setString7(initParams.m_Strings[i]);  break;
                        case 7:  ret = searchAttributes->setString8(initParams.m_Strings[i]);  break;
                        case 8:  ret = searchAttributes->setString9(initParams.m_Strings[i]);  break;
                        case 9:  ret = searchAttributes->setString10(initParams.m_Strings[i]);  break;
                    }
                    if (ret < 0)
                    {
                        return ret;
                    }
                }
                if (initParams.m_IntsSetBits & (1 << i))
                {
                    switch (i)
                    {
                        case 0: searchAttributes->setInteger1(initParams.m_Ints[i]);  break;
                        case 1: searchAttributes->setInteger2(initParams.m_Ints[i]);  break;
                        case 2: searchAttributes->setInteger3(initParams.m_Ints[i]);  break;
                        case 3: searchAttributes->setInteger4(initParams.m_Ints[i]);  break;
                        case 4: searchAttributes->setInteger5(initParams.m_Ints[i]);  break;
                        case 5: searchAttributes->setInteger6(initParams.m_Ints[i]);  break;
                        case 6: searchAttributes->setInteger7(initParams.m_Ints[i]);  break;
                        case 7: searchAttributes->setInteger8(initParams.m_Ints[i]);  break;
                        case 8: searchAttributes->setInteger9(initParams.m_Ints[i]);  break;
                        case 9: searchAttributes->setInteger10(initParams.m_Ints[i]);  break;
                    }
                }
                if (initParams.m_BoolsSetBits & (1 << i))
                {
                    switch (i)
                    {
                        case 0: searchAttributes->setBoolean1(initParams.m_Bools[i]); break;
                        case 1: searchAttributes->setBoolean2(initParams.m_Bools[i]); break;
                        case 2: searchAttributes->setBoolean3(initParams.m_Bools[i]); break;
                        case 3: searchAttributes->setBoolean4(initParams.m_Bools[i]); break;
                        case 4: searchAttributes->setBoolean5(initParams.m_Bools[i]); break;
                        case 5: searchAttributes->setBoolean6(initParams.m_Bools[i]); break;
                        case 6: searchAttributes->setBoolean7(initParams.m_Bools[i]); break;
                        case 7: searchAttributes->setBoolean8(initParams.m_Bools[i]); break;
                        case 8: searchAttributes->setBoolean9(initParams.m_Bools[i]); break;
                        case 9: searchAttributes->setBoolean10(initParams.m_Bools[i]); break;
                    }
                }
            }
            ret = requestGameSessionPtr->setSearchAttributes(searchAttributes);
            if (ret < 0)
            {
                return ret;
            }
        }

        requestGameSessionPtr->setSearchable(initParams.m_Searchable);

        if (initParams.m_CustomDataSize1 > 0)
        {
            ret = requestGameSessionPtr->setCustomData1(initParams.m_CustomData1, initParams.m_CustomDataSize1);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (initParams.m_CustomDataSize2 > 0)
        {
            ret = requestGameSessionPtr->setCustomData2(initParams.m_CustomData2, initParams.m_CustomDataSize2);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestGameSession> > requestGameSessions(libContextPtr);
        ret = requestGameSessions.pushBack(requestGameSessionPtr);
        if (ret < 0)
        {
            return ret;
        }

        // postGameSessionsRequestBody
        IntrusivePtr<PostGameSessionsRequestBody> postGameSessionsRequestBody;
        ret = PostGameSessionsRequestBodyFactory::create(libContextPtr, requestGameSessions, &postGameSessionsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToCreateGameSessions
        GameSessionsApi::ParameterToCreateGameSessions param;
        ret = param.initialize(libContextPtr, postGameSessionsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostGameSessionsResponseBody> PostGameSessionsResponseType;
        PostGameSessionsResponseType response;

        Common::Transaction<PostGameSessionsResponseType> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::createGameSessions(userCtx->GetUserCtxId(), param, transaction);
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

        if (!(response)) return -1;

        IntrusivePtr<Vector<IntrusivePtr<GameSession> > > gameSessionsPtr = response->getGameSessions();

        if (!(gameSessionsPtr)) return -1;

        if (gameSessionsPtr->empty()) return -1;

        IntrusivePtr<GameSession>& gameSessionPtr = (*gameSessionsPtr)[0];

        if (!(gameSessionPtr)) return -1;

        writer.WriteString(gameSessionPtr->getSessionId().c_str());

        Common::IntrusivePtr<ResponseGameSessionMemberPlayer> member = gameSessionPtr->getMember();
        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponseGameSessionPlayer> > > players = member->getPlayers();

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            writer.WriteUInt64(it->getAccountId());
            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
        }

        // Write out account id for the user who created the session
        writer.WriteUInt64(creator.m_AccountId);

        param.terminate();
        transaction.finish();

        return ret;
    }

    int GameSessionCommands::Leave(SceUserServiceUserId userId, const char* sessionId)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            return -1;
        }

        // Get the account id of the user as string
        char accountIdBuf[21];
        ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        if (ret < 0)
        {
            return ret;
        }

        // prepare params
        GameSessionsApi::ParameterToLeaveGameSession param;
        ret = param.initialize(libContextPtr, sessionId, accountIdBuf);
        if (ret < 0)
        {
            return ret;
        }

        Common::Transaction<Common::DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::leaveGameSession(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        // getResponse
        Common::DefaultResponse response;
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        param.terminate();

        transaction.finish();

        return 0;
    }

    int GameSessionCommands::JoinAsPlayer(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        OrderGuaranteedPushEvent* pushEvent = NULL;

        if (swapping == false)
        {
            pushEvent = WebApiNotifications::FindOrderedPushEvent(pushCallbackId);

            if (pushEvent == NULL)
            {
                return -1;
            }
        }

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            return -1;
        }

        // Get the account id of the user as string
        char accountIdBuf[21];
        ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        if (ret < 0)
        {
            return ret;
        }

        // prepare params
        // RequestJoinGameSessionPlayer
        IntrusivePtr<RequestJoinGameSessionPlayer> requestJoinGameSessionPlayerPtr;
        ret = RequestJoinGameSessionPlayerFactory::create(libContextPtr, accountIdBuf, Utils::GetThisPlatformString(), &requestJoinGameSessionPlayerPtr);

        if (ret < 0)
        {
            return ret;
        }

        // WARNING - Can't be set if performing a swap - spectator to player
        if (swapping == false)
        {
            // PlayerSessionPushContext
            IntrusivePtr<GameSessionPushContext> gameSessionPushContextPtr;
            ret = GameSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            Vector<IntrusivePtr<GameSessionPushContext> > gameSessionPushContexts(libContextPtr);
            ret = gameSessionPushContexts.pushBack(gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // set PushContexts
            ret = requestJoinGameSessionPlayerPtr->setPushContexts(gameSessionPushContexts);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestJoinGameSessionPlayer> > requestJoinGameSessionPlayers(libContextPtr);
        ret = requestJoinGameSessionPlayers.pushBack(requestJoinGameSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        // PostPlayerSessionsSessionIdMemberPlayersRequestBody
        IntrusivePtr<PostGameSessionsSessionIdMemberPlayersRequestBody> postGameSessionsSessionIdMemberPlayersRequestBody;
        ret = PostGameSessionsSessionIdMemberPlayersRequestBodyFactory::create(libContextPtr, requestJoinGameSessionPlayers, &postGameSessionsSessionIdMemberPlayersRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToJoinPlayerSessionAsPlayer
        GameSessionsApi::ParameterToJoinGameSessionAsPlayer param;
        ret = param.initialize(libContextPtr, sessionId, postGameSessionsSessionIdMemberPlayersRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostGameSessionsSessionIdMemberPlayersResponseBody> PostGameSessionsSessionIdMemberPlayersResponseBody;
        PostGameSessionsSessionIdMemberPlayersResponseBody response;

        Common::Transaction<PostGameSessionsSessionIdMemberPlayersResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::joinGameSessionAsPlayer(userCtx->m_webapiUserCtxId, param, transaction);
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

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponseGameSessionPlayer> > > players = response->getPlayers();

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            writer.WriteUInt64(it->getAccountId());
            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
        }

        SceNpAccountId accountId = SCE_NP_INVALID_ACCOUNT_ID;
        sceNpGetAccountIdA(userId, &accountId);

        writer.WriteUInt64(accountId);

        param.terminate();
        transaction.finish();

        return ret;
    }

    int GameSessionCommands::JoinAsSpectator(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        OrderGuaranteedPushEvent* pushEvent = NULL;

        if (swapping == false)
        {
            pushEvent = WebApiNotifications::FindOrderedPushEvent(pushCallbackId);

            if (pushEvent == NULL)
            {
                return -1;
            }
        }

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            return -1;
        }

        // Get the account id of the user as string
        char accountIdBuf[21];
        ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        if (ret < 0)
        {
            return ret;
        }

        // prepare params
        // RequestPlayerSessionSpectator
        IntrusivePtr<RequestGameSessionSpectator> requestGameSessionSpectatorPtr;
        ret = RequestGameSessionSpectatorFactory::create(libContextPtr, accountIdBuf, Utils::GetThisPlatformString(), &requestGameSessionSpectatorPtr);
        if (ret < 0)
        {
            return ret;
        }

        // WARNING - Can't be set if performing a swap - player to spectator
        if (swapping == false)
        {
            // PlayerSessionPushContext
            IntrusivePtr<GameSessionPushContext> gameSessionPushContextPtr;
            ret = GameSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            Vector<IntrusivePtr<GameSessionPushContext> > gameSessionPushContexts(libContextPtr);
            ret = gameSessionPushContexts.pushBack(gameSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // set PushContexts
            ret = requestGameSessionSpectatorPtr->setPushContexts(gameSessionPushContexts);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestGameSessionSpectator> > requestGameSessionSpectators(libContextPtr);
        ret = requestGameSessionSpectators.pushBack(requestGameSessionSpectatorPtr);
        if (ret < 0)
        {
            return ret;
        }

        // PostPlayerSessionsSessionIdMemberPlayersRequestBody
        IntrusivePtr<PostGameSessionsSessionIdMemberSpectatorsRequestBody> postGameSessionsSessionIdMemberSpectatorsRequestBody;
        ret = PostGameSessionsSessionIdMemberSpectatorsRequestBodyFactory::create(libContextPtr, requestGameSessionSpectators, &postGameSessionsSessionIdMemberSpectatorsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToJoinGameSessionAsSpectator
        GameSessionsApi::ParameterToJoinGameSessionAsSpectator param;
        ret = param.initialize(libContextPtr, sessionId, postGameSessionsSessionIdMemberSpectatorsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostGameSessionsSessionIdMemberSpectatorsResponseBody> PostGameSessionsSessionIdMemberSpectatorsResponseBody;
        PostGameSessionsSessionIdMemberSpectatorsResponseBody response;

        Common::Transaction<PostGameSessionsSessionIdMemberSpectatorsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::joinGameSessionAsSpectator(userCtx->m_webapiUserCtxId, param, transaction);
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

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponseGameSessionSpectator> > > spectators = response->getSpectators();

        writer.WriteInt32(spectators->size());

        for (auto& it : *spectators)
        {
            writer.WriteUInt64(it->getAccountId());
            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
        }

        SceNpAccountId accountId = SCE_NP_INVALID_ACCOUNT_ID;
        sceNpGetAccountIdA(userId, &accountId);

        writer.WriteUInt64(accountId);

        param.terminate();
        transaction.finish();

        return ret;
    }

    void GameSessionCommands::GetGameSessionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* sessionIds = reader.ReadStringPtr();
        UInt32 numFields = reader.ReadUInt32();

        Vector<String>* fields = new Vector<String>(libContextPtr);

        for (int i = 0; i < numFields; i++)
        {
            const char* str = reader.ReadStringPtr();
            ret = addStringToVector(libContextPtr, str, *fields);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        IntrusivePtr<Vector<String> > fieldsPtr(fields, Utils::DeletePtr, libContextPtr);

        GameSessionsApi::ParameterToGetGameSessions param;
        ret = param.initialize(libContextPtr, sessionIds);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.setfields(fieldsPtr);

        typedef Common::IntrusivePtr<GetGameSessionsResponseBody> GetGameSessionsResponseBody;
        GetGameSessionsResponseBody response;

        Common::Transaction<GetGameSessionsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::getGameSessions(userCtx->m_webapiUserCtxId, param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<GameSessionForRead> > > gameSessionsPtr = response->getGameSessions();

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        SerialiseSessionInfo(writer, gameSessionsPtr);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void GameSessionCommands::SerialiseSessionInfo(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<GameSessionForRead> > > gameSessionsPtr)
    {
        if (!(gameSessionsPtr)) return;

        if (gameSessionsPtr->empty())
        {
            // User left session
            writer.WriteBool(false); // not in session
            return;
        }

        writer.WriteBool(true); // in session

        int numSessions = gameSessionsPtr->size();

        writer.WriteInt32(numSessions);

        for (int session = 0; session < numSessions; session++)
        {
            IntrusivePtr<GameSessionForRead>& gameSessionPtr = (*gameSessionsPtr)[session];

            if (!(gameSessionPtr))
            {
                writer.WriteBool(false);
                return;
            }
            writer.WriteBool(true);

            if (gameSessionPtr->sessionIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(gameSessionPtr->getSessionId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->createdTimestampIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(gameSessionPtr->getCreatedTimestamp().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->maxPlayersIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32(gameSessionPtr->getMaxPlayers());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->maxSpectatorsIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32(gameSessionPtr->getMaxSpectators());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->memberIsSet())
            {
                IntrusivePtr<GameSessionMemberForRead> member = gameSessionPtr->getMember();
                writer.WriteBool(true);

                if (member->playersIsSet())
                {
                    writer.WriteBool(true);

                    IntrusivePtr<Vector<IntrusivePtr<GameSessionPlayer> > > players = member->getPlayers();

                    writer.WriteInt32(players->size());

                    for (auto& it : *players)
                    {
                        writer.WriteBool(false); // Not spectator
                        writer.WriteUInt64(it->getAccountId());
                        writer.WriteString(it->getOnlineId().data);
                        writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
                        writer.WriteString(it->getJoinTimestamp().c_str());

                        if (it->joinStateIsSet())
                        {
                            writer.WriteBool(true);

                            writer.WriteInt32((Int32)it->getJoinState());
                        }
                        else
                        {
                            writer.WriteBool(false);
                        }

                        if (it->customData1IsSet())
                        {
                            writer.WriteBool(true);
                            Binary* binary = it->getCustomData1().get();
                            writer.WriteData((char*)binary->getBinary(), binary->size());
                        }
                        else
                        {
                            writer.WriteBool(false);
                        }
                    }
                }
                else
                {
                    writer.WriteBool(false);
                }

                if (member->spectatorsIsSet())
                {
                    writer.WriteBool(true);

                    IntrusivePtr<Vector<IntrusivePtr<GameSessionSpectator> > > spectators = member->getSpectators();

                    writer.WriteInt32(spectators->size());

                    for (auto& it : *spectators)
                    {
                        writer.WriteBool(true); // Spectator
                        writer.WriteUInt64(it->getAccountId());
                        writer.WriteString(it->getOnlineId().data);
                        writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
                        writer.WriteString(it->getJoinTimestamp().c_str());

                        if (it->joinStateIsSet())
                        {
                            writer.WriteBool(true);

                            writer.WriteInt32((Int32)it->getJoinState());
                        }
                        else
                        {
                            writer.WriteBool(false);
                        }

                        if (it->customData1IsSet())
                        {
                            writer.WriteBool(true);
                            Binary* binary = it->getCustomData1().get();
                            writer.WriteData((char*)binary->getBinary(), binary->size());
                        }
                        else
                        {
                            writer.WriteBool(false);
                        }
                    }
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

            if (gameSessionPtr->joinDisabledIsSet())
            {
                writer.WriteBool(true);
                writer.WriteBool(gameSessionPtr->getJoinDisabled());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->supportedPlatformsIsSet())
            {
                writer.WriteBool(true);

                uint32_t platformFlags = 0;

                IntrusivePtr<Vector<String> > platforms = gameSessionPtr->getSupportedPlatforms();

                for (auto& it : *platforms)
                {
                    platformFlags |= Utils::GetPlatformFlag(it.c_str());
                }

                writer.WriteUInt32(platformFlags);
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->representativeIsSet())
            {
                writer.WriteBool(true);

                writer.WriteUInt64(gameSessionPtr->getRepresentative()->getAccountId());
                writer.WriteString(gameSessionPtr->getRepresentative()->getOnlineId().data);
                writer.WriteUInt32(Utils::GetPlatformFlag(gameSessionPtr->getRepresentative()->getPlatform().c_str()));
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->customData1IsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Binary> customData1 = gameSessionPtr->getCustomData1();

                writer.WriteData((const char*)customData1->getBinary(), customData1->size());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->customData2IsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Binary> customData2 = gameSessionPtr->getCustomData2();

                writer.WriteData((const char*)customData2->getBinary(), customData2->size());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->usePlayerSessionIsSet())
            {
                writer.WriteBool(true);
                writer.WriteBool(gameSessionPtr->getUsePlayerSession());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->matchmakingIsSet())
            {
                writer.WriteBool(true);

                MatchmakingForRead* matchMakingPtr = gameSessionPtr->getMatchmaking().get();

                writer.WriteString(matchMakingPtr->getOfferId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (gameSessionPtr->reservationTimeoutSecondsIsSet())
            {
                writer.WriteBool(true);
                writer.WriteInt32(gameSessionPtr->getReservationTimeoutSeconds());
            }
            else
            {
                writer.WriteBool(false);
            }
        }
    }

    void GameSessionCommands::SetGameSessionPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* sessionId = reader.ReadStringPtr();

        ret = SetGameSessionProps(userCtx, sessionId, reader);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    int GameSessionCommands::SetGameSessionProps(WebApiUserContext* userCtx, const char* sessionId, BinaryReader& reader)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        IntrusivePtr<PatchGameSessionsSessionIdRequestBody> patchGameSessionsSessionIdRequestBody;
        ret = PatchGameSessionsSessionIdRequestBodyFactory::create(libContextPtr, &patchGameSessionsSessionIdRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        bool isSetMaxPlayers = reader.ReadBool();
        if (isSetMaxPlayers)
        {
            UInt32 maxPlayers = reader.ReadUInt32();
            patchGameSessionsSessionIdRequestBody->setMaxPlayers(maxPlayers);
        }

        bool isSetMaxSpectators = reader.ReadBool();
        if (isSetMaxSpectators)
        {
            UInt32 maxSpectators = reader.ReadUInt32();
            patchGameSessionsSessionIdRequestBody->setMaxSpectators(maxSpectators);
        }

        bool isSetJoinDisabled = reader.ReadBool();
        if (isSetJoinDisabled)
        {
            bool joinDisabled = reader.ReadBool();
            patchGameSessionsSessionIdRequestBody->setJoinDisabled(joinDisabled);
        }

        bool isSetCustomData1 = reader.ReadBool();
        if (isSetCustomData1)
        {
            int dataSize = reader.ReadInt32();
            void* data = reader.ReadDataPtr(dataSize);

            int ret = patchGameSessionsSessionIdRequestBody->setCustomData1(data, dataSize);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetCustomData2 = reader.ReadBool();
        if (isSetCustomData2)
        {
            int dataSize = reader.ReadInt32();
            void* data = reader.ReadDataPtr(dataSize);

            int ret = patchGameSessionsSessionIdRequestBody->setCustomData2(data, dataSize);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetSearchable = reader.ReadBool();
        if (isSetSearchable)
        {
            bool searchable = reader.ReadBool();
            patchGameSessionsSessionIdRequestBody->setSearchable(searchable);
        }

        GameSessionsApi::ParameterToSetGameSessionProperties param;
        ret = param.initialize(libContextPtr, sessionId, patchGameSessionsSessionIdRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // Api Call
        ret = GameSessionsApi::setGameSessionProperties(userCtx->m_webapiUserCtxId, param, transaction);
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

        param.terminate();
        transaction.finish();

        return 0;
    }

    void GameSessionCommands::SetGameSessionMemberSystemPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* sessionId = reader.ReadStringPtr();

        int dataSize = reader.ReadInt32();
        void* data = reader.ReadDataPtr(dataSize);

        // Get the account id of the user as string
        char accountIdBuf[21];
        ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::IntrusivePtr<PatchGameSessionsSessionIdMembersAccountIdRequestBody> patchGameSessionsSessionIdMembersAccountIdRequestBody;
        ret = PatchGameSessionsSessionIdMembersAccountIdRequestBodyFactory::create(libContextPtr, &patchGameSessionsSessionIdMembersAccountIdRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = patchGameSessionsSessionIdMembersAccountIdRequestBody->setCustomData1(data, dataSize);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        GameSessionsApi::ParameterToSetGameSessionMemberSystemProperties param;
        ret = param.initialize(libContextPtr, sessionId, accountIdBuf, patchGameSessionsSessionIdMembersAccountIdRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        ret = GameSessionsApi::setGameSessionMemberSystemProperties(userCtx->m_webapiUserCtxId, param, transaction);
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

    void GameSessionCommands::SendGameSessionMessageImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* sessionId = reader.ReadStringPtr();

        const char* payload = reader.ReadStringPtr();

        int count = reader.ReadInt32();

        Vector<IntrusivePtr<MemberWithMultiPlatform> > to(libContextPtr);

        for (int i = 0; i < count; i++)
        {
            SceNpAccountId accountId = reader.ReadUInt64();
            uint32_t platformFlag = reader.ReadUInt32();
            const char * platformStr = Utils::ToPlatformString(platformFlag);
            assert(platformStr);

            IntrusivePtr<MemberWithMultiPlatform> mp;
            ret = MemberWithMultiPlatformFactory::create(libContextPtr, accountId, platformStr, &mp);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            // add to Vector
            ret = to.pushBack(mp);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        Common::IntrusivePtr<PostGameSessionsSessionIdSessionMessageRequestBody> postGameSessionsSessionIdSessionMessageRequestBody;
        ret = PostGameSessionsSessionIdSessionMessageRequestBodyFactory::create(libContextPtr, to, payload, &postGameSessionsSessionIdSessionMessageRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        GameSessionsApi::ParameterToSendGameSessionMessage param;
        ret = param.initialize(libContextPtr, sessionId, postGameSessionsSessionIdSessionMessageRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::sendGameSessionMessage(userCtx->m_webapiUserCtxId, param, transaction);
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

    void GameSessionCommands::GetJoinedGameSessionsByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        // Get the account id of the user as string
        char accountIdBuf[21];
        ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        GameSessionsApi::ParameterToGetJoinedGameSessionsByUser param;
        ret = param.initialize(libContextPtr, accountIdBuf);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = param.setmemberFilter("player,spectator");
        if (ret < 0)
        {
            param.terminate();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = param.setplatformFilter("PS4,PS5");
        if (ret < 0)
        {
            param.terminate();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<GetUsersAccountIdGameSessionsResponseBody> GetUsersAccountIdGameSessionsResponseBody;
        GetUsersAccountIdGameSessionsResponseBody response;

        Common::Transaction<GetUsersAccountIdGameSessionsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::getJoinedGameSessionsByUser(userCtx->m_webapiUserCtxId, param, transaction);
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

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        IntrusivePtr<Vector<IntrusivePtr<JoinedGameSessionWithPlatform> > > gameSessions = response->getGameSessions();

        writer.WriteInt32(gameSessions.get()->size());

        for (auto& it : *gameSessions)
        {
            writer.WriteString(it->getSessionId().c_str());

            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));

            //it->joinStateIsSet
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void GameSessionCommands::DeleteGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* sessionId = reader.ReadStringPtr();

        GameSessionsApi::ParameterToDeleteGameSession param;
        ret = param.initialize(libContextPtr, sessionId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::deleteGameSession(userCtx->m_webapiUserCtxId, param, transaction);
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

    // Post a GameSessions search
    // see Basic Usage of Game Session Search https://p.siedev.net/resources/documents/WebAPI/1/Session_Manager_WebAPI-Overview/0003.html#__document_toc_00000008
    void GameSessionCommands::GameSessionsSearchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        const char* searchIndex = reader.ReadStringPtr();

        int numConditions = reader.ReadInt32();
        Common::Vector<Common::IntrusivePtr<SearchCondition> > searchConditions(libContextPtr);
        if (numConditions > 0)
        {
            for (int cond = 0; cond < numConditions; cond++)
            {
                SearchAttribute attribute = (SearchAttribute)reader.ReadInt32();
                SearchOperator op = (SearchOperator)reader.ReadInt32();

                Common::IntrusivePtr<SearchCondition> searchCondition;
                ret = SearchConditionFactory::create(libContextPtr, attribute, op, &searchCondition);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }

                const char*  value = reader.ReadStringPtr();
                int numvalues = reader.ReadInt32();

                if (op != SearchOperator::kIn)
                {
                    ret = searchCondition->setValue(value);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                    if (numvalues != 0)
                    {
                        ERROR_RESULT(result, "Unexpected values in SearchCondition");
                        return;
                    }
                }
                else
                {
                    Common::Vector<Common::String>* values = new Common::Vector<Common::String>(libContextPtr);
                    for (int val = 0; val < numvalues; val++)
                    {
                        const char*  value = reader.ReadStringPtr();
                        ret = addStringToVector(libContextPtr, value, *values);
                        if (ret < 0)
                        {
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }
                    }
                    ret = searchCondition->setValues(*values);
                    if (ret < 0)
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }
                ret = searchConditions.pushBack(searchCondition);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }
        }

        // GameSessionsSearchRequestBody
        Common::IntrusivePtr<PostGameSessionsSearchRequestBody> postGameSessionsSearchRequestBody;
        ret = PostGameSessionsSearchRequestBodyFactory::create(libContextPtr, searchConditions, &postGameSessionsSearchRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        GameSessionsApi::ParameterToPostGameSessionsSearch param;
        ret = param.initialize(libContextPtr, searchIndex, postGameSessionsSearchRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<PostGameSessionsSearchResponseBody> PostGameSessionsSearchResponseType;
        PostGameSessionsSearchResponseType response;

        Common::Transaction<PostGameSessionsSearchResponseType> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = GameSessionsApi::postGameSessionsSearch(userCtx->GetUserCtxId(), param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        IntrusivePtr<Vector<IntrusivePtr<SearchGameSession> > > gameSessionsPtr = response->getGameSessions();

        int sessionCount = gameSessionsPtr->size(); // can be zero!
        writer.WriteInt32(sessionCount);
        for (int session = 0; session < sessionCount; session++)
        {
            IntrusivePtr<SearchGameSession>& gameSessionPtr = (*gameSessionsPtr)[session];
            writer.WriteString(gameSessionPtr->getSessionId().c_str());
        }


        param.terminate();
        transaction.finish();

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
