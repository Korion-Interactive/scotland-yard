
# Using system dialogs

The steps on this page show how to use the save dialog state machine. 

This package provides a state machine as an example of a save, load or delete sequence. Not all titles need to do this sequence. Your title might need to provide its own state machine for saving data.

The examples below must run inside a `MonoBehaviour` which contains the call to `StartCoroutine`.

## Overview

This page demonstrates how to:

* Set up the save data parameters
* Create the custom file operations
* Start the save dialog process

## Save process dialog

The code example below shows how to start the save dialog coroutine using `SaveDataDialogProcess.StartSaveDialogProcess`.

It also uses a custom file operations class to write the data. See [Creating a save data](CreateSequence.md) for an example of how to use the `ExampleWriteFilesRequest` class.

```CSharp
    // Set the user id for the saves
    int userId = userId;

    // Create the new item for the saves dialog list
    Dialogs.NewItem newItem = null;

    string title = "New save data title";
    newItem = new Dialogs.NewItem();
    newItem.Title = title;

    // The directory name for a new save data
    DirName newDirName = new DirName();
    newDirName.Data = "NewSaveData";

    // Maximum size of the new save data
    UInt64 newSaveDataBlocks = myMaximumBlockSize;

    // Parameters to use for the save data
    SaveDataParams saveDataParams = new SaveDataParams();
    saveDataParams.Title = title;
    saveDataParams.SubTitle = "Subtitle for save data";
    saveDataParams.Detail = "Details for save data";

    // Actual custom file operation to perform on the save data, once it is mounted.
    ExampleWriteFilesRequest fileRequest = new ExampleWriteFilesRequest();
    fileRequest.TextData = "Custom data to write";

    ExampleWriteFilesResponse fileResponse = new ExampleWriteFilesResponse();

    // A callback allowing the caller to specify whether the dialog should allow a new save data or that the user must overwrite an existing one. If the callback is null, the dialog will always allow a new save data item
    SaveDataDialogProcess.AllowNewItemTest allowNewItemCB = AllowNewItemCB;

    // Start the Unity coroutine that will run the SaveDataDialogProcess state machine. 
    StartCoroutine(SaveDataDialogProcess.StartSaveDialogProcess(userId, newItem, newDirName, newSaveDataBlocks, saveDataParams, fileRequest, fileResponse, backup, allowNewItemCB));
```

## Load process dialog

Use the `SaveDataDialogProcess.StartLoadDialogProcess` coroutine to load a save data using the system dialogs.

## Delete dialog

Use the `SaveDataDialogProcess.StartDeleteDialogProcess` coroutine to load a save data using the system dialogs.

