
#include "Search.h"


namespace SaveData
{
	PRX_EXPORT void PrxSaveDataDirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Searching::DirNameSearch(managedRequest, outBuffer, result);
	}

	void DirNameSearchRequest::CopyTo(SceSaveDataDirNameSearchCond& destination, SceSaveDataDirName& sceDirName, SceSaveDataTitleId& sceTitleId)
	{
		dirName.CopyTo(sceDirName);
		titleId.CopyTo(sceTitleId);

		memset(&destination, 0x00, sizeof(destination));

		destination.userId = userId;

		if (strlen(dirName.data) > 0)
		{
			destination.dirName = &sceDirName;
		}
		else
		{
			destination.dirName = NULL;
		}

		if (strlen(titleId.data) > 0)
		{
			destination.titleId = &sceTitleId;
		}
		else
		{
			destination.titleId = NULL;
		}
		destination.key = key;
		destination.order = order;
	}

	void Searching::DirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataDirName dirName;
		SceSaveDataTitleId titleId;
		SceSaveDataDirNameSearchCond cond;

		memset(&dirName, 0, sizeof(SceSaveDataDirName));
		memset(&titleId, 0, sizeof(SceSaveDataTitleId));

		managedRequest->CopyTo(cond, dirName, titleId);

		SceSaveDataDirNameSearchResult searchResult;
		memset(&searchResult, 0x00, sizeof(searchResult));

		searchResult.dirNames = Core::GetTempDirNamesArray(); // new SceSaveDataDirName[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];
		searchResult.dirNamesNum = managedRequest->maxSaveDataCount;

		if (managedRequest->includeParams == true)
		{
			searchResult.params = Core::GetTempParamsArray();
		}

		if (managedRequest->includeBlockInfo == true)
		{
			searchResult.infos = Core::GetTempSearchInfosArray();
		}

		int ret = 0;
		if (managedRequest->searchPS4)
		{
#if SCE_PROSPERO_SDK_VERSION>0x03000000u
			ret = sceSaveDataDirNameSearchPs4(&cond, &searchResult);
#else
			ret = 0x80020013;	// not supported
#endif
		}
		else
		{
			ret = sceSaveDataDirNameSearch(&cond, &searchResult);

		}



		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		// Write the save data directories
		buffer.WriteUInt32(searchResult.setNum);
		buffer.WriteBool(managedRequest->includeParams);
		buffer.WriteBool(managedRequest->includeBlockInfo);

		for (int i = 0; i < searchResult.setNum; i++)
		{
			// Write directory name, params and info if they exists
			Core::WriteToBuffer(searchResult.dirNames[i], buffer);

			if (searchResult.params != NULL)
			{
				Core::WriteToBuffer(searchResult.params[i], buffer);
			}

			if (searchResult.infos != NULL)
			{
				Core::WriteToBuffer(searchResult.infos[i], buffer);
			}
		}

		//outBuffer
		buffer.FinishResponseWrite();
		buffer.CopyTo(outBuffer);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

}