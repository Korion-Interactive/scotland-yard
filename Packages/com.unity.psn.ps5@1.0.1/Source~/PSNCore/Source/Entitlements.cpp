#include "Entitlements.h"
#include "HandleMsg.h"

#include <np/np_entitlement_access.h>

#pragma comment(lib,"libSceNpEntitlementAccess_stub_weak.a")

#if !__ORBIS__
namespace psn
{
    void EntitlementsSystem::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::GetAdditionalContentEntitlementInfoList, EntitlementsSystem::GetAdditionalContentEntitlementInfoListImpl);
        MsgHandler::AddMethod(Methods::GetSkuFlag, EntitlementsSystem::GetSkuFlagImpl);
        MsgHandler::AddMethod(Methods::GetAdditionalContentEntitlementInfo, EntitlementsSystem::GetAdditionalContentEntitlementInfoImpl);
        MsgHandler::AddMethod(Methods::GetEntitlementKey, EntitlementsSystem::GetEntitlementKeyImpl);

        MsgHandler::AddMethod(Methods::AbortRequest, EntitlementsSystem::AbortRequestImpl);
        MsgHandler::AddMethod(Methods::DeleteRequest, EntitlementsSystem::DeleteRequestImpl);
        MsgHandler::AddMethod(Methods::GenerateTransactionId, EntitlementsSystem::GenerateTransactionIdImpl);
        MsgHandler::AddMethod(Methods::PollUnifiedEntitlementInfo, EntitlementsSystem::PollUnifiedEntitlementInfoImpl);
        MsgHandler::AddMethod(Methods::PollUnifiedEntitlementInfoList, EntitlementsSystem::PollUnifiedEntitlementInfoListImpl);
        MsgHandler::AddMethod(Methods::PollServiceEntitlementInfo, EntitlementsSystem::PollServiceEntitlementInfoImpl);
        MsgHandler::AddMethod(Methods::PollServiceEntitlementInfoList, EntitlementsSystem::PollServiceEntitlementInfoListImpl);
        MsgHandler::AddMethod(Methods::PollConsumeEntitlement, EntitlementsSystem::PollConsumeEntitlementImpl);
        MsgHandler::AddMethod(Methods::RequestUnifiedEntitlementInfo, EntitlementsSystem::RequestUnifiedEntitlementInfoImpl);
        MsgHandler::AddMethod(Methods::RequestUnifiedEntitlementInfoList, EntitlementsSystem::RequestUnifiedEntitlementInfoListImpl);
        MsgHandler::AddMethod(Methods::RequestServiceEntitlementInfo, EntitlementsSystem::RequestServiceEntitlementInfoImpl);
        MsgHandler::AddMethod(Methods::RequestServiceEntitlementInfoList, EntitlementsSystem::RequestServiceEntitlementInfoListImpl);
        MsgHandler::AddMethod(Methods::RequestConsumeUnifiedEntitlement, EntitlementsSystem::RequestConsumeUnifiedEntitlementImpl);
        MsgHandler::AddMethod(Methods::RequestConsumeServiceEntitlement, EntitlementsSystem::RequestConsumeServiceEntitlementImpl);
    }

    void EntitlementsSystem::Initialize()
    {
        SceNpEntitlementAccessInitParam initParam;
        SceNpEntitlementAccessBootParam bootParam;

        /* Clear with 0's */
        memset(&initParam, 0, sizeof(SceNpEntitlementAccessInitParam));
        memset(&bootParam, 0, sizeof(SceNpEntitlementAccessBootParam));

        /* Initialize the NpEntitlementAccess library */
        sceNpEntitlementAccessInitialize(&initParam, &bootParam);
    }

    void EntitlementsSystem::GetSkuFlagImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        SceNpEntitlementAccessSkuFlag skuflag;

        int ret = sceNpEntitlementAccessGetSkuFlag(&skuflag);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(skuflag);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Additional Content
    void EntitlementsSystem::GetAdditionalContentEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        SceNpEntitlementAccessAddcontEntitlementInfo *list = NULL; // If NULL is specified, only the number can be obtained
        uint32_t listNum = 0;
        uint32_t hitNum;
        int ret = sceNpEntitlementAccessGetAddcontEntitlementInfoList(serviceLabel, list, listNum, &hitNum);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        /* Prepare the required buffers */
        if (hitNum > 0)
        {
            list = (SceNpEntitlementAccessAddcontEntitlementInfo *)malloc(sizeof(SceNpEntitlementAccessAddcontEntitlementInfo) * hitNum);
            listNum = hitNum;

            /* Get a list of additional content information for which the entitlement is valid */
            ret = sceNpEntitlementAccessGetAddcontEntitlementInfoList(serviceLabel, list, listNum, &hitNum);
            if (ret < 0)
            {
                free(list);
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(hitNum);

        for (int i = 0; i < hitNum; i++)
        {
            writer.WriteString(list[i].entitlementLabel.data);
            writer.WriteUInt32(list[i].packageType);
            writer.WriteUInt32(list[i].downloadStatus);
        }

        *resultsSize = writer.GetWrittenLength();

        if (list != NULL) free(list);

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::GetAdditionalContentEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        char* label = reader.ReadStringPtr();

        SceNpUnifiedEntitlementLabel entitlementLabel;
        SceNpEntitlementAccessAddcontEntitlementInfo info;

        memset(&entitlementLabel, 0, sizeof(SceNpUnifiedEntitlementLabel));
        memset(&info, 0, sizeof(SceNpEntitlementAccessAddcontEntitlementInfo));

        strncpy(entitlementLabel.data, label, SCE_NP_UNIFIED_ENTITLEMENT_LABEL_SIZE);

        int ret = sceNpEntitlementAccessGetAddcontEntitlementInfo(serviceLabel, &entitlementLabel, &info);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(info.entitlementLabel.data);
        writer.WriteUInt32(info.packageType);
        writer.WriteUInt32(info.downloadStatus);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::GetEntitlementKeyImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        char* label = reader.ReadStringPtr();

        SceNpUnifiedEntitlementLabel entitlementLabel;
        SceNpEntitlementAccessEntitlementKey key;

        memset(&entitlementLabel, 0, sizeof(SceNpUnifiedEntitlementLabel));
        memset(&key, 0, sizeof(SceNpEntitlementAccessEntitlementKey));

        strncpy(entitlementLabel.data, label, SCE_NP_UNIFIED_ENTITLEMENT_LABEL_SIZE);

        int ret = sceNpEntitlementAccessGetEntitlementKey(serviceLabel, &entitlementLabel, &key);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(key.data, SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_KEY_SIZE);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Cleanup
    void EntitlementsSystem::AbortRequestImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();

        int ret = sceNpEntitlementAccessAbortRequest(requestId);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::DeleteRequestImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();

        int ret = sceNpEntitlementAccessDeleteRequest(requestId);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    // Transaction
    void EntitlementsSystem::GenerateTransactionIdImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceNpEntitlementAccessTransactionId transactionId;
        memset(&transactionId, 0, sizeof(SceNpEntitlementAccessTransactionId));

        int ret = sceNpEntitlementAccessGenerateTransactionId(&transactionId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(transactionId.transactionId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Unified Entitlement
    void EntitlementsSystem::RequestUnifiedEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        char* label = reader.ReadStringPtr();

        SceNpUnifiedEntitlementLabel entitlementLabel;

        memset(&entitlementLabel, 0, sizeof(SceNpUnifiedEntitlementLabel));

        if (label != NULL)
        {
            strncpy(entitlementLabel.data, label, SCE_NP_UNIFIED_ENTITLEMENT_LABEL_SIZE);
        }

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestUnifiedEntitlementInfo(userId, serviceLabel, &entitlementLabel, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::PollUnifiedEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();

        SceNpEntitlementAccessUnifiedEntitlementInfo info;

        int requestResult = 0;

        int ret = sceNpEntitlementAccessPollUnifiedEntitlementInfo(requestId, &requestResult, &info);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (requestResult < 0)
        {
            SCE_ERROR_RESULT(result, requestResult);
            return;
        }

        // ret will either be SCE_NP_ENTITLEMENT_ACCESS_POLL_ASYNC_RET_RUNNING or SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED
        bool isFinished = ret == SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED;

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(isFinished);

        if (isFinished == true)
        {
            Write(writer, info);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Unified Entitlement List
    void EntitlementsSystem::RequestUnifiedEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        Int32 offset = reader.ReadInt32();
        uint32_t maxNum = reader.ReadUInt32();
        maxNum = maxNum < SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE ? maxNum : SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE;

        SceNpEntitlementAccessPackageType packageType = (SceNpEntitlementAccessPackageType)reader.ReadInt32();

        SceNpEntitlementAccessSortType sort = (SceNpEntitlementAccessSortType)reader.ReadInt32();
        SceNpEntitlementAccessDirectionType direction = (SceNpEntitlementAccessDirectionType)reader.ReadInt32();

        SceNpEntitlementAccessRequestEntitlementInfoListParam param;

        param.size = sizeof(param);
        param.entitlementType = SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_TYPE_UNIFIED;
        param.offset = offset;
        param.limit = maxNum;
        param.sort = sort;
        param.direction = direction;
        param.packageType = packageType;

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestUnifiedEntitlementInfoList(userId, serviceLabel, NULL, 0, &param, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::PollUnifiedEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();
        uint32_t maxNum = reader.ReadUInt32();

        uint32_t hitNum = 0;
        int32_t nextOffset = 0;
        int32_t previousOffset = 0;

        SceNpEntitlementAccessUnifiedEntitlementInfo* list = (SceNpEntitlementAccessUnifiedEntitlementInfo*)malloc(sizeof(SceNpEntitlementAccessUnifiedEntitlementInfo) * SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE);
        uint32_t listNum = maxNum < SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE ? maxNum : SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE;

        int requestResult = 0;

        int ret = sceNpEntitlementAccessPollUnifiedEntitlementInfoList(requestId, &requestResult, list, listNum, &hitNum, &nextOffset, &previousOffset);
        if (ret < 0)
        {
            if (list != NULL) free(list);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (requestResult < 0)
        {
            if (list != NULL) free(list);
            SCE_ERROR_RESULT(result, requestResult);
            return;
        }

        // ret will either be SCE_NP_ENTITLEMENT_ACCESS_POLL_ASYNC_RET_RUNNING or SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED
        bool isFinished = ret == SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED;

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(isFinished);

        if (isFinished == true)
        {
            // number of entitlements returned
            writer.WriteInt32(nextOffset);
            writer.WriteInt32(previousOffset);

            writer.WriteInt32(hitNum);

            for (int i = 0; i < hitNum; i++)
            {
                Write(writer, list[i]);
            }
        }

        *resultsSize = writer.GetWrittenLength();

        if (list != NULL) free(list);

        SUCCESS_RESULT(result);
    }

    // Service Entitlement
    void EntitlementsSystem::RequestServiceEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        char* label = reader.ReadStringPtr();

        SceNpServiceEntitlementLabel entitlementLabel;

        memset(&entitlementLabel, 0, sizeof(SceNpServiceEntitlementLabel));
        strncpy(entitlementLabel.data, label, SCE_NP_SERVICE_ENTITLEMENT_LABEL_SIZE);

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestServiceEntitlementInfo(userId, serviceLabel, &entitlementLabel, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::PollServiceEntitlementInfoImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();

        SceNpEntitlementAccessServiceEntitlementInfo info;

        int requestResult = 0;

        int ret = sceNpEntitlementAccessPollServiceEntitlementInfo(requestId, &requestResult, &info);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (requestResult < 0)
        {
            SCE_ERROR_RESULT(result, requestResult);
            return;
        }

        // ret will either be SCE_NP_ENTITLEMENT_ACCESS_POLL_ASYNC_RET_RUNNING or SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED
        bool isFinished = ret == SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED;

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(isFinished);

        if (isFinished == true)
        {
            Write(writer, info);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Service Entitlement List
    void EntitlementsSystem::RequestServiceEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        Int32 offset = reader.ReadInt32();
        uint32_t maxNum = reader.ReadUInt32();
        maxNum = maxNum < SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE ? maxNum : SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE;

        SceNpEntitlementAccessSortType sort = (SceNpEntitlementAccessSortType)reader.ReadInt32();
        SceNpEntitlementAccessDirectionType direction = (SceNpEntitlementAccessDirectionType)reader.ReadInt32();

        SceNpEntitlementAccessRequestEntitlementInfoListParam param;

        param.size = sizeof(param);
        param.entitlementType = SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_TYPE_SERVICE;
        param.offset = offset;
        param.limit = maxNum;
        param.sort = sort;
        param.direction = direction;
        param.packageType = SCE_NP_ENTITLEMENT_ACCESS_PACKAGE_TYPE_NONE; // Server entitlements don't have a package type

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestServiceEntitlementInfoList(userId, serviceLabel, NULL, 0, &param, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::PollServiceEntitlementInfoListImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();
        uint32_t maxNum = reader.ReadUInt32();

        uint32_t hitNum = 0;
        int32_t nextOffset = 0;
        int32_t previousOffset = 0;

        SceNpEntitlementAccessServiceEntitlementInfo* list = (SceNpEntitlementAccessServiceEntitlementInfo*)malloc(sizeof(SceNpEntitlementAccessServiceEntitlementInfo) * SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE);
        uint32_t listNum = maxNum < SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE ? maxNum : SCE_NP_ENTITLEMENT_ACCESS_ENTITLEMENT_INFO_LIST_MAX_SIZE;

        int requestResult = 0;

        int ret = sceNpEntitlementAccessPollServiceEntitlementInfoList(requestId, &requestResult, list, listNum, &hitNum, &nextOffset, &previousOffset);
        if (ret < 0)
        {
            if (list != NULL) free(list);
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (requestResult < 0)
        {
            if (list != NULL) free(list);
            SCE_ERROR_RESULT(result, requestResult);
            return;
        }

        // ret will either be SCE_NP_ENTITLEMENT_ACCESS_POLL_ASYNC_RET_RUNNING or SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED
        bool isFinished = ret == SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED;

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(isFinished);

        if (isFinished == true)
        {
            // number of entitlements returned
            writer.WriteInt32(nextOffset);
            writer.WriteInt32(previousOffset);

            writer.WriteInt32(hitNum);

            for (int i = 0; i < hitNum; i++)
            {
                Write(writer, list[i]);
            }
        }

        *resultsSize = writer.GetWrittenLength();

        if (list != NULL) free(list);

        SUCCESS_RESULT(result);
    }

    // Consume
    void EntitlementsSystem::RequestConsumeUnifiedEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        Int32 useCount = reader.ReadInt32();

        char* label = reader.ReadStringPtr();

        char* transactionIdStr = reader.ReadStringPtr();

        SceNpUnifiedEntitlementLabel entitlementLabel;

        memset(&entitlementLabel, 0, sizeof(SceNpUnifiedEntitlementLabel));
        strncpy(entitlementLabel.data, label, SCE_NP_UNIFIED_ENTITLEMENT_LABEL_SIZE);

        SceNpEntitlementAccessTransactionId transactionId;
        memset(&transactionId, 0, sizeof(SceNpEntitlementAccessTransactionId));
        strncpy(transactionId.transactionId, transactionIdStr, SCE_NP_ENTITLEMENT_ACCESS_TRANSACTION_ID_MAX_SIZE);

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestConsumeUnifiedEntitlement(userId, serviceLabel, &entitlementLabel, &transactionId, useCount, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::RequestConsumeServiceEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();

        Int32 useCount = reader.ReadInt32();

        char* label = reader.ReadStringPtr();

        char* transactionIdStr = reader.ReadStringPtr();

        SceNpServiceEntitlementLabel entitlementLabel;

        memset(&entitlementLabel, 0, sizeof(SceNpServiceEntitlementLabel));
        strncpy(entitlementLabel.data, label, SCE_NP_SERVICE_ENTITLEMENT_LABEL_SIZE);

        SceNpEntitlementAccessTransactionId transactionId;
        memset(&transactionId, 0, sizeof(SceNpEntitlementAccessTransactionId));
        strncpy(transactionId.transactionId, transactionIdStr, SCE_NP_ENTITLEMENT_ACCESS_TRANSACTION_ID_MAX_SIZE);

        Int64 requestId;
        int ret = sceNpEntitlementAccessRequestConsumeServiceEntitlement(userId, serviceLabel, &entitlementLabel, &transactionId, useCount, &requestId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt64(requestId);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void EntitlementsSystem::PollConsumeEntitlementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int64 requestId = reader.ReadInt64();

        Int32 useLimit = 0;

        int requestResult = 0;

        int ret = sceNpEntitlementAccessPollConsumeEntitlement(requestId, &requestResult, &useLimit);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (requestResult < 0)
        {
            SCE_ERROR_RESULT(result, requestResult);
            return;
        }

        // ret will either be SCE_NP_ENTITLEMENT_ACCESS_POLL_ASYNC_RET_RUNNING or SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED
        bool isFinished = ret == SCE_NP_ENTITLEMENT_ACCESS_POLL_RET_FINISHED;

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(isFinished);

        if (isFinished == true)
        {
            writer.WriteInt32(useLimit);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    // Write entitlements

    void EntitlementsSystem::Write(BinaryWriter& writer, SceNpEntitlementAccessUnifiedEntitlementInfo& unifiedInfo)
    {
        if (unifiedInfo.inactiveDate.tick != SCE_NP_ENTITLEMENT_ACCESS_INVALID_DATE)
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(unifiedInfo.inactiveDate);
        }
        else
        {
            writer.WriteBool(false);
        }

        if (unifiedInfo.activeDate.tick != SCE_NP_ENTITLEMENT_ACCESS_INVALID_DATE)
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(unifiedInfo.activeDate);
        }
        else
        {
            writer.WriteBool(false);
        }

        writer.WriteString(unifiedInfo.entitlementLabel.data);
        writer.WriteInt32(unifiedInfo.entitlementType);
        writer.WriteInt32(unifiedInfo.useCount);
        writer.WriteInt32(unifiedInfo.useLimit);
        writer.WriteUInt32(unifiedInfo.packageType);
        writer.WriteBool(unifiedInfo.activeFlag);
    }

    void EntitlementsSystem::Write(BinaryWriter& writer, SceNpEntitlementAccessServiceEntitlementInfo& serviceInfo)
    {
        if (serviceInfo.inactiveDate.tick != SCE_NP_ENTITLEMENT_ACCESS_INVALID_DATE)
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(serviceInfo.inactiveDate);
        }
        else
        {
            writer.WriteBool(false);
        }

        if (serviceInfo.activeDate.tick != SCE_NP_ENTITLEMENT_ACCESS_INVALID_DATE)
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(serviceInfo.activeDate);
        }
        else
        {
            writer.WriteBool(false);
        }

        writer.WriteString(serviceInfo.entitlementLabel.data);
        writer.WriteInt32(serviceInfo.entitlementType);
        writer.WriteInt32(serviceInfo.useCount);
        writer.WriteInt32(serviceInfo.useLimit);
        writer.WriteBool(serviceInfo.activeFlag);
        writer.WriteBool(serviceInfo.isConsumable);
    }
}
#endif
