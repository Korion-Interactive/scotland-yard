#ifndef _SEARCH_H
#define _SEARCH_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class DirNameSearchRequest : public RequestBaseManaged
	{
	public:
		TitleIdManaged titleId;
		DirNameManaged dirName;
		UInt32 maxSaveDataCount;
		SceSaveDataSortKey key;
		SceSaveDataSortOrder order;

		bool includeParams;
		bool includeBlockInfo;
		bool searchPS4;

		void CopyTo(SceSaveDataDirNameSearchCond &destination, SceSaveDataDirName& sceDirName, SceSaveDataTitleId& sceTitleId);
	};

	class Searching
	{
	public:

		static void DirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
	};
}

#endif	//_SEARCH_H

