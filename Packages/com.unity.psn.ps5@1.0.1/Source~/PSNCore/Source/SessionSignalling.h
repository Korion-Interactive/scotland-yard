#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>

#if (SCE_PROSPERO_SDK_VERSION >= 0x03000000) || (SCE_ORBIS_SDK_VERSION >= 0x08500000)
#define CONTEXT_2_SUPPORTED
#endif

namespace psn
{
    class SessionSignalling
    {
    public:

        enum Methods
        {
            FetchSignallingEvent = 0x1200001u,
            UserToUserSignalling = 0x1200002u,
            ActivateUser = 0x1200003u,
            Deactivate = 0x1200004u,
            GetLocalNetInfo = 0x1200005u,
            GetConnectionStatus = 0x1200006u,
            GetConnectionInfo = 0x1200007u,
            CreateUserContext = 0x1200008u,
            DestroyUserContext = 0x1200009u,
            GetNatRouterInfo = 0x120000au,
            ActivateSession = 0x1200010u,
        };

        class ActiveContext
        {
        public:
            SceNpSessionSignalingContextId m_ctxId;
            SceUserServiceUserId m_userId;
        };

        static std::map<SceNpSessionSignalingContextId, ActiveContext*> s_ActiveCtxList;

        static void AddContext(ActiveContext* ctx);
        static void RemoveContext(ActiveContext* ctx);
        static ActiveContext* FindContext(SceNpSessionSignalingContextId ctxId);
        static ActiveContext* FindContext(Int32 userId);

        static int InitializeLib(int32_t libhttp2CtxId);
        static int TerminateLib();

        static void RegisterMethods();

        //static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);

        static void FetchSignallingEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void CreateUserContextImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DestroyUserContextImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UserToUserSignallingImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void ActivateUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void ActivateSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void DeactivateImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetLocalNetInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetNatRouterInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetConnectionStatusImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetConnectionInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void RequestCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingRequestId reqId, SceNpSessionSignalingRequestEvent event, const void* eventData, int errorCode, void* arg);
        static void GroupCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingGroupId grpId, SceNpSessionSignalingGroupEvent event, const void* eventData, int errorCode, void* arg);

#if defined(CONTEXT_2_SUPPORTED)
        static void ConnectionCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingGroupId grpId, SceNpSessionSignalingConnectionId connId, SceNpSessionSignalingConnectionEvent event, int errorCode, void* arg);
#else
        static void ConnectionCallback(SceNpSessionSignalingContextId ctxId, SceNpSessionSignalingConnectionId connId, SceNpSessionSignalingConnectionEvent event, int errorCode, void* arg);
#endif

        struct SignallingEvent
        {
        public:

            enum EventType
            {
                Request = 0,
                Group = 1,
                Connection = 2
            };

            EventType type;

            SceUserServiceUserId userId;
            SceNpSessionSignalingContextId ctxId;
            int errorCode;

            UInt32 id; // For requests callbacks; the request Id.  For connection callbacks; the connection id.  For group callbacks; the group id.
            Int32 eventDesc;            // Enum of event type

            SceNpPeerAddressA peerActivatedData; // Only used by group events with SCE_NP_SESSION_SIGNALING_GROUP_EVENT_PEER_ACTIVATED.
            SceNpSessionSignalingGroupId grpIdForConnectionEvent; // for connection events only; the id of the group to which the connection belongs.

            void Init();
        };

        static std::list<SignallingEvent> s_PendingSignallingEventsList;
    };
}
