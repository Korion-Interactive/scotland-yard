#pragma once

#include "SharedCoreIncludes.h"
#include <map>

#include "np_cppwebapi.h"

namespace sceCppWebApi = sce::Np::CppWebApi::Common;

using namespace sceCppWebApi;
using namespace sce::Np::CppWebApi;

namespace psn
{
    #define WEB_API_ERROR_BUFFURE_SIZE 1024
    #define SSL_HEAP_SIZE       (384 * 1024)
    #define HTTP2_HEAP_SIZE     (512 * 1024)
    #define NET_HEAP_SIZE       (16 * 1024)
    #define WEBAPI_HEAP_SIZE    (512 * 1024)
    #define JSON_FILE_BUFFER_SIZE   (512)


    #define INVALID_HANDLE_ID -1
    #define MAX_SERVICE_NAME_LENGTH 255

#if !__has_feature(cxx_rtti)
    class WebApiAllocator : public sce::Json::MemAllocator
    {
    public:
        WebApiAllocator() {}
        ~WebApiAllocator() {}
        virtual void *allocate(size_t size, void *user_data);
        virtual void deallocate(void *ptr, void *user_data);
        virtual void notifyError(int32_t error, size_t size, void *userData);
    };
#else
    class WebApiAllocator
    {
    public:
        WebApiAllocator() {}
        ~WebApiAllocator() {}
        static void *allocate(size_t size, void *user_data);
        static void deallocate(void *ptr, void *user_data);
        static void notifyError(int32_t error, size_t size, void *userData);
    };
#endif



    class WebApiUserContext
    {
    public:

        WebApiUserContext(SceUserServiceUserId userId);

        int Create(int32_t webapiLibCtxId);
        int Destroy();

        int32_t GetUserCtxId() { return m_webapiUserCtxId; }

        SceUserServiceUserId m_userId;
        int32_t m_webapiUserCtxId;

        int GetAccountId(SceNpAccountId* accountId)
        {
            *accountId = SCE_NP_INVALID_ACCOUNT_ID;
            return sceNpGetAccountIdA(m_userId, accountId);
        }

        int GetAccountIdStr(char* accountIdBuf, size_t bufferSize)
        {
            SceNpAccountId accountId;
            int ret = GetAccountId(&accountId);

            if (ret < 0)
            {
                return ret;
            }

            sprintf_s(accountIdBuf, bufferSize, "%lu", accountId);

            return SCE_OK;
        }
    };

    class WebApi
    {
    public:

    private:
        bool m_isInitialised;

        int32_t m_libnetMemId;
        int32_t m_libsslCtxId;
        int32_t m_libhttp2CtxId;

    public:

        int32_t m_webapiLibCtxId;
        Common::LibContext m_cppWebapiLibCtx;

        WebApiUserContext* FindUser(SceUserServiceUserId userId);

        int32_t GetLibCtxId() { return m_webapiLibCtxId; }
        Common::LibContext* GetLibCtx() { return &m_cppWebapiLibCtx; }

        int32_t GetLibHttp2CtxId() { return m_libhttp2CtxId; }

        static WebApi* Instance() { return s_webApiInstance; }

        static int Initialise();
        static void Terminate();

        static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);


    private:

        sce::Json::Initializer m_initializer;
#if !__has_feature(cxx_rtti)
        WebApiAllocator m_allocator;
#endif
        WebApi();

        static WebApi* s_webApiInstance;

        int Create();
        void Destroy();

        static UserMap<WebApiUserContext> s_UsersList;
    };

    //class OrderedPushNotificationsInternal
    //{
    //  #define MAX_SERVICE_NAME_LENGTH 255

    //public:
    //  OrderedPushNotificationsInternal(int32_t userCtxId, const char* serviceName, SceNpServiceLabel npServiceLabel);
    //  void Create();

    //private:
    //  char m_serviceName[MAX_SERVICE_NAME_LENGTH];
    //  SceNpServiceLabel m_npServiceLabel;

    //  int32_t m_webapiUserCtxId;
    //};
}
