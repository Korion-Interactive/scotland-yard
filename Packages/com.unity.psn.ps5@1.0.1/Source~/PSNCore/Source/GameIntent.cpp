#include "GameIntent.h"
#include "HandleMsg.h"

#pragma comment(lib,"libSceNpGameIntent_stub_weak.a")

namespace psn
{
    #define GI_VALUE_MAX_SIZE  (50)   // Includes the null terminator

    #define GI_ACTIVITY_ID_MAX_SIZE (33)
    #define GI_PLAYER_SESSION_ID_MAX_SIZE (37)
    #define GI_MEMBER_TYPE_MAX_SIZE (17)

    std::list<SceNpGameIntentInfo> GameIntent::s_PendingGameIntentList;

    void GameIntent::InitializeLib()
    {
        SceNpGameIntentInitParam initParam;
        sceNpGameIntentInitParamInit(&initParam);
        int ret = sceNpGameIntentInitialize(&initParam);

        if (ret < 0)
        {
            UNITY_TRACE("Error initialising GameIntent, 0x%x\n", ret);
        }
    }

    void GameIntent::TerminateLib()
    {
        sceNpGameIntentTerminate();
    }

    void GameIntent::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::FetchGameIntent, GameIntent::FetchGameIntentImpl);

        MsgHandler::RegisterSystemEventCallback(HandleSystemEvent);
    }

    void GameIntent::FetchGameIntentImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingGameIntentList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first game intent off the list and return the results
        SceNpGameIntentInfo intentInfo = s_PendingGameIntentList.front();
        s_PendingGameIntentList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(intentInfo.intentType);

        writer.WriteUInt32(intentInfo.userId);

        if (strncmp(intentInfo.intentType, "joinSession", sizeof(intentInfo.intentType)) == 0)
        {
            WriteIntentProperty(intentInfo.intentData, "playerSessionId", GI_PLAYER_SESSION_ID_MAX_SIZE, writer);
            WriteIntentProperty(intentInfo.intentData, "memberType", GI_MEMBER_TYPE_MAX_SIZE, writer);
        }
        else if (strncmp(intentInfo.intentType, "launchActivity", sizeof(intentInfo.intentType)) == 0)
        {
            WriteIntentProperty(intentInfo.intentData, "activityId", GI_ACTIVITY_ID_MAX_SIZE, writer);
        }
        else if (strncmp(intentInfo.intentType, "launchMultiplayerActivity", sizeof(intentInfo.intentType)) == 0)
        {
            WriteIntentProperty(intentInfo.intentData, "activityId", GI_ACTIVITY_ID_MAX_SIZE, writer);
            WriteIntentProperty(intentInfo.intentData, "playerSessionId", GI_PLAYER_SESSION_ID_MAX_SIZE, writer);
        }

        // https://p.siedev.net/resources/documents/SDK/0.850/Game_Intent_System-Overview/0004.html

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void GameIntent::WriteIntentProperty(SceNpGameIntentData& intentData, const char* key, int maxValueLength, BinaryWriter& writer)
    {
        char value[GI_VALUE_MAX_SIZE];
        sceNpGameIntentGetPropertyValueString(&intentData, key, value, sizeof(value));
        writer.WriteString(value);
    }

    void GameIntent::HandleSystemEvent(SceSystemServiceEvent& sevent)
    {
        if (sevent.eventType == SCE_SYSTEM_SERVICE_EVENT_GAME_INTENT)
        {
            SceNpGameIntentInfo intentInfo;
            sceNpGameIntentInfoInit(&intentInfo);
            int ret = sceNpGameIntentReceiveIntent(&intentInfo);
            if (ret == SCE_OK)
            {
                s_PendingGameIntentList.push_back(intentInfo);
            }
        }
    }
}
