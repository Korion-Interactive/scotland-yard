#ifndef _MAIN_H
#define _MAIN_H

#include "../Includes/PluginCommonIncludes.h"

namespace SaveData
{
	struct InitResult
	{
	public:
		bool initialized;
		UInt32 sceSDKVersion; // SCE_ORBIS_SDK_VERSION 

		InitResult()
		{
			initialized = false;
		}
	};

	struct ThreadSettings
	{
	public:
		char name[32];
		UInt64 affinityMask;
	};

	class Main
	{
	public:

	private:

		static bool s_Initialised;

		static IPluginUnity* s_IUnity;
		static IPluginSceAppParams* s_ISceAppParams;
		static IPluginSceNpParams* s_ISceNpParams;

	public:

		static void Initialize(InitResult& initResult, APIResult* result);
		static void Terminate(APIResult* result);
		static void SetThreadAffinity(ThreadSettings settings, APIResult* result);

		static void LoadModules();
		static void UnloadModules();

	private:

		static void SetupRuntimeInterfaces();

	};
}

#endif	//_MAIN_H

