
#include "Main.h"

#include <rtc.h>
#include <libsysmodule.h>
#include <stdio.h>
#include <sdk_version.h>

#include "Mount.h"

#pragma comment(lib, "SceSaveData_stub_weak")
#pragma comment(lib, "SceSaveDataDialog_stub_weak")
#pragma comment(lib, "SceUserService_stub_weak")
#pragma comment(lib, "SceSysmodule_stub_weak")
#pragma comment(lib, "SceRtc_stub_weak")

namespace SaveData
{
	PRX_EXPORT void PrxSaveDataInitialize(InitResult* initResult, APIResult* result)
	{
		Main::Initialize(*initResult, result);
	}

	PRX_EXPORT void PrxSaveDataTerminate(APIResult* result)
	{
		Main::Terminate(result);
	}

	PRX_EXPORT void PrxSaveDataSetThreadAffinity(ThreadSettings settings, APIResult* result)
	{
		Main::SetThreadAffinity(settings, result);
	}

	bool Main::s_Initialised = false;
#if BUILD_PRX		
	IPluginUnity* Main::s_IUnity = NULL;
	IPluginSceAppParams* Main::s_ISceAppParams = NULL;
	IPluginSceNpParams* Main::s_ISceNpParams = NULL;
#endif
	void Main::Initialize(InitResult& initResult, APIResult* result)
	{
		if (s_Initialised)
		{
			ERROR_RESULT(result, "SaveData Plugin already initialised"); // Already initialised
			return;
		}

		MemoryBuffer::Initialise();

		int32_t ret = sceSaveDataInitialize3(NULL);

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		ret = sceSysmoduleLoadModule(SCE_SYSMODULE_SAVE_DATA_DIALOG);

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		//sceSaveDataDialogInitialize();

		s_Initialised = true;

		initResult.initialized = true;
		initResult.sceSDKVersion = SCE_ORBIS_SDK_VERSION;

		SUCCESS_RESULT(result);
	}

	void Main::Terminate(APIResult* result)
	{
		int32_t ret = sceSaveDataTerminate();

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		//ret = sceSaveDataDialogTerminate();

		//if (ret != SCE_OK)
		//{
		//	SCE_ERROR_RESULT(result, ret);
		//	return;
		//}

		ret = sceSysmoduleUnloadModule(SCE_SYSMODULE_SAVE_DATA_DIALOG);

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		MemoryBuffer::Shutdown();

		s_Initialised = false;

		SUCCESS_RESULT(result);
	}


	void Main::SetThreadAffinity(ThreadSettings settings, APIResult* result)
	{
		ScePthread thread = scePthreadSelf();

		int32_t ret = scePthreadRename(thread, settings.name);

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		ret = scePthreadSetaffinity(thread, settings.affinityMask);

		if (ret != SCE_OK)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Main::LoadModules()
	{
#if BUILD_PRX	
		int res = sceSysmoduleLoadModule(SCE_SYSMODULE_NP_TOOLKIT2);
		if (res != 0)
		{
			//printf("Error loading SCE_SYSMODULE_NP_TOOLKIT, 0x%x\n", res);
		}
#endif	
	}

	void Main::UnloadModules()
	{
#if BUILD_PRX	
		int res = sceSysmoduleUnloadModule(SCE_SYSMODULE_NP_TOOLKIT2);
		if (res != 0)
		{
			//printf("Error loading SCE_SYSMODULE_NP_TOOLKIT, 0x%x\n", res);
		}
#endif	
	}

	void Main::SetupRuntimeInterfaces()
	{
#if BUILD_PRX				
		if (g_QueryInterface)
		{
			s_IUnity = GetRuntimeInterface<IPluginUnity>(PRX_PLUGIN_IFACE_ID_UNITY);
			s_ISceAppParams = GetRuntimeInterface<IPluginSceAppParams>(PRX_PLUGIN_IFACE_ID_SCE_APP_PARAMS);
			s_ISceNpParams = GetRuntimeInterface<IPluginSceNpParams>(PRX_PLUGIN_IFACE_ID_SCE_NP_PARAMS);
		}
#endif		
	}

#if BUILD_PRX
	extern "C" int module_start(SceSize sz, const void* arg)
	{
		if (!ProcessPrxPluginArgs(sz, arg, "SaveData2"))
		{
			// Failed.
			return SCE_KERNEL_START_NO_RESIDENT;
		}

		return SCE_KERNEL_START_SUCCESS;
	}
#endif
}