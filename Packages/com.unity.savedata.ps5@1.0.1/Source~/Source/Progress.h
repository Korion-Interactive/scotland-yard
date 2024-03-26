#ifndef _PROGRESS_H
#define _PROGRESS_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class Progress
	{
	public:
		static void ClearProgress(APIResult* result);
		static float GetProgress(APIResult* result);
	};
}

#endif	//_PROGRESS_H

