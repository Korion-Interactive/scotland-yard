#include <stdio.h>

#include "CoreMain.h"

#include <rtc.h>
#include <libsysmodule.h>
#include <sdk_version.h>

#include "HandleMsg.h"

#include "WebApi.h"
#include "WebApiNotifications.h"

#include "Users.h"
#include "UniversalDataSystem.h"
#include "GameIntent.h"
#include "TrophyService.h"
#include "FeatureGating.h"
#include "GameUpdate.h"
#include "OnlineSafety.h"
#include "Authentication.h"
#include "SessionsManager.h"
#include "Entitlements.h"
#include "Commerce.h"
#include "PlayerReviewDialog.h"
#include "PlayerInvitationDialog.h"
#include "MsgDialog.h"
#include "Leaderboards.h"
#include "SessionSignalling.h"
#include "Sockets.h"
#include "TitleCloudStorage.h"
#include "Bandwidth.h"

#include <common_dialog.h>

#pragma comment(lib,"libSceNpCppWebApi_stub_weak.a")
#pragma comment(lib,"libSceNpWebApi2_stub_weak.a")
#pragma comment(lib,"libSceJson2_stub_weak.a")


extern size_t g_args;
extern const void *g_argp;

namespace psn
{
    DO_EXPORT(void, PSN_PrxInitialize) (InitResult* initResult, APIResult* result)
    {
        Main::Initialize(*initResult, result);
    }

    DO_EXPORT(void, PSN_PrxUpdate) ()
    {
        Main::Update();
    }

    DO_EXPORT(void, PSN_PrxShutDown) (APIResult* result)
    {
        Main::Shutdown(result);
    }

    DO_EXPORT(void, PSN_PrxProcessMsg) (void* sourceData, int sourceSize, void* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        MsgHandler::ProcessMsg((UInt8*)sourceData, sourceSize, (UInt8*)resultsData, resultsMaxSize, resultsSize, result);
    }

    // Main Class

    // Static Initialisation
    bool Main::s_Initialised = false;
    PrxPluginInterface Main::s_PrxInterface;
    SceKernelModule Main::s_CoreHandle;

    // Methods
    void Main::Initialize(InitResult& initResult, APIResult* result)
    {
        if (s_Initialised)
        {
            ERROR_RESULT(result, "PSNCore Plugin already initialised"); // Already initialised
            return;
        }

        int ret = 0;
#if BUILD_AS_PRX
        PRXHelphers::LoadPRX("/app0/Media/Plugins/PSNCommon.prx", s_CoreHandle);
#else
        // as long as the plugin args are initialised correctly in start_module() from the il2cpp plugin then we can start the system event manager using the event queue setup from here.
        if (g_args != -1)
            Main::s_PrxInterface.InitialisePrxPluginArgs(g_args, g_argp, "PSN");
#endif

        ret = LoadModules(result);
        if (ret < 0)
        {
            return;
        }

        s_PrxInterface.SetupRuntimeInterfaces();

#if defined(GLOBAL_EVENT_QUEUE)
        MsgHandler::InitialiseSystemEventManager(s_PrxInterface.m_IEventQueue);
#endif

        s_Initialised = true;

        initResult.initialized = true;
#if __ORBIS__
        initResult.sceSDKVersion = SCE_ORBIS_SDK_VERSION;
#else
        initResult.sceSDKVersion = SCE_PROSPERO_SDK_VERSION;
#endif

        ret = WebApi::Initialise();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        WebApiNotifications::Initialise();

        UserSystem::RegisterMethods();

#if !__ORBIS__
        UniversalDataSystem::InitializeLib();
        UniversalDataSystem::RegisterMethods();
#endif

        GameIntent::InitializeLib();
        GameIntent::RegisterMethods();

        WebApiNotifications::RegisterMethods();

#if !__ORBIS__
        TrophyService::RegisterMethods();
        FeatureGating::RegisterMethods();

        GameUpdate::InitializeLib();
        GameUpdate::RegisterMethods();
#endif
        Bandwidth::RegisterMethods();

        OnlineSafety::RegisterMethods();
        Authentication::RegisterMethods();

        SessionsManager::InitializeLib();
        SessionsManager::RegisterMethods();

        Leaderboards::RegisterMethods();

        TitleCloudStorage::RegisterMethods();

#if !__ORBIS__
        EntitlementsSystem::Initialize();
        EntitlementsSystem::RegisterMethods();

        CommerceCommands::RegisterMethods();
        PlayerReviewDialog::RegisterMethods();
        PlayerInvitationDialog::RegisterMethods();
#endif

        SessionSignalling::InitializeLib(WebApi::Instance()->GetLibHttp2CtxId());
        SessionSignalling::RegisterMethods();
        Sockets::RegisterMethods();

        MsgDialog::RegisterMethods();
        MsgDialog::InitializeLib();

        SUCCESS_RESULT(result);
    }

    void Main::Update()
    {
        if (s_Initialised == false)
        {
            return;
        }

        int ret = sceNpCheckCallback();

        if (ret != 0)
        {
        }
    }

    void Main::Shutdown(APIResult* result)
    {
#if defined(GLOBAL_EVENT_QUEUE)
        MsgHandler::ShutdownSystemEventManager();
#endif

        PRXHelphers::UnloadPRX(s_CoreHandle);
#if !__ORBIS__
        MsgDialog::TerminateLib();
        UniversalDataSystem::TerminateLib();
        GameUpdate::TerminateLib();
#endif
        GameIntent::TerminateLib();

        SessionsManager::TerminateLib();

        WebApiNotifications::Terminate();
        WebApi::Terminate();

        int ret = UnloadModules(NULL);
        if (ret < 0)
        {
            return;
        }

        s_Initialised = false;
    }

    int Main::LoadModules(APIResult* result)
    {
        int ret = 0;

#if !__ORBIS__
        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM", ret);
            return ret;
        }

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_TROPHY2);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_TROPHY2, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_TROPHY2", ret);
            return ret;
        }

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS", ret);
            return ret;
        }
#endif

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_UTILITY);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_UTILITY, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_UTILITY", ret);
            return ret;
        }

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_CPP_WEB_API);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_CPP_WEB_API, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_CPP_WEB_API", ret);
            return ret;
        }

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_AUTH);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_AUTH, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_AUTH", ret);
            return ret;
        }

#if !__ORBIS__
        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_COMMERCE);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_COMMERCE, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_COMMERCE", ret);
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_PLAYER_REVIEW_DIALOG);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_PLAYER_REVIEW_DIALOG, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_PLAYER_REVIEW_DIALOG", ret);
            return ret;
        }
#endif
#endif

        ret = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_SESSION_SIGNALING);

        if (ret < 0)
        {
            //UNITY_TRACE("Error loading SCE_SYSMODULE_NP_SESSION_SIGNALING, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error loading SCE_SYSMODULE_NP_SESSION_SIGNALING", ret);
            return ret;
        }

        return SCE_OK;
    }

    int Main::UnloadModules(APIResult* result)
    {
        int ret;

#if !__ORBIS__
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_SESSION_SIGNALING);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_SESSION_SIGNALING, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_SESSION_SIGNALING", ret);
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_PLAYER_REVIEW_DIALOG);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_PLAYER_REVIEW_DIALOG, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_PLAYER_REVIEW_DIALOG", ret);
            return ret;
        }
#endif
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_COMMERCE);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_COMMERCE, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_COMMERCE", ret);
            return ret;
        }
#endif

        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_CPP_WEB_API);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_CPP_WEB_API, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_CPP_WEB_API", ret);
            return ret;
        }

        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_AUTH);
        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_AUTH, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_AUTH", ret);
            return ret;
        }

#if !__ORBIS__
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM);
        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_UNIVERSAL_DATA_SYSTEM", ret);
            return ret;
        }
#if (0)  // unloading and reloading the trophy module causes a crash in sceNpCheckCallback()
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_TROPHY2);
        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_TROPHY2, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_TROPHY2", ret);
            return ret;
        }
#endif
        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_ENTITLEMENT_ACCESS", ret);
            return ret;
        }
#endif

        ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_UTILITY);

        if (ret < 0)
        {
            //UNITY_TRACE("Error unloading SCE_SYSMODULE_NP_UTILITY, 0x%x\n", ret);
            SCE_ERROR_RESULT_MSG(result, "Error unloading SCE_SYSMODULE_NP_UTILITY", ret);
            return ret;
        }

        return SCE_OK;
    }
#if BUILD_AS_PRX
    extern "C" int module_start(size_t sz, const void* arg)
    {
        if (!Main::s_PrxInterface.InitialisePrxPluginArgs(sz, arg, "PSNCore"))
        {
            // Failed.
            return SCE_KERNEL_START_NO_RESIDENT;
        }

        return SCE_KERNEL_START_SUCCESS;
    }
#endif
}
