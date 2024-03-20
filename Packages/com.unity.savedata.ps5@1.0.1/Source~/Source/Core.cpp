
#include "Core.h"


namespace SaveData
{

	void DirNameManaged::CopyTo(SceSaveDataDirName &destination)
	{
		memcpy_s(destination.data, SCE_SAVE_DATA_DIRNAME_DATA_MAXSIZE, data, SCE_SAVE_DATA_DIRNAME_DATA_MAXSIZE);
	}

	void FingerprintManaged::CopyTo(SceSaveDataFingerprint& destination)
	{
		memcpy_s(destination.data, SCE_SAVE_DATA_FINGERPRINT_DATA_SIZE, data, SCE_SAVE_DATA_FINGERPRINT_DATA_SIZE);
	}

	void TitleIdManaged::CopyTo(SceSaveDataTitleId& destination)
	{
		memcpy_s(destination.data, SCE_SAVE_DATA_TITLE_ID_DATA_SIZE, data, SCE_SAVE_DATA_TITLE_ID_DATA_SIZE);
	}

	static char sIconBuffer[SCE_SAVE_DATA_ICON_FILE_MAXSIZE2];
	static SceSaveDataDirName sDirNames[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];
	static SceSaveDataDirName sDialogDirNames[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];
	static SceSaveDataParam sParams[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];
	static SceSaveDataSearchInfo sInfos[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];

	SceSaveDataDirName* Core::GetTempDirNamesArray()
	{
		return sDirNames;
	}

	SceSaveDataDirName* Core::GetTempDialogDirNamesArray()
	{
		return sDialogDirNames;
	}

	SceSaveDataParam* Core::GetTempParamsArray()
	{
		return sParams;
	}

	SceSaveDataSearchInfo* Core::GetTempSearchInfosArray()
	{
		return sInfos;
	}

	void Core::InitIconForReading(SceSaveDataIcon& icon)
	{
		memset(sIconBuffer, 0xAA, SCE_SAVE_DATA_ICON_FILE_MAXSIZE2);

		memset(&icon, 0x00, sizeof(icon));
		icon.buf = sIconBuffer;
		icon.bufSize = SCE_SAVE_DATA_ICON_FILE_MAXSIZE2;
	}

	void Core::WriteToBuffer(const SceSaveDataMountInfo& info, MemoryBuffer& buffer)
	{
		buffer.WriteUInt64(info.blocks);
		buffer.WriteUInt64(info.freeBlocks);
	}

	void Core::WriteToBuffer(const SceSaveDataSearchInfo& info, MemoryBuffer& buffer)
	{
		buffer.WriteUInt64(info.blocks);
		buffer.WriteUInt64(info.freeBlocks);
	}
		
	void Core::WriteToBuffer(const SceSaveDataParam& params, MemoryBuffer& buffer)
	{
		buffer.WriteString(params.title);
		buffer.WriteString(params.subTitle);
		buffer.WriteString(params.detail);
		buffer.WriteUInt32(params.userParam);
		buffer.WriteInt64(params.mtime);
	}

	void Core::WriteToBuffer(const SceSaveDataDirName& dirName, MemoryBuffer& buffer)
	{
		buffer.WriteString(dirName.data);
	}

	// PNG Writer
	void PNGWriter::SwapBytes(short* val)
	{
		char* bytes = (char*)val;
		char tmp = bytes[0]; bytes[0] = bytes[1]; bytes[1] = tmp;
	}

	void PNGWriter::SwapEndian(int* val)
	{
		short* words = (short*)val;
		short tmp = words[0]; words[0] = words[1]; words[1] = tmp;
		SwapBytes(&words[0]);
		SwapBytes(&words[1]);
	}

	void PNGWriter::WriteToBuffer(const void* iconData, Int32 size, MemoryBuffer& buffer)
	{
		buffer.WriteMarker(BufferIntegrityChecks::PNGBegin);
		if (iconData == NULL)
		{
			buffer.WriteBool(false); // No icon exists
		}
		else
		{
			buffer.WriteBool(true); // Icon exists
			buffer.WriteInt32(size);

			PNG* png = (PNG*)iconData;
			IHDR* header = (IHDR*)(png + 1);
			int width = header->width;
			int height = header->height;
			SwapEndian(&width);
			SwapEndian(&height);

			buffer.WriteInt32(width);
			buffer.WriteInt32(height);
			buffer.WriteData((char*)png, size);
		}

		buffer.WriteMarker(BufferIntegrityChecks::PNGEnd);
	}

	void PNGWriter::GetPNGSizes(const void* iconData, int& width, int& height)
	{
		if (iconData == NULL)
		{
			return;
		}

		PNG* png = (PNG*)iconData;
		IHDR* header = (IHDR*)(png + 1);
		width = header->width;
		height = header->height;
		SwapEndian(&width);
		SwapEndian(&height);
	}
}