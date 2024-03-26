#pragma once
#include "SharedCoreIncludes.h"
#include <map>

#if !__ORBIS__

namespace psn
{
    class FeatureGating
    {
    public:

        enum Methods
        {
            CheckPremium = 0x0700001u,
            NotifyPremiumFeature = 0x0700002u,
            StartPremiumEventCallback = 0x0700003u,
            StopPremiumEventCallback = 0x0700004u,
            FetchPremiumEvent = 0x0700005u,
        };

        static void RegisterMethods();

        static void CheckPremiumImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void NotifyPremiumFeatureImpl(UInt8* sourceData, int sourceSize, APIResult* result);

        static void StartPremiumEventCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void StopPremiumEventCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void FetchPremiumEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void PremiumEventCallback(SceUserServiceUserId userId, SceNpPremiumEventType eventType, void *userData);

        struct PremiumEvent
        {
            SceUserServiceUserId userId;
            SceNpPremiumEventType eventType;
        };

        static std::list<PremiumEvent> s_PendingPremiumEventsList;
    };
}

#endif
