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

	class Backups
	{
	public:

		static void Backup(BackupRequest* managedRequest, APIResult* result);
	};
}

#endif	//_BACKUP_H

