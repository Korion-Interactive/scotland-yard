#include "FeatureGating.h"
#include "HandleMsg.h"
#if !__ORBIS__

#pragma comment(lib,"libSceNpManager_stub_weak.a")


namespace psn
{
    std::list<FeatureGating::PremiumEvent> FeatureGating::s_PendingPremiumEventsList;

    void FeatureGating::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::CheckPremium, FeatureGating::CheckPremiumImpl);
        MsgHandler::AddMethod(Methods::NotifyPremiumFeature, FeatureGating::NotifyPremiumFeatureImpl);

        MsgHandler::AddMethod(Methods::StartPremiumEventCallback, FeatureGating::StartPremiumEventCallbackImpl);
        MsgHandler::AddMethod(Methods::StopPremiumEventCallback, FeatureGating::StopPremiumEventCallbackImpl);
        MsgHandler::AddMethod(Methods::FetchPremiumEvent, FeatureGating::FetchPremiumEventImpl);
    }

    void FeatureGating::CheckPremiumImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceNpCheckPremiumResult checkResult;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        UInt64 features = reader.ReadUInt64();  // SCE_NP_PREMIUM_FEATURE

        int ret = sceNpCreateRequest();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int requestId = ret;

        SceNpCheckPremiumParameter checkParam;
        memset(&checkParam, 0, sizeof(checkParam));
        checkParam.size = sizeof(checkParam);
        checkParam.features = features; // SCE_NP_PREMIUM_FEATURE_REALTIME_MULTIPLAY;
        checkParam.userId = userId;
        ret = sceNpCheckPremium(requestId, &checkParam, &checkResult);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(checkResult.authorized);

        *resultsSize = writer.GetWrittenLength();

        sceNpDeleteRequest(requestId);

        SUCCESS_RESULT(result);
    }

    void FeatureGating::NotifyPremiumFeatureImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        UInt64 features = reader.ReadUInt64(); // SCE_NP_PREMIUM_FEATURE
        UInt64 properties = reader.ReadUInt64(); // SCE_NP_REALTIME_MULTIPLAY_PROPERTY

        SceNpNotifyPremiumFeatureParameter param;
        memset(&param, 0x0, sizeof(param));

        param.size = sizeof(param);
        param.userId = userId;
        param.features = features;
        param.properties = properties;

        int ret = sceNpNotifyPremiumFeature(&param);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void FeatureGating::PremiumEventCallback(SceUserServiceUserId userId, SceNpPremiumEventType eventType, void *userData)
    {
        PremiumEvent event;

        event.userId = userId;
        event.eventType = eventType;

        s_PendingPremiumEventsList.push_back(event);
    }

    void FeatureGating::StartPremiumEventCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        int ret = sceNpRegisterPremiumEventCallback(PremiumEventCallback, NULL);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void FeatureGating::StopPremiumEventCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        int ret = sceNpUnregisterPremiumEventCallback();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void FeatureGating::FetchPremiumEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingPremiumEventsList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first event off the list and return the results
        PremiumEvent event = s_PendingPremiumEventsList.front();
        s_PendingPremiumEventsList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(event.userId);
        writer.WriteInt32(event.eventType);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
#endif
