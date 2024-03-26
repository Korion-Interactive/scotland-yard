#pragma once

#include "SharedCoreIncludes.h"
#include <list>

#include "WebApi.h"

namespace psn
{
    // Filters
    //    Each type of WebApi notification is a filter.
    //        e.g. "np:service:friendlist:friend"
    //    Each type is currently set up as a seperate filter and has an Id.

    class PushFilter
    {
    public:
        PushFilter();
        ~PushFilter();

        int Create(const char* serviceName, SceNpServiceLabel npServiceLabel, const char* filterTypes[], size_t filterTypesNum);
        int Destroy();

        int32_t GetFilterId() { return m_PushFilterId; }

        bool Deserialise(BinaryReader& reader, APIResult* result);

        void IncRefCount() { m_ReferenceCount++; }
        void DecRefCount() { m_ReferenceCount--; }
        int32_t GetRefCount() { return m_ReferenceCount; }

    private:

        int Register();

        int32_t m_PushHandleId;
        int32_t m_PushFilterId;

        char m_ServiceName[MAX_SERVICE_NAME_LENGTH];
        SceNpServiceLabel m_npServiceLabel;

        SceNpWebApi2PushEventFilterParameter* m_FilterParams;
        size_t m_NumberFilterParams;

        int32_t m_LastError;

        int32_t m_ReferenceCount;
    };

    class PushEventBase
    {
    public:

        PushEventBase();

        void SetFilter(PushFilter* filter) { m_Filter = filter; }
        PushFilter* GetFilter() { return m_Filter; }

        int32_t GetCallbackID() { return m_PushCallbackId; }

        bool IsOrderGuaranteed() { return m_IsOrderGuaranteed; }

    protected:
        WebApiUserContext* m_UserContext;
        PushFilter* m_Filter;
        int32_t m_PushCallbackId;
        bool m_IsOrderGuaranteed;
    };

    class PushEvent : public PushEventBase
    {
    public:
        PushEvent();
        ~PushEvent();

        int Create(WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventCallback cbFunc, void* pUserArg);
        int Destroy();

        bool Deserialise(BinaryReader& reader, WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventCallback cbFunc, void* pUserArg, APIResult* result);

    private:
    };

    class OrderGuaranteedPushEvent : public PushEventBase
    {
    public:
        OrderGuaranteedPushEvent();
        ~OrderGuaranteedPushEvent();

        int Create(WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventPushContextCallback cbFunc, void* pUserArg);
        int Destroy();

        SceNpWebApi2PushEventPushContextId* GetPushContextId() { return &m_PushContextId; }
        const char* GetPushContextIdStr() { return m_PushContextId.uuid; }

        bool Deserialise(BinaryReader& reader, WebApiUserContext* userContext, PushFilter* filter, SceNpWebApi2PushEventPushContextCallback cbFunc, void* pUserArg, APIResult* result);

    private:

        SceNpWebApi2PushEventPushContextId m_PushContextId;
        bool m_PushContextIdCreated;
    };
}
