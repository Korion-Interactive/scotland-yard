#if UNITY_PS5
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.PS5;
using System;
using System.IO;
using System.Collections.Generic;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Search;
using Unity.SaveData.PS5.Delete;

namespace SaveDataTests
{

    public partial class Tests : BaseTestFramework
    {
        string[] testDirNames = new string[] { "testA", "testB", "testC", "testD" };

        Mounting.MountPoint[] mountedSaves;

        static public System.UInt64 TestBlockSize = Mounting.MountRequest.BLOCKS_MIN + ((1024 * 1024 * 4) / Mounting.MountRequest.BLOCK_SIZE);

        [UnityTest, Order(20), Description("Initial Search to test no save data is present")]
        public IEnumerator B_InitialSearch()
        {
            yield return new WaitUntil(IsInitialized);

            var searchResponse = InternalSearch();

            yield return new WaitUntil(() => searchResponse.Locked == false);

            Assert.NotNull(searchResponse.SaveDataItems);

            Assert.AreEqual(0, searchResponse.SaveDataItems.Length, "Expected no saves games to be found");

            // Dirnames should be sorted in Searching.SearchSortKey.DirName order so should match the test data array
        //    for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
        //    {
        //        Debug.LogError(searchResponse.SaveDataItems[i].DirName.Data);

        //       // Assert.AreEqual(searchResponse.SaveDataItems[i].DirName.Data, testDirNames[i]);
        //    }
        }

        [UnityTest, Order(30), Description("Create Save Data")]
        public IEnumerator C_CreateSaveData()
        {
            yield return new WaitUntil(IsInitialized);

            mountedSaves = new Mounting.MountPoint[testDirNames.Length];

            Assert.AreEqual(IsInitialized(), true);

            for (int i = 0; i < testDirNames.Length; i++)
            {
                Mounting.MountResponse mountResponse = InternalMountSave(testDirNames[i], true);

                yield return new WaitUntil(() => mountResponse.Locked == false);

                Debug.Log("Mounted Save " + testDirNames[i] + " to " + mountResponse.MountPoint.PathName);

                mountedSaves[i] = mountResponse.MountPoint;
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                EmptyResponse unmountReponse = InternalUnmountSave(mountedSaves[i]);

                yield return new WaitUntil(() => unmountReponse.Locked == false);

                Debug.Log("Unmounted Save " + testDirNames[i] + " from " + mountedSaves[i].PathName);
            }

            Debug.Log("Create Save data finished");
        }

        [UnityTest, Order(40), Description("Search for newly created saves")]
        public IEnumerator D_SearchForNewSave()
        {
            yield return new WaitUntil(IsInitialized);

            var searchResponse = InternalSearch();

            yield return new WaitUntil(() => searchResponse.Locked == false);

            Assert.NotNull(searchResponse.SaveDataItems);

            Assert.AreEqual(testDirNames.Length, searchResponse.SaveDataItems.Length, "Expected same number of saves as the test directory array");

            // Dirnames should be sorted in Searching.SearchSortKey.DirName order so should match the test data array
            for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
            {
                Assert.AreEqual(searchResponse.SaveDataItems[i].DirName.Data, testDirNames[i], "Expecting list of saves to be in same alphabetical order as the test array");
            }

            Debug.Log("Expected saves are present and in the correct order");
        }

        // Write files
        [UnityTest, Order(50), Description("Write files to save directory")]
        public IEnumerator E_WriteFiles()
        {
            yield return new WaitUntil(IsInitialized);

            for (int i = 0; i < testDirNames.Length; i++)
            {
                Mounting.MountResponse mountResponse = InternalMountSave(testDirNames[i], true);

                yield return new WaitUntil(() => mountResponse.Locked == false);

                Debug.Log("Mounted Save " + testDirNames[i] + " to " + mountResponse.MountPoint.PathName);

                mountedSaves[i] = mountResponse.MountPoint;
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                TestWriteFilesResponse writeResponse = InternalWriteFiles(mountedSaves[i]);

                yield return new WaitUntil(() => writeResponse.Locked == false);

                Debug.Log("Files Written to Save " + testDirNames[i] + " : Size written = " + writeResponse.totalFileSizeWritten);

                Assert.AreEqual(2097273, writeResponse.totalFileSizeWritten, "Expected certain size of data to be written to the save data");
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                EmptyResponse unmountReponse = InternalUnmountSave(mountedSaves[i]);

                yield return new WaitUntil(() => unmountReponse.Locked == false);

                Debug.Log("Unmounted Save " + testDirNames[i] + " from " + mountedSaves[i].PathName);
            }

            Debug.Log("Files written");
        }

#if IL2CPP_DIRREAD_BUG_FIXED // disabled untill the il2cpp bug is fixed
        // Enumerate files
        [UnityTest, Order(60), Description("Enumerate files in the save data directory")]
        public IEnumerator F_EnumerateFiles()
        {
            yield return new WaitUntil(IsInitialized);

            for (int i = 0; i < testDirNames.Length; i++)
            {
                Mounting.MountResponse mountResponse = InternalMountSave(testDirNames[i], false);

                yield return new WaitUntil(() => mountResponse.Locked == false);

                Debug.Log("Mounted Save " + testDirNames[i] + " to " + mountResponse.MountPoint.PathName);

                mountedSaves[i] = mountResponse.MountPoint;
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                TestEnumerateFilesResponse enumerateResponse = InternalEnumerateFiles(mountedSaves[i]);

                yield return new WaitUntil(() => enumerateResponse.Locked == false);

                Assert.AreEqual(3, enumerateResponse.files.Length, "Expected 3 files to be written to the save data directory");
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                EmptyResponse unmountReponse = InternalUnmountSave(mountedSaves[i]);

                yield return new WaitUntil(() => unmountReponse.Locked == false);

                Debug.Log("Unmounted Save " + testDirNames[i] + " from " + mountedSaves[i].PathName);
            }

            Debug.Log("Files written");
        }
#endif

        // Read files
        [UnityTest, Order(70), Description("Read files from save directory")]
        public IEnumerator G_ReadFiles()
        {
            yield return new WaitUntil(IsInitialized);

            for (int i = 0; i < testDirNames.Length; i++)
            {
                Mounting.MountResponse mountResponse = InternalMountSave(testDirNames[i], false);

                yield return new WaitUntil(() => mountResponse.Locked == false);

                Debug.Log("Mounted Save " + testDirNames[i] + " to " + mountResponse.MountPoint.PathName);

                mountedSaves[i] = mountResponse.MountPoint;
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                TestReadFilesResponse readResponse = InternalReadFiles(mountedSaves[i]);

                yield return new WaitUntil(() => readResponse.Locked == false);

                Assert.AreEqual("This is some text test data which will be written to a file.", readResponse.myTestData, "Unexpected file contents");
                Assert.AreEqual("This is some more text which is written to another save file.", readResponse.myOtherTestData, "Unexpected file contents");

                for(int d = 0; d < 10; d++)
                {
                    Assert.AreEqual(d, readResponse.largeData[d], "Unexpected file contents in binary at index " + d);
                }
            }

            for (int i = 0; i < mountedSaves.Length; i++)
            {
                EmptyResponse unmountReponse = InternalUnmountSave(mountedSaves[i]);

                yield return new WaitUntil(() => unmountReponse.Locked == false);

                Debug.Log("Unmounted Save " + testDirNames[i] + " from " + mountedSaves[i].PathName);
            }

            Debug.Log("Files read");
        }

        // Delete save data
        [UnityTest, Order(80), Description("Delete save data")]
        public IEnumerator H_DeleteSaveData()
        {
            yield return new WaitUntil(IsInitialized);

            for (int i = 0; i < testDirNames.Length; i++)
            {
                EmptyResponse deleteResponse = InternalDeleteFiles(testDirNames[i]);

                yield return new WaitUntil(() => deleteResponse.Locked == false);

                Debug.Log("Deleted Save " + testDirNames[i]);

            }

            Debug.Log("Files Deleted");
        }

        public EmptyResponse InternalDeleteFiles(string dirTestName)
        {
            EmptyResponse response = new EmptyResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                Deleting.DeleteRequest request = new Deleting.DeleteRequest();

                DirName dirName = new DirName();
                dirName.Data = dirTestName;

                request.UserId = user.userId;
                request.DirName = dirName;     

                int requestId = Deleting.Delete(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        public TestWriteFilesResponse InternalWriteFiles(Mounting.MountPoint mp)
        {
            TestWriteFilesResponse response = new TestWriteFilesResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                TestWriteFilesRequest request = new TestWriteFilesRequest();

                request.UserId = user.userId;
                request.MountPointName = mp.PathName;

                int requestId = FileOps.CustomFileOp(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        public TestEnumerateFilesResponse InternalEnumerateFiles(Mounting.MountPoint mp)
        {
            TestEnumerateFilesResponse response = new TestEnumerateFilesResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                TestEnumerateFilesRequest request = new TestEnumerateFilesRequest();

                request.UserId = user.userId;
                request.MountPointName = mp.PathName;

                int requestId = FileOps.CustomFileOp(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        public TestReadFilesResponse InternalReadFiles(Mounting.MountPoint mp)
        {
            TestReadFilesResponse response = new TestReadFilesResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                TestReadFilesRequest request = new TestReadFilesRequest();

                request.UserId = user.userId;
                request.MountPointName = mp.PathName;

                int requestId = FileOps.CustomFileOp(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }


        public Searching.DirNameSearchResponse InternalSearch()
        {
            Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

                request.UserId = user.userId;

                request.Key = Searching.SearchSortKey.DirName;
                request.Order = Searching.SearchSortOrder.Ascending;
                request.IncludeBlockInfo = false;
                request.IncludeParams = false;
                request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;

                int requestId = Searching.DirNameSearch(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        public EmptyResponse InternalUnmountSave(Mounting.MountPoint mp)
        {
            EmptyResponse response = new EmptyResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                Mounting.UnmountRequest request = new Mounting.UnmountRequest();

                request.UserId = user.userId;
                request.MountPointName = mp.PathName;

                int requestId = Mounting.Unmount(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        public Mounting.MountResponse InternalMountSave(string testDirName, bool readWrite)
        {
            Mounting.MountResponse response = new Mounting.MountResponse();

            try
            {
                PS5Input.LoggedInUser user = GetUser();

                Mounting.MountRequest request = new Mounting.MountRequest();

                DirName dirName = new DirName();
                dirName.Data = testDirName;

                request.UserId = user.userId;
                request.Async = true;
                request.DirName = dirName;

                if (readWrite == true)
                {
                    request.MountMode = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;
                }
                else
                {
                    request.MountMode = Mounting.MountModeFlags.ReadOnly;
                }

                request.Blocks = TestBlockSize;

                int requestId = Mounting.Mount(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }


        public class TestWriteFilesRequest : FileOps.FileOperationRequest
        {
            public string myTestData = "This is some text test data which will be written to a file.";
            public string myOtherTestData = "This is some more text which is written to another save file.";
            public byte[] largeData = new byte[1024 * 1024 * 2];

            public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
            {
                TestWriteFilesResponse fileResponse = response as TestWriteFilesResponse;

                string outpath = mp.PathName.Data + "/MyTestFile.txt";

                File.WriteAllText(outpath, myTestData);

                FileInfo info = new FileInfo(outpath);
                fileResponse.totalFileSizeWritten = info.Length;

                string outpath2 = mp.PathName.Data + "/MyOtherTestFile.txt";

                File.WriteAllText(outpath2, myOtherTestData);

                info = new FileInfo(outpath2);
                fileResponse.totalFileSizeWritten += info.Length;

                string outpath3 = mp.PathName.Data + "/TestData.dat";

                int totalWritten = 0;

                for(int i = 0; i < 10; i++)
                {
                    largeData[i] = (byte)i;
                }

                // Example of updating the progress value.
                using (FileStream fs = File.OpenWrite(outpath3))
                {
                    // Add some information to the file.
                    while (totalWritten < largeData.Length)
                    {
                        int writeSize = Math.Min(largeData.Length - totalWritten, 1000); // Write up to 1000 bytes

                        fs.Write(largeData, totalWritten, writeSize);

                        totalWritten += writeSize;

                        // Update progress value during saving
                        response.UpdateProgress((float)totalWritten / (float)largeData.Length);
                    }
                }

                info = new FileInfo(outpath3);
                fileResponse.lastWriteTime = info.LastWriteTime;
                fileResponse.totalFileSizeWritten += info.Length;
            }
        }

        public class TestWriteFilesResponse : FileOps.FileOperationResponse
        {
            public DateTime lastWriteTime;
            public long totalFileSizeWritten;
        }

        public class TestEnumerateFilesRequest : FileOps.FileOperationRequest
        {
            public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
            {
                TestEnumerateFilesResponse fileResponse = response as TestEnumerateFilesResponse;

                string outpath = mp.PathName.Data;

                string[] textFiles = Directory.GetFiles(outpath, "*.txt", SearchOption.AllDirectories);
                string[] dataFiles = Directory.GetFiles(outpath, "*.dat", SearchOption.AllDirectories);

                List<string> allFiles = new List<string>();

                allFiles.InsertRange(0, textFiles);
                allFiles.InsertRange(0, dataFiles);

                fileResponse.files = allFiles.ToArray();
            }
        }

        public class TestEnumerateFilesResponse : FileOps.FileOperationResponse
        {
            public string[] files;
        }

        public class TestReadFilesRequest : FileOps.FileOperationRequest
        {
            public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
            {
                TestReadFilesResponse fileResponse = response as TestReadFilesResponse;

                string outpath = mp.PathName.Data + "/MyTestFile.txt";

                fileResponse.myTestData = File.ReadAllText(outpath);

                string outpath2 = mp.PathName.Data + "/MyOtherTestFile.txt";

                fileResponse.myOtherTestData = File.ReadAllText(outpath2);

                string outpath3 = mp.PathName.Data + "/TestData.dat";

                //fileResponse.largeData = File.ReadAllBytes(outpath3);

                FileInfo info = new FileInfo(outpath3);

                fileResponse.largeData = new byte[info.Length];

                int totalRead = 0;

                // Example of updating the progress value.
                using (FileStream fs = File.OpenRead(outpath3))
                {
                    // Add some information to the file.
                    while (totalRead < info.Length)
                    {
                        int readSize = Math.Min((int)info.Length - totalRead, 1000); // read up to 1000 bytes

                        fs.Read(fileResponse.largeData, totalRead, readSize);

                        totalRead += readSize;

                        // Update progress value during saving
                        response.UpdateProgress((float)totalRead / (float)info.Length);
                    }
                }
            }
        }

        public class TestReadFilesResponse : FileOps.FileOperationResponse
        {
            public string myTestData;
            public string myOtherTestData;
            public byte[] largeData;
        }

    }
}
#endif

