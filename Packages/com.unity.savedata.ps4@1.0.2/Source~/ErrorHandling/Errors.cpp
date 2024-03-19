
#include "Errors.h"
#include <stdio.h>

namespace SaveData
{


	void APIResult::SetResult(APIResult* result, APIResultTypes type)
	{
		result->apiResult = type;
		result->message = "";
		result->filename = "";
		result->lineNumber = 0;
		result->sceErrorCode = 0;
	}

	void APIResult::SetResult(APIResult* result, APIResultTypes type, char const * message, char const * filename, int lineNumber)
	{
		result->apiResult = type;
		result->message = message;
		result->filename = filename;
		result->lineNumber = lineNumber;
		result->sceErrorCode = 0;
	}

	void APIResult::SetSceResult(APIResult* result, APIResultTypes type, int sceErrorCode, char const * filename, int lineNumber)
	{
		char const* message = LookupSceErrorCode(sceErrorCode);

		result->apiResult = type;

		if ( message == 0) // Null
		{
			result->message = "";
		}
		else
		{
			result->message = message;
		}
		result->filename = filename;
		result->lineNumber = lineNumber;
		result->sceErrorCode = sceErrorCode;
	}
}