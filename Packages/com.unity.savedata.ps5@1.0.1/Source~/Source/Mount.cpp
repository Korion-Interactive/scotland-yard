
#include "Mount.h"
#include "Utils.h"
#include "Transactions.h"

namespace SaveData
{
	PRX_EXPORT void PrxSaveDataMount(MountRequest* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Mounting::Mount(managedRequest, outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataMountPS4(MountPS4Request* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
#if SCE_PROSPERO_SDK_VERSION>0x03000000u
		Mounting::MountPS4(managedRequest, outBuffer, result);
#else
		SCE_ERROR_RESULT(result, 0x80020013);
		return;
#endif
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

	void MountRequest::CopyTo(SceSaveDataMount3 &destination, SceSaveDataDirName& sceDirName)
	{
		dirName.CopyTo(sceDirName);

		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
		destination.blocks = blocks;
		destination.systemBlocks = systemBlocks;
		destination.mountMode = mountMode;
		destination.resource = SCE_SAVE_DATA_TRANSACTION_RESOURCE_ID_INVALID;
	}

	void MountPS4Request::CopyTo(SceSaveDataTransferringMount& destination, SceSaveDataDirName& sceDirName, SceSaveDataTitleId& sceTitleId, SceSaveDataFingerprint& sceFingerprint)
	{
		dirName.CopyTo(sceDirName);
		fingerprint.CopyTo(sceFingerprint);
		titleId.CopyTo(sceTitleId);


		memset(&destination, 0x00, sizeof(destination));
		destination.userId = userId;
		destination.dirName = &sceDirName;
		destination.fingerprint = &sceFingerprint;
		destination.titleId = &sceTitleId;


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

		SceSaveDataMount3 mount;

		managedRequest->CopyTo(mount, dirName);

		// Create the transaction id if the file is read/write
		Transaction* transId = NULL;

		if ((managedRequest->mountMode & SCE_SAVE_DATA_MOUNT_MODE_RDWR) != 0)
		{
			transId = Transactions::CreateTransactionId(SCE_SAVE_DATA_TRANSACTION_RESOURCE_MIN_SIZE);

			mount.resource = transId->m_TransactionId;
		}

		SceSaveDataMountResult mountResult;
		memset(&mountResult, 0x00, sizeof(mountResult));

        // SCE_SAVE_DATA_MOUNT_MODE_RDWR is reserved for future feature expansions. To access the save data directory in the read/write-enabled mode,
		// call sceSaveDataPrepare() after mounting with SCE_SAVE_DATA_MOUNT_MODE_RDONLY.
		// Remove read/write flag.
		mount.mountMode &= ~SCE_SAVE_DATA_MOUNT_MODE_RDWR;
		mount.mountMode |= SCE_SAVE_DATA_MOUNT_MODE_RDONLY;

		int ret = sceSaveDataMount3(&mount, &mountResult);

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
			Transactions::DeleteTransactionId(transId);
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		if (transId != NULL)
		{
			// A transaction id has been used to mount this save data. Record this for later usage.
			Transactions::RecordMountedTransaction(transId, mountResult.mountPoint);

			// Prepare the save data for write operations
			SceSaveDataPrepareParam prepareParam;
			memset(&prepareParam, 0x00, sizeof(prepareParam));
			prepareParam.resource = transId->m_TransactionId;
			prepareParam.prepareMode = 0; /* Always 0*/

			ret = sceSaveDataPrepare(&mountResult.mountPoint, &prepareParam);
			if (ret < 0)
			{
				SCE_ERROR_RESULT(result, ret);
				return;
			}
		}

		SUCCESS_RESULT(result);
	}

#if SCE_PROSPERO_SDK_VERSION>0x03000000u
	void Mounting::MountPS4(MountPS4Request* managedRequest, MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataDirName dirName;
		SceSaveDataFingerprint fingerprint;
		SceSaveDataTitleId titleId;
		memset(&dirName, 0, sizeof(SceSaveDataDirName));
		memset(&fingerprint, 0, sizeof(SceSaveDataFingerprint));
		memset(&titleId, 0, sizeof(SceSaveDataTitleId));

		SceSaveDataTransferringMount mount;

		managedRequest->CopyTo(mount, dirName, titleId, fingerprint );

		// Create the transaction id if the file is read/write
		Transaction* transId = NULL;



		SceSaveDataMountResult mountResult;
		memset(&mountResult, 0x00, sizeof(mountResult));


		int ret = sceSaveDataTransferringMountPs4(&mount, &mountResult);

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
			Transactions::DeleteTransactionId(transId);
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		if (transId != NULL)
		{
			// A transaction id has been used to mount this save data. Record this for later usage.
			Transactions::RecordMountedTransaction(transId, mountResult.mountPoint);

			// Prepare the save data for write operations
			SceSaveDataPrepareParam prepareParam;
			memset(&prepareParam, 0x00, sizeof(prepareParam));
			prepareParam.resource = transId->m_TransactionId;
			prepareParam.prepareMode = 0; /* Always 0*/

			ret = sceSaveDataPrepare(&mountResult.mountPoint, &prepareParam);
			if (ret < 0)
			{
				SCE_ERROR_RESULT(result, ret);
				return;
			}
		}

		SUCCESS_RESULT(result);
	}
#endif
	void Mounting::Unmount(UnmountRequest* managedRequest, APIResult* result)
	{
		SceSaveDataMountPoint mountPoint;

		managedRequest->mountPoint.CopyTo(mountPoint);

		Transaction* transId = Transactions::FindTransaction(mountPoint);

		int ret = 0;

		if (transId != NULL)
		{
			// Commit the update for the read/write mountpoint
			SceSaveDataCommitParam param;
			memset(&param, 0x0, sizeof(param));
			param.resource = transId->m_TransactionId; // Transaction resource specified when mounting save data

			ret = sceSaveDataCommit(&param);

			if (ret < 0)
			{
				SCE_ERROR_RESULT(result, ret);
				return;
			}
		}

		ret = sceSaveDataUmount2(SCE_SAVE_DATA_UMOUNT_MODE_DEFAULT, &mountPoint);

		// Delete the transaction after unmounting
		int ret2 = Transactions::RemoveTransaction(transId);
		if (ret2 < 0)
		{
			// Error from removing transaction. Could this hide any error from sceSaveDataUmount2?
			SCE_ERROR_RESULT(result, ret2);
			return;
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
