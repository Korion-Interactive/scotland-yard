#pragma once
#include "SharedCoreIncludes.h"
#include <map>

namespace psn
{
    class Authentication
    {
    public:

        enum Methods
        {
            GetAuthorizationCode = 0x0900001u,
            GetIdToken = 0x0900002u,
        };

        static void RegisterMethods();
        static void GetAuthorizationCodeImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetIdTokenImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
    };
}
