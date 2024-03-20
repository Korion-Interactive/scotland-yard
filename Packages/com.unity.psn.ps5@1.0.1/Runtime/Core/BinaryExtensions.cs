
using System;
using System.IO;
using UnityEngine;

namespace Unity.PSN.PS5.Internal
{
    internal static class BinaryWriterExtensions
    {
        internal static void WritePrxString(this BinaryWriter writer, string val)//, bool test = false)
        {
            if(val == null)
            {
                writer.Write(0); // 0 length string
                return;
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(val);

            //if(test == true)
            //{
            //    string output = "";
            //    for(int i = 0; i < bytes.Length; i++)
            //    {
            //        output += bytes[i] + " ";
            //    }
            //    Debug.LogError("WritePrxString : " + val + " : " + output);
            //}

            writer.Write(bytes.Length + 1);
            writer.Write(bytes);
            writer.Write((byte)0); // null terminator
        }

        internal static void WriteNullTerminatedString(this BinaryWriter writer, string val)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(val);

            writer.Write(bytes);
            writer.Write((byte)0); // null terminator
        }
    }

    internal static class BinaryReaderExtensions
    {
        internal static string ReadPrxString(this BinaryReader reader)//, bool test = false)
        {
            int numBytes = reader.ReadInt32();

            byte[] bytes = reader.ReadBytes(numBytes);

            string text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            //if (test == true)
            //{
            //    string output = "";
            //    for (int i = 0; i < bytes.Length; i++)
            //    {
            //        output += bytes[i] + " ";
            //    }
            //    Debug.LogError("ReadPrxString : " + text + " : " + output);
            //}

            return text;
        }

        internal static DateTime ReadRtcTicks(this BinaryReader reader)
        {
            const UInt64 sceToDotNetTicks = 10;   // sce ticks are microsecond, .net are 100 nanosecond

            UInt64 rtcTicks = reader.ReadUInt64();

            UInt64 dotNetTicks = rtcTicks * sceToDotNetTicks;

            return new DateTime((Int64)dotNetTicks);
        }

        internal static DateTime ReadUnixTimestampString(this BinaryReader reader)
        {
            string timeStr = reader.ReadPrxString();
            UInt64 unixMs = 0;

            if (UInt64.TryParse(timeStr, out unixMs) == false)
            {
                return new DateTime(0);
            }

            System.DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dt = unixEpoch.AddMilliseconds(unixMs);

            return dt;
        }

        internal static byte[] ReadData(this BinaryReader reader)
        {
            UInt32 size = reader.ReadUInt32();

            byte[] data = new byte[size];

            reader.Read(data, 0, (int)size);

            return data;
        }

        internal static UInt32 ReadData(this BinaryReader reader, byte[] data)
        {
            UInt32 size = reader.ReadUInt32();

            if (size > data.Length)
            {
                byte[] tempData = ReadData(reader);
                Array.Copy(tempData, data, data.Length);
                return (UInt32)data.Length;
            }
            else
            {
                reader.Read(data, 0, (int)size);
                return size;
            }
        }
    }
}
