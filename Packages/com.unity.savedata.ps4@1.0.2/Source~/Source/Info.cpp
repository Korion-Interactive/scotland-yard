
#include "Info.h"


namespace SaveData
{
	//PRX_EXPORT void PrxSaveDataDelete(DeleteRequest* managedRequest, APIResult* result)
	//{
	//	Deleting::Delete(managedRequest, result);
	//}

	//void DeleteRequest::CopyTo(SceSaveDataDelete &destination, SceSaveDataDirName& sceDirName)
	//{
	//	dirName.CopyTo(sceDirName);

	//	memset(&destination, 0x00, sizeof(destination));
	//	destination.userId = userId;
	//	destination.dirName = &sceDirName;
	//}

	//void Deleting::Delete(DeleteRequest* managedRequest, APIResult* result)
	//{
	//	SceSaveDataDelete del;
	//	SceSaveDataDirName dirName;

	//	managedRequest->CopyTo(del, dirName);

	//	int ret = sceSaveDataDelete(&del);

	//	if (ret < 0)
	//	{
	//		SCE_ERROR_RESULT(result, ret);
	//		return;
	//	}

	//	SUCCESS_RESULT(result);
	//}

}