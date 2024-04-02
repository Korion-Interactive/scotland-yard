using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sony
{
    namespace PS4
    {
        namespace SaveData
        {
            /// <summary>
            /// Savedata directory name
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct DirName
            {
                /// <summary>
                /// Max savedata directory name size
                /// </summary>
                public const Int32 DIRNAME_DATA_MAXSIZE = 31;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DIRNAME_DATA_MAXSIZE + 1)]
                internal string data;

                /// <summary>
                /// The savedata directory name
                /// </summary>
                public string Data
                {
                    get { return data; }
                    set
                    {
                        if (value != null)
                        {
                            if (value.Length > DIRNAME_DATA_MAXSIZE)
                            {
                                throw new SaveDataException("The length of the directory name string is more than " + DIRNAME_DATA_MAXSIZE + " characters (DIRNAME_DATA_MAXSIZE)");
                            }
                        }

                        data = value;
                    }
                }

                /// <summary>
                /// Is the directory name empty
                /// </summary>
                public bool IsEmpty
                {
                    get { return data == null || data.Length == 0; }
                }

                internal void Read(MemoryBuffer buffer)
                {
                    buffer.ReadString(ref data);
                }

                /// <summary>
                /// Convert the directory name to a string
                /// </summary>
                /// <returns>Directory name</returns>
                public override string ToString()
                {
                    return data;
                }
            }

            /// <summary>
            /// Savedata paramters
            /// </summary>
            public struct SaveDataParams
            {
                /// <summary>
                /// Max length savedata title text
                /// </summary>
                public const Int32 TITLE_MAXSIZE = 127;

                /// <summary>
                /// Max length savedata subtitle text
                /// </summary>
                public const Int32 SUBTITLE_MAXSIZE = 127;

                /// <summary>
                /// Max length of savedata detail info text
                /// </summary>
                public const Int32 DETAIL_MAXSIZE = 1023;

                internal string title;
                internal string subTitle;
                internal string detail;
                internal DateTime time;
                internal UInt32 userParam;

                /// <summary>
                /// Savedata title name
                /// </summary>
                public string Title
                {
                    get { return title; }
                    set { title = value; }
                }

                /// <summary>
                /// Savedata subtitle name
                /// </summary>
                public string SubTitle
                {
                    get { return subTitle; }
                    set { subTitle = value; }
                }

                /// <summary>
                /// Savedata detail text
                /// </summary>
                public string Detail
                {
                    get { return detail; }
                    set { detail = value; }
                }

                /// <summary>
                /// Savedata user paramter
                /// </summary>
                public UInt32 UserParam
                {
                    get { return userParam; }
                    set { userParam = value; }
                }

                /// <summary>
                /// Date and time of last update
                /// </summary>
                public DateTime Time
                {
                    get { return time; }
                }

                internal void Read(MemoryBuffer buffer)
                {
                    buffer.ReadString(ref title);
                    buffer.ReadString(ref subTitle);
                    buffer.ReadString(ref detail);

                    userParam = buffer.ReadUInt32();

                    Int64 timet = buffer.ReadInt64();

                    try
                    {
                        time = new System.DateTime(1970, 1, 1).AddSeconds(timet);
                    }
                    catch
                    {
                        time = new System.DateTime(1970, 1, 1);
                    }
                }
            }

            /// <summary>
            /// Savedata size info
            /// </summary>
            public struct SaveDataInfo
            {
                internal UInt64 blocks;
                internal UInt64 freeBlocks;

                /// <summary>
                /// Total size of the save data
                /// </summary>
                public UInt64 Blocks
                {
                    get { return blocks; }
                }

                /// <summary>
                /// Free space of the save data
                /// </summary>
                public UInt64 FreeBlocks
                {
                    get { return freeBlocks; }
                }

                internal void Read(MemoryBuffer buffer)
                {
                    blocks = buffer.ReadUInt64();
                    freeBlocks = buffer.ReadUInt64();
                }
            }


            /// <summary>
            /// PNG image store in bytes from NpToolkit
            /// </summary>
            public class Icon
            {
                [StructLayout(LayoutKind.Sequential)]
                internal struct IHDR
                {
                    internal UInt32 png;
                    internal UInt32 crlfczlf;
                    internal UInt32 ihdr;
                    internal Int32 ihdrlen;
                    internal Int32 width;
                    internal Int32 height;
                    internal Byte bitDepth;
                    internal Byte colorType;
                    internal Byte compressionMethod;
                    internal Byte filterMethod;
                    internal Byte interlaceMethod;
                };

                static internal T ByteArrayToStructure<T>(byte[] bytes) where T : struct
                {
                    GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

                    T stuff;
                    try
                    {
                        stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                    }
                    finally
                    {
                        handle.Free();
                    }
                    return stuff;
                }

                static internal IHDR GetPNGHeader(byte[] bytes)
                {
                    return ByteArrayToStructure<IHDR>(bytes);
                }

                internal byte[] rawBytes;
                internal Int32 width;
                internal Int32 height;

                /// <summary>
                /// Get the RawBytes from the Icon. These can be used to create a new Texture2d for example.
                /// </summary>
                public byte[] RawBytes
                {
                    get
                    {
                        return rawBytes;
                    }
                }

                /// <summary>
                /// Width in pixels of the icon.
                /// </summary>
                public Int32 Width { get { return width; } }

                /// <summary>
                /// Height in pixels of the icon.
                /// </summary>
                public Int32 Height { get { return height; } }

                static internal Icon ReadAndCreate(MemoryBuffer buffer)
                {
                    Icon result = null;
                    buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGBegin);

                    bool hasIcon = buffer.ReadBool();

                    if (hasIcon == false)
                    {
                        //Console.WriteLine("No icon data");
                    }
                    else
                    {
                        //Console.WriteLine("Creating icon.");

                        result = new Icon();
                        // Read the image
                        /*Int32 numBytes = */ buffer.ReadInt32();
                        result.width = buffer.ReadInt32();
                        result.height = buffer.ReadInt32();

                        buffer.ReadData(ref result.rawBytes);
                    }

                    buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGEnd);

                    return result;
                }
            }
        }
    }
}

