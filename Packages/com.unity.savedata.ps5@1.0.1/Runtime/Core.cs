using System;

using System.Runtime.InteropServices;
using System.Text;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Internal;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// Name of the save data folder.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DirName
    {
        /// <summary>
        /// Maximum size of the save data folder name.
        /// </summary>
        public const Int32 DIRNAME_DATA_MAXSIZE = 31;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DIRNAME_DATA_MAXSIZE + 1)]
        internal string data;

        /// <summary>
        /// The name of the save data folder.
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
        /// True if the folder name is empty, false otherwise.
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
        /// Converts the folder name to a string.
        /// </summary>
        /// <returns>Folder name</returns>
        public override string ToString()
        {
            return data;
        }
    }

    /// <summary>
    /// Title ID of the transfer source save data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TitleId
    {
        /// <summary>
        /// Maximum size of the Title ID.
        /// </summary>
        public const Int32 DATA_TITLE_ID_DATA_SIZE = 10;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DATA_TITLE_ID_DATA_SIZE)]
        internal string data;

        /// <summary>
        /// The Title ID.
        /// </summary>
        public string Data
        {
            get { return data; }
            set
            {
                if (value != null)
                {
                    if (value.Length > DATA_TITLE_ID_DATA_SIZE)
                    {
                        throw new SaveDataException("The length of the Title ID string is more than " + DATA_TITLE_ID_DATA_SIZE + " characters (DATA_TITLE_ID_DATA_SIZE)");
                    }
                }

                data = value;
            }
        }

        /// <summary>
        /// True if the Title ID is empty, false otherwise.
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
        /// Converts the Title ID to a string.
        /// </summary>
        /// <returns>Title ID</returns>
        public override string ToString()
        {
            return data;
        }
    }

    /// <summary>
    /// Name of the fingerprint data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Fingerprint
    {
        /// <summary>
        /// Maximum size of the save data folder name.
        /// </summary>
        public const Int32 FINGERPRINT_DATA_SIZE = 65;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = FINGERPRINT_DATA_SIZE)]
        internal string data;

        /// <summary>
        /// The fingerprint data.
        /// </summary>
        public string Data
        {
            get { return data; }
            set
            {
                if (value != null)
                {
                    if (value.Length > FINGERPRINT_DATA_SIZE)
                    {
                        throw new SaveDataException("The length of the fingerprint data string is more than " + FINGERPRINT_DATA_SIZE + " characters (FINGERPRINT_DATA_SIZE)");
                    }
                }

                data = value;
            }
        }

        /// <summary>
        /// True if the fingerprint data is empty, false otherwise.
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
        /// Converts the fingerprint data to a string.
        /// </summary>
        /// <returns>fingerprint data</returns>
        public override string ToString()
        {
            return data;
        }
    }        

}

namespace Unity.SaveData.PS5.Info
{

    /// <summary>
    /// Save data paramters.
    /// </summary>
    public struct SaveDataParams
    {
        /// <summary>
        /// Maximum length of the save data title text, in bytes.
        /// </summary>
        public const Int32 TITLE_MAXSIZE = 127;

        /// <summary>
        /// Maximum length of the save data subtitle text, in bytes.
        /// </summary>
        public const Int32 SUBTITLE_MAXSIZE = 127;

        /// <summary>
        /// Maximum length of the save data details text, in bytes.
        /// </summary>
        public const Int32 DETAIL_MAXSIZE = 1023;

        internal string title;
        internal string subTitle;
        internal string detail;
        internal DateTime time;
        internal UInt32 userParam;

        /// <summary>
        /// Name of the save data title.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if(Encoding.UTF8.GetByteCount(value) > TITLE_MAXSIZE)
                {
                    throw new SaveDataException("The byte length of the Title string is more than " + TITLE_MAXSIZE + " bytes (TITLE_MAXSIZE)");
                }
                title = value;
            }
        }

        /// <summary>
        /// Name of the save data subtitle.
        /// </summary>
        public string SubTitle
        {
            get { return subTitle; }
            set
            {
                if (Encoding.UTF8.GetByteCount(value) > SUBTITLE_MAXSIZE)
                {
                    throw new SaveDataException("The byte length of the SubTitle string is more than " + SUBTITLE_MAXSIZE + " bytes (SUBTITLE_MAXSIZE)");
                }
                subTitle = value;
            }
        }

        /// <summary>
        /// Name of the save data details text.
        /// </summary>
        public string Detail
        {
            get { return detail; }
            set
            {
                if (Encoding.UTF8.GetByteCount(value) > DETAIL_MAXSIZE)
                {
                    throw new SaveDataException("The byte length of the Detail string is more than " + DETAIL_MAXSIZE + " bytes (DETAIL_MAXSIZE)");
                }
                detail = value;
            }
        }

        /// <summary>
        /// User parameter of the save data.
        /// </summary>
        public UInt32 UserParam
        {
            get { return userParam; }
            set { userParam = value; }
        }

        /// <summary>
        /// Date and time of last update.
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
    /// Information about the save data size.
    /// </summary>
    public struct SaveDataInfo
    {
        internal UInt64 blocks;
        internal UInt64 freeBlocks;

        /// <summary>
        /// Total size of the save data.
        /// </summary>
        public UInt64 Blocks
        {
            get { return blocks; }
        }

        /// <summary>
        /// Free space on the save data.
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
    /// PNG image store from NpToolkit, in bytes.
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
        /// Gets the RawBytes from the Icon. You can use these to create a new Texture2d, for example.
        /// </summary>
        public byte[] RawBytes
        {
            get
            {
                return rawBytes;
            }
        }

        /// <summary>
        /// Icon width, in pixels.
        /// </summary>
        public Int32 Width { get { return width; } }

        /// <summary>
        /// Icon height, in pixels.
        /// </summary>
        public Int32 Height { get { return height; } }

        static internal Icon ReadAndCreate(MemoryBuffer buffer)
        {
            Icon result = null;
            buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGBegin);

            bool hasIcon = buffer.ReadBool();

            if (hasIcon == false)
            {
            }
            else
            {
                result = new Icon();
                // Read the image
                /*Int32 numBytes = */
                buffer.ReadInt32();
                result.width = buffer.ReadInt32();
                result.height = buffer.ReadInt32();

                buffer.ReadData(ref result.rawBytes);
            }

            buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.PNGEnd);

            return result;
        }
    }
}

