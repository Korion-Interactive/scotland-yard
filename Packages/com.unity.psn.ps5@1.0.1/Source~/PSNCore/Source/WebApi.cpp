#include "WebApi.h"
#include "libhttp2.h"

#if __has_feature(cxx_rtti)
#include <json2_rtti.h>
#else
#include <json2.h>
#endif

#pragma comment(lib,"libSceSsl_stub_weak.a")
#pragma comment(lib,"libSceHttp2_stub_weak.a")



namespace psn
{
    UserMap<WebApiUserContext> WebApi::s_UsersList;

    void *WebApiAllocator::allocate(size_t size, void *user_data)
    {
        void *p = malloc(size);
        return p;
    }

    void WebApiAllocator::deallocate(void *ptr, void *user_data)
    {
        free(ptr);
    }

    void WebApiAllocator::notifyError(int32_t error, size_t size, void *userData)
    {
        switch (error)
        {
            case SCE_JSON_ERROR_NOMEM:
                UNITY_TRACE("allocate Fail. size = %ld\n", size);
                abort();
                break;
            default:
                UNITY_TRACE("unknown[%#x]\n", error);
                break;
        }
    }

    WebApiUserContext::WebApiUserContext(SceUserServiceUserId userId)
    {
        m_userId = userId;
        m_webapiUserCtxId = INVALID_HANDLE_ID;
    }

    int WebApiUserContext::Create(int32_t webapiLibCtxId)
    {
        int ret;

        ret = sceNpWebApi2CreateUserContext(webapiLibCtxId, m_userId);
        if (ret < 0)
        {
            return ret;
        }

        m_webapiUserCtxId = ret;

        return ret;
    }

    int WebApiUserContext::Destroy()
    {
        int ret = 0;

        if (m_webapiUserCtxId != INVALID_HANDLE_ID)
        {
            ret = sceNpWebApi2DeleteUserContext(m_webapiUserCtxId);

            if (ret < 0)
            {
                return ret;
            }
        }

        m_webapiUserCtxId = INVALID_HANDLE_ID;

        return ret;
    }

    WebApi::WebApi() :
        m_isInitialised(false),
        m_libnetMemId(INVALID_HANDLE_ID),
        m_libsslCtxId(INVALID_HANDLE_ID),
        m_libhttp2CtxId(INVALID_HANDLE_ID),
        m_webapiLibCtxId(INVALID_HANDLE_ID)
    {
    }

    WebApi* WebApi::s_webApiInstance = NULL;

    int WebApi::Create()
    {
        if (m_isInitialised == true)
        {
            return SCE_OK;
        }

        int32_t ret = 0;
        Common::InitParams initParams;
#if __has_feature(cxx_rtti)
        sce::Json::InitParameterRtti2 jsonInitParam;
#else
        sce::Json::InitParameter2 jsonInitParam;
#endif

        /* libnet */
        ret = sceNetPoolCreate("WebApiUserProfile", NET_HEAP_SIZE, 0);
        if (ret < 0)
        {
            return ret;
        }
        m_libnetMemId = ret;

        /* libSsl */
        ret = sceSslInit(SSL_HEAP_SIZE);
        if (ret < 0)
        {
            UNITY_TRACE("sceSslInit() error: 0x%x\n", ret);
            return ret;
        }
        m_libsslCtxId = ret;

        /* libHttp */
        ret = sceHttp2Init(m_libnetMemId, m_libsslCtxId, HTTP2_HEAP_SIZE, 1);
        if (ret < 0)
        {
            UNITY_TRACE("sceHttpInit() error: 0x%x\n", ret);
            return ret;
        }
        m_libhttp2CtxId = ret;

        ret = sceNpWebApi2Initialize(m_libhttp2CtxId, WEBAPI_HEAP_SIZE);
        if (ret < 0)
        {
            UNITY_TRACE("sceNpWebApi2Initialize() failed. ret = 0x%x\n", ret);
            return ret;
        }
        m_webapiLibCtxId = ret;

        ret = Common::initialize(initParams, m_cppWebapiLibCtx);
        if (ret < 0)
        {
            UNITY_TRACE("CppWebApi::Common::initialize() failed. ret = 0x%x\n", ret);
            return ret;
        }
#if __has_feature(cxx_rtti)
	    sce::Json::AllocParamRtti allocParam(WebApiAllocator::allocate, WebApiAllocator::deallocate, WebApiAllocator::notifyError);
        jsonInitParam.setAllocatorRtti(&allocParam, 0);
#else
        jsonInitParam.setAllocator(&m_allocator, 0);
#endif

        jsonInitParam.setFileBufferSize(JSON_FILE_BUFFER_SIZE);
        ret = m_initializer.initialize(&jsonInitParam);
        if (ret < 0)
        {
            UNITY_TRACE("CppWebApi::Common::initialize() failed. ret = 0x%x\n", ret);
            return ret;
        }

        m_isInitialised = true;
        return SCE_OK;
    }

    void WebApi::Destroy()
    {
        m_isInitialised = false;
    }

    int WebApi::Initialise()
    {
        if (s_webApiInstance != NULL)
        {
            return SCE_OK;
        }

        s_webApiInstance = new WebApi();
        int ret = s_webApiInstance->Create();
        if (ret < 0)
        {
            return ret;
        }

        MsgHandler::RegisterUserCallback(HandleUserState);

        return SCE_OK;
    }

    void WebApi::Terminate()
    {
        if (s_webApiInstance == NULL)
        {
            return;
        }

        s_webApiInstance->Destroy();
        delete s_webApiInstance;
        s_webApiInstance = NULL;
    }

    WebApiUserContext* WebApi::FindUser(SceUserServiceUserId userId)
    {
        WebApiUserContext* user = s_UsersList.FindUser(userId);

        if (user != NULL && user->m_webapiUserCtxId == INVALID_HANDLE_ID)
        {
            // User wasn't created correctly so have another go.
            user->Create(WebApi::Instance()->m_webapiLibCtxId);
        }

        return user;
    }

    void WebApi::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    {
        if (state == MsgHandler::UserState::Added)
        {
            if (s_UsersList.DoesUserExist(userId) == true)
            {
                // User already registered so don't do this again
                WARNING_RESULT(result, "User already initialised with Trophy service");
                return;
            }

            WebApiUserContext* user = s_UsersList.CreateUser(userId);

            user->Create(WebApi::Instance()->m_webapiLibCtxId);
        }
        else if (state == MsgHandler::UserState::Removed)
        {
            WebApiUserContext* user = s_UsersList.FindUser(userId);

            if (user == NULL)
            {
                WARNING_RESULT(result, "User not registered with trophy service");
                return;
            }

            if (user->m_webapiUserCtxId == 0)
            {
                ERROR_RESULT(result, "User WebApi context is invalid");
                return;
            }

            user->Destroy();

            s_UsersList.DeleteUser(userId);
        }

        SUCCESS_RESULT(result);
    }

    //OrderedPushNotificationsInternal::OrderedPushNotificationsInternal(int32_t userCtxId, const char* serviceName, SceNpServiceLabel npServiceLabel)
    //{
    //  strncpy(m_serviceName, serviceName, MAX_SERVICE_NAME_LENGTH);
    //  m_npServiceLabel = npServiceLabel;
    //  m_webapiUserCtxId = userCtxId;
    //}

    //void OrderedPushNotificationsInternal::Create()
    //{
    //  WebApiNotifications::s_Instance
    //}
}
