
# Creating a save data file

The steps on this page show how to:

* create a save data file
* read and write files

The steps provide a high-level overview of the required sequence of requests. Some of the API detail has been omitted for clarity. 

## Overview

This page demonstrates how to:

1. Mount the save data in read/write mode
2. Write the parameters of the save data
3. Write a save data icon
4. Write the save data files into the save data
5. Unmount the save data

## Mount the save data in read/write mode

```CSharp
    Mounting.MountRequest request = new Mounting.MountRequest();

    DirName dirName = new DirName();
    dirName.Data = "MySaveDataName"; // Unique name for the save data, not visible by the player

    request.UserId = userId;
    request.DirName = dirName;
    request.MountMode = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;
    request.Blocks = maximumBlockSizeForTitle; // Set the maximum number of blocks a save data will require

    // Response object containing the mount point once the save data has been created.
    Mounting.MountResponse response = new Mounting.MountResponse();

    int requestId = Mounting.Mount(request, response);
```

This code example asynchronously creates a save data file and then opens it for read and write access. If the save data already exists, the code opens the existing data for read and write accesss.

The `MountResponse` object returns the `MountPoint` which is used in subsequent operations on the mounted save data. 

## Write the parameters of the save data

The code example below shows how to write displayable info to the save data.

```CSharp
    Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

    request.UserId = userId;
    request.MountPointName = mp.PathName;

    SaveDataParams sdParams = new SaveDataParams();

    // Set displayable parameters for the save data.
    sdParams.Title = "My Save Data";
    sdParams.SubTitle = "My Save Data Subtitle";
    sdParams.Detail = "This is the long description of the save data.";
    
    request.Params = sdParams;

    // This request doesn't have any response data, but the callback will still happen to indicate the request has completed.
    EmptyResponse response = new EmptyResponse();

    int requestId = Mounting.SetMountParams(request, response);
```

## Write a save data icon

The code example below shows how to set the icon to use for the save data. 

```CSharp
    Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

    request.UserId = userId;
    request.MountPointName = mp.PathName;
   
    // Pathname to a save icon. This also supports writing an icon
    // using a PNG from memory.
    request.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";

    EmptyResponse response = new EmptyResponse();

    int requestId = Mounting.SaveIcon(request, response);
```

## Write the save data files into the save data

The system provides an asynchronous method to read, write, and enumerate files stored inside a save data. 

Your title can provide custom request types that can read and write files using the systems request thread.

```CSharp
    // The custom class provided by your title to write save data
    public class ExampleWriteFilesRequest : FileOps.FileOperationRequest
    {
        public string TextData { get; set; }

        public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
        {
             ExampleWriteFilesResponse fileResponse = response as ExampleWriteFilesResponse;

             string outpath = mp.PathName.Data + "/MySaveFile.txt";

             File.WriteAllText(outpath, TextData);

             // Read the info about the file just written and set this on the custom response object.
             FileInfo info = new FileInfo(outpath);
             fileResponse.lastWriteTime = info.LastWriteTime;
             fileResponse.totalFileSizeWritten = info.Length;
        }
    }

    // The custom class containing any response data the title might want to retrieve from the operation
    public class ExampleWriteFilesResponse : FileOps.FileOperationResponse
    {
        public DateTime lastWriteTime;
        public long totalFileSizeWritten;
    }
```

`DoFileOperations` is called when the request is processed on the systems background thread. You can use any standard .NET File operation, including create directories. For example, to enumerate all the files inside the save data, you can use the .NET `Directory.GetFiles` method. When performing file operations, the `MountPoint` is used as the first part of the path name.

To call the ExampleWriteFilesRequest request:

```CSharp
    ExampleWriteFilesRequest request = new ExampleWriteFilesRequest();

    request.UserId = userId;
    request.MountPointName = mp.PathName;
    request.TextData = "Custom data to write";

    ExampleWriteFilesResponse response = new ExampleWriteFilesResponse();

    int requestId = FileOps.CustomFileOp(request, response);
```

## Unmount the save data

After the save data has been written, it must be unmounted. There are limits on the number of mount points that can be in use at any one time, and also how long a save data can remain mounted while in write mode. For more information on these limits, please refer to the Sony documentation.

```CSharp
    Mounting.UnmountRequest request = new Mounting.UnmountRequest();

    request.UserId = userId;
    request.MountPointName = mp.PathName;

    EmptyResponse response = new EmptyResponse();

    int requestId = Mounting.Unmount(request, response);
```

