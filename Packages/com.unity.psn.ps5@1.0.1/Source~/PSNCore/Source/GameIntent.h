#pragma once
#include "SharedCoreIncludes.h"
#include <map>

namespace psn
{
    class GameIntent
    {
    public:

        enum Methods
        {
            FetchGameIntent = 0x0400001u,
        };

        static void InitializeLib();
        static void TerminateLib();
        static void RegisterMethods();

        static void FetchGameIntentImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void HandleSystemEvent(SceSystemServiceEvent& sevent);

        static void WriteIntentProperty(SceNpGameIntentData& intentData, const char* key, int maxValueLength, BinaryWriter& writer);

        static std::list<SceNpGameIntentInfo> s_PendingGameIntentList;
    };
}
