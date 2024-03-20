#pragma once

#include "SharedCoreIncludes.h"
#include <list>

#include "WebApi.h"

#include "WebApiPushEvents.h"

#include "Utils.h"

namespace psn
{
    // Filters
    //    Each type of WebApi notification is a filter.
    //        e.g. "np:service:friendlist:friend"
    //    Each type is currently set up as a seperate filter and has an Id.

    // Users
    //    Each user shares a set of filter ids.
    //    Each user/filter combo has a pushCallbackId


    // C# API?
    //
    //   Create a set of filters, 1 or more filters in a single set.
    //   Assign a set of filters to a user.
    //
    //

    class WebApiNotifications
    {
    public:

        enum Methods
        {
            RegisterFilter = 0x0600001u,
            UnregisterFilter = 0x0600002u,
            RegisterPushEvent = 0x0600003u,
            UnregisterPushEvent = 0x0600004u,
        };

        // Must be packed to help with marshalling struct to C#
        #pragma pack(1)
        struct CallbackExtdData
        {
            const char *extdDataKey;
            int32_t extdDataKeyLen;
            const char *pData;
            int32_t dataLen;
        };
        #pragma pack()

        // Must be packed to help with marshalling struct to C#
        #pragma pack(1)
        struct CallbackParams
        {
            int32_t userCtxId;
            int32_t callbackId;

            bool orderGuaranteed;

            const char *pNpServiceName;
            int32_t serviceNameLen;

            SceNpServiceLabel npServiceLabel;

            bool hasToPeer;
            SceNpAccountId toAccountId;
            SceNpPlatformType toPlatform;
            const char *toOnlineId;
            int32_t toOnlineIdLen;

            bool hasFromPeer;
            SceNpAccountId fromAccountId;
            SceNpPlatformType fromPlatform;
            const char *fromOnlineId;
            int32_t fromOnlineIdLen;

            const char *pDataType;
            int32_t dataTypeLen;

            const char *pData;
            int32_t dataLen;

            CallbackExtdData *extdData;
            int32_t extdDataNum;
            int32_t extdStructSize;

            const char* pushCtxId;
            int32_t pushCtxIdLen;
            int32_t cbType;
        };
        #pragma pack()

        WebApiNotifications();
        ~WebApiNotifications();

        static WebApiNotifications* Instance() { return s_Instance; }

        static void Initialise();
        static void Terminate();

        static void RegisterMethods();

        static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);

        static void RegisterFilterImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UnregisterFilterImpl(UInt8* sourceData, int sourceSize, APIResult* result);

        static void RegisterPushEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UnregisterPushEventImpl(UInt8* sourceData, int sourceSize, APIResult* result);

        static PushEventBase* FindPushEvent(int pushCallbackId);
        static OrderGuaranteedPushEvent* FindOrderedPushEvent(int pushCallbackId);

    private:
        static WebApiNotifications* s_Instance;

        std::list<SceNpGameIntentInfo> m_PendingGameIntentList;
        //int32_t m_pushHandleId;

        void Create();
        void Destroy();

        PushFilter* GetOrCreateFilter(BinaryReader& reader, APIResult* result);
        PushFilter* GetFilter(BinaryReader& reader, APIResult* result);

        static void CommonEventCallback(int32_t userCtxId, int32_t callbackId, const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
            const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
            const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
            const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg,
            bool isOrderGuaranteed = false, const SceNpWebApi2PushEventPushContextId *pPushCtxId = NULL,
            SceNpWebApi2PushEventPushContextCallbackType cbType = SceNpWebApi2PushEventPushContextCallbackType::SCE_NP_WEBAPI2_PUSH_EVENT_PUSH_CONTEXT_CALLBACK_TYPE_UNKNOWN);

        static void PushEventCallback(int32_t userCtxId, int32_t callbackId, const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
            const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
            const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
            const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg);

        static void OrderGuaranteedEventCallback(int32_t userCtxId, int32_t callbackId,
            const SceNpWebApi2PushEventPushContextId *pPushCtxId, SceNpWebApi2PushEventPushContextCallbackType cbType,
            const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
            const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
            const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
            const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg);

        IDMap<PushEventBase> m_ActivePushEvents;
        IDMap<PushFilter> m_ActivePushFilters;
    };
}
