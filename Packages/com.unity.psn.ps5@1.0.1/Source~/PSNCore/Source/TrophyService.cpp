#include "TrophyService.h"
#include "HandleMsg.h"
#if !__ORBIS__

#pragma comment(lib,"libSceNpTrophy2_stub_weak.a")

namespace psn
{
    UserMap<TrophyService::UserTrophies> TrophyService::s_UsersList;
    std::list<Int32> TrophyService::s_PendingUnlockEventsList;

    TrophyService::UserTrophies::UserTrophies(SceUserServiceUserId userId)
    {
        m_userId = userId;
        m_context = SCE_NP_TROPHY2_INVALID_CONTEXT;
        m_handle = SCE_NP_TROPHY2_INVALID_HANDLE;
    }

    int TrophyService::UserTrophies::Create()
    {
        int ret;

        ret = sceNpTrophy2CreateContext(&m_context, m_userId, 0, 0);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpTrophy2CreateHandle(&m_handle);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpTrophy2RegisterContext(m_context, m_handle, 0);

        if (ret < 0)
        {
            return ret;
        }

        return ret;
    }

    int TrophyService::UserTrophies::Destroy()
    {
        int ret;

        ret = sceNpTrophy2DestroyHandle(m_handle);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpTrophy2DestroyContext(m_context);

        if (ret < 0)
        {
            return ret;
        }

        return ret;
    }

    void TrophyService::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::StartService, TrophyService::StartServiceImpl);
        MsgHandler::AddMethod(Methods::StopService, TrophyService::StopServiceImpl);
        MsgHandler::AddMethod(Methods::GetGameInfo, TrophyService::GetGameInfoImpl);
        MsgHandler::AddMethod(Methods::GetGroupInfo, TrophyService::GetGroupInfoImpl);
        MsgHandler::AddMethod(Methods::GetTrophyInfo, TrophyService::GetTrophyInfoImpl);
        MsgHandler::AddMethod(Methods::GetGameIcon, TrophyService::GetGameIconImpl);
        MsgHandler::AddMethod(Methods::GeGroupIcon, TrophyService::GeGroupIconImpl);
        MsgHandler::AddMethod(Methods::GetTrophyIcon, TrophyService::GetTrophyIconImpl);
        MsgHandler::AddMethod(Methods::GetRewardIcon, TrophyService::GetRewardIconImpl);
        MsgHandler::AddMethod(Methods::ShowTrophyList, TrophyService::ShowTrophyListImpl);
        MsgHandler::AddMethod(Methods::FetchUnlockEvent, TrophyService::FetchUnlockEventImpl);

        MsgHandler::RegisterUserCallback(HandleUserState);
    }

    void TrophyService::StartServiceImpl(APIResult* result)
    {
        int ret = sceNpTrophy2RegisterUnlockCallback(UnlockCallback, NULL);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void CleanUpUsers(TrophyService::UserTrophies* userData)
    {
        sceNpTrophy2DestroyContext(userData->m_context);
    }

    void TrophyService::StopServiceImpl(APIResult* result)
    {
        // Unregister any users
        //s_UsersList.Clean(CleanUpUsers);

        int ret = sceNpTrophy2UnregisterUnlockCallback();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void TrophyService::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    {
        if (state == MsgHandler::UserState::Added)
        {
            if (s_UsersList.DoesUserExist(userId) == true)
            {
                // User already registered so don't do this again
                WARNING_RESULT(result, "User already initialised with Trophy service");
                return;
            }

            UserTrophies* user = s_UsersList.CreateUser(userId);

            user->Create();
        }
        else if (state == MsgHandler::UserState::Removed)
        {
            UserTrophies* user = s_UsersList.FindUser(userId);

            if (user == NULL)
            {
                WARNING_RESULT(result, "User not registered with trophy service");
                return;
            }

            if (user->m_context == SCE_NP_TROPHY2_INVALID_CONTEXT)
            {
                ERROR_RESULT(result, "User context is invalid");
                return;
            }

            user->Destroy();

            s_UsersList.DeleteUser(userId);
        }

        SUCCESS_RESULT(result);
    }

    void TrophyService::GetGameInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        InitialiseData* data = (InitialiseData *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(data->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        SceNpTrophy2GameDetails gameDetails;
        SceNpTrophy2GameData gameData;

        memset(&gameDetails, 0, sizeof(gameDetails));
        memset(&gameData, 0, sizeof(gameData));

        int ret;

        ret = sceNpTrophy2GetGameInfo(user->m_context, user->m_handle, &gameDetails, &gameData);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2GameDetails
        writer.WriteUInt32(gameDetails.numGroups);
        writer.WriteUInt32(gameDetails.numTrophies);
        writer.WriteUInt32(gameDetails.numPlatinum);
        writer.WriteUInt32(gameDetails.numGold);
        writer.WriteUInt32(gameDetails.numSilver);
        writer.WriteUInt32(gameDetails.numBronze);

        writer.WriteString(gameDetails.title);

        // Write SceNpTrophy2GameData
        writer.WriteUInt32(gameData.unlockedTrophies);
        writer.WriteUInt32(gameData.unlockedPlatinum);
        writer.WriteUInt32(gameData.unlockedGold);
        writer.WriteUInt32(gameData.unlockedSilver);
        writer.WriteUInt32(gameData.unlockedBronze);
        writer.WriteUInt32(gameData.progressPercentage);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    struct GroupInfoParams
    {
        SceUserServiceUserId userId;
        SceNpTrophy2GroupId groupId;
    };

    void TrophyService::GetGroupInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        GroupInfoParams* params = (GroupInfoParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        SceNpTrophy2GroupDetails groupDetails;
        SceNpTrophy2GroupData groupData;

        memset(&groupDetails, 0, sizeof(groupDetails));
        memset(&groupData, 0, sizeof(groupData));

        int ret = sceNpTrophy2GetGroupInfo(user->m_context, user->m_handle, params->groupId, &groupDetails, &groupData);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2GroupDetails
        writer.WriteInt32(groupDetails.groupId);
        writer.WriteUInt32(groupDetails.numTrophies);
        writer.WriteUInt32(groupDetails.numPlatinum);
        writer.WriteUInt32(groupDetails.numGold);
        writer.WriteUInt32(groupDetails.numSilver);
        writer.WriteUInt32(groupDetails.numBronze);

        writer.WriteString(groupDetails.title);

        // Write SceNpTrophy2GroupData
        writer.WriteInt32(groupData.groupId);
        writer.WriteUInt32(groupData.unlockedTrophies);
        writer.WriteUInt32(groupData.unlockedPlatinum);
        writer.WriteUInt32(groupData.unlockedGold);
        writer.WriteUInt32(groupData.unlockedSilver);
        writer.WriteUInt32(groupData.unlockedBronze);
        writer.WriteUInt32(groupData.progressPercentage);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    struct TrophyInfoParams
    {
        SceUserServiceUserId userId;
        SceNpTrophy2Id trophyId;
    };

    void TrophyService::GetTrophyInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        TrophyInfoParams* params = (TrophyInfoParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        SceNpTrophy2Details trophyDetails;
        SceNpTrophy2Data trophyData;

        memset(&trophyDetails, 0, sizeof(trophyDetails));
        memset(&trophyData, 0, sizeof(trophyData));

        int ret = sceNpTrophy2GetTrophyInfo(user->m_context, user->m_handle, params->trophyId, &trophyDetails, &trophyData);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2Details
        writer.WriteInt32(trophyDetails.trophyId);
        writer.WriteInt32(trophyDetails.trophyGrade);
        writer.WriteInt32(trophyDetails.groupId);

        writer.WriteBool(trophyDetails.hidden);
        writer.WriteBool(trophyDetails.hasReward);

        bool isProgress = trophyDetails.target.type == SCE_NP_TROPHY2_PROGRESS_TYPE_UINT64;

        writer.WriteBool(isProgress);

        if (isProgress == true)
        {
            writer.WriteInt64(trophyDetails.target.value.valueUInt64);
        }

        writer.WriteString(trophyDetails.name);
        writer.WriteString(trophyDetails.description);
        writer.WriteString(trophyDetails.reward);

        // Write SceNpTrophy2Data
        writer.WriteInt32(trophyData.trophyId);
        writer.WriteBool(trophyData.unlocked);

        isProgress = trophyData.progress.type == SCE_NP_TROPHY2_PROGRESS_TYPE_UINT64;

        writer.WriteBool(isProgress);

        if (isProgress == true)
        {
            writer.WriteInt64(trophyData.progress.value.valueUInt64);
        }

        //trophyData.progress
        writer.WriteRtcTick(trophyData.timestamp);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    struct GameIconParams
    {
        SceUserServiceUserId userId;
    };

    void TrophyService::GetGameIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        GameIconParams* params = (GameIconParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        void *buf = NULL;
        size_t bufferSize = 0;

        int ret = sceNpTrophy2GetGameIcon(user->m_context, user->m_handle, NULL, &bufferSize);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        buf = malloc(bufferSize);

        ret = sceNpTrophy2GetGameIcon(user->m_context, user->m_handle, buf, &bufferSize);

        if (ret < 0)
        {
            free(buf);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2Details
        writer.WriteData((char*)buf, bufferSize);

        *resultsSize = writer.GetWrittenLength();

        free(buf);

        SUCCESS_RESULT(result);
    }

    struct TrophyIconIdParams
    {
        SceUserServiceUserId userId;
        int32_t id; // group or trophy id
    };

    void TrophyService::GeGroupIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        TrophyIconIdParams* params = (TrophyIconIdParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        void *buf = NULL;
        size_t bufferSize = 0;

        SceNpTrophy2GroupId groupId = params->id;

        int ret = sceNpTrophy2GetGroupIcon(user->m_context, user->m_handle, groupId, NULL, &bufferSize);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        buf = malloc(bufferSize);

        ret = sceNpTrophy2GetGroupIcon(user->m_context, user->m_handle, groupId, buf, &bufferSize);

        if (ret < 0)
        {
            free(buf);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2Details
        writer.WriteData((char*)buf, bufferSize);

        *resultsSize = writer.GetWrittenLength();

        free(buf);

        SUCCESS_RESULT(result);
    }

    void TrophyService::GetTrophyIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        TrophyIconIdParams* params = (TrophyIconIdParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        void *buf = NULL;
        size_t bufferSize = 0;

        SceNpTrophy2GroupId trophyId = params->id;

        int ret = sceNpTrophy2GetTrophyIcon(user->m_context, user->m_handle, trophyId, NULL, &bufferSize);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        buf = malloc(bufferSize);

        ret = sceNpTrophy2GetTrophyIcon(user->m_context, user->m_handle, trophyId, buf, &bufferSize);

        if (ret < 0)
        {
            free(buf);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2Details
        writer.WriteData((char*)buf, bufferSize);

        *resultsSize = writer.GetWrittenLength();

        free(buf);

        SUCCESS_RESULT(result);
    }

    void TrophyService::GetRewardIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        TrophyIconIdParams* params = (TrophyIconIdParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        void *buf = NULL;
        size_t bufferSize = 0;

        SceNpTrophy2GroupId trophyId = params->id;

        int ret = sceNpTrophy2GetRewardIcon(user->m_context, user->m_handle, trophyId, NULL, &bufferSize);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        buf = malloc(bufferSize);

        ret = sceNpTrophy2GetRewardIcon(user->m_context, user->m_handle, trophyId, buf, &bufferSize);

        if (ret < 0)
        {
            free(buf);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        // Write SceNpTrophy2Details
        writer.WriteData((char*)buf, bufferSize);

        *resultsSize = writer.GetWrittenLength();

        free(buf);

        SUCCESS_RESULT(result);
    }

    void TrophyService::ShowTrophyListImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        GameIconParams* params = (GameIconParams *)(sourceData);

        UserTrophies* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with trophy service");
            return;
        }

        int ret = sceNpTrophy2ShowTrophyList(user->m_context);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void TrophyService::Update()
    {
    }

    void TrophyService::UnlockCallback(SceNpTrophy2Context context, SceNpTrophy2Id trophyId, void *userdata)
    {
        //UNITY_TRACE("UnlockCallback\n");
        s_PendingUnlockEventsList.push_back(trophyId);
    }

    void TrophyService::FetchUnlockEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingUnlockEventsList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first event off the list and return the results
        Int32 trophyId = s_PendingUnlockEventsList.front();
        s_PendingUnlockEventsList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(trophyId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
#endif
