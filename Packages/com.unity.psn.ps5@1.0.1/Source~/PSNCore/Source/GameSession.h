#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

namespace sceCppWebApi = sce::Np::CppWebApi::Common;
namespace sceSessionManager = sce::Np::CppWebApi::SessionManager::V1;

using namespace sceCppWebApi;
using namespace sceSessionManager;

namespace psn
{
    const int kNumSearchAttributes = 10;

    class GameSessionCommands
    {
    public:

        enum Methods
        {
            CreateGameSession = 0x0B00001u,
            LeaveGameSession = 0x0B00002u,
            JoinGameSession = 0x0B00003u,
            GetGameSessions = 0x0B00004u,
            SetGameSessionProperties = 0x0B00005u,
            SetGameSessionMemberSystemProperties = 0x0B00006u,
            SendGameSessionMessage = 0x0B00007u,
            GetJoinedGameSessionsByUser = 0x0B00008u,
            DeleteGameSession = 0x0B00009u,
            GameSessionsSearch = 0x0B0000Au,
        };

        struct GSMember
        {
            int m_UserId;
            int m_PushCallbackId;
            SceNpAccountId m_AccountId;
            SceNpPlatformType m_Platform;
            InitialJoinState m_JoinState;
            int m_NatType;  // Valid values are 1 to 3, 0 indicates not set

            int m_CustomDataSize1;
            void* m_CustomData1;

            void Deserialise(BinaryReader& reader);
        };

        class InitializationParams
        {
        public:
            InitializationParams();
            ~InitializationParams();

            uint32_t m_MaxPlayers;
            uint32_t m_MaxSpectators;
            uint32_t m_PlatformFlags;
            bool m_JoinDisabled;
            bool m_UsePlayerSession;
            int32_t m_ReservationTimeoutSeconds;

            uint32_t m_NumberMembers;
            GSMember* m_Members;

            int m_CustomDataSize1;
            void* m_CustomData1;

            int m_CustomDataSize2;
            void* m_CustomData2;

            bool m_Searchable;
            const char *m_SearchIndex;

            unsigned int m_StringsSetBits, m_IntsSetBits, m_BoolsSetBits;
            int m_Ints[kNumSearchAttributes];
            bool m_Bools[kNumSearchAttributes];
            char *m_Strings[kNumSearchAttributes];

            void Deserialise(BinaryReader& reader);
        };

        static void RegisterMethods();

        static void CreateGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void LeaveGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void JoinGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetGameSessionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SetGameSessionPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SetGameSessionMemberSystemPropertiesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SendGameSessionMessageImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetJoinedGameSessionsByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeleteGameSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GameSessionsSearchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void SerialiseSessionInfo(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<GameSessionForRead> > > gameSessionsPtr);
        static int SetGameSessionProps(WebApiUserContext* userCtx, const char* sessionId, BinaryReader& reader);

        static int AddUser(GSMember& member, Vector<IntrusivePtr<RequestGameSessionPlayer> >& requestGameSessionPlayers);

        static int Create(GSMember& creator, InitializationParams& params, BinaryWriter& writer);
        static int Leave(SceUserServiceUserId userId, const char* sessionId);
        static int JoinAsPlayer(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer);
        static int JoinAsSpectator(SceUserServiceUserId userId, Int32 pushCallbackId, const char* sessionId, bool swapping, BinaryWriter& writer);
    };
}
