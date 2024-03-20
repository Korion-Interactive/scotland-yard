#pragma once
#include "SharedCoreIncludes.h"
#include <map>

namespace psn
{
    class OnlineSafety
    {
    public:

        enum Methods
        {
            GetCRS = 0x0800001u,    // Communication Restriction Status
            FilterProfanity = 0x0800002u,
            TestProfanity = 0x0800003u,
        };

        static void RegisterMethods();

        static void GetCRSImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void FilterProfanityImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void TestProfanityImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
    };
}
