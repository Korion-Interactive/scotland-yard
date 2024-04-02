
#include "Search.h"


namespace SaveData
{
	PRX_EXPORT void PrxSaveDataDirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Searching::DirNameSearch(managedRequest, outBuffer, result);
	}

	void DirNameSearchRequest::CopyTo(SceSaveDataDirNameSearchCond &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

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

		destination.key = key;
		destination.order = order;
	}

	void Searching::DirNameSearch(DirNameSearchRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataDirName dirName;
		SceSaveDataDirNameSearchCond cond;

		managedRequest->CopyTo(cond, dirName);

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

		int ret = sceSaveDataDirNameSearch(&cond, &searchResult);

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