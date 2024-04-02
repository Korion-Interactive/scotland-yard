
#include "Utils.h"

#include <sceerror.h>
#include <save_data.h>
#include <save_data_dialog.h>

namespace SaveData
{
	//int32_t Utils::LoadFile(const char *mountPoint, const char *fileName, uint8_t** loadeddata, __int64_t *datasize, bool dialogEnabled)
	//{
	//	int32_t ret = SCE_OK;

	//	char path[SAVE_DATA_FILEPATH_LENGTH];
	//	snprintf(path, sizeof(path), "%s/%s", mountPoint, fileName);
	//	SceKernelStat st;
	//	ret = sceKernelStat(path, &st);
	//	if (ret < SCE_OK)
	//	{
	//		return ret;
	//	}

	//	// allocate memory

	//	uint8_t *data = new uint8_t[st.st_size];
	//	if (!data)
	//	{
	//		return -1;
	//	}
	//	// load file

	//	int fd = sceKernelOpen(path, SCE_KERNEL_O_RDONLY, SCE_KERNEL_S_INONE);
	//	if (fd < SCE_OK)
	//	{
	//		delete[] data;
	//		return fd;
	//	}

	//	size_t buffersize = 64 * 1024;
	//	uint8_t *curData = data;
	//	size_t totalsize = static_cast<size_t>(st.st_size);
	//	size_t amountToRead = totalsize;
	//	while (amountToRead>0)
	//	{
	//		size_t thisRead = amountToRead<buffersize ? amountToRead : buffersize;
	//		ret = static_cast<int32_t>(sceKernelRead(fd, curData, thisRead));
	//		if (ret < SCE_OK)
	//		{
	//			sceKernelClose(fd);
	//			return ret;
	//		}

	//		amountToRead -= thisRead;
	//		curData += thisRead;
	//		if (dialogEnabled)
	//		{
	//			sceSaveDataDialogProgressBarSetValue(SCE_SAVE_DATA_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, ((totalsize - amountToRead) * 100) / totalsize);
	//		}
	//	};

	//	*loadeddata = data;
	//	*datasize = st.st_size;

	//	ret = SCE_OK;

	//	sceKernelClose(fd);

	//	return ret;
	//}

	int32_t Utils::LoadFile(const char *path, uint8_t** data, size_t *datasize)
	{
		int32_t ret = SCE_OK;

		SceKernelStat st;
		ret = sceKernelStat(path, &st);
		if (ret < SCE_OK)
		{
			return ret;
		}

		// allocate memory

		*data = new uint8_t[st.st_size];

		if (*data == NULL)
		{
			return -1;
		}
		// load file

		int fd = sceKernelOpen(path, SCE_KERNEL_O_RDONLY, SCE_KERNEL_S_INONE);
		if (fd < SCE_OK)
		{
			delete[] data;
			return fd;
		}

		ret = static_cast<int32_t>(sceKernelRead(fd, *data, static_cast<size_t>(st.st_size)));
		if (ret < SCE_OK)
		{
			sceKernelClose(fd);
			return ret;
		}

		*datasize = st.st_size;

		sceKernelClose(fd);

		return ret;
	}
}