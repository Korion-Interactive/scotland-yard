#pragma once
#include "SharedCoreIncludes.h"
#include <map>
#if !__ORBIS__
namespace psn
{
    class TrophyService
    {
    public:
        enum Methods
        {
            StartService = 0x0300001u,
            StopService = 0x0300002u,
            GetGameInfo = 0x0300003u,
            GetGroupInfo = 0x0300004u,
            GetTrophyInfo = 0x0300005u,
            GetGameIcon = 0x0300006u,
            GeGroupIcon = 0x0300007u,
            GetTrophyIcon = 0x0300008u,
            GetRewardIcon = 0x0300009u,
            ShowTrophyList = 0x030000Au,
            FetchUnlockEvent = 0x030000Bu,
        };

        class UserTrophies
        {
        public:

            UserTrophies(SceUserServiceUserId userId);

            int Create();
            int Destroy();

            SceUserServiceUserId m_userId;

            SceNpTrophy2Context m_context;
            SceNpTrophy2Handle m_handle;
        };

        static void RegisterMethods();

        static void StartServiceImpl(APIResult* result);
        static void StopServiceImpl(APIResult* result);

        static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);

        static void GetGameInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetGroupInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetTrophyInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetGameIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GeGroupIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetTrophyIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetRewardIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void ShowTrophyListImpl(UInt8* sourceData, int sourceSize, APIResult* result);

        static void Update();

        static UserMap<UserTrophies> s_UsersList;
        static std::list<Int32> s_PendingUnlockEventsList;

        static void UnlockCallback(SceNpTrophy2Context context, SceNpTrophy2Id trophyId, void *userdata);
        static void FetchUnlockEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
    };
}


#endif
