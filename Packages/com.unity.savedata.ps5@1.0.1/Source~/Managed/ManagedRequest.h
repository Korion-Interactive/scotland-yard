#pragma once

#include "../Includes/SonyCommonIncludes.h"
#include "../Includes/CommonTypes.h"

namespace SaveData
{
	// A marshalled version of RequestBase
	class RequestBaseManaged
	{
	public:
		UInt32 functionType;
		Int32 userId;
		bool async;
		bool locked;
		bool ignoreCallback;
		UInt32 padding;
	};

}
