#pragma once

#include "../Includes/SonyCommonIncludes.h"
#include "../ErrorHandling/Errors.h"

#include <list>

namespace SaveData
{
	// Up to 256 integrity markers can be added to validate
	// different structures being written into the buffer
	enum class BufferIntegrityChecks
	{
		BufferBegin = 0,		// The start marker in the buffer
		BufferEnd,				// The end marker of the buffer

		PNGBegin,
		PNGEnd,
	};

	struct MemoryBufferManaged
	{
		UInt32 length;
		void* data;
	};

	class MemoryBuffer
	{
		const static Int32 MAX_BUFFER_SIZE = 1024*1024;

		void* data;
		UInt32 defaultBufferSize;
		UInt32 maxBufferSize;
		char* pos;

		static MemoryBuffer* s_MemoryBuffer;
		static MemoryBuffer* s_NotificationBuffer;

	public:

		void CopyTo(MemoryBufferManaged* destination);

		MemoryBuffer(UInt32 defaultSize);

		static void Initialise();
		static void Shutdown();
		static MemoryBuffer& GetBuffer();
		static MemoryBuffer& GetNotificationBuffer();

		void StartResponseWrite();	
		void FinishResponseWrite();

		void Reset();

		void WriteMarker(BufferIntegrityChecks value);

		void WriteBool(bool value); // Write 1 byte

		void WriteInt8(Int8 value); 
		void WriteUInt8(UInt8 value); 

		void WriteInt16(Int16 value); 
		void WriteUInt16(UInt16 value); 

		void WriteInt32(Int32 value);
		void WriteUInt32(UInt32 value);

		void WriteInt64(Int64 value);
		void WriteUInt64(UInt64 value);

		void WritePtr(void* ptr);

		void WriteDouble(double value);

		void WriteData(const char* data, UInt32 size);

		void WriteString(const char* str);
		void WriteString(const char* str, UInt32 size);

	private:
		void GrowBuffer(size_t requiredSize);
	};

	
}