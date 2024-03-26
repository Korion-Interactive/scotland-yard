#ifndef _SEARCH_H
#define _SEARCH_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class DirNameSearchRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;
		UInt32 maxSaveDataCount;
		SceSaveDataSortKey key;
		SceSaveDataSortOrder order;

		bool includeParams;
		bool includeBlockInfo;

		void CopyTo(SceSaveDataDirNameSearchCond &destination, SceSaveDataDirName& sceDirName);
	};

	class Searching
	{
	public:

		static void DirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
	};
}

#endif	//_SEARCH_H

