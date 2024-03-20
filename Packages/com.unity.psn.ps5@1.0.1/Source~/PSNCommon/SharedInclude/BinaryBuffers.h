#pragma once

#include "SonyCommonIncludes.h"
#include "CommonTypes.h"

#include <list>

namespace psn
{
    class BinaryWriter
    {
        void* buffer;
        UInt32 maxBufferSize;
        char* pos;

    public:

        BinaryWriter(void* stream, UInt32 maxSize);

        void Reset();

        UInt32 GetWrittenLength();

        void WriteBool(bool value); // Write 1 byte

        void WriteInt8(Int8 value);
        void WriteUInt8(UInt8 value);

        void WriteInt16(Int16 value);
        void WriteUInt16(UInt16 value);

        void WriteInt32(Int32 value);
        void WriteUInt32(UInt32 value);

        void WriteInt64(Int64 value);
        void WriteUInt64(UInt64 value);

        void WriteRtcTick(SceRtcTick value);

        void WritePtr(void* ptr);

        void WriteDouble(double value);

        void WriteData(const char* data, UInt32 size);
        void WriteDataBlock(const char* data, UInt32 size);

        void WriteString(const char* str);
        void WriteString(const char* str, UInt32 size);

        UInt32* ReservePlaceholderUInt32();

    private:
        void Check(size_t requiredSize);
    };

    class BinaryReader
    {
        void* buffer;
        UInt32 maxBufferSize;
        char* pos;

    public:

        BinaryReader(void* stream, UInt32 maxSize);

        void Reset();

        bool ReadBool(); // Write 1 byte

        Int8 ReadInt8();
        UInt8 ReadUInt8();

        Int16 ReadInt16();
        UInt16 ReadUInt16();

        Int32 ReadInt32();
        UInt32 ReadUInt32();

        Int64 ReadInt64();
        UInt64 ReadUInt64();

        float ReadFloat();
        double ReadDouble();

        void ReadData(char* data, UInt32 size);

        void ReadString(char* str, int maxLength);
        char* ReadStringPtr();

        void* ReadDataPtr(UInt32 size);

        void* GetPosPtr();

    private:
        void Check(size_t requiredSize);
    };
}
