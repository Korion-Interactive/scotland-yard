#include "PlayerSession.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>

namespace psn
{
    //SessionMap<PlayerSession> PlayerSessionCommands::s_PlayerSessions;

    void PlayerSessionCommands::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::CreatePlayerSession, PlayerSessionCommands::CreatePlayerSessionImpl);
        MsgHandler::AddMethod(Methods::LeavePlayerSession, PlayerSessionCommands::LeavePlayerSessionImpl);
        MsgHandler::AddMethod(Methods::JoinPlayerSession, PlayerSessionCommands::JoinPlayerSessionImpl);
        MsgHandler::AddMethod(Methods::GetPlayerSessions, PlayerSessionCommands::GetPlayerSessionsImpl);
        MsgHandler::AddMethod(Methods::SendPlayerSessionsInvitation, PlayerSessionCommands::SendPlayerSessionsInvitationImpl);

        MsgHandler::AddMethod(Methods::GetPlayerSessionInvitations, PlayerSessionCommands::GetPlayerSessionInvitationsImpl);
        MsgHandler::AddMethod(Methods::SetPlayerSessionProperties, PlayerSessionCommands::SetPlayerSessionPropertiesImpl);
        MsgHandler::AddMethod(Methods::ChangePlayerSessionLeader, PlayerSessionCommands::ChangePlayerSessionLeaderImpl);
        MsgHandler::AddMethod(Methods::AddPlayerSessionJoinableSpecifiedUsers, PlayerSessionCommands::AddPlayerSessionJoinableSpecifiedUsersImpl);
        MsgHandler::AddMethod(Methods::DeletePlayerSessionJoinableSpecifiedUsers, PlayerSessionCommands::DeletePlayerSessionJoinableSpecifiedUsersImpl);
        MsgHandler::AddMethod(Methods::SetPlayerSessionMemberSystemProperties, PlayerSessionCommands::SetPlayerSessionMemberSystemPropertiesImpl);
        MsgHandler::AddMethod(Methods::SendPlayerSessionMessage, PlayerSessionCommands::SendPlayerSessionMessageImpl);
        MsgHandler::AddMethod(Methods::GetJoinedPlayerSessionsByUser, PlayerSessionCommands::GetJoinedPlayerSessionsByUserImpl);
    }

//  const char* PlayerSessionCommands::GetThisPlatformString()
//  {
//#ifdef __PROSPERO__
//      return "PS5";
//#else
//      return "PS4";
//#endif
//  }

    /*int PlayerSessionCommands::AddPlatformStrings(uint32_t platformFlags, Vector< String >& supportedPlatforms)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        int ret = 0;

        if ((platformFlags & PLATFORM_PS5_FLAG) != 0)
        {
            ret = addStringToVector(libContextPtr, "PS5", supportedPlatforms);
            if (ret < 0) return ret;
        }

        if ((platformFlags & PLATFORM_PS4_FLAG) != 0)
        {
            ret = addStringToVector(libContextPtr, "PS4", supportedPlatforms);
            if (ret < 0) return ret;
        }

        return ret;
    }*/

    int PlayerSessionCommands::LocalisedStrings::Deserialise(BinaryReader& reader)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        const char* defaultStr = reader.ReadStringPtr();

        int strCount = reader.ReadInt32();

        sce::Json::Object localizedText;

        for (int i = 0; i < strCount; i++)
        {
            const char* localeStr = reader.ReadStringPtr();
            const char* textStr = reader.ReadStringPtr();

            localizedText[localeStr] = sce::Json::String(textStr);
        }

        //localizedText["ja-JP"] = sce::Json::String("ja-JP のセッション名");

        int ret = LocalizedStringFactory::create(libContextPtr, defaultStr, localizedText, &m_LocalizedStringPtr);

        return ret;
    }

    PlayerSessionCommands::InitializationParams::InitializationParams() :
        m_MaxPlayers(0),
        m_MaxSpectators(0),
        m_SwapSupported(false),
        m_JoinDisabled(false),
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        m_JoinableUserType(JoinableUserType::kNoOne),
        m_InvitableUserType(InvitableUserType::kNoOne),
#else
        m_JoinableUserType(JoinableUserType::NO_ONE),
        m_InvitableUserType(InvitableUserType::NO_ONE),
#endif
        m_PlatformFlags(0),
        m_LeaderPrivileges(NULL),
        m_ExclusiveLeaderPrivileges(NULL),
        m_DisableSystemUiMenu(NULL),
        m_CustomDataSize1(0),
        m_CustomData1(NULL),
        m_CustomDataSize2(0),
        m_CustomData2(NULL)
    {
    }

    PlayerSessionCommands::InitializationParams::~InitializationParams()
    {
        if (m_LeaderPrivileges != NULL) delete m_LeaderPrivileges;
        if (m_ExclusiveLeaderPrivileges != NULL) delete m_ExclusiveLeaderPrivileges;
        if (m_DisableSystemUiMenu != NULL) delete m_DisableSystemUiMenu;
    }

    void PlayerSessionCommands::InitializationParams::Deserialise(BinaryReader& reader)
    {
        m_UserId = reader.ReadInt32();
        m_MaxPlayers = reader.ReadUInt32();
        m_MaxSpectators = reader.ReadUInt32();
        m_SwapSupported = reader.ReadBool();
        m_JoinDisabled = reader.ReadBool();
        m_JoinableUserType = (sceSessionManager::JoinableUserType)reader.ReadUInt32();
        m_InvitableUserType = (sceSessionManager::InvitableUserType)reader.ReadUInt32();
        m_PlatformFlags = reader.ReadUInt32();

        m_SessionName.Deserialise(reader);

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        m_LeaderPrivileges = new Vector<String>(libContextPtr);
        m_ExclusiveLeaderPrivileges = new Vector<String>(libContextPtr);
        m_DisableSystemUiMenu = new Vector<String>(libContextPtr);

        PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, m_LeaderPrivileges);
        PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, m_ExclusiveLeaderPrivileges);
        PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, m_DisableSystemUiMenu);

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
    }

    int PlayerSessionCommands::DeserialiseLeaderPrivileges(BinaryReader& reader, Vector<String> *strings)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        int ret = 0;

        int count = reader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            const char* str = reader.ReadStringPtr();

            int ret = addStringToVector(libContextPtr, str, *strings);
            if (ret < 0)
            {
                return ret;
            }
        }

        return ret;
    }

    void PlayerSessionCommands::CreatePlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 pushCallbackId = reader.ReadInt32();

        InitializationParams params;
        params.Deserialise(reader);

        int creatorDataSize1 = reader.ReadInt32();
        void* creatorCustomData1 = NULL;
        if (creatorDataSize1 > 0)
        {
            creatorCustomData1 = reader.ReadDataPtr(creatorDataSize1);
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        int ret = Create(pushCallbackId, creatorCustomData1, creatorDataSize1, params, writer);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::LeavePlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

    void PlayerSessionCommands::JoinPlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

    void DeletePtr(Vector<String>* ptr)
    {
        delete ptr;
    }

    void PlayerSessionCommands::GetPlayerSessionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        IntrusivePtr<Vector<String> > fieldsPtr(fields, DeletePtr, libContextPtr);

        PlayerSessionsApi::ParameterToGetPlayerSessions param;
        ret = param.initialize(libContextPtr, sessionIds);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.setfields(fieldsPtr);

        typedef Common::IntrusivePtr<GetPlayerSessionsResponseBody> GetPlayerSessionsResponseBody;
        GetPlayerSessionsResponseBody response;

        Common::Transaction<GetPlayerSessionsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::getPlayerSessions(userCtx->m_webapiUserCtxId, param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<PlayerSessionForRead> > > playerSessionsPtr = response->getPlayerSessions();

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        SerialiseSessionInfo(writer, playerSessionsPtr);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::SendPlayerSessionsInvitationImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();
        const char* sessionId = reader.ReadStringPtr();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Int32 numAccountIds = reader.ReadInt32();

        Vector<IntrusivePtr<RequestPlayerSessionInvitation> > invitations(libContextPtr);

        for (int i = 0; i < numAccountIds; i++)
        {
            UInt64 accountId = reader.ReadUInt64();
            IntrusivePtr<To> to;
            ret = ToFactory::create(libContextPtr, accountId, &to);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            IntrusivePtr<RequestPlayerSessionInvitation> requestPlayerSessionInvitation;
            ret = RequestPlayerSessionInvitationFactory::create(libContextPtr, to, &requestPlayerSessionInvitation);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            ret = invitations.pushBack(requestPlayerSessionInvitation);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        IntrusivePtr<PostPlayerSessionsSessionIdInvitationsRequestBody> postPlayerSessionsSessionIdInvitationsRequestBody;
        ret = PostPlayerSessionsSessionIdInvitationsRequestBodyFactory::create(libContextPtr, &postPlayerSessionsSessionIdInvitationsRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = postPlayerSessionsSessionIdInvitationsRequestBody->setInvitations(invitations);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        PlayerSessionsApi::ParameterToSendPlayerSessionInvitations param;
        ret = param.initialize(libContextPtr, sessionId, postPlayerSessionsSessionIdInvitationsRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<PostPlayerSessionsSessionIdInvitationsResponseBody> PostPlayerSessionsSessionIdInvitationsResponseBody;
        PostPlayerSessionsSessionIdInvitationsResponseBody response;

        Common::Transaction<PostPlayerSessionsSessionIdInvitationsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::sendPlayerSessionInvitations(userCtx->m_webapiUserCtxId, param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<ResponsePlayerSessionInvitation> > > invitationsPtr = response->getInvitations();

        if (!(invitationsPtr))
        {
            writer.WriteInt32(0);
        }
        else
        {
            writer.WriteInt32(invitationsPtr->size());

            for (auto& it : *invitationsPtr)
            {
                writer.WriteString(it->getInvitationId().c_str());
            }
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

//  const size_t INVITATION_INVALID_FILTER_SIZE = 11; // "true,false\0"

    #define INVITATION_INVALID_FILTER_VALID_ONLY "false"
    #define INVITATION_INVALID_FILTER_INVALID_ONLY "true"
    #define INVITATION_INVALID_FILTER_ALL "true,false\0"

    void PlayerSessionCommands::GetPlayerSessionInvitationsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

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

        Int32 filter = reader.ReadInt32();

        const char * invitationInvalidFilter = NULL;

        if (filter == 0)
        {
            invitationInvalidFilter = INVITATION_INVALID_FILTER_VALID_ONLY;
        }
        else if (filter == 1)
        {
            invitationInvalidFilter = INVITATION_INVALID_FILTER_INVALID_ONLY;
        }
        else if (filter == 2)
        {
            invitationInvalidFilter = INVITATION_INVALID_FILTER_ALL;
        }

        IntrusivePtr<Vector<String> > fieldsPtr(fields, DeletePtr, libContextPtr);

        PlayerSessionsApi::ParameterToGetPlayerSessionInvitations param;
        ret = param.initialize(libContextPtr, accountIdBuf);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = param.setinvitationInvalidFilter(invitationInvalidFilter);
        if (ret < 0)
        {
            param.terminate();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.setfields(fieldsPtr);

        typedef Common::IntrusivePtr<GetUsersAccountIdPlayerSessionsInvitationsResponseBody> GetUsersAccountIdPlayerSessionsInvitationsResponseBody;
        GetUsersAccountIdPlayerSessionsInvitationsResponseBody response;

        Common::Transaction<GetUsersAccountIdPlayerSessionsInvitationsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::getPlayerSessionInvitations(userCtx->m_webapiUserCtxId, param, transaction);
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

        if (!(response->invitationsIsSet()))
        {
            // No invitations
            writer.WriteBool(false);
        }
        else
        {
            writer.WriteBool(true);

            IntrusivePtr<Vector<IntrusivePtr<UsersPlayerSessionsInvitationForRead> > > invitations = response->getInvitations();

            writer.WriteInt32(invitations.get()->size());

            for (auto& it : *invitations)
            {
                // "invitationId"
                if (it->invitationIdIsSet())
                {
                    writer.WriteBool(true);
                    writer.WriteString(it->getInvitationId().c_str());
                }
                else
                {
                    writer.WriteBool(false);
                }

                // "from"
                if (it->fromIsSet())
                {
                    writer.WriteBool(true);
                    IntrusivePtr<FromMember> fromMemberPtr = it->getFrom();
                    writer.WriteUInt64(fromMemberPtr->getAccountId());
                    writer.WriteString(fromMemberPtr->getOnlineId().data);
                    writer.WriteUInt32(Utils::GetPlatformFlag(fromMemberPtr->getPlatform().c_str()));
                }
                else
                {
                    writer.WriteBool(false);
                }

                // "sessionId"
                if (it->sessionIdIsSet())
                {
                    writer.WriteBool(true);
                    writer.WriteString(it->getSessionId().c_str());
                }
                else
                {
                    writer.WriteBool(false);
                }

                // "supportedPlatforms"
                if (it->supportedPlatformsIsSet())
                {
                    writer.WriteBool(true);

                    uint32_t platformFlags = 0;

                    IntrusivePtr<Vector<String> > platforms = it->getSupportedPlatforms();

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

                // "receivedTimestamp"
                if (it->receivedTimestampIsSet())
                {
                    writer.WriteBool(true);
                    writer.WriteString(it->getReceivedTimestamp().c_str());
                }
                else
                {
                    writer.WriteBool(false);
                }

                // "invitationInvalid"
                if (it->invitationInvalidIsSet())
                {
                    writer.WriteBool(true);
                    writer.WriteBool(it->getInvitationInvalid());
                }
                else
                {
                    writer.WriteBool(false);
                }
            }
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::SetPlayerSessionPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        ret = SetPlayerSessionProps(userCtx, sessionId, reader);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::ChangePlayerSessionLeaderImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        SceNpAccountId accountId = reader.ReadUInt64();

        const char* platform = reader.ReadStringPtr();

        IntrusivePtr<PutPlayerSessionsSessionIdLeaderRequestBody> putPlayerSessionsSessionIdLeaderRequestBodyPtr;
        ret = PutPlayerSessionsSessionIdLeaderRequestBodyFactory::create(libContextPtr, accountId, platform, &putPlayerSessionsSessionIdLeaderRequestBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        PlayerSessionsApi::ParameterToChangePlayerSessionLeader param;
        ret = param.initialize(libContextPtr, sessionId, putPlayerSessionsSessionIdLeaderRequestBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = param.setsessionId(sessionId);
        if (ret < 0)
        {
            param.terminate();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        ret = PlayerSessionsApi::changePlayerSessionLeader(userCtx->m_webapiUserCtxId, param, transaction);
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

    void PlayerSessionCommands::AddPlayerSessionJoinableSpecifiedUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        int count = reader.ReadInt32();

        Vector<IntrusivePtr<JoinableUser> > joinableSpecifiedUsers(libContextPtr);

        for (int i = 0; i < count; i++)
        {
            SceNpAccountId accountId = reader.ReadUInt64();

            IntrusivePtr<JoinableUser> joinableUserPtr;
            ret = JoinableUserFactory::create(libContextPtr, accountId, &joinableUserPtr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            // add to Vector
            ret = joinableSpecifiedUsers.pushBack(joinableUserPtr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        Common::IntrusivePtr<PostPlayerSessionsSessionIdJoinableSpecifiedUsersRequestBody> postPlayerSessionsSessionIdJoinableSpecifiedUsersRequestBody;
        ret = PostPlayerSessionsSessionIdJoinableSpecifiedUsersRequestBodyFactory::create(libContextPtr, joinableSpecifiedUsers, &postPlayerSessionsSessionIdJoinableSpecifiedUsersRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        PlayerSessionsApi::ParameterToAddPlayerSessionJoinableSpecifiedUsers param;
        ret = param.initialize(libContextPtr, sessionId, postPlayerSessionsSessionIdJoinableSpecifiedUsersRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<PostPlayerSessionsSessionIdJoinableSpecifiedUsersResponseBody> PostPlayerSessionsSessionIdJoinableSpecifiedUsersResponseBody;
        PostPlayerSessionsSessionIdJoinableSpecifiedUsersResponseBody response;

        Common::Transaction<PostPlayerSessionsSessionIdJoinableSpecifiedUsersResponseBody> transaction;
        transaction.start(libContextPtr);

        ret = PlayerSessionsApi::addPlayerSessionJoinableSpecifiedUsers(userCtx->m_webapiUserCtxId, param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<JoinableUser> > > joinableUsers = response->getJoinableSpecifiedUsers();

        writer.WriteInt32(joinableUsers.get()->size());

        for (auto& it : *joinableUsers)
        {
            writer.WriteUInt64(it->getAccountId());
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::DeletePlayerSessionJoinableSpecifiedUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        const char* accountIds = reader.ReadStringPtr();

        PlayerSessionsApi::ParameterToDeletePlayerSessionJoinableSpecifiedUsers param;
        ret = param.initialize(libContextPtr, sessionId, accountIds);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        ret = PlayerSessionsApi::deletePlayerSessionJoinableSpecifiedUsers(userCtx->m_webapiUserCtxId, param, transaction);
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

    void PlayerSessionCommands::SetPlayerSessionMemberSystemPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        Common::IntrusivePtr<PatchPlayerSessionsSessionIdMembersAccountIdRequestBody> patchPlayerSessionsSessionIdMembersAccountIdRequestBody;
        ret = PatchPlayerSessionsSessionIdMembersAccountIdRequestBodyFactory::create(libContextPtr, &patchPlayerSessionsSessionIdMembersAccountIdRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ret = patchPlayerSessionsSessionIdMembersAccountIdRequestBody->setCustomData1(data, dataSize);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        PlayerSessionsApi::ParameterToSetPlayerSessionMemberSystemProperties param;
        ret = param.initialize(libContextPtr, sessionId, accountIdBuf, patchPlayerSessionsSessionIdMembersAccountIdRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        ret = PlayerSessionsApi::setPlayerSessionMemberSystemProperties(userCtx->m_webapiUserCtxId, param, transaction);
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

    void PlayerSessionCommands::SendPlayerSessionMessageImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        Common::IntrusivePtr<PostPlayerSessionsSessionIdSessionMessageRequestBody> postPlayerSessionsSessionIdSessionMessageRequestBody;
        ret = PostPlayerSessionsSessionIdSessionMessageRequestBodyFactory::create(libContextPtr, to, payload, &postPlayerSessionsSessionIdSessionMessageRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        PlayerSessionsApi::ParameterToSendPlayerSessionMessage param;
        ret = param.initialize(libContextPtr, sessionId, postPlayerSessionsSessionIdSessionMessageRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::sendPlayerSessionMessage(userCtx->m_webapiUserCtxId, param, transaction);
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

    void PlayerSessionCommands::GetJoinedPlayerSessionsByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

        PlayerSessionsApi::ParameterToGetJoinedPlayerSessionsByUser param;
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

        typedef Common::IntrusivePtr<GetUsersAccountIdPlayerSessionsResponseBody> GetUsersAccountIdPlayerSessionsResponseBody;
        GetUsersAccountIdPlayerSessionsResponseBody response;

        Common::Transaction<GetUsersAccountIdPlayerSessionsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::getJoinedPlayerSessionsByUser(userCtx->m_webapiUserCtxId, param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<JoinedPlayerSessionWithPlatform> > > playerSessions = response->getPlayerSessions();

        writer.WriteInt32(playerSessions.get()->size());

        for (auto& it : *playerSessions)
        {
            writer.WriteString(it->getSessionId().c_str());

            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void PlayerSessionCommands::SerialiseSessionInfo(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<PlayerSessionForRead> > > playerSessionsPtr)
    {
        if (!(playerSessionsPtr)) return;

        if (playerSessionsPtr->empty())
        {
            // User left session
            writer.WriteBool(false); // not in session
            return;
        }

        writer.WriteBool(true); // in session

        int numSessions = playerSessionsPtr->size();

        writer.WriteInt32(numSessions);

        for (int session = 0; session < numSessions; session++)
        {
            IntrusivePtr<PlayerSessionForRead>& playerSessionPtr = (*playerSessionsPtr)[session];
            if (!(playerSessionPtr))
            {
                writer.WriteBool(false);
                return;
            }
            writer.WriteBool(true);

            if (playerSessionPtr->sessionIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(playerSessionPtr->getSessionId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->createdTimestampIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(playerSessionPtr->getCreatedTimestamp().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->maxPlayersIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32(playerSessionPtr->getMaxPlayers());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->maxSpectatorsIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32(playerSessionPtr->getMaxSpectators());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->memberIsSet())
            {
                IntrusivePtr<PlayerSessionMemberForRead> member = playerSessionPtr->getMember();
                writer.WriteBool(true);

                if (member->playersIsSet())
                {
                    writer.WriteBool(true);

                    IntrusivePtr<Vector<IntrusivePtr<PlayerSessionPlayer> > > players = member->getPlayers();

                    writer.WriteInt32(players->size());

                    for (auto& it : *players)
                    {
                        writer.WriteBool(false); // Not spectator
                        writer.WriteUInt64(it->getAccountId());
                        writer.WriteString(it->getOnlineId().data);
                        writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
                        writer.WriteString(it->getJoinTimestamp().c_str());

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

                    IntrusivePtr<Vector<IntrusivePtr<PlayerSessionSpectator> > > spectators = member->getSpectators();

                    writer.WriteInt32(spectators->size());

                    for (auto& it : *spectators)
                    {
                        writer.WriteBool(true); // Spectator
                        writer.WriteUInt64(it->getAccountId());
                        writer.WriteString(it->getOnlineId().data);
                        writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
                        writer.WriteString(it->getJoinTimestamp().c_str());

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

            if (playerSessionPtr->joinDisabledIsSet())
            {
                writer.WriteBool(true);
                writer.WriteBool(playerSessionPtr->getJoinDisabled());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->supportedPlatformsIsSet())
            {
                writer.WriteBool(true);

                uint32_t platformFlags = 0;

                IntrusivePtr<Vector<String> > platforms = playerSessionPtr->getSupportedPlatforms();

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

            if (playerSessionPtr->sessionNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(playerSessionPtr->getSessionName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->localizedSessionNameIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<LocalizedString> localizedName = playerSessionPtr->getLocalizedSessionName();

                LocalizedString* ptr = localizedName.get();

                writer.WriteString(ptr->getDefaultLanguage().c_str());

                writer.WriteUInt32(ptr->getLocalizedText().size());

                for (auto& it : ptr->getLocalizedText())
                {
                    const char* locale = it.first.c_str();
                    auto str = it.second.getString();
                    const char* text = str.c_str();

                    writer.WriteString(locale); // locale
                    writer.WriteString(text);   // text
                }
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->leaderIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<LeaderWithOnlineId> leader = playerSessionPtr->getLeader();
                writer.WriteUInt64(leader->getAccountId());
                writer.WriteUInt32(Utils::GetPlatformFlag(leader->getPlatform().c_str()));
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->joinableUserTypeIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32((UInt32)playerSessionPtr->getJoinableUserType());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->joinableSpecifiedUsersIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Vector<IntrusivePtr<JoinableUser> > > specifiedUsers = playerSessionPtr->getJoinableSpecifiedUsers();

                writer.WriteUInt32(specifiedUsers.get()->size());

                for (auto& it : *specifiedUsers)
                {
                    writer.WriteUInt64(it.get()->getAccountId());
                }
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->invitableUserTypeIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt32((UInt32)playerSessionPtr->getInvitableUserType());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->leaderPrivilegesIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Vector<String> > privileges = playerSessionPtr->getLeaderPrivileges();

                writer.WriteUInt32(privileges.get()->size());

                for (auto& it : *privileges)
                {
                    writer.WriteString(it.c_str());
                }
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->exclusiveLeaderPrivilegesIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Vector<String> > excPrivileges = playerSessionPtr->getExclusiveLeaderPrivileges();

                writer.WriteUInt32(excPrivileges.get()->size());

                for (auto& it : *excPrivileges)
                {
                    writer.WriteString(it.c_str());
                }
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->disableSystemUiMenuIsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Vector<String> > disableSystemUI = playerSessionPtr->getDisableSystemUiMenu();

                writer.WriteUInt32(disableSystemUI.get()->size());

                for (auto& it : *disableSystemUI)
                {
                    writer.WriteString(it.c_str());
                }
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->customData1IsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Binary> customData1 = playerSessionPtr->getCustomData1();

                writer.WriteData((const char*)customData1->getBinary(), customData1->size());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->customData2IsSet())
            {
                writer.WriteBool(true);

                IntrusivePtr<Binary> customData2 = playerSessionPtr->getCustomData2();

                writer.WriteData((const char*)customData2->getBinary(), customData2->size());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (playerSessionPtr->swapSupportedIsSet())
            {
                writer.WriteBool(true);
                writer.WriteBool(playerSessionPtr->getSwapSupported());
            }
            else
            {
                writer.WriteBool(false);
            }
        }
    }

    int PlayerSessionCommands::Create(Int32 pushCallbackId, void* creatorCustomData1, int creatorDataSize1, InitializationParams& initParams, BinaryWriter& writer)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        OrderGuaranteedPushEvent* pushEvent = WebApiNotifications::FindOrderedPushEvent(pushCallbackId);

        if (pushEvent == NULL)
        {
            return -1;
        }

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(initParams.m_UserId);

        if (userCtx == NULL)
        {
            return -1;
        }

        // Get the account id of the user as string
        char accountIdBuf[21];
        int ret = userCtx->GetAccountIdStr(accountIdBuf, sizeof(accountIdBuf));

        if (ret < 0)
        {
            return ret;
        }

        // PlayerSessionPushContext
        IntrusivePtr<PlayerSessionPushContext> playerSessionPushContextPtr;
        ret = PlayerSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &playerSessionPushContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        Vector<IntrusivePtr<PlayerSessionPushContext> > playerSessionPushContexts(libContextPtr);
        ret = playerSessionPushContexts.pushBack(playerSessionPushContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        // RequestCreatePlayerSessionPlayer
        IntrusivePtr<RequestCreatePlayerSessionPlayer> requestCreatePlayerSessionPlayerPtr;
        ret = RequestCreatePlayerSessionPlayerFactory::create(libContextPtr, accountIdBuf, Utils::GetThisPlatformString(), playerSessionPushContexts, &requestCreatePlayerSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        if (creatorDataSize1 > 0 && creatorCustomData1 != NULL)
        {
            ret = requestCreatePlayerSessionPlayerPtr->setCustomData1(creatorCustomData1, creatorDataSize1);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestCreatePlayerSessionPlayer> > requestCreatePlayerSessionPlayers(libContextPtr);
        ret = requestCreatePlayerSessionPlayers.pushBack(requestCreatePlayerSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        // RequestPlayerSessionMemberPlayer
        IntrusivePtr<RequestPlayerSessionMemberPlayer> requestPlayerSessionMemberPlayerPtr;
        ret = RequestPlayerSessionMemberPlayerFactory::create(libContextPtr, requestCreatePlayerSessionPlayers, &requestPlayerSessionMemberPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        Vector<String> playerSessionSupportedPlatforms(libContextPtr);
        ret = Utils::AddPlatformStrings(initParams.m_PlatformFlags, playerSessionSupportedPlatforms);
        if (ret < 0)
        {
            return ret;
        }

        // RequestPlayerSession
        IntrusivePtr<RequestPlayerSession> requestPlayerSessionPtr;
        ret = RequestPlayerSessionFactory::create(libContextPtr, initParams.m_MaxPlayers, requestPlayerSessionMemberPlayerPtr, playerSessionSupportedPlatforms, initParams.m_SessionName.m_LocalizedStringPtr, &requestPlayerSessionPtr);
        if (ret < 0)
        {
            return ret;
        }

        // maxSpectators
        requestPlayerSessionPtr->setMaxSpectators(initParams.m_MaxSpectators);
        requestPlayerSessionPtr->setJoinableUserType(initParams.m_JoinableUserType);
        requestPlayerSessionPtr->setInvitableUserType(initParams.m_InvitableUserType);

        if (initParams.m_LeaderPrivileges != NULL && initParams.m_LeaderPrivileges->size() > 0)
        {
            ret = requestPlayerSessionPtr->setLeaderPrivileges(*initParams.m_LeaderPrivileges);
            if (ret < 0)
            {
                return ret;
            }
        }

        // ExclusiveLeaderPrivlages
        if (initParams.m_ExclusiveLeaderPrivileges != NULL && initParams.m_ExclusiveLeaderPrivileges->size() > 0)
        {
            ret = requestPlayerSessionPtr->setExclusiveLeaderPrivileges(*initParams.m_ExclusiveLeaderPrivileges);
            if (ret < 0)
            {
                return ret;
            }
        }

        // DisableSystemUiMenu
        if (initParams.m_DisableSystemUiMenu != NULL && initParams.m_DisableSystemUiMenu->size() > 0)
        {
            ret = requestPlayerSessionPtr->setDisableSystemUiMenu(*initParams.m_DisableSystemUiMenu);
            if (ret < 0)
            {
                return ret;
            }
        }

        // swapSupported
        requestPlayerSessionPtr->setSwapSupported(initParams.m_SwapSupported);
        requestPlayerSessionPtr->setJoinDisabled(initParams.m_JoinDisabled);

        if (initParams.m_CustomDataSize1 > 0)
        {
            ret = requestPlayerSessionPtr->setCustomData1(initParams.m_CustomData1, initParams.m_CustomDataSize1);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (initParams.m_CustomDataSize2 > 0)
        {
            ret = requestPlayerSessionPtr->setCustomData2(initParams.m_CustomData2, initParams.m_CustomDataSize2);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestPlayerSession> > requestPlayerSessions(libContextPtr);
        ret = requestPlayerSessions.pushBack(requestPlayerSessionPtr);
        if (ret < 0)
        {
            return ret;
        }

        // PostPlayerSessionsRequestBody
        IntrusivePtr<PostPlayerSessionsRequestBody> postPlayerSessionsRequestBodyPtr;
        ret = PostPlayerSessionsRequestBodyFactory::create(libContextPtr, requestPlayerSessions, &postPlayerSessionsRequestBodyPtr);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToCreatePlayerSessions
        PlayerSessionsApi::ParameterToCreatePlayerSessions param;
        ret = param.initialize(libContextPtr, postPlayerSessionsRequestBodyPtr);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostPlayerSessionsResponseBody> PostPlayerSessionsResponseType;
        PostPlayerSessionsResponseType response;

        Common::Transaction<PostPlayerSessionsResponseType> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::createPlayerSessions(userCtx->GetUserCtxId(), param, transaction);
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

        IntrusivePtr<Vector<IntrusivePtr<sce::Np::CppWebApi::SessionManager::V1::PlayerSession> > > playerSessionsPtr = response->getPlayerSessions();

        IntrusivePtr<sce::Np::CppWebApi::SessionManager::V1::PlayerSession>& playerSessionPtr = (*playerSessionsPtr)[0];

        writer.WriteString(playerSessionPtr->getSessionId().c_str());

        Common::IntrusivePtr<ResponsePlayerSessionMemberPlayer> member = playerSessionPtr->getMember();
        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponsePlayerSessionPlayer> > > players = member->getPlayers();

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            writer.WriteUInt64(it->getAccountId());
            writer.WriteUInt32(Utils::GetPlatformFlag(it->getPlatform().c_str()));
        }

        // Write out account id for the user who created the session
        SceNpAccountId accountId = SCE_NP_INVALID_ACCOUNT_ID;
        sceNpGetAccountIdA(initParams.m_UserId, &accountId);

        writer.WriteUInt64(accountId);

        param.terminate();

        transaction.finish();

        return ret;
    }

    int PlayerSessionCommands::Leave(SceUserServiceUserId userId, const char* sessionId)
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
        PlayerSessionsApi::ParameterToLeavePlayerSession param;
        ret = param.initialize(libContextPtr, sessionId, accountIdBuf);
        if (ret < 0)
        {
            return ret;
        }

        Common::Transaction<Common::DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::leavePlayerSession(userCtx->m_webapiUserCtxId, param, transaction);
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

    int PlayerSessionCommands::JoinAsPlayer(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer)
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
        // RequestPlayerSessionPlayer
        IntrusivePtr<RequestPlayerSessionPlayer> requestPlayerSessionPlayerPtr;
        ret = RequestPlayerSessionPlayerFactory::create(libContextPtr, accountIdBuf, Utils::GetThisPlatformString(), &requestPlayerSessionPlayerPtr);

        if (ret < 0)
        {
            return ret;
        }

        // WARNING - Can't be set if performing a swap - spectator to player
        if (swapping == false)
        {
            // PlayerSessionPushContext
            IntrusivePtr<PlayerSessionPushContext> playerSessionPushContextPtr;
            ret = PlayerSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &playerSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            Vector<IntrusivePtr<PlayerSessionPushContext> > playerSessionPushContexts(libContextPtr);
            ret = playerSessionPushContexts.pushBack(playerSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // set PushContexts
            ret = requestPlayerSessionPlayerPtr->setPushContexts(playerSessionPushContexts);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestPlayerSessionPlayer> > requestPlayerSessionPlayers(libContextPtr);
        ret = requestPlayerSessionPlayers.pushBack(requestPlayerSessionPlayerPtr);
        if (ret < 0)
        {
            return ret;
        }

        // PostPlayerSessionsSessionIdMemberPlayersRequestBody
        IntrusivePtr<PostPlayerSessionsSessionIdMemberPlayersRequestBody> postPlayerSessionsSessionIdMemberPlayersRequestBody;
        ret = PostPlayerSessionsSessionIdMemberPlayersRequestBodyFactory::create(libContextPtr, requestPlayerSessionPlayers, &postPlayerSessionsSessionIdMemberPlayersRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToJoinPlayerSessionAsPlayer
        PlayerSessionsApi::ParameterToJoinPlayerSessionAsPlayer param;
        ret = param.initialize(libContextPtr, sessionId, postPlayerSessionsSessionIdMemberPlayersRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostPlayerSessionsSessionIdMemberPlayersResponseBody> PostPlayerSessionsSessionIdMemberPlayersResponseBody;
        PostPlayerSessionsSessionIdMemberPlayersResponseBody response;

        Common::Transaction<PostPlayerSessionsSessionIdMemberPlayersResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::joinPlayerSessionAsPlayer(userCtx->m_webapiUserCtxId, param, transaction);
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

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponsePlayerSessionPlayer> > > players = response->getPlayers();

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

    int PlayerSessionCommands::JoinAsSpectator(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer)
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
        IntrusivePtr<RequestPlayerSessionSpectator> requestPlayerSessionSpectatorPtr;
        ret = RequestPlayerSessionSpectatorFactory::create(libContextPtr, accountIdBuf, Utils::GetThisPlatformString(), &requestPlayerSessionSpectatorPtr);
        if (ret < 0)
        {
            return ret;
        }

        // WARNING - Can't be set if performing a swap - player to spectator
        if (swapping == false)
        {
            // PlayerSessionPushContext
            IntrusivePtr<PlayerSessionPushContext> playerSessionPushContextPtr;
            ret = PlayerSessionPushContextFactory::create(libContextPtr, pushEvent->GetPushContextIdStr(), &playerSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // add to Vector
            Vector<IntrusivePtr<PlayerSessionPushContext> > playerSessionPushContexts(libContextPtr);
            ret = playerSessionPushContexts.pushBack(playerSessionPushContextPtr);
            if (ret < 0)
            {
                return ret;
            }

            // set PushContexts
            ret = requestPlayerSessionSpectatorPtr->setPushContexts(playerSessionPushContexts);
            if (ret < 0)
            {
                return ret;
            }
        }

        // add to Vector
        Vector<IntrusivePtr<RequestPlayerSessionSpectator> > requestPlayerSessionSpectators(libContextPtr);
        ret = requestPlayerSessionSpectators.pushBack(requestPlayerSessionSpectatorPtr);
        if (ret < 0)
        {
            return ret;
        }

        // PostPlayerSessionsSessionIdMemberPlayersRequestBody
        IntrusivePtr<PostPlayerSessionsSessionIdMemberSpectatorsRequestBody> postPlayerSessionsSessionIdMemberSpectatorsRequestBody;
        ret = PostPlayerSessionsSessionIdMemberSpectatorsRequestBodyFactory::create(libContextPtr, requestPlayerSessionSpectators, &postPlayerSessionsSessionIdMemberSpectatorsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        // ParameterToJoinPlayerSessionAsPlayer
        PlayerSessionsApi::ParameterToJoinPlayerSessionAsSpectator param;
        ret = param.initialize(libContextPtr, sessionId, postPlayerSessionsSessionIdMemberSpectatorsRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<PostPlayerSessionsSessionIdMemberSpectatorsResponseBody> PostPlayerSessionsSessionIdMemberSpectatorsResponseBody;
        PostPlayerSessionsSessionIdMemberSpectatorsResponseBody response;

        Common::Transaction<PostPlayerSessionsSessionIdMemberSpectatorsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = PlayerSessionsApi::joinPlayerSessionAsSpectator(userCtx->m_webapiUserCtxId, param, transaction);
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

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<ResponsePlayerSessionSpectator> > > spectators = response->getSpectators();

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

    int PlayerSessionCommands::SetPlayerSessionProps(WebApiUserContext* userCtx, const char* sessionId, BinaryReader& reader)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        IntrusivePtr<PatchPlayerSessionsSessionIdRequestBody> patchPlayerSessionsSessionIdRequestBody;
        ret = PatchPlayerSessionsSessionIdRequestBodyFactory::create(libContextPtr, &patchPlayerSessionsSessionIdRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        bool isSetMaxPlayers = reader.ReadBool();
        if (isSetMaxPlayers)
        {
            UInt32 maxPlayers = reader.ReadUInt32();
            patchPlayerSessionsSessionIdRequestBody->setMaxPlayers(maxPlayers);
        }

        bool isSetMaxSpectators = reader.ReadBool();
        if (isSetMaxSpectators)
        {
            UInt32 maxSpectators = reader.ReadUInt32();
            patchPlayerSessionsSessionIdRequestBody->setMaxSpectators(maxSpectators);
        }

        bool isSetJoinDisabled = reader.ReadBool();
        if (isSetJoinDisabled)
        {
            bool joinDisabled = reader.ReadBool();
            patchPlayerSessionsSessionIdRequestBody->setJoinDisabled(joinDisabled);
        }

        bool isSetJoinableUserType = reader.ReadBool();
        if (isSetJoinableUserType)
        {
            //enum class JoinableUserType {
            //  _NOT_SET = 0,
            //  NO_ONE, // "NO_ONE"
            //  FRIENDS, // "FRIENDS"
            //  FRIENDS_OF_FRIENDS, // "FRIENDS_OF_FRIENDS"
            //  ANYONE, // "ANYONE"
            //  SPECIFIED_USERS, // "SPECIFIED_USERS"
            //};

            //enum class CustomJoinableUserType {
            //  _NOT_SET = 0,
            //  NO_ONE, // "NO_ONE"
            //  FRIENDS, // "FRIENDS"
            //  FRIENDS_OF_FRIENDS, // "FRIENDS_OF_FRIENDS"
            //  ANYONE, // "ANYONE"
            //  SPECIFIED_USERS, // "SPECIFIED_USERS"
            //};

            CustomJoinableUserType jut = (CustomJoinableUserType)reader.ReadUInt32();
            patchPlayerSessionsSessionIdRequestBody->setJoinableUserType(jut);
        }

        bool isSetInvitableUserType = reader.ReadBool();
        if (isSetInvitableUserType)
        {
            CustomInvitableUserType iut = (CustomInvitableUserType)reader.ReadUInt32();
            patchPlayerSessionsSessionIdRequestBody->setInvitableUserType(iut);
        }

        bool isSetLocalizedSessionName = reader.ReadBool();
        if (isSetLocalizedSessionName)
        {
            LocalisedStrings sessionName;
            ret = sessionName.Deserialise(reader);
            if (ret < 0)
            {
                return ret;
            }

            ret = patchPlayerSessionsSessionIdRequestBody->setLocalizedSessionName(sessionName.m_LocalizedStringPtr);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetLeaderPrivileges = reader.ReadBool();
        if (isSetLeaderPrivileges)
        {
            Vector<String> leaderPrivileges(libContextPtr);
            ret = PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, &leaderPrivileges);
            if (ret < 0)
            {
                return ret;
            }

            ret = patchPlayerSessionsSessionIdRequestBody->setLeaderPrivileges(leaderPrivileges);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetExclusiveLeaderPrivileges = reader.ReadBool();
        if (isSetExclusiveLeaderPrivileges)
        {
            Vector<String> exclusiveLeaderPrivileges(libContextPtr);
            ret = PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, &exclusiveLeaderPrivileges);

            if (ret < 0)
            {
                return ret;
            }

            ret = patchPlayerSessionsSessionIdRequestBody->setExclusiveLeaderPrivileges(exclusiveLeaderPrivileges);
            if (ret < 0)
            {
                return ret;
            }
        }


        bool isSetDisableSystemUiMenu = reader.ReadBool();

        if (isSetDisableSystemUiMenu)
        {
            Vector<String> disableSystemUiMenu(libContextPtr);
            ret = PlayerSessionCommands::DeserialiseLeaderPrivileges(reader, &disableSystemUiMenu);

            if (ret < 0)
            {
                return ret;
            }

            ret = patchPlayerSessionsSessionIdRequestBody->setDisableSystemUiMenu(disableSystemUiMenu);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetCustomData1 = reader.ReadBool();
        if (isSetCustomData1)
        {
            int dataSize = reader.ReadInt32();
            void* data = reader.ReadDataPtr(dataSize);

            int ret = patchPlayerSessionsSessionIdRequestBody->setCustomData1(data, dataSize);
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

            int ret = patchPlayerSessionsSessionIdRequestBody->setCustomData2(data, dataSize);
            if (ret < 0)
            {
                return ret;
            }
        }

        bool isSetSwapSupported = reader.ReadBool();
        if (isSetSwapSupported)
        {
            bool swapSupported = reader.ReadBool();
            patchPlayerSessionsSessionIdRequestBody->setSwapSupported(swapSupported);
        }

        PlayerSessionsApi::ParameterToSetPlayerSessionProperties param;
        ret = param.initialize(libContextPtr, sessionId, patchPlayerSessionsSessionIdRequestBody);
        if (ret < 0)
        {
            return ret;
        }

        DefaultResponse response;

        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // Api Call
        ret = PlayerSessionsApi::setPlayerSessionProperties(userCtx->m_webapiUserCtxId, param, transaction);
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
}
