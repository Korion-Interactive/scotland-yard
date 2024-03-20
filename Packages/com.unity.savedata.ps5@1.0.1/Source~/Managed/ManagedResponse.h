#pragma once

#include "../Includes/SonyCommonIncludes.h"
#include "../ErrorHandling/Errors.h"
#include "../Managed/MemoryBufferManaged.h"

namespace SaveData
{
	class ResponseBase
	{
	public:
		static void MarshalResponseBase(int returnCode, MemoryBuffer& buffer);
	};

	//struct ResponseBaseMarshalled
	//{
	//public:
	//	Int32 returnCode;
	//};
}