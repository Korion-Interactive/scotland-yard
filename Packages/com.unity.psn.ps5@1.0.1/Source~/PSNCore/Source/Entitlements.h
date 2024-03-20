#pragma once
#include "SharedCoreIncludes.h"
#include <map>

#include <np/np_entitlement_access.h>

namespace psn
{
#if !__ORBIS__
    class EntitlementsSystem
    {
    public:

        enum Methods
        {
            GetAdditionalContentEntitlementInfoList = 0xFA00001u,
            GetSkuFlag = 0xFA00002u,
            GetAdditionalContentEntitlementInfo = 0xFA00003u,
            GetEntitlementKey = 0xFA00004u,

            AbortRequest = 0xFA00005u,
            DeleteRequest = 0xFA00006u,
            GenerateTransactionId = 0xFA00007u,
            PollUnifiedEntitlementInfo = 0xFA00008u,
            PollUnifiedEntitlementInfoList = 0xFA00009u,
            PollServiceEntitlementInfo = 0xFA00010u,
            PollServiceEntitlementInfoList = 0xFA00011u,
            PollConsumeEntitlement = 0xFA00012u,
            RequestUnifiedEntitlementInfo = 0xFA00013u,
            RequestUnifiedEntitlementInfoList = 0xFA00014u,
            RequestServiceEntitlementInfo = 0xFA00015u,
            RequestServiceEntitlementInfoList = 0xFA00016u,
            RequestConsumeUnifiedEntitlement = 0xFA00017u,
            RequestConsumeServiceEntitlement = 0xFA00018u,
        };

        static void RegisterMethods();

        static void Initialize();

        static void GetAdditionalContentEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetSkuFlagImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetAdditionalContentEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetEntitlementKeyImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void AbortRequestImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeleteRequestImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GenerateTransactionIdImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollUnifiedEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollUnifiedEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollServiceEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollServiceEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollConsumeEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestUnifiedEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestUnifiedEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestServiceEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestServiceEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestConsumeUnifiedEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RequestConsumeServiceEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void Write(BinaryWriter& writer, SceNpEntitlementAccessUnifiedEntitlementInfo& unifiedInfo);
        static void Write(BinaryWriter& writer, SceNpEntitlementAccessServiceEntitlementInfo& serviceInfo);
    };
#endif
}
