#ifndef _UTILS_H
#define _UTILS_H

//#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class Utils
	{
	public:

		static int32_t LoadFile(const char *mountPoint, const char *fileName, uint8_t** loadeddata, __int64_t *datasize, bool dialogEnabled);
		static int32_t LoadFile(const char *path, uint8_t** data, size_t *datasize);
	};
}

#endif