#pragma once
#include "SharedCoreIncludes.h"
#include <map>

#if !__ORBIS__

namespace psn
{
    class GameUpdate
    {
    public:

        enum Methods
        {
            GameUpdateCheck = 0x1500001u,
            GameUpdateGetAddcontLatestVersion = 0x1500002u,
        };

        static void RegisterMethods();

        static void InitializeLib();
        static void TerminateLib();

        static void GameUpdateCheckImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GameUpdateGetAddcontLatestVersionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static bool s_ModuleLoaded;
    };
}

#endif
