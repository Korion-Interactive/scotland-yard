#pragma once

#include "../Includes/SonyCommonIncludes.h"
#include "../Includes/CommonTypes.h"

namespace SaveData
{
	// Lookup an SCE error and return a string version.
	const char* LookupSceErrorCode(int errorCode);

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
	};
}

#define SUCCESS_RESULT(result) (SaveData::APIResult::SetResult(result, SaveData::APIResultTypes::Success))

#define WARNING_RESULT(result, message) (SaveData::APIResult::SetResult(result,  SaveData::APIResultTypes::Warning, message, __FILE__, __LINE__))
#define ERROR_RESULT(result, message) (SaveData::APIResult::SetResult(result,  SaveData::APIResultTypes::Error, message, __FILE__, __LINE__))

#define SCE_ERROR_RESULT(result, sceErrorCode) (SaveData::APIResult::SetSceResult(result, SaveData::APIResultTypes::Error, sceErrorCode, __FILE__, __LINE__))

