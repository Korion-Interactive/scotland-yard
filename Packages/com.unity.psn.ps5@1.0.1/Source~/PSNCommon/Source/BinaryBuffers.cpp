#include "BinaryBuffers.h"

namespace psn
{
    BinaryWriter::BinaryWriter(void* stream, UInt32 maxSize)
    {
        maxBufferSize = maxSize;
        buffer = stream;

        pos = (char*)buffer;
    }

    void BinaryWriter::Reset()
    {
        pos = (char*)buffer;
    }

    UInt32 BinaryWriter::GetWrittenLength()
    {
        int bytesUsed = pos - (char*)buffer;

        return bytesUsed;
    }

    // Write a single byte - 0 = False, 1 = True
    void BinaryWriter::WriteBool(bool value)
    {
        Check(1);

        if (value == false) *((char*)pos) = 0;
        else *((char*)pos) = 1;

        pos += 1;
    }

    void  BinaryWriter::WriteInt8(Int8 value)
    {
        Check(sizeof(Int8));
        *((Int8*)pos) = value;
        pos += sizeof(Int8);
    }

    void  BinaryWriter::WriteUInt8(UInt8 value)
    {
        Check(sizeof(UInt8));
        *((UInt8*)pos) = value;
        pos += sizeof(UInt8);
    }

    void  BinaryWriter::WriteInt16(Int16 value)
    {
        Check(sizeof(Int16));
        *((Int16*)pos) = value;
        pos += sizeof(Int16);
    }

    void  BinaryWriter::WriteUInt16(UInt16 value)
    {
        Check(sizeof(UInt16));
        *((UInt16*)pos) = value;
        pos += sizeof(UInt16);
    }

    void BinaryWriter::WriteInt32(Int32 value)
    {
        Check(sizeof(Int32));
        *((Int32*)pos) = value;
        pos += sizeof(Int32);
    }

    void BinaryWriter::WriteUInt32(UInt32 value)
    {
        Check(sizeof(UInt32));
        *((UInt32*)pos) = value;
        pos += sizeof(UInt32);
    }

    void BinaryWriter::WriteInt64(Int64 value)
    {
        Check(sizeof(UInt64));
        *((Int64*)pos) = value;
        pos += sizeof(Int64);
    }

    UInt32* BinaryWriter::ReservePlaceholderUInt32()
    {
        Check(sizeof(UInt32));
        UInt32* ptr = (UInt32*)pos;
        pos += sizeof(UInt32);

        return ptr;
    }

    void BinaryWriter::WriteUInt64(UInt64 value)
    {
        Check(sizeof(UInt64));
        *((UInt64*)pos) = value;
        pos += sizeof(UInt64);
    }

    void BinaryWriter::WriteRtcTick(SceRtcTick value)
    {
        Check(sizeof(UInt64));
        *((UInt64*)pos) = value.tick;
        pos += sizeof(UInt64);
    }

    void BinaryWriter::WritePtr(void* ptr)
    {
        Check(sizeof(void*));

        *((UInt64*)pos) = (UInt64)ptr;
        pos += sizeof(void*);
    }

    void BinaryWriter::WriteDouble(double value)
    {
        Check(sizeof(double));
        *((double*)pos) = value;
        pos += sizeof(double);
    }

    void BinaryWriter::WriteString(const char* str)
    {
        if (str == NULL)
        {
            WriteData("", strlen(""));
            return;
        }

        UInt32 strLen = strlen(str);

        WriteData(str, strLen);
    }

    void BinaryWriter::WriteString(const char* str, UInt32 size)
    {
        WriteData(str, size);
    }

    void BinaryWriter::WriteData(const char* data, UInt32 size)
    {
        Check(size + sizeof(UInt32));

        *((UInt32*)pos) = size;
        pos += sizeof(UInt32);

        if (size > 0)
        {
            memcpy(pos, data, size);
            pos += size;
        }
    }

    void BinaryWriter::WriteDataBlock(const char* data, UInt32 size)
    {
        if (size > 0)
        {
            memcpy(pos, data, size);
            pos += size;
        }
    }

    void BinaryWriter::Check(size_t requiredSize)
    {
        int sizeUsed = pos - (char*)buffer;
        if (sizeUsed + requiredSize > maxBufferSize)
        {
        }
    }

    BinaryReader::BinaryReader(void* stream, UInt32 maxSize)
    {
        maxBufferSize = maxSize;
        buffer = stream;

        pos = (char*)buffer;
    }

    void BinaryReader::Reset()
    {
        pos = (char*)buffer;
    }

    // Write a single byte - 0 = False, 1 = True
    bool BinaryReader::ReadBool()
    {
        Check(1);

        bool value = false;
        if (*pos == 1) value = true;

        pos += 1;

        return value;
    }

    Int8  BinaryReader::ReadInt8()
    {
        Check(sizeof(Int8));

        Int8 value = *((Int8*)pos);

        pos += sizeof(Int8);

        return value;
    }

    UInt8  BinaryReader::ReadUInt8()
    {
        Check(sizeof(UInt8));
        UInt8 value = *((UInt8*)pos);

        pos += sizeof(UInt8);

        return value;
    }

    Int16  BinaryReader::ReadInt16()
    {
        Check(sizeof(Int16));
        Int16 value = *((Int16*)pos);
        pos += sizeof(Int16);

        return value;
    }

    UInt16  BinaryReader::ReadUInt16()
    {
        Check(sizeof(UInt16));
        UInt16 value = *((UInt16*)pos);
        pos += sizeof(UInt16);

        return value;
    }

    Int32 BinaryReader::ReadInt32()
    {
        Check(sizeof(Int32));
        Int32 value = *((Int32*)pos);
        pos += sizeof(Int32);

        return value;
    }

    UInt32 BinaryReader::ReadUInt32()
    {
        Check(sizeof(UInt32));
        UInt32 value = *((UInt32*)pos);
        pos += sizeof(UInt32);

        return value;
    }

    Int64 BinaryReader::ReadInt64()
    {
        Check(sizeof(UInt64));
        Int64 value = *((Int64*)pos);
        pos += sizeof(Int64);

        return value;
    }

    UInt64 BinaryReader::ReadUInt64()
    {
        Check(sizeof(UInt64));
        UInt64 value = *((UInt64*)pos);
        pos += sizeof(UInt64);

        return value;
    }

    float BinaryReader::ReadFloat()
    {
        Check(sizeof(float));
        double value = *((float*)pos);
        pos += sizeof(float);

        return value;
    }

    double BinaryReader::ReadDouble()
    {
        Check(sizeof(double));
        double value = *((double*)pos);
        pos += sizeof(double);

        return value;
    }

    void BinaryReader::ReadString(char* str, int maxLength)
    {
        UInt32 numBytes = ReadUInt32();

        if (numBytes > maxLength)
        {
            // error
            return;
        }

        if (numBytes == 0)
        {
            if (maxLength > 0) str[0] = 0;
            return;
        }

        ReadData(str, numBytes);

        //str[numBytes] = 0; // null terminator
    }

    void BinaryReader::ReadData(char* data, UInt32 size)
    {
        Check(size + sizeof(UInt32));

        if (size > 0)
        {
            memcpy(data, pos, size);
            pos += size;
        }
    }

    char* BinaryReader::ReadStringPtr()
    {
        UInt32 numBytes = ReadUInt32();

        if (numBytes == 0)
        {
            return NULL;
        }

        char* str = pos;

        pos += numBytes;

        return str;
    }

    void* BinaryReader::ReadDataPtr(UInt32 size)
    {
        char* ptr = pos;

        pos += size;

        return ptr;
    }

    void* BinaryReader::GetPosPtr()
    {
        return pos;
    }

    void BinaryReader::Check(size_t requiredSize)
    {
        int sizeUsed = pos - (char*)buffer;
        if (sizeUsed + requiredSize > maxBufferSize)
        {
        }
    }
}
