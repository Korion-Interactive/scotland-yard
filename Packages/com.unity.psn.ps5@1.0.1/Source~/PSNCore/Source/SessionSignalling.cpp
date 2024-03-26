#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <libnetctl.h>
#include "SessionSignalling.h"

#pragma comment(lib,"libSceNpSessionSignaling_stub_weak.a")

namespace psn
{
    std::list<SessionSignalling::SignallingEvent> SessionSignalling::s_PendingSignallingEventsList;

    std::map<SceNpSessionSignalingContextId, SessionSignalling::ActiveContext*> SessionSignalling::s_ActiveCtxList;

    void SessionSignalling::AddContext(SessionSignalling::ActiveContext* ctx)
    {
        s_ActiveCtxList.insert(std::pair<SceUserServiceUserId, ActiveContext*>(ctx->m_ctxId, ctx));
    }

    void SessionSignalling::RemoveContext(SessionSignalling::ActiveContext* ctx)
    {
        s_ActiveCtxList.erase(ctx->m_ctxId);
    }

    SessionSignalling::ActiveContext* SessionSignalling::FindContext(SceNpSessionSignalingContextId ctxId)
    {
        auto it = s_ActiveCtxList.find(ctxId);

        if (it == s_ActiveCtxList.end())
        {
            return NULL;
        }

        return it->second;
    }

    SessionSignalling::ActiveContext* SessionSignalling::FindContext(Int32 userId)
    {
        for (auto it = s_ActiveCtxList.begin(); it != s_ActiveCtxList.end(); it++)
        {
            if (it->second != NULL && it->second->m_userId == userId)
            {
                return it->second;
            }
        }

        return NULL;
    }

    void SessionSignalling::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::FetchSignallingEvent, SessionSignalling::FetchSignallingEventImpl);
        MsgHandler::AddMethod(Methods::UserToUserSignalling, SessionSignalling::UserToUserSignallingImpl);
        MsgHandler::AddMethod(Methods::ActivateUser, SessionSignalling::ActivateUserImpl);
        MsgHandler::AddMethod(Methods::Deactivate, SessionSignalling::DeactivateImpl);
        MsgHandler::AddMethod(Methods::GetLocalNetInfo, SessionSignalling::GetLocalNetInfoImpl);
        MsgHandler::AddMethod(Methods::GetNatRouterInfo, SessionSignalling::GetNatRouterInfoImpl);
        MsgHandler::AddMethod(Methods::GetConnectionStatus, SessionSignalling::GetConnectionStatusImpl);
        MsgHandler::AddMethod(Methods::GetConnectionInfo, SessionSignalling::GetConnectionInfoImpl);
        MsgHandler::AddMethod(Methods::CreateUserContext, SessionSignalling::CreateUserContextImpl);
        MsgHandler::AddMethod(Methods::DestroyUserContext, SessionSignalling::DestroyUserContextImpl);
        MsgHandler::AddMethod(Methods::ActivateSession, SessionSignalling::ActivateSessionImpl);
    }

    int SessionSignalling::InitializeLib(int32_t libhttp2CtxId)
    {
        // sceNpSessionSignalingInitialize
        SceNpSessionSignalingInitParam initParam;
        initParam.libhttp2CtxId = libhttp2CtxId;
        initParam.poolSize = SCE_NP_SESSION_SIGNALING_POOLSIZE_DEFAULT;
        initParam.cpuAffinityMask = 0;
        initParam.threadPriority = SCE_NP_SESSION_SIGNALING_THREAD_PRIORITY_DEFAULT;
        initParam.threadStackSize = SCE_NP_SESSION_SIGNALING_THREAD_STACK_SIZE_DEFAULT;

        return sceNpSessionSignalingInitialize(&initParam);
    }

    int SessionSignalling::TerminateLib()
    {
        return sceNpSessionSignalingTerminate();
    }

    //void SessionSignalling::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    //{
    //  if (state == MsgHandler::UserState::Added)
    //  {
    //      if (s_UsersList.DoesUserExist(userId) == true)
    //      {
    //          // User already registered so don't do this again
    //          WARNING_RESULT(result, "User already initialised with SessionSignalling service");
    //          return;
    //      }

    //      UserContext* user = s_UsersList.CreateUser(userId);

    //      user->Create(user, RequestCallback, GroupCallback, ConnectionCallback);
    //  }
    //  else if (state == MsgHandler::UserState::Removed)
    //  {
    //      UserContext* user = s_UsersList.FindUser(userId);

    //      if (user == NULL)
    //      {
    //          WARNING_RESULT(result, "User not registered with SessionSignalling");
    //          return;
    //      }

    //      if (user->m_ctxId == SCE_NP_SESSION_SIGNALING_INVALID_ID)
    //      {
    //          ERROR_RESULT(result, "User context is invalid");
    //          return;
    //      }

    //      user->Destroy();

    //      s_UsersList.DeleteUser(userId);
    //  }

    //  SUCCESS_RESULT(result);
    //}

    void SessionSignalling::SignallingEvent::Init()
    {
        type = Request;

        userId = 0;

        ctxId = SCE_NP_SESSION_SIGNALING_INVALID_ID;
        errorCode = 0;

        id = SCE_NP_SESSION_SIGNALING_INVALID_ID;
        eventDesc = -1;

        grpIdForConnectionEvent = SCE_NP_SESSION_SIGNALING_INVALID_ID;
    }

    void SessionSignalling::FetchSignallingEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingSignallingEventsList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first event off the list and return the results
        SignallingEvent event = s_PendingSignallingEventsList.front();
        s_PendingSignallingEventsList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(event.type);

        writer.WriteInt32(event.userId);
        writer.WriteUInt32(event.ctxId);
        writer.WriteInt32(event.errorCode);

        writer.WriteUInt32(event.id);
        writer.WriteInt32(event.eventDesc);

        if (event.type == SignallingEvent::EventType::Group && event.eventDesc == SCE_NP_SESSION_SIGNALING_GROUP_EVENT_PEER_ACTIVATED && event.errorCode == 0)
        {
            writer.WriteUInt64(event.peerActivatedData.accountId);
            writer.WriteInt32(event.peerActivatedData.platform);
        }

        if (event.type == SignallingEvent::EventType::Connection)
        {
            writer.WriteUInt32(event.grpIdForConnectionEvent);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::RequestCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingRequestId reqId, SceNpSessionSignalingRequestEvent event, const void* eventData, int errorCode, void* arg)
    {
        //printf("SessionSignalling::RequestCallback\n");

        SignallingEvent sEvent;

        sEvent.Init();

        SessionSignalling::ActiveContext* activeContext = SessionSignalling::FindContext(ctxId);
        if (activeContext != NULL)
        {
            sEvent.userId = activeContext->m_userId;
        }

        sEvent.type = SignallingEvent::Request;

        sEvent.ctxId = ctxId;
        sEvent.errorCode = errorCode;

        sEvent.id = reqId;
        sEvent.eventDesc = event;

        s_PendingSignallingEventsList.push_back(sEvent);
    }

    void SessionSignalling::GroupCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingGroupId grpId, SceNpSessionSignalingGroupEvent event, const void* eventData, int errorCode, void* arg)
    {
        //printf("SessionSignalling::GroupCallback\n");

        SignallingEvent sEvent;

        sEvent.Init();

        SessionSignalling::ActiveContext* activeContext = SessionSignalling::FindContext(ctxId);
        if (activeContext != NULL)
        {
            sEvent.userId = activeContext->m_userId;
        }

        sEvent.type = SignallingEvent::Group;

        sEvent.ctxId = ctxId;
        sEvent.errorCode = errorCode;

        sEvent.id = grpId;
        sEvent.eventDesc = event;

        if (event == SCE_NP_SESSION_SIGNALING_GROUP_EVENT_PEER_ACTIVATED && errorCode == 0)
        {
            memcpy(&sEvent.peerActivatedData, eventData, sizeof(sEvent.peerActivatedData));
        }

        s_PendingSignallingEventsList.push_back(sEvent);
    }

#if defined(CONTEXT_2_SUPPORTED)
    void SessionSignalling::ConnectionCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingGroupId grpId, SceNpSessionSignalingConnectionId connId, SceNpSessionSignalingConnectionEvent event, int errorCode, void* arg)
#else
    void SessionSignalling::ConnectionCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingConnectionId connId, SceNpSessionSignalingConnectionEvent event, int errorCode, void* arg)
#endif
    {
        //printf("SessionSignalling::ConnectionCallback\n");

        SignallingEvent sEvent;

        sEvent.Init();

        SessionSignalling::ActiveContext* activeContext = SessionSignalling::FindContext(ctxId);
        if (activeContext != NULL)
        {
            sEvent.userId = activeContext->m_userId;
        }

        sEvent.type = SignallingEvent::Connection;

        sEvent.ctxId = ctxId;
        sEvent.errorCode = errorCode;

        sEvent.id = connId;
        sEvent.eventDesc = event;

#if defined (CONTEXT_2_SUPPORTED)
        sEvent.grpIdForConnectionEvent = grpId;
#endif

        s_PendingSignallingEventsList.push_back(sEvent);
    }

    void SessionSignalling::CreateUserContextImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

#if defined(CONTEXT_2_SUPPORTED)
        SceNpSessionSignalingCreateContext2Param createCtxParam;
        createCtxParam.userId = userId;
        createCtxParam.serviceLabel = 0;
        createCtxParam.reqCbFunc = RequestCallback;
        createCtxParam.reqCbArg = NULL;
        createCtxParam.grpCbFunc = GroupCallback;
        createCtxParam.grpCbArg = NULL;
        createCtxParam.connCbFunc = ConnectionCallback;
        createCtxParam.connCbArg = NULL;

        SceNpSessionSignalingContextId ctxId;

        ret = sceNpSessionSignalingCreateContext2(&createCtxParam, &ctxId);
#else
        SceNpSessionSignalingCreateContextParam createCtxParam;
        createCtxParam.userId = userId;
        createCtxParam.serviceLabel = 0;
        createCtxParam.reqCbFunc = RequestCallback;
        createCtxParam.reqCbArg = NULL;
        createCtxParam.grpCbFunc = GroupCallback;
        createCtxParam.grpCbArg = NULL;
        createCtxParam.connCbFunc = ConnectionCallback;
        createCtxParam.connCbArg = NULL;

        SceNpSessionSignalingContextId ctxId;

        ret = sceNpSessionSignalingCreateContext(&createCtxParam, &ctxId);
#endif

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ActiveContext* newCtx = new ActiveContext();
        newCtx->m_ctxId = ctxId;
        newCtx->m_userId = userId;

        AddContext(newCtx);

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(ctxId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::DestroyUserContextImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        ret = sceNpSessionSignalingDestroyContext(ctxId);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        ActiveContext* activeCtx = FindContext(ctxId);
        if (activeCtx != NULL)
        {
            RemoveContext(activeCtx);
            delete activeCtx;
        }

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::UserToUserSignallingImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        SceNpSessionSignalingRequestId reqId = SCE_NP_SESSION_SIGNALING_INVALID_ID;

        ret = sceNpSessionSignalingRequestPrepare(ctxId, &reqId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(reqId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::ActivateUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        SceNpPeerAddressA peerAddrA;

        memset(&peerAddrA, 0, sizeof(peerAddrA));
        peerAddrA.accountId = reader.ReadUInt64();
        peerAddrA.platform = reader.ReadInt32();

        SceNpSessionSignalingRequestId grpId = SCE_NP_SESSION_SIGNALING_INVALID_ID;

        ret = sceNpSessionSignalingActivateUser(ctxId, &peerAddrA, &grpId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(grpId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::ActivateSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        char* sessionId = reader.ReadStringPtr();

        SceNpSessionSignalingSessionType sessionType = (SceNpSessionSignalingSessionType)reader.ReadInt32();

        SceNpSessionSignalingTopologyType topology = (SceNpSessionSignalingTopologyType)reader.ReadInt32();

        SceNpSessionSignalingHostType host = (SceNpSessionSignalingHostType)reader.ReadInt32();

        SceNpSessionSignalingSessionOptParam params;
        params.topologyType = topology;
        params.hostType = host;

        SceNpSessionSignalingRequestId grpId = SCE_NP_SESSION_SIGNALING_INVALID_ID;

        ret = sceNpSessionSignalingActivateSession(ctxId, sessionId, sessionType, &params, &grpId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(grpId);
        writer.WriteInt32(params.topologyType);
        writer.WriteInt32(params.hostType);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::DeactivateImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        UInt32 grpId = reader.ReadUInt32();

        ret = sceNpSessionSignalingDeactivate(ctxId, grpId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::GetLocalNetInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();

        SceNpSessionSignalingNetInfo localNetInfo;
        ret = sceNpSessionSignalingGetLocalNetInfo(ctxId, &localNetInfo);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(localNetInfo.localAddr.s_addr);
        writer.WriteUInt32(localNetInfo.mappedAddr.s_addr);
        writer.WriteInt32(localNetInfo.natStatus);

        char buf1[32];
        char buf2[32];
        sceNetInetNtop(SCE_NET_AF_INET, &localNetInfo.localAddr.s_addr, buf1, sizeof(buf1));
        sceNetInetNtop(SCE_NET_AF_INET, &localNetInfo.mappedAddr.s_addr, buf2, sizeof(buf2));

        writer.WriteString(buf1);
        writer.WriteString(buf2);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::GetNatRouterInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;
        int ret = 0;
        SceNetCtlNatInfo natRouterInfo;
        memset(&natRouterInfo, 0, sizeof(natRouterInfo));
        natRouterInfo.size = sizeof(natRouterInfo);

        ret = sceNetCtlGetNatInfo(&natRouterInfo);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(natRouterInfo.stunStatus);
        writer.WriteInt32(natRouterInfo.natType);
        writer.WriteUInt32(natRouterInfo.mappedAddr.s_addr);

        char buf1[32];
        sceNetInetNtop(SCE_NET_AF_INET, &natRouterInfo.mappedAddr.s_addr, buf1, sizeof(buf1));

        writer.WriteString(buf1);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::GetConnectionStatusImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        //Int32 userId = reader.ReadInt32();
        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();
        SceNpSessionSignalingConnectionId connId = reader.ReadUInt32();

        SceNpSessionSignalingConnectionStatus connStatus;
        SceNetInAddr peerAddr;
        SceNetInPort_t peerPort; // Network order.

        ret = sceNpSessionSignalingGetConnectionStatus(ctxId, connId, &connStatus, &peerAddr, &peerPort);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(connStatus);
        writer.WriteUInt32(peerAddr.s_addr);

        char buf1[32];
        sceNetInetNtop(SCE_NET_AF_INET, &peerAddr.s_addr, buf1, sizeof(buf1));
        writer.WriteString(buf1);

        writer.WriteUInt16(sceNetNtohs(peerPort));

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void SessionSignalling::GetConnectionInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        //Int32 userId = reader.ReadInt32();
        SceNpSessionSignalingContextId ctxId = reader.ReadUInt32();
        SceNpSessionSignalingConnectionId connId = reader.ReadUInt32();
        SceNpSessionSignalingConnectionInfoCode code = (SceNpSessionSignalingConnectionInfoCode)reader.ReadInt32();

        SceNpSessionSignalingConnectionInfo connInfo;
        ret = sceNpSessionSignalingGetConnectionInfo(ctxId, connId, code, &connInfo);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        //SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_RTT = 1,
        //SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_NET_ADDRESS = 4,
        //SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_MAPPED_ADDRESS = 5,
        //SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_PACKET_LOSS = 6,
        //SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_PEER_ADDRESS = 7

        writer.WriteInt32(code);

        if (code == SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_RTT)
        {
            writer.WriteUInt32(connInfo.rtt);
        }
        else if (code == SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_PEER_ADDRESS)
        {
            writer.WriteUInt64(connInfo.peerAddrA.accountId);
            writer.WriteInt32(connInfo.peerAddrA.platform);
        }
        else if (code == SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_NET_ADDRESS || code == SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_MAPPED_ADDRESS)
        {
            writer.WriteUInt32(connInfo.address.addr.s_addr);

            char buf1[32];
            sceNetInetNtop(SCE_NET_AF_INET, &connInfo.address.addr.s_addr, buf1, sizeof(buf1));
            writer.WriteString(buf1);

            writer.WriteUInt16(sceNetNtohs(connInfo.address.port));
        }
        else if (code == SCE_NP_SESSION_SIGNALING_CONNECTION_INFO_PACKET_LOSS)
        {
            writer.WriteUInt32(connInfo.packetLoss);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
