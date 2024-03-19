
#include "Notifications.h"
#include "Core.h"
#include <sce_atomic.h>

namespace SaveData
{
	PRX_EXPORT void PrxNotificationPoll(MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Notifications::NotificationPoll(outBuffer, result);	
	}

	void Notifications::NotificationPoll(MemoryBufferManaged* outBuffer, APIResult* result)
	{
		MemoryBuffer buffer = MemoryBuffer::GetNotificationBuffer();
		buffer.StartResponseWrite();

		// Check to see if any of the systems are polling for events to occur
		SceSaveDataEvent event;
		memset(&event, 0x00, sizeof(event));

		int ret = sceSaveDataGetEventResult(NULL, &event);

		// Write return code
		buffer.WriteInt32(ret);

		if (ret == 0)
		{
			// Event has occured
			buffer.WriteInt32(event.type);
			buffer.WriteInt32(event.userId);
			buffer.WriteInt32(event.errorCode);

			Core::WriteToBuffer(event.dirName, buffer);
			//buffer.WriteData(event.dirName.data, SCE_SAVE_DATA_DIRNAME_DATA_MAXSIZE);
		}

		buffer.FinishResponseWrite();
		buffer.CopyTo(outBuffer);

		SUCCESS_RESULT(result);
	}

}