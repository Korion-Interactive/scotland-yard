#include "Leaderboards.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include <string.h>

namespace PsnLeaderboards = sce::Np::CppWebApi::Leaderboards::V1;

namespace psn
{
    void Leaderboards::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::GetBoardDefinition, Leaderboards::GetBoardDefinitionImpl);
        MsgHandler::AddMethod(Methods::RecordScore, Leaderboards::RecordScoreImpl);
        MsgHandler::AddMethod(Methods::GetRanking, Leaderboards::GetRankingImpl);
        MsgHandler::AddMethod(Methods::GetLargeDateByObjectId, Leaderboards::GetLargeDataByObjectIdImpl);
    }

    void Leaderboards::Initialize()
    {
    }

    void Leaderboards::GetBoardDefinitionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        char* fields = NULL;
        SceNpServiceLabel serviceLabel = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        int boardId = reader.ReadInt32();

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet == true)
        {
            serviceLabel = reader.ReadUInt32();
        }

        bool isFieldsSet = reader.ReadBool();
        if (isFieldsSet == true)
        {
            fields = reader.ReadStringPtr();
        }

        BoardsApi::ParameterToGetBoardDefinition param;
        ret = param.initialize(libContextPtr, boardId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isServiceLabelSet == true)
        {
            param.setnpServiceLabel(serviceLabel);
        }

        if (isFieldsSet == true)
        {
            ret = param.setfields(fields);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        typedef Common::IntrusivePtr<GetBoardDefinitionResponseBody> GetBoardDefinitionResponseBody;
        GetBoardDefinitionResponseBody response;

        Common::Transaction<GetBoardDefinitionResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = BoardsApi::getBoardDefinition(userCtx->m_webapiUserCtxId, param, transaction);
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

        BinaryWriter writer(resultsData, resultsMaxSize);

        if (response->entryLimitIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32(response->getEntryLimit());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->largeDataNumLimitIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32(response->getLargeDataNumLimit());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->largeDataSizeLimitIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt64(response->getLargeDataSizeLimit());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->maxScoreLimitIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt64(response->getMaxScoreLimit());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->minScoreLimitIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt64(response->getMinScoreLimit());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->sortModeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)response->getSortMode());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (response->updateModeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)response->getUpdateMode());
        }
        else
        {
            writer.WriteBool(false);
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void Leaderboards::GetRankingImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        int boardId = reader.ReadInt32();

        SceNpServiceLabel serviceLabel = 0;

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet == true)
        {
            serviceLabel = reader.ReadUInt32();
        }

        int offset = reader.ReadInt32();
        int limit = reader.ReadInt32();
        int startSerialRank = reader.ReadInt32();

        Group group = (Group)reader.ReadInt32();

        Common::IntrusivePtr<User> aroundUser;
        int hasCenteredAroundUser = reader.ReadBool();
        if (hasCenteredAroundUser)
        {
            UserFactory::create(libContextPtr, &aroundUser);

            SceNpAccountId accountId = reader.ReadUInt64();
            Int32 pcId = reader.ReadInt32();

            aroundUser->setAccountId(accountId);
            aroundUser->setPcId(pcId);
        }

        int hasEdgeLimit = reader.ReadBool();
        Int32 edgeLimit = 0;
        if (hasEdgeLimit)
        {
            edgeLimit = reader.ReadInt32();
        }

        Common::Vector<Common::IntrusivePtr<User> > users(libContextPtr);
        int numUsers = reader.ReadInt32();

        if (numUsers > 0)
        {
            Common::IntrusivePtr<User> user;
            UserFactory::create(libContextPtr, &user);

            SceNpAccountId accountId = reader.ReadUInt64();
            Int32 pcId = reader.ReadInt32();

            user->setAccountId(accountId);
            user->setPcId(pcId);

            ret = users.pushBack(user);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        Common::IntrusivePtr<GetRankingRequestBody> requestBody;
        ret = GetRankingRequestBodyFactory::create(libContextPtr, &requestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (isServiceLabelSet)
        {
            requestBody->setNpServiceLabel(serviceLabel);
        }

        requestBody->setOffset(offset);
        requestBody->setLimit(limit);
        requestBody->setStartSerialRank(startSerialRank);

        requestBody->setGroup(group);

        if (hasCenteredAroundUser)
        {
            ret = requestBody->setUserCenteredAround(aroundUser);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        if (hasEdgeLimit && edgeLimit > 0)
        {
            requestBody->setCenterToEdgeLimit(edgeLimit);
        }

        if (numUsers > 0)
        {
            ret = requestBody->setUsers(users);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        ViewApi::ParameterToGetRanking param;
        ret = param.initialize(libContextPtr, boardId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.setgetRankingRequestBody(requestBody);

        typedef Common::IntrusivePtr<GetRankingResponseBody> GetRankingResponseBody;
        GetRankingResponseBody response;

        Common::Transaction<GetRankingResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = ViewApi::getRanking(userCtx->m_webapiUserCtxId, param, transaction);
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

        BinaryWriter writer(resultsData, resultsMaxSize);

        if (response->entriesIsSet())
        {
            Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<Entry> > > entries = response->getEntries();

            writer.WriteInt32(entries->size());

            for (const auto &entry : *entries)
            {
                writer.WriteUInt64(entry->getAccountId());

                writer.WriteBool(entry->pcIdIsSet());

                if (entry->pcIdIsSet())
                {
                    writer.WriteInt32(entry->getPcId());
                }

                writer.WriteInt32(entry->getSerialRank());
                writer.WriteInt32(entry->getHighestSerialRank());

                writer.WriteInt32(entry->getRank());
                writer.WriteInt32(entry->getHighestRank());
                writer.WriteInt64(entry->getScore());

                writer.WriteBool(entry->smallDataIsSet());
                if (entry->smallDataIsSet())
                {
                    Common::IntrusivePtr<Common::Binary> smallData = entry->getSmallData();
                    writer.WriteData((char*)smallData->getBinary(), smallData->size());
                }

                writer.WriteBool(entry->objectIdIsSet());
                if (entry->objectIdIsSet())
                {
                    writer.WriteString(entry->getObjectId().c_str());
                }

                writer.WriteBool(entry->commentIsSet());
                if (entry->commentIsSet())
                {
                    writer.WriteString(entry->getComment().c_str());
                }

                writer.WriteString(entry->getOnlineId().data);
            }
        }
        else
        {
            writer.WriteInt32(0); // no entries
        }

        writer.WriteRtcTick(response->getLastUpdatedDateTime());
        writer.WriteInt32(response->getTotalEntryCount());

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void Leaderboards::GetLargeDataByObjectIdImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        char* objectId = reader.ReadStringPtr();

        ViewApi::ParameterToGetLargeDataByObjectId param;
        ret = param.initialize(libContextPtr, objectId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isServiceLabelSet = reader.ReadBool();
        if (isServiceLabelSet == true)
        {
            SceNpServiceLabel serviceLabel = reader.ReadUInt32();
            param.setnpServiceLabel(serviceLabel);
        }

        bool isRangeSet = reader.ReadBool();
        if (isRangeSet == true)
        {
            char* rangeStr = reader.ReadStringPtr();
            ret = param.setrange(rangeStr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        bool isIfMatchSet = reader.ReadBool();
        if (isIfMatchSet == true)
        {
            char* matchStr = reader.ReadStringPtr();
            ret = param.setifMatch(matchStr);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        Common::DownStreamTransaction<Common::IntrusivePtr<ViewApi::GetLargeDataByObjectIdResponseHeaders> > transaction;
        transaction.start(libContextPtr);

        // API call
        ret = ViewApi::getLargeDataByObjectId(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        // Get a space in the output buffer where the size of the buffer can be writen once the size is known.
        UInt32* posForSize = writer.ReservePlaceholderUInt32();

        Int64 totalSize = 0;
        bool done = false;

        while (done == false)
        {
            size_t readDataSize = 0;
            char dataBuf[1024] = { 0 };
            readDataSize = transaction.readData(dataBuf, sizeof(dataBuf) - 1);

            if (readDataSize <= 0)
            {
                done = true;
            }
            else
            {
                // Have some data. Write the data to the buffer.
                // Make sure there is enough room in the buffer and throw an error if there isn't enough space.
                if ((writer.GetWrittenLength() + readDataSize) >= resultsMaxSize)
                {
                    if (ret < 0)
                    {
                        param.terminate();
                        transaction.finish();
                        ERROR_RESULT(result, "Not enough space in Results buffer for Leaderboard large data");
                        return;
                    }
                }

                // Just write the data to the buffer and not it's size.
                // Size will be added at the end.
                writer.WriteDataBlock(dataBuf, readDataSize);

                totalSize += readDataSize;
            }
        }

        *posForSize = totalSize;

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void Leaderboards::RecordScoreImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;
        int ret = 0;

        ScoreData scoreData;

        BinaryReader reader(sourceData, sourceSize);
        scoreData.userId = reader.ReadInt32();

        scoreData.userCtx = WebApi::Instance()->FindUser(scoreData.userId);

        if (scoreData.userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        scoreData.boardId = reader.ReadInt32();
        scoreData.score = reader.ReadUInt64();

        scoreData.largeDataSize = reader.ReadInt32();
        scoreData.largeData = NULL;

        if (scoreData.largeDataSize > 0)
        {
            scoreData.largeData = reader.ReadDataPtr(scoreData.largeDataSize);
        }

        scoreData.waitsForData = false;
        scoreData.needsTmpRank = false;
        scoreData.pcId = 0;
        scoreData.serviceLabel = 0;
        scoreData.smallDataSize = 0;
        scoreData.smallDataPtr = NULL;
        scoreData.objectId = NULL;
        scoreData.comment = NULL;
        scoreData.comparedDateTime.tick = 0;
        scoreData.tempRanksSet = false;

        scoreData.waitsForData = reader.ReadBool();
        scoreData.needsTmpRank = reader.ReadBool();

        scoreData.isServiceLabelSet = reader.ReadBool();
        if (scoreData.isServiceLabelSet == true)
        {
            scoreData.serviceLabel = reader.ReadUInt32();
        }

        scoreData.isObjectIdSet = reader.ReadBool();
        if (scoreData.isObjectIdSet == true)
        {
            scoreData.objectId = reader.ReadStringPtr();
        }

        scoreData.isPcIdSet = reader.ReadBool();
        if (scoreData.isPcIdSet == true)
        {
            scoreData.pcId = reader.ReadInt32();
        }

        scoreData.isSmallDataSet = reader.ReadBool();
        if (scoreData.isSmallDataSet == true)
        {
            scoreData.smallDataSize = reader.ReadUInt64();
            scoreData.smallDataPtr = reader.ReadDataPtr(scoreData.smallDataSize);
        }

        scoreData.isCommentSet = reader.ReadBool();
        if (scoreData.isCommentSet == true)
        {
            scoreData.comment = reader.ReadStringPtr();
        }

        scoreData.isComparedDateTimeSet = reader.ReadBool();
        if (scoreData.isComparedDateTimeSet == true)
        {
            scoreData.comparedDateTime.tick = reader.ReadUInt64();
        }

        // All data read.
        // Now record the score.
        // if large data is involved the recording of a score and large data has to be handled in a specific order.

        ret = RecordLeaderboardScore(scoreData);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (scoreData.largeDataSize > 0 && scoreData.largeData != NULL)
        {
            ret = RecordLargeData(scoreData);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        if (scoreData.tempRanksSet)
        {
            writer.WriteBool(true);
            writer.WriteInt32(scoreData.tmpRankResult);
            writer.WriteInt32(scoreData.tmpSerialRankResult);
        }
        else
        {
            writer.WriteBool(false);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int Leaderboards::RecordLeaderboardScore(ScoreData& scoreData)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        Common::IntrusivePtr<RecordScoreRequestBody> requestBody;

        ret = RecordScoreRequestBodyFactory::create(libContextPtr, scoreData.score, &requestBody);
        if (ret < 0)
        {
            return ret;
        }

        bool hasLargeData = scoreData.largeDataSize > 0 && scoreData.largeData != NULL;

        //  requestBody->setWaitsForData(scoreData.waitsForData);
        requestBody->setWaitsForData(hasLargeData);
        requestBody->setNeedsTmpRank(scoreData.needsTmpRank);

        if (scoreData.isServiceLabelSet == true)
        {
            requestBody->setNpServiceLabel(scoreData.serviceLabel);
        }
#if (defined(SCE_PROSPERO_SDK_VERSION) && (SCE_PROSPERO_SDK_VERSION < 0x08000000u)) || (defined(SCE_ORBIS_SDK_VERSION) && (SCE_ORBIS_SDK_VERSION < 0x11000000u))
        if (scoreData.isObjectIdSet == true)
        {
            ret = requestBody->setObjectId(scoreData.objectId);
            if (ret < 0)
            {
                return ret;
            }
        }
#endif
        if (scoreData.isPcIdSet == true)
        {
            requestBody->setPcId(scoreData.pcId);
        }

        if (scoreData.isSmallDataSet == true)
        {
            ret = requestBody->setSmallData(scoreData.smallDataPtr, scoreData.smallDataSize);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (scoreData.isCommentSet == true)
        {
            ret = requestBody->setComment(scoreData.comment);
            if (ret < 0)
            {
                return ret;
            }
        }

        if (scoreData.isComparedDateTimeSet == true)
        {
            requestBody->setComparedDateTime(scoreData.comparedDateTime);
        }

        RecordApi::ParameterToRecordScore param;
        ret = param.initialize(libContextPtr, scoreData.boardId, requestBody);
        if (ret < 0)
        {
            return ret;
        }

        typedef Common::IntrusivePtr<RecordScoreResponseBody> RecordScoreResponseBody;
        RecordScoreResponseBody response;

        Common::Transaction<RecordScoreResponseBody, Common::IntrusivePtr<RecordApi::RecordScoreResponseHeaders> > transaction;
        transaction.start(libContextPtr);

        if (hasLargeData)
        {
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
            param.setxPsnAtomicOperation(RecordApi::ParameterToRecordScore::XPsnAtomicOperation::kBegin);
#else
            param.setxPsnAtomicOperation(RecordApi::ParameterToRecordScore::XPsnAtomicOperation::BEGIN);
#endif
        }

        // API call
        ret = RecordApi::recordScore(scoreData.userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        if (requestBody->getNeedsTmpRank())
        {
            scoreData.tempRanksSet = true;
            scoreData.tmpRankResult = response->getTmpRank();
            scoreData.tmpSerialRankResult = response->getTmpSerialRank();
        }
        else
        {
            scoreData.tempRanksSet = false;
        }

        if (hasLargeData)
        {
            Common::IntrusivePtr<RecordApi::RecordScoreResponseHeaders> responseHeaderOfScore;
            ret = transaction.getResponseHeaders(responseHeaderOfScore);
            if (ret < 0)
            {
                param.terminate();
                transaction.finish();
                return ret;
            }

            if (!responseHeaderOfScore->hasXPsnAtomicOperationId())
            {
                param.terminate();
                transaction.finish();
                return -1;
            }

            strcpy_s(scoreData.operationId, sizeof(scoreData.operationId), responseHeaderOfScore->getXPsnAtomicOperationId().c_str());
        }

        param.terminate();
        transaction.finish();

        return 0;
    }

    int Leaderboards::RecordLargeData(ScoreData& scoreData)
    {
        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        RecordApi::ParameterToRecordLargeData param;
#if (SCE_PROSPERO_SDK_VERSION > 0x08000000u) || (SCE_ORBIS_SDK_VERSION > 0x11000000u)
        param.initialize(libContextPtr, scoreData.boardId, PsnLeaderboards::RecordApi::ParameterToRecordLargeData::XPsnAtomicOperation::kEnd, scoreData.operationId);
#else
        param.initialize(libContextPtr, scoreData.boardId);
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        param.setxPsnAtomicOperation(PsnLeaderboards::RecordApi::ParameterToRecordLargeData::XPsnAtomicOperation::kEnd);
#else
        param.setxPsnAtomicOperation(PsnLeaderboards::RecordApi::ParameterToRecordLargeData::XPsnAtomicOperation::END);
#endif
        ret = param.setxPsnAtomicOperationId(scoreData.operationId);
#endif
        if (ret < 0)
        {
            return ret;
        }

#if (SCE_PROSPERO_SDK_VERSION > 0x08000000u) || (SCE_ORBIS_SDK_VERSION > 0x11000000u)
        Common::UpStreamTransaction<Common::IntrusivePtr<RecordLargeDataResponseBody>> transaction;
#else
        Common::UpStreamTransaction<Common::IntrusivePtr<RecordLargeDataResponseBody>, Common::IntrusivePtr<RecordApi::RecordLargeDataResponseHeaders> > transaction;
#endif
        transaction.start(libContextPtr, scoreData.largeDataSize);

        // API call
        ret = RecordApi::recordLargeData(scoreData.userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        ret = transaction.sendData(scoreData.largeData, scoreData.largeDataSize);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            return ret;
        }

        param.terminate();
        transaction.finish();

        return 0;
    }
}
