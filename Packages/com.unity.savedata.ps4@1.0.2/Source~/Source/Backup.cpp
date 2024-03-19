
#include "Backup.h"


namespace SaveData
{
	PRX_EXPORT void PrxSaveDataBackup(BackupRequest* managedRequest, APIResult* result)
	{
		Backups::Backup(managedRequest, result);
	}

	PRX_EXPORT void PrxSaveDataCheckBackup(CheckBackupRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Backups::CheckBackup(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataRestoreBackup(RestoreBackupRequest* managedRequest, APIResult* result)
	{
		Backups::RestoreBackup(managedRequest, result);
	}

	void BackupRequest::CopyTo(SceSaveDataBackup &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
	}

	void CheckBackupRequest::CopyTo(SceSaveDataCheckBackupData &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
	}

	void RestoreBackupRequest::CopyTo(SceSaveDataRestoreBackupData &destination, SceSaveDataDirName& sceDirName)
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

	void Backups::CheckBackup(CheckBackupRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataCheckBackupData checkBackup;
		SceSaveDataDirName dirName;

		managedRequest->CopyTo(checkBackup, dirName);

		SceSaveDataIcon icon;
		memset(&icon, 0x00, sizeof(icon));

		SceSaveDataParam params;
		memset(&params, 0x00, sizeof(params));
		
		if (managedRequest->includeParams == true)
		{		
			checkBackup.param = &params;
		}

		if (managedRequest->includeIcon == true)
		{
			Core::InitIconForReading(icon);
			checkBackup.icon = &icon;
		}

		int ret = sceSaveDataCheckBackupData(&checkBackup);

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		bool valid = (ret == 0);

		buffer.WriteBool(valid);

		if (valid == true)
		{
			bool wasIconFound = true;
			if (managedRequest->includeIcon == false || icon.dataSize == 0)
			{
				// an icon was requested but it doesn't have one
				wasIconFound = false;
			}

			// Write the info and icon if required
			buffer.WriteBool(managedRequest->includeParams);
			buffer.WriteBool(wasIconFound);

			if (managedRequest->includeParams == true)
			{
				Core::WriteToBuffer(params, buffer);
			}

			if (wasIconFound == true)
			{
				PNGWriter::WriteToBuffer(icon.buf, (Int32)icon.dataSize, buffer);  ///< The icon retrieved in case it was explicitely specified in the request		
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

	void Backups::RestoreBackup(RestoreBackupRequest* managedRequest, APIResult* result)
	{
		SceSaveDataRestoreBackupData restore;
		SceSaveDataDirName dirName;

		managedRequest->CopyTo(restore, dirName);

		int ret = sceSaveDataRestoreBackupData(&restore);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}
}