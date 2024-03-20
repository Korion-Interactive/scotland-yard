#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

#include "SessionMap.h"

namespace sceCppWebApi = sce::Np::CppWebApi::Common;
namespace sceSessionManager = sce::Np::CppWebApi::SessionManager::V1;

using namespace sceCppWebApi;
using namespace sceSessionManager;

namespace psn
{
    class PlayerSessionCommands
    {
    public:

        enum Methods
        {
            CreatePlayerSession = 0x0A00001u,
            LeavePlayerSession = 0x0A00002u,
            JoinPlayerSession = 0x0A00003u,
            GetPlayerSessions = 0x0A00004u,
            SendPlayerSessionsInvitation = 0x0A00005u,
            GetPlayerSessionInvitations = 0x0A00006u,
            SetPlayerSessionProperties = 0x0A00007u,
            ChangePlayerSessionLeader = 0x0A00008u,
            AddPlayerSessionJoinableSpecifiedUsers = 0x0A00009u,
            DeletePlayerSessionJoinableSpecifiedUsers = 0x0A0000Au,
            SetPlayerSessionMemberSystemProperties = 0x0A0000Bu,
            SendPlayerSessionMessage = 0x0A0000Cu,
            GetJoinedPlayerSessionsByUser = 0x0A0000Du,
        };

        //#define PLATFORM_PS5_FLAG 1
        //#define PLATFORM_PS4_FLAG 2

        struct LocalisedStrings
        {
            IntrusivePtr<LocalizedString> m_LocalizedStringPtr;

            int Deserialise(BinaryReader& reader);
        };

        class InitializationParams
        {
        public:
            InitializationParams();
            ~InitializationParams();

            int m_UserId;
            uint32_t m_MaxPlayers;
            uint32_t m_MaxSpectators;
            bool m_SwapSupported;
            bool m_JoinDisabled;

            sceSessionManager::JoinableUserType m_JoinableUserType;
            sceSessionManager::InvitableUserType m_InvitableUserType;

            uint32_t m_PlatformFlags;

            LocalisedStrings m_SessionName;

            Vector<String> *m_LeaderPrivileges;
            Vector<String> *m_ExclusiveLeaderPrivileges;
            Vector<String> *m_DisableSystemUiMenu;

            int m_CustomDataSize1;
            void* m_CustomData1;

            int m_CustomDataSize2;
            void* m_CustomData2;

            void Deserialise(BinaryReader& reader);
        };


        static void RegisterMethods();

        static void CreatePlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void LeavePlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void JoinPlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetPlayerSessionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SendPlayerSessionsInvitationImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void GetPlayerSessionInvitationsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SetPlayerSessionPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void ChangePlayerSessionLeaderImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void AddPlayerSessionJoinableSpecifiedUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeletePlayerSessionJoinableSpecifiedUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SetPlayerSessionMemberSystemPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SendPlayerSessionMessageImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetJoinedPlayerSessionsByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void SerialiseSessionInfo(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<PlayerSessionForRead> > > playerSessionsPtr);

        //static const char* GetThisPlatformString();

        //static int AddPlatformStrings(uint32_t platformFlags, Vector< String >& supportedPlatforms);

        static int Create(Int32 pushCallbackId, void* creatorCustomData1, int creatorDataSize1, InitializationParams& params, BinaryWriter& writer);
        static int Leave(SceUserServiceUserId userId, const char* sessionId);
        static int JoinAsPlayer(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer);
        static int JoinAsSpectator(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer);

        static int SetPlayerSessionProps(WebApiUserContext* userCtx, const char* sessionId, BinaryReader& reader);

        static int DeserialiseLeaderPrivileges(BinaryReader& reader, Vector<String> *strings);

        //static uint32_t GetPlatformFlag(const char* platforms);
    };
}
