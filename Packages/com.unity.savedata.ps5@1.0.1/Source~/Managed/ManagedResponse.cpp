#include "ManagedResponse.h"

namespace SaveData
{
	void ResponseBase::MarshalResponseBase(int returnCode, MemoryBuffer& buffer)
	{
		buffer.WriteInt32(returnCode);
	}
}