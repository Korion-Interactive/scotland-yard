#include "WebApiPushEvents.h"


namespace psn
{
    PushFilter::PushFilter() :
        m_PushHandleId(INVALID_HANDLE_ID),
        m_PushFilterId(INVALID_HANDLE_ID),
        m_npServiceLabel(0),
        m_FilterParams(NULL),
        m_NumberFilterParams(0),
        m_LastError(0),
        m_ReferenceCount(0)
    {
    }

    PushFilter::~PushFilter()
    {
        m_LastError = Destroy();
    }

    int PushFilter::Destroy()
    {
        int32_t libCtxId = WebApi::Instance()->GetLibCtxId();
        int ret = 0;

        if (m_FilterParams != NULL)
        {
            delete m_FilterParams;
            m_FilterParams = NULL;
            m_NumberFilterParams = 0;
        }

        if (m_PushFilterId != INVALID_HANDLE_ID)
        {
            ret = sceNpWebApi2PushEventDeleteFilter(libCtxId, m_PushFilterId);
            if (ret < 0)
            {
                m_LastError = ret;
                return ret;
            }
            m_PushFilterId = INVALID_HANDLE_ID;
        }

        if (m_PushHandleId != INVALID_HANDLE_ID)
        {
            ret = sceNpWebApi2PushEventDeleteHandle(libCtxId, m_PushHandleId);
            if (ret < 0)
            {
                m_LastError = ret;
                return ret;
            }
            m_PushHandleId = INVALID_HANDLE_ID;
        }

        return ret;
    }

    int PushFilter::Create(const char* serviceName, SceNpServiceLabel npServiceLabel, const char* filterTypes[], size_t filterTypesNum)
    {
        strncpy(m_ServiceName, serviceName, MAX_SERVICE_NAME_LENGTH);
        m_npServiceLabel = npServiceLabel;

        m_NumberFilterParams = filterTypesNum;

        m_FilterParams = new SceNpWebApi2PushEventFilterParameter[m_NumberFilterParams];

        for (size_t i = 0; i < m_NumberFilterParams; ++i)
        {
            memset(&m_FilterParams[i].dataType, 0, sizeof(m_FilterParams[i].dataType));
            snprintf(m_FilterParams[i].dataType.val, SCE_NP_WEBAPI2_PUSH_EVENT_DATA_TYPE_LEN_MAX, "%s", filterTypes[i]);
            m_FilterParams[i].pExtdDataKey = nullptr;
            m_FilterParams[i].extdDataKeyNum = 0;
        }

        int ret = PushFilter::Register();

        return ret;
    }

    int PushFilter::Register()
    {
        int32_t libCtxId = WebApi::Instance()->GetLibCtxId();

        int ret = sceNpWebApi2PushEventCreateHandle(libCtxId);
        if (ret < 0)
        {
            m_LastError = ret;
            return ret;
        }
        m_PushHandleId = ret;

        char* serviceName = m_ServiceName;
        if (m_ServiceName[0] == 0)
        {
            serviceName = SCE_NP_WEBAPI2_NP_SERVICE_NAME_NONE;
        }

        ret = sceNpWebApi2PushEventCreateFilter(libCtxId, m_PushHandleId, serviceName, m_npServiceLabel, m_FilterParams, m_NumberFilterParams);

        if (ret < 0)
        {
            Destroy();
            m_LastError = ret;
            return ret;
        }

        m_PushFilterId = ret;

        return ret;
    }

    bool PushFilter::Deserialise(BinaryReader& reader, APIResult* result)
    {
        char* serviceName = reader.ReadStringPtr();
        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        if (serviceName != NULL)
        {
            strncpy(m_ServiceName, serviceName, MAX_SERVICE_NAME_LENGTH);
        }
        else
        {
            m_ServiceName[0] = 0;
        }

        m_npServiceLabel = serviceLabel;

        m_NumberFilterParams = reader.ReadUInt32();

        if (m_NumberFilterParams == 0)
        {
            SUCCESS_RESULT(result);
            return false;
        }

        m_FilterParams = new SceNpWebApi2PushEventFilterParameter[m_NumberFilterParams];

        for (int i = 0; i < m_NumberFilterParams; i++)
        {
            // Read dataType string e.g. "np:service:friendlist:friend"
            char* dataType = reader.ReadStringPtr();

            memset(&m_FilterParams[i].dataType, 0, sizeof(m_FilterParams[i].dataType));

            snprintf(m_FilterParams[i].dataType.val, SCE_NP_WEBAPI2_PUSH_EVENT_DATA_TYPE_LEN_MAX, "%s", dataType);
            m_FilterParams[i].pExtdDataKey = nullptr;
            m_FilterParams[i].extdDataKeyNum = 0;

            UInt32 numberExts = reader.ReadUInt32();

            if (numberExts > 0)
            {
                // Filter has extended keys
                SceNpWebApi2PushEventExtdDataKey* extDataKey = new SceNpWebApi2PushEventExtdDataKey[numberExts];

                for (int e = 0; e < numberExts; e++)
                {
                    char* extKey = reader.ReadStringPtr();

                    memset(&extDataKey[e], 0, sizeof(extDataKey[e]));

                    snprintf(extDataKey[e].val, SCE_NP_WEBAPI2_PUSH_EVENT_EXTD_DATA_KEY_LEN_MAX, "%s", extKey);
                }

                m_FilterParams[i].pExtdDataKey = extDataKey;
                m_FilterParams[i].extdDataKeyNum = numberExts;
            }
        }

        int ret = PushFilter::Register();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        return true;
    }

    PushEventBase::PushEventBase() :
        m_UserContext(NULL),
        m_PushCallbackId(INVALID_HANDLE_ID),
        m_IsOrderGuaranteed(false)
    {
    }

    PushEvent::PushEvent()
    {
    }

    PushEvent::~PushEvent()
    {
    }

    int PushEvent::Create(WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventCallback cbFunc, void* pUserArg)
    {
        m_UserContext = userContext;
        m_Filter = filter;

        int ret;

        /*ret = m_Filter.Create(serviceName, npServiceLabel, filterTypes, filterTypesNum);
        if (ret < 0)
        {
            Destroy();
            return ret;
        }*/

        ret = sceNpWebApi2PushEventRegisterCallback(m_UserContext->GetUserCtxId(), m_Filter->GetFilterId(), cbFunc, pUserArg);
        if (ret < 0)
        {
            Destroy();
            return ret;
        }

        m_PushCallbackId = ret;

        m_Filter->IncRefCount();

        return ret;
    }

    int PushEvent::Destroy()
    {
        int ret = 0;

        if (m_PushCallbackId != INVALID_HANDLE_ID)
        {
            ret = sceNpWebApi2PushEventUnregisterCallback(m_UserContext->GetUserCtxId(), m_PushCallbackId);
            if (ret < 0)
            {
                return ret;
            }
            m_PushCallbackId = INVALID_HANDLE_ID;
        }

        m_Filter->DecRefCount();

        /*  ret = m_Filter.Destroy();
            if (ret < 0)
            {
                return ret;
            }*/

        m_UserContext = NULL;

        return ret;
    }

    bool PushEvent::Deserialise(BinaryReader& reader, WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventCallback cbFunc, void* pUserArg, APIResult* result)
    {
        m_UserContext = userContext;
        m_Filter = filter;

        /*bool ok = m_Filter.Deserialise(reader, result);

        if (ok == false)
        {
            Destroy();
            return false;
        }*/

        int ret = sceNpWebApi2PushEventRegisterCallback(m_UserContext->GetUserCtxId(), m_Filter->GetFilterId(), cbFunc, pUserArg);
        if (ret < 0)
        {
            Destroy();
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        m_PushCallbackId = ret;
        m_Filter->IncRefCount();

        SUCCESS_RESULT(result);

        return true;
    }

    OrderGuaranteedPushEvent::OrderGuaranteedPushEvent() :
        m_PushContextIdCreated(false)
    {
        m_IsOrderGuaranteed = true;
    }

    OrderGuaranteedPushEvent::~OrderGuaranteedPushEvent()
    {
    }

    int OrderGuaranteedPushEvent::Create(WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventPushContextCallback cbFunc, void* pUserArg)
    {
        m_UserContext = userContext;
        m_Filter = filter;

        int ret;

        /*ret = m_Filter.Create(serviceName, npServiceLabel, filterTypes, filterTypesNum);
        if (ret < 0)
        {
            Destroy();
            return ret;
        }*/

        ret = sceNpWebApi2PushEventRegisterPushContextCallback(m_UserContext->GetUserCtxId(), m_Filter->GetFilterId(), cbFunc, pUserArg);

        if (ret < 0)
        {
            Destroy();
            return ret;
        }

        m_PushCallbackId = ret;
        m_Filter->IncRefCount();

        ret = sceNpWebApi2PushEventCreatePushContext(m_UserContext->GetUserCtxId(), &m_PushContextId);

        if (ret < 0)
        {
            Destroy();
            return ret;
        }

        m_PushContextIdCreated = true;

        ret = sceNpWebApi2PushEventStartPushContextCallback(m_UserContext->GetUserCtxId(), &m_PushContextId);
        if (ret < 0)
        {
            Destroy();
            return ret;
        }

        return ret;
    }

    int OrderGuaranteedPushEvent::Destroy()
    {
        int ret = 0;

        if (m_PushContextIdCreated == true)
        {
            ret = sceNpWebApi2PushEventDeletePushContext(m_UserContext->GetUserCtxId(), &m_PushContextId);
            if (ret < 0)
            {
                return ret;
            }
            m_PushContextIdCreated = false;
        }

        if (m_PushCallbackId != INVALID_HANDLE_ID)
        {
            ret = sceNpWebApi2PushEventUnregisterCallback(m_UserContext->GetUserCtxId(), m_PushCallbackId);
            if (ret < 0)
            {
                return ret;
            }
            m_PushCallbackId = INVALID_HANDLE_ID;
        }

        m_Filter->DecRefCount();

        /*ret = m_Filter.Destroy();
        if (ret < 0)
        {
            return ret;
        }
*/
        m_UserContext = NULL;

        return ret;
    }

    bool OrderGuaranteedPushEvent::Deserialise(BinaryReader& reader, WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventPushContextCallback cbFunc, void* pUserArg, APIResult* result)
    {
        m_UserContext = userContext;
        m_Filter = filter;

        /*bool ok = m_Filter.Deserialise(reader, result);

        if (ok == false)
        {
            Destroy();
            return false;
        }*/

        int ret = sceNpWebApi2PushEventRegisterPushContextCallback(m_UserContext->GetUserCtxId(), m_Filter->GetFilterId(), cbFunc, pUserArg);

        if (ret < 0)
        {
            Destroy();
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        m_PushCallbackId = ret;
        m_Filter->IncRefCount();

        ret = sceNpWebApi2PushEventCreatePushContext(m_UserContext->GetUserCtxId(), &m_PushContextId);

        if (ret < 0)
        {
            Destroy();
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        m_PushContextIdCreated = true;

        ret = sceNpWebApi2PushEventStartPushContextCallback(m_UserContext->GetUserCtxId(), &m_PushContextId);
        if (ret < 0)
        {
            Destroy();
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        SUCCESS_RESULT(result);

        return true;
    }
}
