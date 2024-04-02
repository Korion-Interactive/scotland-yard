#ifndef _DELETE_H
#define _DELETE_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	class DeleteRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;

		void CopyTo(SceSaveDataDelete &destination, SceSaveDataDirName& sceDirName);
	};

	class Deleting
	{
	public:

		static void Delete(DeleteRequest* managedRequest, APIResult* result);
	};
}

#endif	//_DELETE_H

