#ifndef _MAIN_H
#define _MAIN_H

#include "SharedCoreIncludes.h"

namespace psn
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

    class Main
    {
    public:

    private:

        static bool s_Initialised;

    public:

        static void Initialize(InitResult& initResult, APIResult* result);
        static void Update();
        static void Shutdown(APIResult* result);

        static int LoadModules(APIResult* result);
        static int UnloadModules(APIResult* result);

        //static void InitializeNpUniversalDataSystemLib();
        //static void TerminateNpUniversalDataSystemLib();

        static PrxPluginInterface s_PrxInterface;

    private:

        static SceKernelModule s_CoreHandle;
    };
}

#endif  //_MAIN_H
