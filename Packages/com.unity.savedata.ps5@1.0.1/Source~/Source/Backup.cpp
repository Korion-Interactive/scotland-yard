
#include "Backup.h"


namespace SaveData
{
	PRX_EXPORT void PrxSaveDataBackup(BackupRequest* managedRequest, APIResult* result)
	{
		Backups::Backup(managedRequest, result);
	}

	void BackupRequest::CopyTo(SceSaveDataBackup &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
	}

	void Backups::Backup(BackupRequest* managedRequest, APIResult* result)
	{
		SceSaveDataBackup del;
		SceSaveDataDirName dirName;

		managedRequest->CopyTo(del, dirName);

		int ret = sceSaveDataBackup(&del);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

}