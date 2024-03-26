
#include "Mount.h"
#include "Utils.h"

namespace SaveData
{
	PRX_EXPORT void PrxSaveDataMount(MountRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Mounting::Mount(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataUnmount(UnmountRequest* managedRequest, APIResult* result)
	{
		Mounting::Unmount(managedRequest, result);
	}

	PRX_EXPORT void PrxSaveDataGetMountInfo(GetMountInfoRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Mounting::GetMountInfo(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataGetMountParams(GetMountParamsRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Mounting::GetMountParams(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataSetMountParams(SetMountParamsRequest* managedRequest, APIResult* result)
	{
		Mounting::SetMountParams(managedRequest, result);
	}

	PRX_EXPORT void PrxSaveDataSaveIcon(SaveIconRequest* managedRequest, APIResult* result)
	{
		Mounting::SaveIcon(managedRequest, result);
	}

	PRX_EXPORT void PrxSaveDataLoadIcon(LoadIconRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Mounting::LoadIcon(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataGetIconSize(void *pngData, Int32* width, Int32* height)
	{
		*width = 0;
		*height = 0;

		PNGWriter::GetPNGSizes(pngData, *width, *height);
	}

	void MountPointManaged::CopyTo(SceSaveDataMountPoint &destination)
	{
		memcpy_s(destination.data, SCE_SAVE_DATA_MOUNT_POINT_DATA_MAXSIZE, data, SCE_SAVE_DATA_MOUNT_POINT_DATA_MAXSIZE);
	}

	void MountRequest::CopyTo(SceSaveDataMount2 &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
		destination.blocks = blocks;
		destination.mountMode = mountMode;
	}

	void SetMountParamsRequest::CopyTo(SceSaveDataParam &destination)
	{
		memset(&destination, 0x00, sizeof(destination));

		memcpy_s(destination.title, SCE_SAVE_DATA_TITLE_MAXSIZE, title, SCE_SAVE_DATA_TITLE_MAXSIZE);
		memcpy_s(destination.subTitle, SCE_SAVE_DATA_SUBTITLE_MAXSIZE, subTitle, SCE_SAVE_DATA_SUBTITLE_MAXSIZE);
		memcpy_s(destination.detail, SCE_SAVE_DATA_DETAIL_MAXSIZE, detail, SCE_SAVE_DATA_DETAIL_MAXSIZE);

		destination.userParam = userParam;
	}

	void Mounting::Mount(MountRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataDirName dirName;

		SceSaveDataMount2 mount;

		managedRequest->CopyTo(mount, dirName);

		SceSaveDataMountResult mountResult;
		memset(&mountResult, 0x00, sizeof(mountResult));

		int ret = sceSaveDataMount2(&mount, &mountResult);

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		buffer.WriteString(mountResult.mountPoint.data);

		buffer.WriteUInt64(mountResult.requiredBlocks);
		buffer.WriteUInt32(mountResult.mountStatus);

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

	void Mounting::Unmount(UnmountRequest* managedRequest, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		managedRequest->mountPoint.CopyTo(mountPoint);

		int ret = 0;

		if (managedRequest->backup == false)
		{
			ret = sceSaveDataUmount(&mountPoint);
		}
		else
		{
			ret = sceSaveDataUmountWithBackup(&mountPoint);
		}

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Mounting::GetMountInfo(GetMountInfoRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		managedRequest->mountPoint.CopyTo(mountPoint);

		SceSaveDataMountInfo mountInfo;
		memset(&mountInfo, 0x00, sizeof(mountInfo));

		int ret = sceSaveDataGetMountInfo(&mountPoint, &mountInfo);

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		Core::WriteToBuffer(mountInfo, buffer);

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

	void Mounting::GetMountParams(GetMountParamsRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		managedRequest->mountPoint.CopyTo(mountPoint);

		size_t gotSize = 0;
		SceSaveDataParam params;
		memset(&params, 0x00, sizeof(params));

		int ret = sceSaveDataGetParam(&mountPoint, SCE_SAVE_DATA_PARAM_TYPE_ALL, &params, sizeof(SceSaveDataParam), &gotSize);

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		Core::WriteToBuffer(params, buffer);

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

	void Mounting::SetMountParams(SetMountParamsRequest* managedRequest, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		SceSaveDataParam params;

		managedRequest->mountPoint.CopyTo(mountPoint);
		managedRequest->CopyTo(params);

		int ret = sceSaveDataSetParam(&mountPoint, SCE_SAVE_DATA_PARAM_TYPE_ALL, &params, sizeof(SceSaveDataParam));

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Mounting::SaveIcon(SaveIconRequest* managedRequest, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		managedRequest->mountPoint.CopyTo(mountPoint);

		uint8_t *data = NULL;
		size_t data_size = 0;

		// Load the png data if required
		int ret = 0;

		if (managedRequest->pngDataSize == 0)
		{
			/*ret = Utils::LoadFile(managedRequest->iconPath, &data, &data_size);
			if (ret < 0)
			{
				SCE_ERROR_RESULT(result, ret);
				return;
			}*/
			SCE_ERROR_RESULT(result, SCE_SAVE_DATA_ERROR_PARAMETER);
			return;
		}
		else
		{
			data = (uint8_t*)managedRequest->pngData;
			data_size = managedRequest->pngDataSize;
		}

		SceSaveDataIcon icon;
		memset(&icon, 0x00, sizeof(icon));
		icon.buf = data;
		icon.bufSize = data_size;
		icon.dataSize = data_size;

		ret = sceSaveDataSaveIcon(&mountPoint, &icon);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Mounting::LoadIcon(LoadIconRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		SceSaveDataIcon icon;
		Core::InitIconForReading(icon);

		managedRequest->mountPoint.CopyTo(mountPoint);

		int ret = sceSaveDataLoadIcon(&mountPoint, &icon);

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		if (ret < 0)
		{
			buffer.WriteBool(false);
		}
		else
		{
			buffer.WriteBool(true);
			PNGWriter::WriteToBuffer(icon.buf, (Int32)icon.dataSize, buffer);  ///< The icon retrieved in case it was explicitely specified in the request		
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