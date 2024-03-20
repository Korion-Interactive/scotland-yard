#include "WebApiNotifications.h"


namespace psn
{
    typedef void(*EventHandler)(void*, Int32 size);
    EventHandler sEventHandlerCallback = NULL;

    DO_EXPORT(void, RegisterEventHandler) (EventHandler callback)
    {
        sEventHandlerCallback = callback;
    }

    WebApiNotifications* WebApiNotifications::s_Instance = NULL;

    WebApiNotifications::WebApiNotifications() //:
    //  m_pushHandleId(-1)
    {
    }

    WebApiNotifications::~WebApiNotifications()
    {
    }

    void WebApiNotifications::Initialise()
    {
        if (s_Instance != NULL)
        {
            return;
        }

        if (WebApi::Instance() == NULL)
        {
            return;
        }

        s_Instance = new WebApiNotifications();

        s_Instance->Create();
    }

    void WebApiNotifications::Terminate()
    {
        if (s_Instance == NULL)
        {
            return;
        }

        s_Instance->Destroy();
        delete s_Instance;
        s_Instance = NULL;
    }

    void WebApiNotifications::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::RegisterFilter, WebApiNotifications::RegisterFilterImpl);
        MsgHandler::AddMethod(Methods::UnregisterFilter, WebApiNotifications::UnregisterFilterImpl);
        MsgHandler::AddMethod(Methods::RegisterPushEvent, WebApiNotifications::RegisterPushEventImpl);
        MsgHandler::AddMethod(Methods::UnregisterPushEvent, WebApiNotifications::UnregisterPushEventImpl);

        MsgHandler::RegisterUserCallback(HandleUserState);
    }

    void WebApiNotifications::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    {
    }

    PushFilter* WebApiNotifications::GetOrCreateFilter(BinaryReader& reader, APIResult* result)
    {
        Int32 currentFilterId = reader.ReadInt32();
        PushFilter* filter = NULL;

        if (currentFilterId != -1)
        {
            // filter should already exists
            PushFilter* filter = m_ActivePushFilters.Find(currentFilterId);

            if (filter == NULL)
            {
                ERROR_RESULT(result, "Existing filter can't be found");
                return NULL;
            }

            return filter;
        }

        filter = new PushFilter();

        if (filter->Deserialise(reader, result) == false)
        {
            delete filter;
            return NULL;
        }

        m_ActivePushFilters.Add(filter->GetFilterId(), filter);

        return filter;
    }

    PushFilter* WebApiNotifications::GetFilter(BinaryReader& reader, APIResult* result)
    {
        Int32 currentFilterId = reader.ReadInt32();

        if (currentFilterId != -1)
        {
            // filter should already exists
            PushFilter* filter = m_ActivePushFilters.Find(currentFilterId);

            if (filter == NULL)
            {
                ERROR_RESULT(result, "Existing filter can't be found");
                return NULL;
            }

            return filter;
        }

        return NULL;
    }

    void WebApiNotifications::RegisterFilterImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        WebApiNotifications* instance = Instance();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        PushFilter* filter = instance->GetOrCreateFilter(reader, result);

        if (filter == NULL)
        {
            ERROR_RESULT(result, "The Filter couldn't be created.");
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(filter->GetFilterId());

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void WebApiNotifications::UnregisterFilterImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        WebApiNotifications* instance = Instance();

        BinaryReader reader(sourceData, sourceSize);

        Int32 filterId = reader.ReadInt32();

        PushFilter* filter = instance->m_ActivePushFilters.Find(filterId);

        if (filter == NULL)
        {
            ERROR_RESULT(result, "Filter Id not found");
            return;
        }

        // Unregister the filter from the system.
        filter->Destroy();

        delete filter;

        instance->m_ActivePushFilters.Remove(filterId);

        SUCCESS_RESULT(result);
    }

    void WebApiNotifications::RegisterPushEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        WebApiNotifications* instance = Instance();

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 currentPushCallbackId = reader.ReadInt32();

        if (currentPushCallbackId != -1)
        {
            ERROR_RESULT(result, "Push event looks like it is already registered");
            return;
        }

        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userContext = WebApi::Instance()->FindUser(userId);

        if (userContext == NULL)
        {
            ERROR_RESULT(result, "No user context found");
            return;
        }

        bool orderGuaranteed = reader.ReadBool();

        PushFilter* filter = instance->GetFilter(reader, result);

        if (filter == NULL)
        {
            ERROR_RESULT(result, "The PushEvents filter can't be created or found");
            return;
        }

        bool ok = false;
        PushEventBase* newEvent;

        if (orderGuaranteed == false)
        {
            PushEvent* pushEvent = new PushEvent();
            ok = pushEvent->Deserialise(reader, userContext, filter, PushEventCallback, instance, result);
            newEvent = pushEvent;
        }
        else
        {
            OrderGuaranteedPushEvent* pushEvent = new OrderGuaranteedPushEvent();
            ok = pushEvent->Deserialise(reader, userContext, filter, OrderGuaranteedEventCallback, instance, result);
            newEvent = pushEvent;
        }

        int32_t callbackId = newEvent->GetCallbackID();

        if (ok == true)
        {
            // Keep track of the event
            instance->m_ActivePushEvents.Add(callbackId, newEvent);
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(callbackId);

        *resultsSize = writer.GetWrittenLength();

        if (ok == false)
        {
            return;
        }

        SUCCESS_RESULT(result);
    }

    void WebApiNotifications::UnregisterPushEventImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        WebApiNotifications* instance = Instance();

        BinaryReader reader(sourceData, sourceSize);

        Int32 callbackId = reader.ReadInt32();

        PushEventBase* pushEvent = instance->m_ActivePushEvents.Find(callbackId);

        if (pushEvent == NULL)
        {
            ERROR_RESULT(result, "Event callback Id not found");
            return;
        }

        if (pushEvent->IsOrderGuaranteed())
        {
            OrderGuaranteedPushEvent* pEvent = (OrderGuaranteedPushEvent*)pushEvent;
            pEvent->Destroy();
            delete pEvent;
        }
        else
        {
            PushEvent* pEvent = (PushEvent*)pushEvent;
            pEvent->Destroy();
            delete pEvent;
        }

        instance->m_ActivePushEvents.Remove(callbackId);

        SUCCESS_RESULT(result);
    }

    PushEventBase* WebApiNotifications::FindPushEvent(int pushCallbackId)
    {
        WebApiNotifications* instance = Instance();

        return instance->m_ActivePushEvents.Find(pushCallbackId);
    }

    OrderGuaranteedPushEvent* WebApiNotifications::FindOrderedPushEvent(int pushCallbackId)
    {
        WebApiNotifications* instance = Instance();

        PushEventBase* pushEvent = instance->m_ActivePushEvents.Find(pushCallbackId);

        if (pushEvent == NULL)
        {
            return NULL;
        }

        if (pushEvent->IsOrderGuaranteed() == false)
        {
            return NULL;
        }

        return (OrderGuaranteedPushEvent*)pushEvent;
    }

    void WebApiNotifications::CommonEventCallback(int32_t userCtxId, int32_t callbackId, const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
        const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
        const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
        const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg,
        bool isOrderGuaranteed, const SceNpWebApi2PushEventPushContextId *pPushCtxId, SceNpWebApi2PushEventPushContextCallbackType cbType)
    {
        if (!pUserArg || !pDataType)
        {
            return;
        }

        CallbackParams* params = new CallbackParams();

        memset(params, 0, sizeof(CallbackParams));

        params->userCtxId = userCtxId;
        params->callbackId = callbackId;

        params->orderGuaranteed = isOrderGuaranteed;

        params->pNpServiceName = pNpServiceName;
        params->serviceNameLen = pNpServiceName != NULL ? strlen(pNpServiceName) : 0;

        params->npServiceLabel = npServiceLabel;

        params->hasToPeer = pTo != NULL ? true : false;
        params->toAccountId = pTo != NULL ? pTo->accountId : 0;
        params->toPlatform = pTo != NULL ? pTo->platform : 0;

        params->toOnlineId = pToOnlineId != NULL ? pToOnlineId->data : NULL;
        params->toOnlineIdLen = pToOnlineId != NULL ? strlen(pToOnlineId->data) : 0;

        params->hasFromPeer = pFrom != NULL ? true : false;
        params->fromAccountId = pFrom != NULL ? pFrom->accountId : 0;
        params->fromPlatform = pFrom != NULL ? pFrom->platform : 0;

        params->fromOnlineId = pFromOnlineId != NULL ? pFromOnlineId->data : NULL;
        params->fromOnlineIdLen = pFromOnlineId != NULL ? strlen(pFromOnlineId->data) : 0;

        params->pDataType = pDataType != NULL ? pDataType->val : NULL;
        params->dataTypeLen = pDataType != NULL ? strlen(pDataType->val) : 0;

        params->pData = pData;
        params->dataLen = dataLen;

        if (extdDataNum == 0)
        {
            params->extdData = 0;
            params->extdDataNum = 0;
        }
        else
        {
            params->extdData = new CallbackExtdData[extdDataNum];
            params->extdDataNum = extdDataNum;
            params->extdStructSize = sizeof(CallbackExtdData);

            for (int i = 0; i < extdDataNum; i++)
            {
                params->extdData[i].extdDataKey = pExtdData[i].extdDataKey.val;
                params->extdData[i].extdDataKeyLen = strlen(pExtdData[i].extdDataKey.val);
                params->extdData[i].pData = pExtdData[i].pData;
                params->extdData[i].dataLen = pExtdData[i].dataLen;
            }
        }

        params->pushCtxId = NULL;
        params->pushCtxIdLen = 0;
        params->cbType = SceNpWebApi2PushEventPushContextCallbackType::SCE_NP_WEBAPI2_PUSH_EVENT_PUSH_CONTEXT_CALLBACK_TYPE_UNKNOWN;

        if (isOrderGuaranteed == true)
        {
            params->pushCtxId = pPushCtxId != NULL ? pPushCtxId->uuid : NULL;
            params->pushCtxIdLen = pPushCtxId != NULL ? strlen(pPushCtxId->uuid) : 0;
            params->cbType = cbType;
        }

        int32_t size = sizeof(CallbackParams);

        if (sEventHandlerCallback)
        {
            sEventHandlerCallback(params, size);
        }

        if (params->extdData != NULL)
        {
            delete[] params->extdData;
        }

        delete params;
    }

    void WebApiNotifications::PushEventCallback(int32_t userCtxId, int32_t callbackId, const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
        const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
        const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
        const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg)
    {
        //printf("WebApiNotifications::PushEventCallback");
        CommonEventCallback(userCtxId, callbackId, pNpServiceName, npServiceLabel,
            pTo, pToOnlineId, pFrom, pFromOnlineId,
            pDataType, pData, dataLen,
            pExtdData, extdDataNum, pUserArg);
    }

    void WebApiNotifications::OrderGuaranteedEventCallback(int32_t userCtxId, int32_t callbackId,
        const SceNpWebApi2PushEventPushContextId *pPushCtxId, SceNpWebApi2PushEventPushContextCallbackType cbType,
        const char *pNpServiceName, SceNpServiceLabel npServiceLabel,
        const SceNpPeerAddressA *pTo, const SceNpOnlineId *pToOnlineId, const SceNpPeerAddressA *pFrom, const SceNpOnlineId *pFromOnlineId,
        const SceNpWebApi2PushEventDataType *pDataType, const char *pData, size_t dataLen,
        const SceNpWebApi2PushEventExtdData *pExtdData, size_t extdDataNum, void *pUserArg)
    {
        //printf("WebApiNotifications::OrderGuaranteedEventCallback");
        CommonEventCallback(userCtxId, callbackId, pNpServiceName, npServiceLabel,
            pTo, pToOnlineId, pFrom, pFromOnlineId,
            pDataType, pData, dataLen,
            pExtdData, extdDataNum, pUserArg,
            true, pPushCtxId, cbType);
    }

    void WebApiNotifications::Create()
    {
    }

    void WebApiNotifications::Destroy()
    {
    }
}
