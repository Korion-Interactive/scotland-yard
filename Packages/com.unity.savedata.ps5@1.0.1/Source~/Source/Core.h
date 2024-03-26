#ifndef _CORE_H
#define _CORE_H

#include "../Includes/PluginCommonIncludes.h"


namespace SaveData
{
	struct DirNameManaged
	{
	public:
		char data[SCE_SAVE_DATA_DIRNAME_DATA_MAXSIZE];

		void CopyTo(SceSaveDataDirName &destination);
	};

	struct TitleIdManaged
	{
	public:
		char data[SCE_SAVE_DATA_TITLE_ID_DATA_SIZE];

		void CopyTo(SceSaveDataTitleId &destination);
	};


	struct FingerprintManaged
	{
	public:
		char data[SCE_SAVE_DATA_FINGERPRINT_DATA_SIZE];

		void CopyTo(SceSaveDataFingerprint  &destination);
	};


	class PNGWriter
	{
	public:
		struct PNG
		{
			char png[4];
			char crlfczlf[4];
		};

		struct IHDR
		{
			char ihdr[4];
			int ihdrlen;
			int width;
			int height;
			char bitDepth;
			char colorType;
			char compressionMethod;
			char filterMethod;
			char interlaceMethod;
		};

		static void WriteToBuffer(const void* iconData, Int32 size, MemoryBuffer& buffer);
		static void GetPNGSizes(const void* iconData, int& width, int& height);

	private:
		static void SwapBytes(short* val);
		static void SwapEndian(int* val);
	};

	class Core
	{
	public:

		static void WriteToBuffer(const SceSaveDataMountInfo& info, MemoryBuffer& buffer);
		static void WriteToBuffer(const SceSaveDataSearchInfo& info, MemoryBuffer& buffer);
		static void WriteToBuffer(const SceSaveDataParam& params, MemoryBuffer& buffer);
		static void WriteToBuffer(const SceSaveDataDirName& dirName, MemoryBuffer& buffer);

		static void InitIconForReading(SceSaveDataIcon& icon);

		static SceSaveDataDirName* GetTempDirNamesArray();
		static SceSaveDataDirName* GetTempDialogDirNamesArray();
		static SceSaveDataParam* GetTempParamsArray();
		static SceSaveDataSearchInfo* GetTempSearchInfosArray();

	private:
	};
}

#endif	//_CORE_H

