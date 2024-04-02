#ifndef _BACKUP_H
#define _BACKUP_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class BackupRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;

		void CopyTo(SceSaveDataBackup &destination, SceSaveDataDirName& sceDirName);
	};

	class CheckBackupRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;
		bool includeParams;
		bool includeIcon;

		void CopyTo(SceSaveDataCheckBackupData  &destination, SceSaveDataDirName& sceDirName);
	};

	class RestoreBackupRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;

		void CopyTo(SceSaveDataRestoreBackupData &destination, SceSaveDataDirName& sceDirName);
	};

	class Backups
	{
	public:

		static void Backup(BackupRequest* managedRequest, APIResult* result);
		static void CheckBackup(CheckBackupRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void RestoreBackup(RestoreBackupRequest* managedRequest, APIResult* result);
	};
}

#endif	//_BACKUP_H

