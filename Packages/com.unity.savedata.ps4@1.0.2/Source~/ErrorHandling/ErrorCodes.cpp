
#include "../Includes/PluginCommonIncludes.h"

#define CASE_ERROR_TO_STRING(errCode) case (int)errCode: errorString = #errCode; break;

namespace SaveData
{
	const char* LookupSceErrorCode(int errorCode)
	{
		const char* errorString = NULL;

		switch(errorCode)
		{
			//CASE_ERROR_TO_STRING(SCE_NP_MATCHING2_SERVER_ERROR_ROOM_INCONSISTENCY)

		default:
			break;
		}

		return errorString;
	}

}	// namespace UnityPlugin
