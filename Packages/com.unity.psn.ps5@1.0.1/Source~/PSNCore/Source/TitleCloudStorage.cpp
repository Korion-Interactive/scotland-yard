#include "TitleCloudStorage.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include <np_cppwebapi.h>
#include <string>

namespace CppWebApi = sce::Np::CppWebApi;

namespace psn
{
    void TitleCloudStorage::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::AddAndGetVariable, TitleCloudStorage::AddAndGetVariableImpl);
        MsgHandler::AddMethod(Methods::SetVariableWithConditions, TitleCloudStorage::SetVariableWithConditionsImpl);
        MsgHandler::AddMethod(Methods::GetMultiVariablesBySlot, TitleCloudStorage::GetMultiVariablesBySlotImpl);

        MsgHandler::AddMethod(Methods::SetMultiVariablesByUser, TitleCloudStorage::SetMultiVariablesByUserImpl);
        MsgHandler::AddMethod(Methods::GetMultiVariablesByUser, TitleCloudStorage::GetMultiVariablesByUserImpl);
        MsgHandler::AddMethod(Methods::DeleteMultiVariablesByUser, TitleCloudStorage::DeleteMultiVariablesByUserImpl);

        MsgHandler::AddMethod(Methods::UploadData, TitleCloudStorage::UploadDataImpl);
        MsgHandler::AddMethod(Methods::DownloadData, TitleCloudStorage::DownloadDataImpl);
        MsgHandler::AddMethod(Methods::DeleteMultiDataBySlot, TitleCloudStorage::DeleteMultiDataBySlotImpl);
        MsgHandler::AddMethod(Methods::DeleteMultiDataByUser, TitleCloudStorage::DeleteMultiDataByUserImpl);
        MsgHandler::AddMethod(Methods::GetMultiDataStatusesBySlot, TitleCloudStorage::GetMultiDataStatusesBySlotImpl);
        MsgHandler::AddMethod(Methods::GetMultiDataStatusesByUser, TitleCloudStorage::GetMultiDataStatusesByUserImpl);
    }

    void TitleCloudStorage::AddAndGetVariableImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        int slotId = reader.ReadInt32();
        Int64 value = reader.ReadInt64();

        IntrusivePtr<TCS::AddAndGetVariableRequestBody> reqBodyPtr;
        ret = TCS::AddAndGetVariableRequestBodyFactory::create(libContextPtr, value, &reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            Int32 serviceLabel = reader.ReadUInt32();
            reqBodyPtr->setNpServiceLabel(serviceLabel);
        }

        bool isComparedLastUpdatedDateTimeSet = reader.ReadBool();
        if (isComparedLastUpdatedDateTimeSet)
        {
            SceRtcTick compareTick;
            compareTick.tick = reader.ReadUInt64();
            reqBodyPtr->setComparedLastUpdatedDateTime(compareTick);
        }

        bool isComparedLastUpdatedUserAccountId = reader.ReadBool();
        if (isComparedLastUpdatedUserAccountId)
        {
            UInt64 compareAccountId = reader.ReadUInt64();
            char compareAccountIdStr[32] = { 0 };
            snprintf(compareAccountIdStr, sizeof(compareAccountIdStr) - 1, "%lu", compareAccountId);
            ret = reqBodyPtr->setComparedLastUpdatedUserAccountId(compareAccountIdStr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        TCS::VariablesApi::ParameterToAddAndGetVariable param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotId, reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<TCS::Variable> VariableResponseBody;
        VariableResponseBody response;

        Common::Transaction<VariableResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::addAndGetVariable(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        TCS::Variable* variable = response.get();

        WriteVariable(writer, variable);

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::SetVariableWithConditionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        int slotId = reader.ReadInt32();
        Int64 value = reader.ReadInt64();

        TCS::Condition condition = TCS::Condition::_NOT_SET;

        condition = (TCS::Condition)reader.ReadInt32();

        IntrusivePtr<TCS::SetVariableWithConditionsRequestBody> reqBodyPtr;
        ret = TCS::SetVariableWithConditionsRequestBodyFactory::create(libContextPtr, condition, value, &reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            Int32 serviceLabel = reader.ReadUInt32();
            reqBodyPtr->setNpServiceLabel(serviceLabel);
        }

        bool isComparedLastUpdatedDateTimeSet = reader.ReadBool();
        if (isComparedLastUpdatedDateTimeSet)
        {
            SceRtcTick compareTick;
            compareTick.tick = reader.ReadUInt64();
            reqBodyPtr->setComparedLastUpdatedDateTime(compareTick);
        }

        bool isComparedLastUpdatedUserAccountId = reader.ReadBool();
        if (isComparedLastUpdatedUserAccountId)
        {
            UInt64 compareAccountId = reader.ReadUInt64();
            char compareAccountIdStr[32] = { 0 };
            snprintf(compareAccountIdStr, sizeof(compareAccountIdStr) - 1, "%lu", compareAccountId);
            ret = reqBodyPtr->setComparedLastUpdatedUserAccountId(compareAccountIdStr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        TCS::VariablesApi::ParameterToSetVariableWithConditions param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotId, reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<TCS::Variable> VariableResponseBody;
        VariableResponseBody response;

        Common::Transaction<VariableResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::setVariableWithConditions(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        TCS::Variable* variable = response.get();

        WriteVariable(writer, variable);

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::GetMultiVariablesBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        std::string accountIdsStr = "";

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt32 accountIdCount = reader.ReadUInt32();

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else if (accountIdCount > 0)
        {
            // A comma seperate string up to 32 account ids.
            for (int i = 0; i < accountIdCount; i++)
            {
                UInt64 accountId = reader.ReadUInt64();

                if (accountIdsStr.length() > 0) accountIdsStr += ",";
                accountIdsStr += std::to_string(accountId);
            }
        }

        bool hasAccountIds = accountIdsStr.length() > 0;

        int slotId = reader.ReadInt32();

        bool isLimitSet = reader.ReadBool();
        int limit = 0;
        if (isLimitSet)
        {
            limit = reader.ReadInt32();
        }

        bool isOffsetSet = reader.ReadBool();
        int offset = 0;
        if (isOffsetSet)
        {
            offset = reader.ReadInt32();
        }

        bool isGroupSet = reader.ReadBool();
        TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Group group = TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Group::_NOT_SET;
        if (isGroupSet)
        {
            group = (TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Group)reader.ReadInt32();
        }

        bool isSortSet = reader.ReadBool();
        TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Sort sort = TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Sort::_NOT_SET;
        if (isSortSet)
        {
            sort = (TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::Sort)reader.ReadInt32();
        }

        bool isSortModeSet = reader.ReadBool();
        TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::SortMode sortMode = TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::SortMode::_NOT_SET;
        if (isSortModeSet)
        {
            sortMode = (TCS::VariablesApi::ParameterToGetMultiVariablesBySlot::SortMode)reader.ReadInt32();
        }

        Int32 serviceLabel = 0;
        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::VariablesApi::ParameterToGetMultiVariablesBySlot param;
        ret = param.initialize(libContextPtr, slotId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (hasAccountIds)
        {
            ret = param.setaccountIds(accountIdsStr.c_str());
            if (ret < 0)
            {
                param.terminate();
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isLimitSet)
        {
            param.setlimit(limit);
        }

        if (isOffsetSet)
        {
            param.setoffset(offset);
        }

        if (isGroupSet)
        {
            param.setgroup(group);
        }

        if (isSortSet)
        {
            param.setsort(sort);
        }

        if (isSortModeSet)
        {
            param.setsortMode(sortMode);
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        typedef Common::IntrusivePtr<TCS::GetMultiVariablesResponseBody> GetMultiVariablesBody;
        GetMultiVariablesBody response;

        Common::Transaction<GetMultiVariablesBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::getMultiVariablesBySlot(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(response->limitIsSet());
        if (response->limitIsSet())
        {
            writer.WriteInt32(response->getLimit());
        }

        writer.WriteBool(response->offsetIsSet());
        if (response->offsetIsSet())
        {
            writer.WriteInt32(response->getOffset());
        }

        writer.WriteBool(response->totalVariableCountIsSet());
        if (response->totalVariableCountIsSet())
        {
            writer.WriteInt32(response->getTotalVariableCount());
        }

        size_t numVars = 0;

        if (response->variablesIsSet())
        {
            numVars = response->getVariables()->size();
        }

        writer.WriteInt32(numVars);

        if (numVars > 0)
        {
            for (const auto& variable : *response->getVariables())
            {
                WriteVariable(writer, variable.get());
            }
        }

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::SetMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        IntrusivePtr<TCS::SetMultiVariablesRequestBody> reqBodyPtr;
        ret = TCS::SetMultiVariablesRequestBodyFactory::create(libContextPtr, &reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int numVars = reader.ReadInt32();

        Common::Vector<Common::IntrusivePtr<TCS::SetMultiVariablesRequestBody_variables> > variables(libContextPtr);
        Common::IntrusivePtr<TCS::SetMultiVariablesRequestBody_variables> variable;

        for (int i = 0; i < numVars; i++)
        {
            int slotId = reader.ReadInt32();
            Int64 value = reader.ReadInt64();

            TCS::SetMultiVariablesRequestBody_variablesFactory::create(libContextPtr, &variable);
            (*variable).setSlotId(slotId);
            (*variable).setValue(value);
            ret = variables.pushBack(variable);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        ret = reqBodyPtr->setVariables(variables);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            Int32 serviceLabel = reader.ReadUInt32();
            reqBodyPtr->setNpServiceLabel(serviceLabel);
        }

        TCS::VariablesApi::ParameterToSetMultiVariablesByUser param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), reqBodyPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::DefaultResponse response;

        Common::Transaction<Common::DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::setMultiVariablesByUser(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::GetMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        std::string slotIdsStr = "";

        // A comma seperate string up to 32 account ids. Can include "me" and "anyone" ids.
        UInt32 slotIdCount = reader.ReadUInt32();

        if (slotIdCount > 0)
        {
            for (int i = 0; i < slotIdCount; i++)
            {
                int slotId = reader.ReadInt32();

                if (slotIdsStr.length() > 0) slotIdsStr += ",";
                slotIdsStr += std::to_string(slotId);
            }
        }

        bool isLimitSet = reader.ReadBool();
        int limit = 0;
        if (isLimitSet)
        {
            limit = reader.ReadInt32();
        }

        bool isOffsetSet = reader.ReadBool();
        int offset = 0;
        if (isOffsetSet)
        {
            offset = reader.ReadInt32();
        }

        bool isSortSet = reader.ReadBool();
        TCS::VariablesApi::ParameterToGetMultiVariablesByUser::Sort sort = TCS::VariablesApi::ParameterToGetMultiVariablesByUser::Sort::_NOT_SET;
        if (isSortSet)
        {
            sort = (TCS::VariablesApi::ParameterToGetMultiVariablesByUser::Sort)reader.ReadInt32();
        }

        bool isSortModeSet = reader.ReadBool();
        TCS::VariablesApi::ParameterToGetMultiVariablesByUser::SortMode sortMode = TCS::VariablesApi::ParameterToGetMultiVariablesByUser::SortMode::_NOT_SET;
        if (isSortModeSet)
        {
            sortMode = (TCS::VariablesApi::ParameterToGetMultiVariablesByUser::SortMode)reader.ReadInt32();
        }

        Int32 serviceLabel = 0;
        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::VariablesApi::ParameterToGetMultiVariablesByUser param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotIdsStr.c_str());
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isLimitSet)
        {
            param.setlimit(limit);
        }

        if (isOffsetSet)
        {
            param.setoffset(offset);
        }

        if (isSortSet)
        {
            param.setsort(sort);
        }

        if (isSortModeSet)
        {
            param.setsortMode(sortMode);
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        typedef Common::IntrusivePtr<TCS::GetMultiVariablesResponseBody> GetMultiVariablesBody;
        GetMultiVariablesBody response;

        Common::Transaction<GetMultiVariablesBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::getMultiVariablesByUser(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(response->limitIsSet());
        if (response->limitIsSet())
        {
            writer.WriteInt32(response->getLimit());
        }

        writer.WriteBool(response->offsetIsSet());
        if (response->offsetIsSet())
        {
            writer.WriteInt32(response->getOffset());
        }

        writer.WriteBool(response->totalVariableCountIsSet());
        if (response->totalVariableCountIsSet())
        {
            writer.WriteInt32(response->getTotalVariableCount());
        }

        size_t numVars = 0;
        if (response->variablesIsSet())
        {
            numVars = response->getVariables()->size();
        }

        writer.WriteInt32(numVars);

        if (numVars > 0)
        {
            for (const auto& variable : *response->getVariables())
            {
                WriteVariable(writer, variable.get());
            }
        }

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::DeleteMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        std::string slotIdsStr = "";

        // A comma seperate string up to 32 account ids. Can include "me" and "anyone" ids.
        UInt32 slotIdCount = reader.ReadUInt32();

        if (slotIdCount > 0)
        {
            for (int i = 0; i < slotIdCount; i++)
            {
                int slotId = reader.ReadInt32();

                if (slotIdsStr.length() > 0) slotIdsStr += ",";
                slotIdsStr += std::to_string(slotId);
            }
        }

        TCS::VariablesApi::ParameterToDeleteMultiVariablesByUser param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotIdsStr.c_str());
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::DefaultResponse response;
        Common::Transaction<Common::DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = TCS::VariablesApi::deleteMultiVariablesByUser(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::UploadDataImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        int slotId = reader.ReadInt32();

        Int32 dataSize = reader.ReadInt32();
        void* dataBuffer = NULL;

        if (dataSize > 0)
        {
            Int64 intPtr = reader.ReadInt64();
            dataBuffer = (void*)intPtr;
        }

        Int32 infoSize = reader.ReadInt32();
        void* info = reader.ReadDataPtr(infoSize);

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        bool isComparedLastUpdatedDateTimeSet = reader.ReadBool();
        SceRtcTick compareTick;
        if (isComparedLastUpdatedDateTimeSet)
        {
            compareTick.tick = reader.ReadUInt64();
        }

        bool isComparedLastUpdatedUserAccountId = reader.ReadBool();
        char compareAccountIdStr[32] = { 0 };

        if (isComparedLastUpdatedUserAccountId)
        {
            UInt64 compareAccountId = reader.ReadUInt64();
            snprintf(compareAccountIdStr, sizeof(compareAccountIdStr) - 1, "%lu", compareAccountId);
        }

        UploadParams params;

        params.userWebCtx = userWebCtx;
        params.accountId = accountIdsStr.c_str();
        params.slotId = slotId;

        params.dataSize = dataSize;
        params.data = dataBuffer;

        params.infoSize = infoSize;
        params.info = info;

        params.isServiceLabelSet = isServiceLabelSet;
        params.serviceLabel = serviceLabel;

        params.isComparedLastUpdatedDateTimeSet = isComparedLastUpdatedDateTimeSet;
        params.compareTick = compareTick;

        params.isComparedLastUpdatedUserAccountId = isComparedLastUpdatedUserAccountId;
        params.compareAccountIdStr = compareAccountIdStr;

        ret = UploadDataAndSetInfo(&params);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    int TitleCloudStorage::UploadDataAndSetInfo(UploadParams* uploadParams) //WebApiUserContext* userWebCtx, const char* accountId, Int32 slotId, Int32 dataSize, void* data, size_t infoSize, void* info, bool isServiceLabelSet, Int32 serviceLabel)
    {
        // Update and set the data info

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        int ret = 0;

        // First Part - Send the data
        Common::UpStreamTransaction<Common::IntrusivePtr<TCS::UploadDataResponseBody>, Common::IntrusivePtr<TCS::DataApi::UploadDataResponseHeaders> > transOfData;
        Common::IntrusivePtr<TCS::DataApi::UploadDataResponseHeaders> headersOfData;

        TCS::DataApi::ParameterToUploadData paramOfData;

        ret = transOfData.start(libContextPtr, uploadParams->dataSize);
        if (ret < 0)
        {
            return ret;
        }

        ret = paramOfData.initialize(libContextPtr, uploadParams->accountId, uploadParams->slotId);
        if (ret < 0)
        {
            transOfData.finish();
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        paramOfData.setxPsnAtomicOperation(TCS::DataApi::ParameterToUploadData::XPsnAtomicOperation::kBegin);
#else
        paramOfData.setxPsnAtomicOperation(TCS::DataApi::ParameterToUploadData::XPsnAtomicOperation::BEGIN);
#endif

        ret = TCS::DataApi::uploadData(uploadParams->userWebCtx->m_webapiUserCtxId, paramOfData, transOfData);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            return ret;
        }

        ret = transOfData.sendData(uploadParams->data, uploadParams->dataSize);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            return ret;
        }

        ret = transOfData.getResponseHeaders(headersOfData);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            return ret;
        }

        if (!headersOfData->hasXPsnAtomicOperationId())
        {
            ret = -1;
            transOfData.finish();
            paramOfData.terminate();
            return ret;
        }

        const char* atomicOperationId = nullptr;
        atomicOperationId = headersOfData->getXPsnAtomicOperationId().c_str();

        // Second Part - Set the data info

        Common::Transaction<Common::DefaultResponse, Common::IntrusivePtr<TCS::DataApi::SetDataInfoResponseHeaders> > transOfInfo;
        Common::IntrusivePtr<TCS::SetDataInfoRequestBody> reqBodyOfInfo;
        TCS::DataApi::ParameterToSetDataInfo paramOfInfo;

        ret = transOfInfo.start(libContextPtr);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            return ret;
        }

        ret = TCS::SetDataInfoRequestBodyFactory::create(libContextPtr, uploadParams->info, uploadParams->infoSize, &reqBodyOfInfo);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            transOfInfo.finish();
            return ret;
        }

        if (uploadParams->isComparedLastUpdatedDateTimeSet)
        {
            reqBodyOfInfo->setComparedLastUpdatedDateTime(uploadParams->compareTick);
        }

        if (uploadParams->isComparedLastUpdatedUserAccountId)
        {
            ret = reqBodyOfInfo->setComparedLastUpdatedUserAccountId(uploadParams->compareAccountIdStr);
            if (ret < 0)
            {
                transOfData.finish();
                paramOfData.terminate();
                transOfInfo.finish();
                return ret;
            }
        }

        if (uploadParams->isServiceLabelSet)
        {
            reqBodyOfInfo->setNpServiceLabel(uploadParams->serviceLabel);
        }

        ret = paramOfInfo.initialize(libContextPtr, uploadParams->accountId, uploadParams->slotId, reqBodyOfInfo);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            transOfInfo.finish();
            paramOfInfo.terminate();
            return ret;
        }
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        paramOfInfo.setxPsnAtomicOperation(TCS::DataApi::ParameterToSetDataInfo::XPsnAtomicOperation::kEnd);
#else
        paramOfInfo.setxPsnAtomicOperation(TCS::DataApi::ParameterToSetDataInfo::XPsnAtomicOperation::END);
#endif
        ret = paramOfInfo.setxPsnAtomicOperationId(atomicOperationId);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            transOfInfo.finish();
            paramOfInfo.terminate();
            return ret;
        }

        ret = TCS::DataApi::setDataInfo(uploadParams->userWebCtx->m_webapiUserCtxId, paramOfInfo, transOfInfo);
        if (ret < 0)
        {
            transOfData.finish();
            paramOfData.terminate();
            transOfInfo.finish();
            paramOfInfo.terminate();
            return ret;
        }

        transOfData.finish();
        paramOfData.terminate();
        transOfInfo.finish();
        paramOfInfo.terminate();

        return ret;
    }

    void TitleCloudStorage::DownloadDataImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        int slotId = reader.ReadInt32();

        char* objectId = reader.ReadStringPtr();

        char* range = NULL;
        bool isRangeSet = reader.ReadBool();
        if (isRangeSet)
        {
            range = reader.ReadStringPtr();
        }

        char* ifMatch = NULL;
        bool isIfMatchSet = reader.ReadBool();
        if (isIfMatchSet)
        {
            ifMatch = reader.ReadStringPtr();
        }

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        UInt32 bufferSize = reader.ReadUInt32();
        void* dataBuffer = NULL;

        if (bufferSize > 0)
        {
            Int64 intPtr = reader.ReadInt64();
            dataBuffer = (void*)intPtr;
        }

        TCS::DataApi::ParameterToDownloadData param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (objectId)
        {
            ret = param.setobjectId(objectId);
            if (ret < 0)
            {
                param.terminate();
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isRangeSet)
        {
            ret = param.setrange(range);
            if (ret < 0)
            {
                param.terminate();
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isIfMatchSet)
        {
            ret = param.setifMatch(ifMatch);
            if (ret < 0)
            {
                param.terminate();
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        typedef Common::IntrusivePtr<TCS::DataApi::DownloadDataResponseHeaders> DownloadDataBody;
        DownloadDataBody response;

        Common::DownStreamTransaction<DownloadDataBody> transaction;
        transaction.start(libContextPtr);

        ret = TCS::DataApi::downloadData(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Read the data from TCS and copy them into the special pinned buffer.
        // This allows large quantities of data to be marshalled to the C#
        char* ptr = (char*)dataBuffer;
        UInt64 writtenSize = 0;
        UInt64 totalReadSize = 0;

        while (true)
        {
            size_t readDataSize = 0;
            char dataBuf[1024 * 8] = { 0 }; // Read 8k at a time
            readDataSize = transaction.readData(dataBuf, sizeof(dataBuf) - 1);

            if (readDataSize <= 0)
            {
                break;
            }

            // even if the buffer is full keep reading until
            // the data is fully read from TCS. This is so the full size of the data
            // can be returned to the C# along with the actual size read.
            totalReadSize += readDataSize;

            // Copy the read data into the buffer. Only copy the required amount and don't write over the end of the buffer
            if (writtenSize < bufferSize)
            {
                int bytesToCopy = readDataSize;
                // More data has been read and needs to be copied to the buffer
                // Check there is room in the buffer for it
                if (writtenSize + readDataSize > bufferSize)
                {
                    bytesToCopy = bufferSize - writtenSize;
                }

                memcpy(ptr, dataBuf, bytesToCopy);

                ptr += bytesToCopy;

                writtenSize += bytesToCopy;
            }
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt64(writtenSize);
        writer.WriteUInt64(totalReadSize);

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    // One slot id, multiple account ids
    void TitleCloudStorage::DeleteMultiDataBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt32 accountIdCount = reader.ReadUInt32();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else if (accountIdCount > 0)
        {
            // A comma seperate string up to 32 account ids.
            for (int i = 0; i < accountIdCount; i++)
            {
                UInt64 accountId = reader.ReadUInt64();

                if (accountIdsStr.length() > 0) accountIdsStr += ",";
                accountIdsStr += std::to_string(accountId);
            }
        }

        int slotId = reader.ReadInt32();

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::DataApi::ParameterToDeleteMultiDataBySlot param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        Common::DefaultResponse response;
        Common::Transaction<Common::DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = TCS::DataApi::deleteMultiDataBySlot(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    // Multiple slot ids, one account id
    void TitleCloudStorage::DeleteMultiDataByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        std::string slotIdsStr = "";

        // A comma seperate string up to 32 account ids. Can include "me" and "anyone" ids.
        UInt32 slotIdCount = reader.ReadUInt32();

        if (slotIdCount > 0)
        {
            for (int i = 0; i < slotIdCount; i++)
            {
                int slotId = reader.ReadInt32();

                if (slotIdsStr.length() > 0) slotIdsStr += ",";
                slotIdsStr += std::to_string(slotId);
            }
        }

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::DataApi::ParameterToDeleteMultiDataByUser param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotIdsStr.c_str());
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        Common::DefaultResponse response;
        Common::Transaction<Common::DefaultResponse> transaction;

        transaction.start(libContextPtr);

        // API call
        ret = TCS::DataApi::deleteMultiDataByUser(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    // One slot id, multiple account ids
    void TitleCloudStorage::GetMultiDataStatusesBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt32 accountIdCount = reader.ReadUInt32();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else if (accountIdCount > 0)
        {
            // A comma seperate string up to 32 account ids.
            for (int i = 0; i < accountIdCount; i++)
            {
                UInt64 accountId = reader.ReadUInt64();

                if (accountIdsStr.length() > 0) accountIdsStr += ",";
                accountIdsStr += std::to_string(accountId);
            }
        }

        bool hasAccountIds = accountIdsStr.length() > 0;

        int slotId = reader.ReadInt32();

        bool isFieldsSet = reader.ReadBool();
        char* fields = NULL;
        if (isFieldsSet)
        {
            fields = reader.ReadStringPtr();
        }

        bool isLimitSet = reader.ReadBool();
        int limit = 0;
        if (isLimitSet)
        {
            limit = reader.ReadInt32();
        }

        bool isOffsetSet = reader.ReadBool();
        int offset = 0;
        if (isOffsetSet)
        {
            offset = reader.ReadInt32();
        }

        bool isGroupSet = reader.ReadBool();
        TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Group group = TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Group::_NOT_SET;
        if (isGroupSet)
        {
            group = (TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Group)reader.ReadInt32();
        }

        bool isSortSet = reader.ReadBool();
        TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Sort sort = TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Sort::_NOT_SET;
        if (isSortSet)
        {
            sort = (TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::Sort)reader.ReadInt32();
        }

        bool isSortModeSet = reader.ReadBool();
        TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::SortMode sortMode = TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::SortMode::_NOT_SET;
        if (isSortModeSet)
        {
            sortMode = (TCS::DataApi::ParameterToGetMultiDataStatusesBySlot::SortMode)reader.ReadInt32();
        }

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::DataApi::ParameterToGetMultiDataStatusesBySlot param;
        ret = param.initialize(libContextPtr, slotId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (hasAccountIds)
        {
            ret = param.setaccountIds(accountIdsStr.c_str());
            if (ret < 0)
            {
                param.terminate();
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isFieldsSet)
        {
            ret = param.setfields(fields);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isLimitSet)
        {
            param.setlimit(limit);
        }

        if (isOffsetSet)
        {
            param.setoffset(offset);
        }

        if (isGroupSet)
        {
            param.setgroup(group);
        }

        if (isSortSet)
        {
            param.setsort(sort);
        }

        if (isSortModeSet)
        {
            param.setsortMode(sortMode);
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        typedef Common::IntrusivePtr<TCS::GetMultiDataStatusesResponseBody> GetMultiDataStatusesResponseBody;
        GetMultiDataStatusesResponseBody response;

        Common::Transaction<GetMultiDataStatusesResponseBody> transaction;
        transaction.start(libContextPtr);

        ret = TCS::DataApi::getMultiDataStatusesBySlot(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(response->limitIsSet());
        if (response->limitIsSet())
        {
            writer.WriteInt32(response->getLimit());
        }

        writer.WriteBool(response->offsetIsSet());
        if (response->offsetIsSet())
        {
            writer.WriteInt32(response->getOffset());
        }

        writer.WriteBool(response->totalDataStatusCountIsSet());
        if (response->totalDataStatusCountIsSet())
        {
            writer.WriteInt32(response->getTotalDataStatusCount());
        }

        size_t numVars = response->getDataStatusList()->size();

        writer.WriteInt32(numVars);

        if (numVars > 0)
        {
            for (const auto& dataStatus : *response->getDataStatusList())
            {
                WriteDataStatus(writer, dataStatus.get());
            }
        }

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    // Multiple slot ids, one account id
    void TitleCloudStorage::GetMultiDataStatusesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        *resultsSize = 0;

        int ret = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        bool isMe = reader.ReadBool();
        bool isAnyone = reader.ReadBool();
        UInt64 accountId = reader.ReadUInt64();

        std::string accountIdsStr = "";

        if (isMe)
        {
            accountIdsStr = "me";
        }
        else if (isAnyone)
        {
            accountIdsStr = "anyone";
        }
        else
        {
            accountIdsStr = std::to_string(accountId);
        }

        std::string slotIdsStr = "";

        // A comma seperate string up to 32 account ids. Can include "me" and "anyone" ids.
        UInt32 slotIdCount = reader.ReadUInt32();

        if (slotIdCount > 0)
        {
            for (int i = 0; i < slotIdCount; i++)
            {
                int slotId = reader.ReadInt32();

                if (slotIdsStr.length() > 0) slotIdsStr += ",";
                slotIdsStr += std::to_string(slotId);
            }
        }

        bool isFieldsSet = reader.ReadBool();
        char* fields = NULL;
        if (isFieldsSet)
        {
            fields = reader.ReadStringPtr();
        }

        bool isLimitSet = reader.ReadBool();
        int limit = 0;
        if (isLimitSet)
        {
            limit = reader.ReadInt32();
        }

        bool isOffsetSet = reader.ReadBool();
        int offset = 0;
        if (isOffsetSet)
        {
            offset = reader.ReadInt32();
        }

        bool isSortSet = reader.ReadBool();
        TCS::DataApi::ParameterToGetMultiDataStatusesByUser::Sort sort = TCS::DataApi::ParameterToGetMultiDataStatusesByUser::Sort::_NOT_SET;
        if (isSortSet)
        {
            sort = (TCS::DataApi::ParameterToGetMultiDataStatusesByUser::Sort)reader.ReadInt32();
        }

        bool isSortModeSet = reader.ReadBool();
        TCS::DataApi::ParameterToGetMultiDataStatusesByUser::SortMode sortMode = TCS::DataApi::ParameterToGetMultiDataStatusesByUser::SortMode::_NOT_SET;
        if (isSortModeSet)
        {
            sortMode = (TCS::DataApi::ParameterToGetMultiDataStatusesByUser::SortMode)reader.ReadInt32();
        }

        bool isServiceLabelSet = reader.ReadBool();
        Int32 serviceLabel = 0;
        if (isServiceLabelSet)
        {
            serviceLabel = reader.ReadUInt32();
        }

        TCS::DataApi::ParameterToGetMultiDataStatusesByUser param;
        ret = param.initialize(libContextPtr, accountIdsStr.c_str(), slotIdsStr.c_str());
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isFieldsSet)
        {
            ret = param.setfields(fields);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (isLimitSet)
        {
            param.setlimit(limit);
        }

        if (isOffsetSet)
        {
            param.setoffset(offset);
        }

        if (isSortSet)
        {
            param.setsort(sort);
        }

        if (isSortModeSet)
        {
            param.setsortMode(sortMode);
        }

        if (isServiceLabelSet)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        typedef Common::IntrusivePtr<TCS::GetMultiDataStatusesResponseBody> GetMultiDataStatusesResponseBody;
        GetMultiDataStatusesResponseBody response;

        Common::Transaction<GetMultiDataStatusesResponseBody> transaction;
        transaction.start(libContextPtr);

        ret = TCS::DataApi::getMultiDataStatusesByUser(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(response->limitIsSet());
        if (response->limitIsSet())
        {
            writer.WriteInt32(response->getLimit());
        }

        writer.WriteBool(response->offsetIsSet());
        if (response->offsetIsSet())
        {
            writer.WriteInt32(response->getOffset());
        }

        writer.WriteBool(response->totalDataStatusCountIsSet());
        if (response->totalDataStatusCountIsSet())
        {
            writer.WriteInt32(response->getTotalDataStatusCount());
        }

        size_t numVars = response->getDataStatusList()->size();

        writer.WriteInt32(numVars);

        if (numVars > 0)
        {
            for (const auto& dataStatus : *response->getDataStatusList())
            {
                WriteDataStatus(writer, dataStatus.get());
            }
        }

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void TitleCloudStorage::WriteVariable(BinaryWriter& writer, const TCS::IdempotentVariable* variable)
    {
        bool ownerIsSet = variable->ownerIsSet();
        bool slotIdIsSet = variable->slotIdIsSet();
        bool valueIsSet = variable->valueIsSet();
        bool prevValueIsSet = false;
        bool lastUpdatedDateTimeIsSet = variable->lastUpdatedDateTimeIsSet();
        bool lastUpdatedUserIsSet = variable->lastUpdatedUserIsSet();

        writer.WriteBool(ownerIsSet);
        if (ownerIsSet)
        {
            Common::IntrusivePtr<TCS::Owner> owner = variable->getOwner();

            bool accountIdIsSet = owner->accountIdIsSet();
            bool onlineIdIsSet = owner->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(owner->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(owner->getOnlineId().data);
            }
        }

        writer.WriteBool(slotIdIsSet);
        if (slotIdIsSet)
        {
            writer.WriteInt32(variable->getSlotId());
        }

        writer.WriteBool(valueIsSet);
        if (valueIsSet)
        {
            writer.WriteInt64(variable->getValue());
        }

        writer.WriteBool(prevValueIsSet);

        writer.WriteBool(lastUpdatedDateTimeIsSet);
        if (lastUpdatedDateTimeIsSet)
        {
            writer.WriteRtcTick(variable->getLastUpdatedDateTime());
        }

        writer.WriteBool(lastUpdatedUserIsSet);
        if (lastUpdatedUserIsSet)
        {
            Common::IntrusivePtr<TCS::LastUpdatedUser> updatedUser = variable->getLastUpdatedUser();

            bool accountIdIsSet = updatedUser->accountIdIsSet();
            bool onlineIdIsSet = updatedUser->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(updatedUser->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(updatedUser->getOnlineId().data);
            }
        }
    }

    void TitleCloudStorage::WriteVariable(BinaryWriter& writer, const TCS::Variable* variable)
    {
        bool ownerIsSet = variable->ownerIsSet();
        bool slotIdIsSet = variable->slotIdIsSet();
        bool valueIsSet = variable->valueIsSet();
        bool prevValueIsSet = variable->prevValueIsSet();
        bool lastUpdatedDateTimeIsSet = variable->lastUpdatedDateTimeIsSet();
        bool lastUpdatedUserIsSet = variable->lastUpdatedUserIsSet();

        writer.WriteBool(ownerIsSet);
        if (ownerIsSet)
        {
            Common::IntrusivePtr<TCS::Owner> owner = variable->getOwner();

            bool accountIdIsSet = owner->accountIdIsSet();
            bool onlineIdIsSet = owner->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(owner->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(owner->getOnlineId().data);
            }
        }

        writer.WriteBool(slotIdIsSet);
        if (slotIdIsSet)
        {
            writer.WriteInt32(variable->getSlotId());
        }

        writer.WriteBool(valueIsSet);
        if (valueIsSet)
        {
            writer.WriteInt64(variable->getValue());
        }

        writer.WriteBool(prevValueIsSet);
        if (prevValueIsSet)
        {
            writer.WriteInt64(variable->getPrevValue());
        }

        writer.WriteBool(lastUpdatedDateTimeIsSet);
        if (lastUpdatedDateTimeIsSet)
        {
            writer.WriteRtcTick(variable->getLastUpdatedDateTime());
        }

        writer.WriteBool(lastUpdatedUserIsSet);
        if (lastUpdatedUserIsSet)
        {
            Common::IntrusivePtr<TCS::LastUpdatedUser> updatedUser = variable->getLastUpdatedUser();

            bool accountIdIsSet = updatedUser->accountIdIsSet();
            bool onlineIdIsSet = updatedUser->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(updatedUser->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(updatedUser->getOnlineId().data);
            }
        }
    }

    void TitleCloudStorage::WriteDataStatus(BinaryWriter& writer, const TCS::DataStatus* dataStatus)
    {
        bool ownerIsSet = dataStatus->ownerIsSet();
        bool slotIdIsSet = dataStatus->slotIdIsSet();

        bool dataSizeIsSet = dataStatus->dataSizeIsSet();
        bool infoIsSet = dataStatus->infoIsSet();
        bool objectIdIsSet = dataStatus->objectIdIsSet();

        bool lastUpdatedDateTimeIsSet = dataStatus->lastUpdatedDateTimeIsSet();
        bool lastUpdatedUserIsSet = dataStatus->lastUpdatedUserIsSet();

        writer.WriteBool(ownerIsSet);
        if (ownerIsSet)
        {
            Common::IntrusivePtr<TCS::Owner> owner = dataStatus->getOwner();

            bool accountIdIsSet = owner->accountIdIsSet();
            bool onlineIdIsSet = owner->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(owner->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(owner->getOnlineId().data);
            }
        }

        writer.WriteBool(slotIdIsSet);
        if (slotIdIsSet)
        {
            writer.WriteInt32(dataStatus->getSlotId());
        }

        writer.WriteBool(dataSizeIsSet);
        if (dataSizeIsSet)
        {
            writer.WriteInt64(dataStatus->getDataSize());
        }

        writer.WriteBool(infoIsSet);
        if (infoIsSet)
        {
            // Maximum of 512 bytes
            Binary* binary = dataStatus->getInfo().get();
            writer.WriteData((char*)binary->getBinary(), binary->size());
        }

        writer.WriteBool(objectIdIsSet);
        if (objectIdIsSet)
        {
            writer.WriteString(dataStatus->getObjectId().c_str());
        }

        writer.WriteBool(lastUpdatedDateTimeIsSet);
        if (lastUpdatedDateTimeIsSet)
        {
            writer.WriteRtcTick(dataStatus->getLastUpdatedDateTime());
        }

        writer.WriteBool(lastUpdatedUserIsSet);
        if (lastUpdatedUserIsSet)
        {
            Common::IntrusivePtr<TCS::LastUpdatedUser> updatedUser = dataStatus->getLastUpdatedUser();

            bool accountIdIsSet = updatedUser->accountIdIsSet();
            bool onlineIdIsSet = updatedUser->onlineIdIsSet();

            writer.WriteBool(accountIdIsSet);
            if (accountIdIsSet)
            {
                writer.WriteString(updatedUser->getAccountId().c_str());
            }

            writer.WriteBool(onlineIdIsSet);
            if (onlineIdIsSet)
            {
                writer.WriteString(updatedUser->getOnlineId().data);
            }
        }
    }
}
