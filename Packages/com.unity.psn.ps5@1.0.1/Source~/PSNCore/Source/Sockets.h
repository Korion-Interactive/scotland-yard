#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>

namespace psn
{
    class Sockets
    {
    public:

        enum Methods
        {
            SetupUdpP2PSocket = 0x1300001u,
            TerminateSocket = 0x1300002u,
            SendTo = 0x1300003u,
            RecvThreadUpdate = 0x1300004u,
        };

        static void RegisterMethods();

        static void SetupUdpP2PSocketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void TerminateSocketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SendToImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RecvThreadUpdateImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
    };
}
