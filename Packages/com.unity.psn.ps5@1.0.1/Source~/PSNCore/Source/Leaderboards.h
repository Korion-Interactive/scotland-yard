#pragma once
#include "SharedCoreIncludes.h"
#include <map>
#include <np_cppwebapi.h>
#include "WebApi.h"

namespace sceCppWebApi = sce::Np::CppWebApi::Common;
namespace PsnLeaderboards = sce::Np::CppWebApi::Leaderboards::V1;

using namespace sceCppWebApi;
using namespace sce::Np::CppWebApi;
using namespace PsnLeaderboards;

namespace psn
{
    class Leaderboards
    {
    public:

        enum Methods
        {
            GetBoardDefinition = 0x1100001u,
            RecordScore = 0x1100002u,
            GetRanking = 0x1100003u,
            GetLargeDateByObjectId = 0x1100004u,
        };

        static void RegisterMethods();

        static void Initialize();

        static void GetBoardDefinitionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void RecordScoreImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetRankingImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetLargeDataByObjectIdImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        struct ScoreData
        {
            Int32 userId;
            WebApiUserContext* userCtx;
            int boardId;
            int64_t score;
            bool waitsForData;
            bool needsTmpRank;
            int32_t pcId;
            SceNpServiceLabel serviceLabel;
            int64_t smallDataSize;
            void* smallDataPtr;
            char* objectId;
            char* comment;
            SceRtcTick comparedDateTime;

            Int32 largeDataSize;
            void* largeData;

            bool isServiceLabelSet;
            bool isObjectIdSet;
            bool isPcIdSet;
            bool isSmallDataSet;
            bool isCommentSet;
            bool isComparedDateTimeSet;

            bool tempRanksSet;
            Int32 tmpRankResult;
            Int32 tmpSerialRankResult;

            char operationId[40]; // UUID for atomic recording sessions (large data)
        };

        static int RecordLeaderboardScore(ScoreData& scoreData);
        static int RecordLargeData(ScoreData& scoreData);
    };
}
