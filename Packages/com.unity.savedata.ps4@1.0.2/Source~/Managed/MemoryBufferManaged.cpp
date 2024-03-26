#include "MemoryBufferManaged.h"

namespace SaveData
{
	MemoryBuffer* MemoryBuffer::s_MemoryBuffer; 
	MemoryBuffer* MemoryBuffer::s_NotificationBuffer;
	
	// Static initialisation
	void MemoryBuffer::Initialise()
	{
		s_MemoryBuffer = new MemoryBuffer(MAX_BUFFER_SIZE);
		s_NotificationBuffer = new MemoryBuffer(MAX_BUFFER_SIZE);
	}

	void MemoryBuffer::Shutdown()
	{
		delete s_MemoryBuffer;
	}
	
	// Static methods
	MemoryBuffer& MemoryBuffer::GetBuffer()
	{
		return *s_MemoryBuffer;
	}

	MemoryBuffer& MemoryBuffer::GetNotificationBuffer()
	{
		return *s_NotificationBuffer;
	}

	// Methods
	void MemoryBuffer::StartResponseWrite()
	{
		Reset();
		WriteMarker(BufferIntegrityChecks::BufferBegin);
	}

	void MemoryBuffer::FinishResponseWrite()
	{
		WriteMarker(BufferIntegrityChecks::BufferEnd);
	}

	MemoryBuffer::MemoryBuffer(UInt32 defaultSize)
	{
		defaultBufferSize = defaultSize;
		maxBufferSize = defaultBufferSize;
		data = new char[maxBufferSize];
	}

	void MemoryBuffer::Reset()
	{
		if (maxBufferSize != defaultBufferSize)
		{
			// reset buffer to its default value if it has been increased...
			delete (char*)data;
			maxBufferSize = defaultBufferSize;
			data = new char[maxBufferSize];
		}

		pos = (char*)data;
	}

	void MemoryBuffer::CopyTo(MemoryBufferManaged* destination)
	{
		destination->data = data;
		destination->length = pos - (char*)data;
	}

	// Write 4 bytes, the first three 255,254,253 as a sanity check
	// followed by the BufferIntegrityChecks value
	void MemoryBuffer::WriteMarker(BufferIntegrityChecks value)
	{
		GrowBuffer(4);

		*((char*)pos) = (unsigned char)255; pos += 1;
		*((char*)pos) = (unsigned char)254; pos += 1;
		*((char*)pos) = (unsigned char)253; pos += 1;

		*((char*)pos) = (char)value; 
		pos += 1;
	}

	// Write a single byte - 0 = False, 1 = True
	void MemoryBuffer::WriteBool(bool value)
	{
		GrowBuffer(1);

		if ( value == false ) *((char*)pos) = 0;
		else *((char*)pos) = 1;

		pos += 1;
	}

	void  MemoryBuffer::WriteInt8(Int8 value)
	{
		GrowBuffer(sizeof(Int8));
		*((Int8*)pos) = value;
		pos += sizeof(Int8);
	}

	void  MemoryBuffer::WriteUInt8(UInt8 value)
	{
		GrowBuffer(sizeof(UInt8));
		*((UInt8*)pos) = value;
		pos += sizeof(UInt8);
	}

	void  MemoryBuffer::WriteInt16(Int16 value)
	{
		GrowBuffer(sizeof(Int16));
		*((Int16*)pos) = value;
		pos += sizeof(Int16);
	}
		
	void  MemoryBuffer::WriteUInt16(UInt16 value)
	{
		GrowBuffer(sizeof(UInt16));
		*((UInt16*)pos) = value;
		pos += sizeof(UInt16);
	}

	void MemoryBuffer::WriteInt32(Int32 value)
	{
		GrowBuffer(sizeof(Int32));
		*((Int32*)pos) = value;
		pos += sizeof(Int32);
	}

	void MemoryBuffer::WriteUInt32(UInt32 value)
	{
		GrowBuffer(sizeof(UInt32));
		*((UInt32*)pos) = value;
		pos += sizeof(UInt32);
	}

	void MemoryBuffer::WriteInt64(Int64 value)
	{
		GrowBuffer(sizeof(UInt64));
		*((Int64*)pos) = value;
		pos += sizeof(Int64);
	}

	void MemoryBuffer::WriteUInt64(UInt64 value)
	{
		GrowBuffer(sizeof(UInt64));
		*((UInt64*)pos) = value;
		pos += sizeof(UInt64);
	}

	void MemoryBuffer::WritePtr(void* ptr)
	{
		GrowBuffer(sizeof(void*));

		*((UInt64*)pos) = (UInt64)ptr;
		pos += sizeof(void*);
	}

	void MemoryBuffer::WriteDouble(double value)
	{
		GrowBuffer(sizeof(double));
		*((double*)pos) = value;
		pos += sizeof(double);
	}

	void MemoryBuffer::WriteString(const char* str)
	{
		UInt32 strLen = strlen(str);

		WriteData(str, strLen);
	}

	void MemoryBuffer::WriteString(const char* str, UInt32 size)
	{
		WriteData(str, size);
	}

	void MemoryBuffer::WriteData(const char* data, UInt32 size)
	{
		GrowBuffer(size + sizeof(UInt32));

		*((UInt32*)pos) = size;
		pos += sizeof(UInt32);
		memcpy(pos, data, size);
		pos += size;
	}

	void MemoryBuffer::GrowBuffer(size_t requiredSize)
	{
		int sizeUsed = pos - (char*)data;
		if ( sizeUsed + requiredSize > maxBufferSize )
		{
			// Increased the size of the buffer (double it until it's enough)
			while (maxBufferSize < sizeUsed + requiredSize)
			{
				maxBufferSize *= 2;
			}
			void* extendedData = new char[maxBufferSize];
			memcpy(extendedData, data, sizeUsed);   // we don't need to copy more than used...

			delete (char*)data;
			data = extendedData;

			// Reset the current position so it is in the correct place in the new memory location
			pos = ((char*)data) + sizeUsed;
		}
	}

}