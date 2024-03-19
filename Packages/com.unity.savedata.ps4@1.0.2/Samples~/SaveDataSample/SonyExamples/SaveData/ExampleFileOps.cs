using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveDataSample
{

public class ExampleWriteFilesRequest : Sony.PS4.SaveData.FileOps.FileOperationRequest
{
    public string myTestData = "This is some text test data which will be written to a file.";
    public string myOtherTestData = "This is some more text which is written to another save file.";
    public byte[] largeData = new byte[1024 * 1024 * 2];

    public override void DoFileOperations(Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.FileOps.FileOperationResponse response)
    {
        ExampleWriteFilesResponse fileResponse = response as ExampleWriteFilesResponse;

        string outpath = mp.PathName.Data + "/MySaveFile.txt";

        File.WriteAllText(outpath, myTestData);

        FileInfo info = new FileInfo(outpath);
        fileResponse.totalFileSizeWritten = info.Length;

        string outpath2 = mp.PathName.Data + "/MyOtherSaveFile.txt";

        File.WriteAllText(outpath2, myOtherTestData);

        info = new FileInfo(outpath2);
        fileResponse.totalFileSizeWritten += info.Length;

        string outpath3 = mp.PathName.Data + "/Data.dat";

        // File.WriteAllBytes(outpath2, largeData);
        int totalWritten = 0;

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

public class ExampleWriteFilesResponse : Sony.PS4.SaveData.FileOps.FileOperationResponse
{
    public DateTime lastWriteTime;
    public long totalFileSizeWritten;
}

public class ExampleEnumerateFilesRequest : Sony.PS4.SaveData.FileOps.FileOperationRequest
{
    public override void DoFileOperations(Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.FileOps.FileOperationResponse response)
    {
        ExampleEnumerateFilesResponse fileResponse = response as ExampleEnumerateFilesResponse;

        string outpath = mp.PathName.Data;

        fileResponse.files = Directory.GetFiles(outpath, "*.txt", SearchOption.AllDirectories);
    }
}

public class ExampleEnumerateFilesResponse : Sony.PS4.SaveData.FileOps.FileOperationResponse
{
    public string[] files;
}

public class ExampleReadFilesRequest : Sony.PS4.SaveData.FileOps.FileOperationRequest
{
    public override void DoFileOperations(Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.FileOps.FileOperationResponse response)
    {
        ExampleReadFilesResponse fileResponse = response as ExampleReadFilesResponse;

        string outpath = mp.PathName.Data + "/MySaveFile.txt";

        fileResponse.myTestData = File.ReadAllText(outpath);

        string outpath2 = mp.PathName.Data + "/MyOtherSaveFile.txt";

        fileResponse.myOtherTestData = File.ReadAllText(outpath2);

        string outpath3 = mp.PathName.Data + "/Data.dat";

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

public class ExampleReadFilesResponse : Sony.PS4.SaveData.FileOps.FileOperationResponse
{
    public string myTestData;
    public string myOtherTestData;
    public byte[] largeData;
}
}
