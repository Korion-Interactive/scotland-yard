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
		UInt64 systemBlocks;
		SceSaveDataMountMode mountMode; // uint32

		void CopyTo(SceSaveDataMount3 &destination, SceSaveDataDirName& sceDirName);
	};

	class MountPS4Request : public RequestBaseManaged
	{
	public:
		DirNameManaged dirName;
		TitleIdManaged titleId;
		FingerprintManaged fingerprint;

		void CopyTo(SceSaveDataTransferringMount &destination, SceSaveDataDirName& sceDirName, SceSaveDataTitleId& sceTitleId, SceSaveDataFingerprint& sceFingerprint);
	};


	class UnmountRequest : public RequestBaseManaged
	{
	public:
		MountPointManaged mountPoint;
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
		char iconPath[SCE_SAVE_DATA_ICON_PATH_MAXSIZE];

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
#if SCE_PROSPERO_SDK_VERSION>0x03000000u
		static void MountPS4(MountPS4Request* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
#endif
		static void Unmount(UnmountRequest* managedRequest, APIResult* result);
		static void GetMountInfo(GetMountInfoRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void GetMountParams(GetMountParamsRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
		static void SetMountParams(SetMountParamsRequest* managedRequest, APIResult* result);
		static void SaveIcon(SaveIconRequest* managedRequest, APIResult* result);
		static void LoadIcon(LoadIconRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result);
	};
}

#endif	//_MOUNT_H

