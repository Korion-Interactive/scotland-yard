#ifndef _NOTIFICATIONS_H
#define _NOTIFICATIONS_H

#include "../Includes/PluginCommonIncludes.h"


namespace SaveData
{
	class Notifications
	{
	public:

		static void NotificationPoll(MemoryBufferManaged* outBuffer, APIResult* result);
	};

}

#endif