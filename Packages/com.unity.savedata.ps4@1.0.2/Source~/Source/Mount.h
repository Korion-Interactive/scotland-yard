#ifndef _MOUNT_H
#define _MOUNT_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	struct MountPointManaged
	{
	public:
		char data[SCE_SAVE_DATA_MOUNT_POINT_DATA_MAXSIZE];

		void CopyTo(SceSaveDataMountPoint &destination);
	};

	class MountRequest : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;
		UInt64 blocks;
		SceSaveDataMountMode mountMode; // uint32

		void CopyTo(SceSaveDataMount2 &destination, SceSaveDataDirName& sceDirName);
	};

	class UnmountRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
		bool backup;
	};

	class GetMountInfoRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
	};

	class GetMountParamsRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
	};

	class SaveIconRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
		char iconPath[SAVE_DATA_FILEPATH_LENGTH];

		void *pngData;
		UInt64 pngDataSize;
	};

	class LoadIconRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
	};

	//struct ParamsManaged
	//{
	//public:
	//	char title[SCE_SAVE_DATA_TITLE_MAXSIZE];
	//	char subTitle[SCE_SAVE_DATA_SUBTITLE_MAXSIZE];
	//	char detail[SCE_SAVE_DATA_DETAIL_MAXSIZE];
	//	UInt32 userParam;
	//	UInt32 time;

	//	void CopyTo(SceSaveDataParam &destination);
	//};

	class SetMountParamsRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;

		char title[SCE_SAVE_DATA_TITLE_MAXSIZE];
		char subTitle[SCE_SAVE_DATA_SUBTITLE_MAXSIZE];
		char detail[SCE_SAVE_DATA_DETAIL_MAXSIZE];
		UInt32 userParam;

		void CopyTo(SceSaveDataParam &destination);
	};

	class Mounting
	{
	public:

		static void Mount(MountRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void Unmount(UnmountRequest* managedRequest, APIResult* result);
		static void GetMountInfo(GetMountInfoRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void GetMountParams(GetMountParamsRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void SetMountParams(SetMountParamsRequest* managedRequest, APIResult* result);
		static void SaveIcon(SaveIconRequest* managedRequest, APIResult* result);
		static void LoadIcon(LoadIconRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
	};
}

#endif	//_MOUNT_H

