using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Internal;

namespace Unity.SaveData.PS5.Search
{
    /// <summary>
    /// Save data search methods.
    /// </summary>
    public class Searching
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern void PrxSaveDataDirNameSearch(DirNameSearchRequest request, out MemoryBufferNative data, out APIResult result);

        #endregion

        #region Requests

        /// <summary>
        /// Sort key. This contains various criteria to sort save data search results.
        /// </summary>
        public enum SearchSortKey : uint
        {
            /// <summary> Directory name. See SCE_SAVE_DATA_SORT_KEY_DIRNAME </summary>
            DirName = 0,
            /// <summary> User parameter. See SCE_SAVE_DATA_SORT_KEY_USER_PARAM </summary>
            UserParam = 1,
            /// <summary> Number of blocks. See SCE_SAVE_DATA_SORT_KEY_BLOCKS </summary>
            Blocks = 2,
            /// <summary> Date and time of last update. See SCE_SAVE_DATA_SORT_KEY_MTIME </summary>
            Time = 3,
            // Option 4 is missing
            /// <summary> Number of free blocks. See SCE_SAVE_DATA_SORT_KEY_FREE_BLOCKS </summary>
            FreeBlocks = 5
        }

        /// <summary>
        /// Sort order for save data search results.
        /// </summary>
        public enum SearchSortOrder : uint
        {
            /// <summary> Ascending order. See SCE_SAVE_DATA_SORT_ORDER_ASCENT </summary>
            Ascending = 0,
            /// <summary> Descending order. See SCE_SAVE_DATA_SORT_ORDER_DESCENT </summary>
            Descending = 1,
        }

        /// <summary>
        /// Request paramters for searching existing save data folders.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class DirNameSearchRequest : RequestBase
        {
            /// <summary>
            /// Maximum number of folder names to return.
            /// </summary>
            public const Int32 DIR_NAME_MAXSIZE = 1024; // SCE_SAVE_DATA_DIRNAME_MAX_COUNT

            internal TitleId titleId;
            internal DirName dirName;
            internal UInt32 maxDirNameCount = DIR_NAME_MAXSIZE;
            internal SearchSortKey key;
            internal SearchSortOrder order;

            [MarshalAs(UnmanagedType.I1)]
            internal bool includeParams;

            [MarshalAs(UnmanagedType.I1)]
            internal bool includeBlockInfo;
            [MarshalAs(UnmanagedType.I1)]
            internal bool searchPS4;


            /// <summary>
            /// TitleId to search for. This is empty by default so that all title ids are included in the search.
            /// </summary>
            public TitleId TitleId
            {
                get { return titleId; }
                set { ThrowExceptionIfLocked(); titleId = value; }
            }
            /// <summary>
            /// Filter for folder names. This is empty by default so that all folder names are included in the search.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
                set { ThrowExceptionIfLocked(); dirName = value; }
            }
            /// <summary>
            /// Maximum number of folder names to return.
            /// </summary>
            public UInt32 MaxDirNameCount
            {
                get { return maxDirNameCount; }
                set { ThrowExceptionIfLocked(); maxDirNameCount = value; }
            }

            /// <summary>
            /// Search key to use. Defaults to <see cref="SearchSortKey.DirName"/>.
            /// </summary>
            public SearchSortKey Key
            {
                get { return key; }
                set { ThrowExceptionIfLocked(); key = value; }
            }

            /// <summary>
            /// Sort order. Defaults to <see cref="SearchSortOrder.Ascending"/>.
            /// </summary>
            public SearchSortOrder Order
            {
                get { return order; }
                set { ThrowExceptionIfLocked(); order = value; }
            }

            /// <summary>
            /// Include parameters in returned results.
            /// </summary>
            public bool IncludeParams
            {
                get { return includeParams; }
                set { ThrowExceptionIfLocked(); includeParams = value; }
            }

            /// <summary>
            /// Include block size info in returned results.
            /// </summary>
            public bool IncludeBlockInfo
            {
                get { return includeBlockInfo; }
                set { ThrowExceptionIfLocked(); includeBlockInfo = value; }
            }

           /// <summary>
            /// Search for PS4 titles only.
            /// </summary>
            public bool SearchPS4
            {
                get { return searchPS4; }
                set { ThrowExceptionIfLocked(); searchPS4 = value; }
            }


            /// <summary>
            /// Initializes a new instance of the <see cref="DirNameSearchRequest"/> class.
            /// </summary>
            public DirNameSearchRequest()
                : base(FunctionTypes.DirNameSearch)
            {
                searchPS4=false;
            }

            internal override void Execute(PendingRequest pendingRequest)
            {
                APIResult result;

                MemoryBufferNative data = new MemoryBufferNative();

                PrxSaveDataDirNameSearch(this, out data, out result);

                DirNameSearchResponse response = pendingRequest.response as DirNameSearchResponse;

                if (response != null)
                {
                    response.Populate(result, data);
                }
            }
        }

        #endregion

        /// <summary>
        /// Search folder name details.
        /// </summary>
        public class SearchSaveDataItem
        {
            internal DirName dirName;
            internal SaveDataParams sdParams;
            internal SaveDataInfo sdInfo;

            /// <summary>
            /// Save data folder name.
            /// </summary>
            public DirName DirName
            {
                get { return dirName; }
            }

            /// <summary>
            /// Save data folder parameters.
            /// </summary>
            public SaveDataParams Params
            {
                get { return sdParams; }
            }

            /// <summary>
            /// Save data folder size info.
            /// </summary>
            public SaveDataInfo Info
            {
                get { return sdInfo; }
            }

            internal void Read(MemoryBuffer buffer, bool hasParams, bool hasBlockInfo)
            {
                dirName.Read(buffer);

                if (hasParams == true)
                {
                    sdParams.Read(buffer);
                }

                if (hasBlockInfo == true)
                {
                    sdInfo.Read(buffer);
                }
            }
        }


        #region Response

        /// <summary>
        /// Response class that contains a list of found save data directory names.
        /// </summary>
        public class DirNameSearchResponse : ResponseBase
        {
            internal bool hasParams;
            internal bool hasBlockInfo;
            internal SearchSaveDataItem[] saveDataItems;

            /// <summary>
            /// Names of the flders found in a search.
            /// </summary>
            public SearchSaveDataItem[] SaveDataItems
            {
                get { ThrowExceptionIfLocked(); return saveDataItems; }
            }

            /// <summary>
            /// True if folder parameters are included in search results, false otherwise.
            /// </summary>
            public bool HasParams
            {
                get { ThrowExceptionIfLocked(); return hasParams; }
            }

            /// <summary>
            /// True if folder block size is included in search results, false otherwise.
            /// </summary>
            public bool HasInfo
            {
                get { ThrowExceptionIfLocked(); return hasBlockInfo; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DirNameSearchResponse"/> class.
            /// </summary>
            public DirNameSearchResponse()
            {

            }

            internal void Populate(APIResult result, MemoryBufferNative data)
            {
                base.Populate(result);

                MemoryBuffer readBuffer = new MemoryBuffer(data);
                readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                UInt32 foundCount = readBuffer.ReadUInt32();
                hasParams = readBuffer.ReadBool();
                hasBlockInfo = readBuffer.ReadBool();

                // now read any found savedata items
                saveDataItems = new SearchSaveDataItem[foundCount];

                for (int i = 0; i < foundCount; i++)
                {
                    saveDataItems[i] = new SearchSaveDataItem();

                    saveDataItems[i].Read(readBuffer, hasParams, hasBlockInfo);
                }

                readBuffer.CheckEndMarker();
            }
        }

        #endregion

        /// <summary>
        /// Searches for save data folders for a specified user ID. For more information, see Sony's documentation on sceSaveDataDirNameSearch for the PS5.
        /// </summary>
        /// <param name="request">Search parameters to use.</param>
        /// <param name="response">Search results.</param>
        /// <returns>If the operation is asynchronous, the method provides the request ID.</returns>
        /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
        public static int DirNameSearch(DirNameSearchRequest request, DirNameSearchResponse response)
        {
            DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

            PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

            return ProcessQueueThread.WaitIfSyncRequest(pe);
        }
    }
}

