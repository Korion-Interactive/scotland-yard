#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <vector>

#include "Sockets.h"

namespace psn
{
    void Sockets::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::SetupUdpP2PSocket, Sockets::SetupUdpP2PSocketImpl);
        MsgHandler::AddMethod(Methods::TerminateSocket, Sockets::TerminateSocketImpl);
        MsgHandler::AddMethod(Methods::SendTo, Sockets::SendToImpl);
        MsgHandler::AddMethod(Methods::RecvThreadUpdate, Sockets::RecvThreadUpdateImpl);
    }

    void Sockets::SetupUdpP2PSocketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        char* socketName = reader.ReadStringPtr();
        UInt16 virtualPort = reader.ReadUInt16();

        SceNetInAddr addr;
        addr.s_addr = reader.ReadUInt32();

        ret = sceNetSocket(socketName, SCE_NET_AF_INET, SCE_NET_SOCK_DGRAM_P2P, 0);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        const SceNetId socket = ret;

        SceNetSockaddrIn sinaddr;
        memset(&sinaddr, 0, sizeof(sinaddr));

        sinaddr.sin_family = SCE_NET_AF_INET;
        sinaddr.sin_len = sizeof(sinaddr);
        sinaddr.sin_port = sceNetHtons(SCE_NP_PORT);
        sinaddr.sin_vport = sceNetHtons(virtualPort);

        if (addr.s_addr != 0)
        {
            sinaddr.sin_addr.s_addr = addr.s_addr;
        }

        ret = sceNetBind(socket, (struct SceNetSockaddr*)&sinaddr, sizeof(sinaddr));
        if (ret < 0)
        {
            sceNetSocketClose(socket);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(socket);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void Sockets::TerminateSocketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNetId netId = reader.ReadInt32();

        sceNetSocketAbort(netId, 0);

        ret = sceNetSocketClose(netId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void Sockets::SendToImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNetId netId = reader.ReadInt32();

        int dataSize = reader.ReadInt32();
        void* data = reader.ReadDataPtr(dataSize);

        UInt16 recvVirtualPort = reader.ReadUInt16();

        UInt32 sendToAddr = reader.ReadUInt32();
        UInt16 sendToPort = reader.ReadUInt16();

        bool encrypt = reader.ReadBool();

        SceNetSockaddrIn sin;
        memset(&sin, 0, sizeof(sin));
        sin.sin_len = sizeof(sin);
        sin.sin_family = SCE_NET_AF_INET;
        sin.sin_vport = sceNetHtons(recvVirtualPort);

        sin.sin_addr.s_addr = sendToAddr;
        sin.sin_port = sceNetHtons(sendToPort);

        int flags = 0;
        if (encrypt == true)
        {
            flags |= SCE_NET_MSG_USECRYPTO;
        }

        ret = sceNetSendto(netId, data, dataSize, flags, (SceNetSockaddr*)&sin, sizeof(sin));
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void Sockets::RecvThreadUpdateImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        SceNetId netId = reader.ReadUInt32();
        UInt32 maxReceiveSize = reader.ReadUInt32();

        std::vector<char> payload(maxReceiveSize);

        SceNetSockaddrIn sin;
        memset(&sin, 0, sizeof(sin));
        SceSize len = sizeof(sin);

        ret = sceNetRecvfrom(netId, &payload[0], maxReceiveSize, SCE_NET_MSG_DONTWAIT, (SceNetSockaddr*)&sin, (SceNetSocklen_t*)&len);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int dataLen = ret;

        if (dataLen > maxReceiveSize)
        {
            ERROR_RESULT(result, "RecvThreadUpdate has received more data than expected");
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        if (dataLen > 0)
        {
            writer.WriteBool(true);
            writer.WriteData(&payload[0], dataLen);

            writer.WriteUInt32(sin.sin_addr.s_addr);
            writer.WriteUInt16(sceNetNtohs(sin.sin_port));
            writer.WriteUInt16(sceNetNtohs(sin.sin_vport));
        }
        else
        {
            writer.WriteBool(false);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
