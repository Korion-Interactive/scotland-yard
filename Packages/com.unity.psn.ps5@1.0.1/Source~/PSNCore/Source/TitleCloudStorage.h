#pragma once
#include "SharedCoreIncludes.h"
#include <map>
#include <np_cppwebapi.h>
#include "WebApi.h"

namespace TCS = sce::Np::CppWebApi::TitleCloudStorage::V1;

namespace psn
{
    class TitleCloudStorage
    {
    public:

        enum Methods
        {
            AddAndGetVariable = 0x1400001u,    // addAndGetVariable
            SetVariableWithConditions = 0x1400002u,    // setVariableWithConditions
            GetMultiVariablesBySlot = 0x1400003u, // getMultiVariablesBySlot

            SetMultiVariablesByUser = 0x1400004u, // getMultiVariablesBySlot
            GetMultiVariablesByUser = 0x1400005u, // getMultiVariablesBySlot
            DeleteMultiVariablesByUser = 0x1400006u, // getMultiVariablesBySlot

            UploadData = 0x1400007u,
            DownloadData = 0x1400008u,
            DeleteMultiDataBySlot = 0x140009u,
            DeleteMultiDataByUser = 0x140010u,
            GetMultiDataStatusesBySlot = 0x140011u,
            GetMultiDataStatusesByUser = 0x140012u,
        };

        static void RegisterMethods();

        static void AddAndGetVariableImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void SetVariableWithConditionsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetMultiVariablesBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void SetMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeleteMultiVariablesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void UploadDataImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DownloadDataImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeleteMultiDataBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void DeleteMultiDataByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetMultiDataStatusesBySlotImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetMultiDataStatusesByUserImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        struct UploadParams
        {
            WebApiUserContext* userWebCtx;
            const char* accountId;
            Int32 slotId;

            Int32 dataSize;
            void* data;

            size_t infoSize;
            void* info;

            bool isServiceLabelSet;
            Int32 serviceLabel;

            bool isComparedLastUpdatedDateTimeSet;
            SceRtcTick compareTick;

            bool isComparedLastUpdatedUserAccountId;
            char* compareAccountIdStr;
        };

        static int UploadDataAndSetInfo(UploadParams* uploadParams); // WebApiUserContext* userWebCtx, const char* accountId, Int32 slotId, Int32 dataSize, void* data, size_t infoSize, void* info, bool isServiceLabelSet, Int32 serviceLabel);

        static void WriteVariable(BinaryWriter& writer, const TCS::Variable* variable);
        static void WriteVariable(BinaryWriter& writer, const TCS::IdempotentVariable* variable);

        static void WriteDataStatus(BinaryWriter& writer, const TCS::DataStatus* dataStatus);
    };
}
