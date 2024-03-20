#pragma once

#include "SonyCommonIncludes.h"
#include "CommonTypes.h"

namespace psn
{
    enum APIResultTypes
    {
        Success = 0,
        Warning = 1,
        Error = 2,
    };

    struct APIResult
    {
    public:
        APIResultTypes apiResult;
        char const * message;
        char const * filename;
        Int32 lineNumber;
        Int32 sceErrorCode;

        static void SetResult(APIResult* result, APIResultTypes type);
        static void SetResult(APIResult* result, APIResultTypes type, char const * message, char const * filename, Int32 lineNumber);
        static void SetSceResult(APIResult* result, APIResultTypes type, Int32 sceErrorCode, char const * filename, Int32 lineNumber);
        static void SetSceResultMessage(APIResult* result, APIResultTypes type, Int32 sceErrorCode, char const* message, char const* filename, Int32 lineNumber);
    };
}

#define SUCCESS_RESULT(result) (psn::APIResult::SetResult(result, psn::APIResultTypes::Success))

#define WARNING_RESULT(result, message) (psn::APIResult::SetResult(result,  psn::APIResultTypes::Warning, message, __FILE__, __LINE__))
#define ERROR_RESULT(result, message) (psn::APIResult::SetResult(result,  psn::APIResultTypes::Error, message, __FILE__, __LINE__))

#define SCE_ERROR_RESULT(result, sceErrorCode) (psn::APIResult::SetSceResult(result, psn::APIResultTypes::Error, sceErrorCode, __FILE__, __LINE__))
#define SCE_ERROR_RESULT_MSG(result, message, sceErrorCode) (psn::APIResult::SetSceResultMessage(result, psn::APIResultTypes::Error, sceErrorCode, message, __FILE__, __LINE__))
