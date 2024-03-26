#pragma once
#include "SharedCoreIncludes.h"
#include <map>

namespace psn
{
    class Bandwidth
    {
    public:

        enum Methods
        {
            StartMeasurement = 0x1700001u,
            PollMeasurement = 0x1700002u,
            AbortMeasurement = 0x1700003u,
        };

        static void RegisterMethods();
        static void StartMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PollMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void AbortMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static int GetStatus(int ctxId);
    };
}
