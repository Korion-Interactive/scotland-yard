using System;
using System.Collections;
using System.Linq;

namespace Sony
{
    namespace PS4
    {
        namespace SaveData
        {
            /// <summary>
            /// Dialog state machine for running the save/load/delete savedata process
            /// </summary>
            public class SaveDataDialogProcess
            {
                static Sony.PS4.SaveData.DirName emptyDirName = new Sony.PS4.SaveData.DirName();

                /// <summary>
                /// Save process states
                /// </summary>
                private enum SaveState
                {
                    Begin,
                    Searching,
                    ShowListStart,
                    ShowList,
                    ShowNoDataStart,

                    ShowSaveStart,
                    ShowSaveWaitForDialog,
                    ShowSave,

                    ShowLoadStart,
                    ShowLoadWaitForDialog,
                    ShowLoad,

                    ShowDeleteStart,
                    ShowDeleteWaitForDialog,
                    ShowDelete,

                    OverwriteStart,
                    Overwrite,

                    ConfirmDeleteStart,
                    ConfirmDelete,

                    ConfirmRestoreStart,
                    ConfirmRestore,
                    ShowRestoreStart,
                    ShowRetoreWaitForDialog,
                    ShowRestore,

                    ShowErrorNoSpaceStart,
                    ShowErrorNoSpace,
                    ShowErrorStart,
                    ShowError,

                    Finished,

                    Exit,
                }

                /// <summary>
                /// Callback for the user to specify whether a new item should be allowed in a save.
                /// </summary>
                /// <remarks>
                /// This allows the user to comply with TRC 4100.
                /// Given the information in the DirNameSearchResponse,
                /// the user can return whether to allow (<c>true</c>) or disallow (<c>false</c>)
                /// a new item to be created.
                /// </remarks>
                /// 
                /// To comply with TRC 4100, the callback should disallow a new item
                /// if the total size of all savedatas would exceed 1 GiB.
                /// <param name="response">The result of the DirNameSearch</param>
                /// <returns>Whether to allow (true) or disallow (false) a new item.</returns>
                public delegate bool AllowNewItemTest (Searching.DirNameSearchResponse response);


                /// <summary>
                /// Start the save process as a Unity Coroutine.
                /// </summary>
                /// <param name="userId">The userId who will save data</param>
                /// <param name="newItem">The new item details displayed in the save list</param>
                /// <param name="newDirName">The directory name of a new save data if new save is selected from the list.</param>
                /// <param name="newSaveDataBlocks">The size of a new save data if new save is selected from the list.</param>
                /// <param name="saveDataParams">The save data params for a new save data or overwritten save data.</param>
                /// <param name="fileRequest">The custom file IO operations for the actually files inside the save data</param>
                /// <param name="fileResponse">The custom file IO response containing the results of the file operation</param>
                /// <param name="backup">Should the save data be backed up when saving is complete. This will generate a notification once backup is complete.</param>
                /// <param name="allowNewCB">Callback to determine whether a new item should be allowed in the dialog showing a list of current saves.</param>
                /// <returns>The enumerator, which can be used in a Unity coroutine.</returns>
                public static IEnumerator StartSaveDialogProcess(int userId, Sony.PS4.SaveData.Dialogs.NewItem newItem, Sony.PS4.SaveData.DirName newDirName, UInt64 newSaveDataBlocks, Sony.PS4.SaveData.SaveDataParams saveDataParams,
                                                                  Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse, bool backup, AllowNewItemTest allowNewCB)
                {
                    SaveState currentState = SaveState.Begin;

                    Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse openDialogResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse progressBarResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse overwriteResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse confirmRestoreResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.EmptyResponse restoreResponse = new Sony.PS4.SaveData.EmptyResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse errorResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse noSpaceResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Mounting.MountResponse mountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();

                    int errorCode = 0;

                    Sony.PS4.SaveData.DirName selectedDirName = new Sony.PS4.SaveData.DirName();

                    while (currentState != SaveState.Exit)
                    {
                        switch (currentState)
                        {
                            case SaveState.Begin:
                                errorCode = FullSearch(userId, searchResponse);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.Searching;
                                }
                                break;
                            case SaveState.Searching:
                                if (searchResponse.Locked == false)
                                {
                                    if (searchResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = searchResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // Search has completed
                                        currentState = SaveState.ShowListStart;
                                    }
                                }
                                break;
                            case SaveState.ShowListStart:
                                // Show the save dialog.
                                // A callback allows the caller to specify whether the dialog should allow a new save data,
                                // or that the user must overwrite an existing one.
                                bool allowNew = allowNewCB != null ? allowNewCB(searchResponse) : true;
                                errorCode = ListDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, openDialogResponse, searchResponse, allowNew ? newItem : null);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowList;
                                }
                                break;
                            case SaveState.ShowList:

                                if (openDialogResponse.Locked == false)
                                {
                                    if (openDialogResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = openDialogResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            if (dialogResult.DirName.IsEmpty == true)
                                            {
                                                // New save here
                                                currentState = SaveState.ShowSaveStart;
                                            }
                                            else
                                            {
                                                selectedDirName = dialogResult.DirName;
                                                currentState = SaveState.OverwriteStart;
                                            }
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowSaveStart:
                                {
                                    Sony.PS4.SaveData.Dialogs.NewItem useNewItem = newItem;
                                    Sony.PS4.SaveData.DirName dirName = newDirName;

                                    // If an existing directory name has been selected then use that and don't use the newItem
                                    if (selectedDirName.IsEmpty == false)
                                    {
                                        useNewItem = null;
                                        dirName = selectedDirName;
                                    }

                                    errorCode = ProgressBarDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, progressBarResponse, useNewItem, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Progress, dirName);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ShowSaveWaitForDialog;
                                    }
                                }
                                break;
                            case SaveState.ShowSaveWaitForDialog:
                                {
                                    var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                                    if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
                                        {
                                            currentState = SaveState.ShowSave;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowSave:
                                {
                                    // At this point the save list dialog is displayed and its safe to mount a save data in read/write mode
                                    Sony.PS4.SaveData.Mounting.MountModeFlags flags = Sony.PS4.SaveData.Mounting.MountModeFlags.Create2 | Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite;

                                    Sony.PS4.SaveData.DirName dirName;

                                    // Is this a new save data name or an existing selected one
                                    if (selectedDirName.IsEmpty == false)
                                    {
                                        dirName = selectedDirName; // Existing save data - overwrite
                                    }
                                    else
                                    {
                                        dirName = newDirName; // Use new save data
                                    }

                                    errorCode = MountSaveData(userId, newSaveDataBlocks, mountResponse, dirName, flags);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        // Wait for save data to be mounted.
                                        while (mountResponse.Locked == true)
                                        {
                                            yield return null;
                                        }

                                        if (mountResponse.IsErrorCode == true)
                                        {
                                            if (mountResponse.ReturnCode == Sony.PS4.SaveData.ReturnCodes.DATA_ERROR_NO_SPACE_FS)
                                            {
                                                currentState = SaveState.ShowErrorNoSpaceStart;
                                            }
                                            else if (mountResponse.ReturnCode == Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                            {
                                                Sony.PS4.SaveData.Backups.CheckBackupResponse backupResponse = new Sony.PS4.SaveData.Backups.CheckBackupResponse();
                                                // Test if backup save data exists
                                                errorCode = CheckBackup(userId, backupResponse, dirName);

                                                if (errorCode < 0)
                                                {
                                                    currentState = SaveState.ShowErrorStart;
                                                }
                                                else
                                                {
                                                    while (backupResponse.Locked == true)
                                                    {
                                                        yield return null;
                                                    }

                                                    if (backupResponse.IsErrorCode == true)
                                                    {
                                                        currentState = SaveState.ShowErrorStart;
                                                        errorCode = mountResponse.ReturnCodeValue;
                                                    }
                                                    else
                                                    {
                                                        currentState = SaveState.ConfirmRestoreStart;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                                errorCode = mountResponse.ReturnCodeValue;
                                            }
                                        }
                                        else
                                        {
                                            // Save data is now mounted, so get mountpoint
                                            Sony.PS4.SaveData.Mounting.MountPoint mp = mountResponse.MountPoint;

                                            // Do actual saving
                                            fileRequest.MountPointName = mp.PathName;
                                            fileRequest.Async = true;
                                            fileRequest.UserId = userId;

                                            errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(fileRequest, fileResponse);

                                            if (errorCode < 0)
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                            }
                                            else
                                            {
                                                while (fileResponse.Locked == true)
                                                {
                                                    Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                                    yield return null;
                                                }

                                                Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                                // Write the icon and any detail parmas set here.
                                                Sony.PS4.SaveData.EmptyResponse iconResponse = new Sony.PS4.SaveData.EmptyResponse();

                                                errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                                                if (errorCode < 0)
                                                {
                                                    currentState = SaveState.ShowErrorStart;
                                                }
                                                else
                                                {
                                                    Sony.PS4.SaveData.EmptyResponse paramsResponse = new Sony.PS4.SaveData.EmptyResponse();

                                                    errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);

                                                    if (errorCode < 0)
                                                    {
                                                        currentState = SaveState.ShowErrorStart;
                                                    }
                                                    else
                                                    {
                                                        // Wait for save icon to be mounted.
                                                        while (iconResponse.Locked == true || paramsResponse.Locked == true)
                                                        {
                                                            yield return null;
                                                        }

                                                        // unmount the save data
                                                        Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                                                        errorCode = UnmountSaveData(userId, unmountResponse, mp, backup);

                                                        if (errorCode < 0)
                                                        {
                                                            currentState = SaveState.ShowErrorStart;
                                                        }
                                                        else
                                                        {
                                                            while (unmountResponse.Locked == true)
                                                            {
                                                                yield return null;
                                                            }

                                                            // Save data unmounted so close the progress bar dialog
                                                            ForceCloseDialog();

                                                            currentState = SaveState.Finished;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaveState.OverwriteStart:

                                errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, overwriteResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.Overwrite, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.Overwrite;
                                }
                                break;
                            case SaveState.Overwrite:

                                if (overwriteResponse.Locked == false)
                                {
                                    if (overwriteResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = overwriteResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = overwriteResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            if (dialogResult.ButtonId == Sony.PS4.SaveData.Dialogs.DialogButtonIds.Yes)
                                            {
                                                // New save here
                                                currentState = SaveState.ShowSaveStart;
                                            }
                                            else
                                            {
                                                currentState = SaveState.Finished;
                                            }
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ConfirmRestoreStart:

                                // Must close any open dialog. This should be the progess bar for saving the file, but the save data was corrupted so display a confirm message.
                                // There is no need to animate the entire dialog box.
                                ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                {
                                    yield return null;
                                }

                                errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, confirmRestoreResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.CurruptedAndRestore, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ConfirmRestore;
                                }
                                break;
                            case SaveState.ConfirmRestore:

                                if (confirmRestoreResponse.Locked == false)
                                {
                                    if (confirmRestoreResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = confirmRestoreResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = confirmRestoreResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            if (dialogResult.ButtonId == Sony.PS4.SaveData.Dialogs.DialogButtonIds.Yes)
                                            {
                                                // New save here
                                                currentState = SaveState.ShowRestoreStart;
                                            }
                                            else
                                            {
                                                currentState = SaveState.Finished;
                                            }
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;

                            case SaveState.ShowRestoreStart:

                                // Open progress bar dialog for restore
                                progressBarResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

                                errorCode = ProgressBarDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, progressBarResponse, null, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Restore, emptyDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowRetoreWaitForDialog;
                                }

                                break;
                            case SaveState.ShowRetoreWaitForDialog:
                                {
                                    var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                                    if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
                                        {
                                            Sony.PS4.SaveData.Progress.ClearProgress();

                                            errorCode = RestoreBackup(userId, restoreResponse, selectedDirName);

                                            if (errorCode < 0)
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                            }
                                            else
                                            {
                                                currentState = SaveState.ShowRestore;
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowRestore:

                                if (restoreResponse.Locked == false)
                                {
                                    // Backup restore is complete.
                                    // The progress bar for restoring a savedata from back is currently displayed.
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    currentState = SaveState.ShowSaveStart;
                                }
                                else
                                {
                                    float progress = Sony.PS4.SaveData.Progress.GetProgress();

                                    Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(progress * 100.0f));
                                }
                                break;
                            case SaveState.ShowErrorNoSpaceStart:
                                {
                                    // The progress bar dialog will be displayed at this point so close it.
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    Sony.PS4.SaveData.Dialogs.NewItem useNewItem = newItem;

                                    if (selectedDirName.IsEmpty == false)
                                    {
                                        useNewItem = null;
                                    }

                                    // Note - This needs to show the RequiredBlocks from the mounting process and not the actual blocks size required for the save data. These numbers can be very different.
                                    errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, noSpaceResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.NoSpace, emptyDirName,
                                                              Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On, useNewItem, mountResponse.RequiredBlocks);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ShowErrorNoSpace;
                                    }
                                }
                                break;
                            case SaveState.ShowErrorNoSpace:

                                while (noSpaceResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                if (noSpaceResponse.IsErrorCode)
                                {
                                    // An error has occured
                                    currentState = SaveState.ShowErrorStart;
                                    errorCode = noSpaceResponse.ReturnCodeValue;
                                }
                                else
                                {
                                    // Confirm no-space dialog has completed
                                    currentState = SaveState.Finished;
                                }
                                break;
                            case SaveState.ShowErrorStart:
                                {
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    if (ErrorDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Save, errorResponse, errorCode) == false)
                                    {
                                        currentState = SaveState.Finished;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ShowError;
                                    }
                                }
                                break;
                            case SaveState.ShowError:

                                if (errorResponse.Locked == false)
                                {
                                    currentState = SaveState.Finished;
                                }

                                break;
                            case SaveState.Finished:
                                currentState = SaveState.Exit;
                                break;
                        }

                        yield return null;
                    }
                }


                /// <summary>
                /// Start the load process as a Unity Coroutine.
                /// </summary>
                /// <param name="userId">The userId who will save data</param>
                /// <param name="fileRequest">The custom file IO operations for the actually files inside the save data</param>
                /// <param name="fileResponse">The custom file IO response containing the results of the file operation</param>
                /// <returns>The enumerator, which can be used in a Unity coroutine.</returns>
                public static IEnumerator StartLoadDialogProcess(int userId, Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse)
                {
                    SaveState currentState = SaveState.Begin;

                    Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse openDialogResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse progressBarResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse confirmRestoreResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.EmptyResponse restoreResponse = new Sony.PS4.SaveData.EmptyResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse errorResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse noDataResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

                    int errorCode = 0;

                    Sony.PS4.SaveData.DirName selectedDirName = new Sony.PS4.SaveData.DirName();

                    while (currentState != SaveState.Exit)
                    {
                        switch (currentState)
                        {
                            case SaveState.Begin:
                                errorCode = FullSearch(userId, searchResponse);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.Searching;
                                }
                                break;
                            case SaveState.Searching:
                                if (searchResponse.Locked == false)
                                {
                                    if (searchResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = searchResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // Search has completed
                                        if (searchResponse.SaveDataItems != null && searchResponse.SaveDataItems.Length > 0)
                                        {
                                            currentState = SaveState.ShowListStart;
                                        }
                                        else
                                        {
                                            currentState = SaveState.ShowNoDataStart;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowNoDataStart:

                                errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, noDataResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.NoData, emptyDirName,
                                                          Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    while (noDataResponse.Locked == true)
                                    {
                                        yield return null;
                                    }

                                    if (noDataResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = noDataResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // Confirm no-space dialog has completed
                                        currentState = SaveState.Finished;
                                    }
                                }
                                break;
                            case SaveState.ShowListStart:

                                errorCode = ListDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, openDialogResponse, searchResponse, null);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowList;
                                }
                                break;
                            case SaveState.ShowList:

                                if (openDialogResponse.Locked == false)
                                {
                                    if (openDialogResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = openDialogResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            // New load here
                                            selectedDirName = dialogResult.DirName;
                                            currentState = SaveState.ShowLoadStart;
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowLoadStart:

                                errorCode = ProgressBarDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, progressBarResponse, null, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Progress, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowLoadWaitForDialog;
                                }
                                break;
                            case SaveState.ShowLoadWaitForDialog:
                                {
                                    var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                                    if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
                                        {
                                            currentState = SaveState.ShowLoad;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowLoad:

                                // At this point the save list dialog is displayed and its safe to mount a save data in read/write mode

                                Sony.PS4.SaveData.Mounting.MountResponse mountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();

                                Sony.PS4.SaveData.Mounting.MountModeFlags flags = Sony.PS4.SaveData.Mounting.MountModeFlags.ReadOnly;

                                Sony.PS4.SaveData.DirName dirName = selectedDirName;

                                errorCode = MountSaveData(userId, 0, mountResponse, dirName, flags);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    // Wait for save data to be mounted.
                                    while (mountResponse.Locked == true)
                                    {
                                        yield return null;
                                    }

                                    if (mountResponse.IsErrorCode == true)
                                    {
                                        if (mountResponse.ReturnCode == Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                        {
                                            Sony.PS4.SaveData.Backups.CheckBackupResponse backupResponse = new Sony.PS4.SaveData.Backups.CheckBackupResponse();
                                            // Test if backup save data exists
                                            errorCode = CheckBackup(userId, backupResponse, dirName);

                                            if (errorCode < 0)
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                            }
                                            else
                                            {
                                                while (backupResponse.Locked == true)
                                                {
                                                    yield return null;
                                                }

                                                if (backupResponse.IsErrorCode == true)
                                                {
                                                    currentState = SaveState.ShowErrorStart;
                                                    errorCode = mountResponse.ReturnCodeValue;
                                                }
                                                else
                                                {
                                                    currentState = SaveState.ConfirmRestoreStart;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            currentState = SaveState.ShowErrorStart;
                                            errorCode = mountResponse.ReturnCodeValue;
                                        }
                                    }
                                    else
                                    {
                                        // Save data is now mounted, so get mountpoint
                                        Sony.PS4.SaveData.Mounting.MountPoint mp = mountResponse.MountPoint;

                                        // Do actual saving
                                        fileRequest.MountPointName = mp.PathName;
                                        fileRequest.Async = true;
                                        fileRequest.UserId = userId;

                                        errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(fileRequest, fileResponse);

                                        if (errorCode < 0)
                                        {
                                            currentState = SaveState.ShowErrorStart;
                                        }
                                        else
                                        {
                                            while (fileResponse.Locked == true)
                                            {
                                                Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                                yield return null;
                                            }

                                            // Update the last progress value as this will have been updated on another thread reading the fileResponse. 
                                            // As long as the developer has set the Progress value to 1.0 the progress dialog will show 100%
                                            Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                            // Yeild for a frame to make sure the progress bar dialog gets a chance to refresh.
                                            yield return null;

                                            // unmount the save data
                                            Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                                            errorCode = UnmountSaveData(userId, unmountResponse, mp, false);

                                            if (errorCode < 0)
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                            }
                                            else
                                            {
                                                while (unmountResponse.Locked == true)
                                                {
                                                    yield return null;
                                                }

                                                // Save data unmounted so close the progress bar dialog
                                                ForceCloseDialog();

                                                currentState = SaveState.Finished;
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaveState.ConfirmRestoreStart:

                                // Must close any open dialog. This should be the progess bar for saving the file, but the save data was corrupted so display a confirm message.
                                // There is no need to animate the entire dialog box.
                                ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                {
                                    yield return null;
                                }

                                errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, confirmRestoreResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.CurruptedAndRestore, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ConfirmRestore;
                                }
                                break;
                            case SaveState.ConfirmRestore:

                                if (confirmRestoreResponse.Locked == false)
                                {
                                    if (confirmRestoreResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = confirmRestoreResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = confirmRestoreResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            if (dialogResult.ButtonId == Sony.PS4.SaveData.Dialogs.DialogButtonIds.Yes)
                                            {
                                                // New save here
                                                currentState = SaveState.ShowRestoreStart;
                                            }
                                            else
                                            {
                                                currentState = SaveState.Finished;
                                            }
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;

                            case SaveState.ShowRestoreStart:

                                // Open progress bar dialog for restore
                                progressBarResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

                                errorCode = ProgressBarDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, progressBarResponse, null, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Restore, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowRetoreWaitForDialog;
                                }

                                break;
                            case SaveState.ShowRetoreWaitForDialog:
                                {
                                    var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                                    if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
                                        {
                                            Sony.PS4.SaveData.Progress.ClearProgress();

                                            errorCode = RestoreBackup(userId, restoreResponse, selectedDirName);

                                            if (errorCode < 0)
                                            {
                                                currentState = SaveState.ShowErrorStart;
                                            }
                                            else
                                            {
                                                currentState = SaveState.ShowRestore;
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowRestore:

                                if (restoreResponse.Locked == false)
                                {
                                    // Backup restore is complete.
                                    // The progress bar for restoring a savedata from back is currently displayed.
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    currentState = SaveState.ShowLoadStart;
                                }
                                else
                                {
                                    float progress = Sony.PS4.SaveData.Progress.GetProgress();

                                    Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(progress * 100.0f));
                                }
                                break;
                            case SaveState.ShowErrorStart:
                                {
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    if (ErrorDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Load, errorResponse, errorCode) == false)
                                    {
                                        currentState = SaveState.Finished;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ShowError;
                                    }
                                }
                                break;
                            case SaveState.ShowError:

                                if (errorResponse.Locked == false)
                                {
                                    currentState = SaveState.Finished;
                                }

                                break;
                            case SaveState.Finished:
                                currentState = SaveState.Exit;
                                break;
                        }

                        yield return null;
                    }
                }

                /// <summary>
                /// Start the delete process as a Unity Coroutine.
                /// </summary>
                /// <param name="userId">The userId who will save data</param>
                /// <returns>The enumerator, which can be used in a Unity coroutine.</returns>
                public static IEnumerator StartDeleteDialogProcess(int userId)
                {
                    SaveState currentState = SaveState.Begin;

                    Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse openDialogResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse progressBarResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse confirmDeleteResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse errorResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();
                    Sony.PS4.SaveData.Dialogs.OpenDialogResponse noDataResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

                    int errorCode = 0;

                    Sony.PS4.SaveData.DirName selectedDirName = new Sony.PS4.SaveData.DirName();

                    while (currentState != SaveState.Exit)
                    {
                        switch (currentState)
                        {
                            case SaveState.Begin:
                                errorCode = FullSearch(userId, searchResponse);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.Searching;
                                }
                                break;
                            case SaveState.Searching:
                                if (searchResponse.Locked == false)
                                {
                                    if (searchResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = searchResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // Search has completed
                                        if (searchResponse.SaveDataItems != null && searchResponse.SaveDataItems.Length > 0)
                                        {
                                            currentState = SaveState.ShowListStart;
                                        }
                                        else
                                        {
                                            currentState = SaveState.ShowNoDataStart;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowNoDataStart:

                                errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Delete, noDataResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.NoData, emptyDirName,
                                                          Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    while (noDataResponse.Locked == true)
                                    {
                                        yield return null;
                                    }

                                    if (noDataResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = noDataResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // Confirm no-space dialog has completed
                                        currentState = SaveState.Finished;
                                    }
                                }
                                break;
                            case SaveState.ShowListStart:

                                errorCode = ListDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Delete, openDialogResponse, searchResponse, null);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowList;
                                }
                                break;
                            case SaveState.ShowList:

                                if (openDialogResponse.Locked == false)
                                {
                                    if (openDialogResponse.IsErrorCode)
                                    {
                                        // An error has occured
                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = openDialogResponse.ReturnCodeValue;
                                    }
                                    else
                                    {
                                        // List dialog has completed
                                        Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                        if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                        {
                                            // New load here
                                            selectedDirName = dialogResult.DirName;
                                            currentState = SaveState.ConfirmDeleteStart;
                                        }
                                        else
                                        {
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowDeleteStart:

                                errorCode = ProgressBarDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Delete, progressBarResponse, null, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Progress, selectedDirName);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    currentState = SaveState.ShowDeleteWaitForDialog;
                                }
                                break;
                            case SaveState.ShowDeleteWaitForDialog:
                                {
                                    var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                                    if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
                                        {
                                            currentState = SaveState.ShowDelete;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowDelete:
                                {
                                    Sony.PS4.SaveData.EmptyResponse deleteResponse = new Sony.PS4.SaveData.EmptyResponse();

                                    Sony.PS4.SaveData.Progress.ClearProgress();

                                    errorCode = DeleteSaveData(userId, deleteResponse, selectedDirName);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        // Wait for save data to be mounted.
                                        while (deleteResponse.Locked == true)
                                        {
                                            float progress = Sony.PS4.SaveData.Progress.GetProgress();

                                            Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)(progress * 100.0f));

                                            yield return null;
                                        }

                                        Sony.PS4.SaveData.Dialogs.ProgressBarSetValue(100);

                                        // Yeild for a frame.
                                        yield return null;

                                        if (deleteResponse.IsErrorCode == true)
                                        {
                                            // An error has occured
                                            currentState = SaveState.ShowErrorStart;
                                            errorCode = deleteResponse.ReturnCodeValue;
                                        }
                                        else
                                        {
                                            ForceCloseDialog();
                                            currentState = SaveState.Finished;
                                        }
                                    }
                                }
                                break;
                            case SaveState.ConfirmDeleteStart:
                                {
                                    // Must close any open dialog. This should be the progess bar for saving the file, but the save data was corrupted so display a confirm message.
                                    // There is no need to animate the entire dialog box.
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    errorCode = ConfirmDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Delete, confirmDeleteResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType.Confirm, selectedDirName);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ConfirmDelete;
                                    }
                                }
                                break;
                            case SaveState.ConfirmDelete:
                                {
                                    if (confirmDeleteResponse.Locked == false)
                                    {
                                        if (confirmDeleteResponse.IsErrorCode)
                                        {
                                            // An error has occured
                                            currentState = SaveState.ShowErrorStart;
                                            errorCode = confirmDeleteResponse.ReturnCodeValue;
                                        }
                                        else
                                        {
                                            // Confirm delete has finished
                                            Sony.PS4.SaveData.Dialogs.DialogResult dialogResult = confirmDeleteResponse.Result;

                                            if (dialogResult.CallResult == Sony.PS4.SaveData.Dialogs.DialogCallResults.OK)
                                            {
                                                if (dialogResult.ButtonId == Sony.PS4.SaveData.Dialogs.DialogButtonIds.Yes)
                                                {
                                                    // New save here
                                                    currentState = SaveState.ShowDeleteStart;
                                                }
                                                else
                                                {
                                                    currentState = SaveState.Finished;
                                                }
                                            }
                                            else
                                            {
                                                currentState = SaveState.Finished;
                                            }
                                        }
                                    }
                                }
                                break;
                            case SaveState.ShowErrorStart:
                                {
                                    ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation.Off);

                                    while (Sony.PS4.SaveData.Dialogs.DialogGetStatus() == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                                    {
                                        yield return null;
                                    }

                                    if (ErrorDialog(userId, Sony.PS4.SaveData.Dialogs.DialogType.Delete, errorResponse, errorCode) == false)
                                    {
                                        currentState = SaveState.Finished;
                                    }
                                    else
                                    {
                                        currentState = SaveState.ShowError;
                                    }
                                }
                                break;
                            case SaveState.ShowError:

                                if (errorResponse.Locked == false)
                                {
                                    currentState = SaveState.Finished;
                                }

                                break;
                            case SaveState.Finished:
                                currentState = SaveState.Exit;
                                break;
                        }

                        yield return null;
                    }
                }

                internal static void ForceCloseDialog(Sony.PS4.SaveData.Dialogs.Animation anim = Sony.PS4.SaveData.Dialogs.Animation.On)
                {
                    try
                    {
                        var dialogStatus = Sony.PS4.SaveData.Dialogs.DialogGetStatus();

                        if (dialogStatus == Sony.PS4.SaveData.Dialogs.DialogStatus.Running)
                        {
                            Sony.PS4.SaveData.Dialogs.CloseParam closeParam = new Sony.PS4.SaveData.Dialogs.CloseParam();
                            closeParam.Anim = anim;
                            Sony.PS4.SaveData.Dialogs.Close(closeParam);
                        }
                    }
                    catch
                    {

                    }
                }

                internal static int DeleteSaveData(int userId, Sony.PS4.SaveData.EmptyResponse deleteResponse, Sony.PS4.SaveData.DirName dirName)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Deleting.DeleteRequest request = new Sony.PS4.SaveData.Deleting.DeleteRequest();

                        request.UserId = userId;
                        request.IgnoreCallback = true;
                        request.DirName = dirName;

                        Sony.PS4.SaveData.Deleting.Delete(request, deleteResponse);
                        errorCode = 0;
                    }
                    catch
                    {
                        if (deleteResponse.ReturnCodeValue < 0)
                        {
                            errorCode = deleteResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int MountSaveData(int userId, UInt64 blocks, Sony.PS4.SaveData.Mounting.MountResponse mountResponse, Sony.PS4.SaveData.DirName dirName, Sony.PS4.SaveData.Mounting.MountModeFlags flags)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Mounting.MountRequest request = new Sony.PS4.SaveData.Mounting.MountRequest();

                        request.UserId = userId;
                        request.IgnoreCallback = true;
                        request.DirName = dirName;

                        request.MountMode = flags;

                        if (blocks < Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN)
                        {
                            blocks = Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN;
                        }

                        request.Blocks = blocks;

                        Sony.PS4.SaveData.Mounting.Mount(request, mountResponse);
                        errorCode = 0;
                    }
                    catch
                    {
                        if (mountResponse.ReturnCodeValue < 0)
                        {
                            errorCode = mountResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int UnmountSaveData(int userId, Sony.PS4.SaveData.EmptyResponse unmountResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, bool backup)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Mounting.UnmountRequest request = new Sony.PS4.SaveData.Mounting.UnmountRequest();

                        request.UserId = userId;
                        request.MountPointName = mp.PathName;
                        request.Backup = backup;
                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Mounting.Unmount(request, unmountResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (unmountResponse.ReturnCodeValue < 0)
                        {
                            errorCode = unmountResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int CheckBackup(int userId, Sony.PS4.SaveData.Backups.CheckBackupResponse backupResponse, Sony.PS4.SaveData.DirName dirName)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Backups.CheckBackupRequest request = new Sony.PS4.SaveData.Backups.CheckBackupRequest();

                        request.UserId = userId;
                        request.DirName = dirName;
                        request.IncludeParams = false;
                        request.IncludeIcon = false;
                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Backups.CheckBackup(request, backupResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (backupResponse.ReturnCodeValue < 0)
                        {
                            errorCode = backupResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int RestoreBackup(int userId, Sony.PS4.SaveData.EmptyResponse restoreResponse, Sony.PS4.SaveData.DirName dirName)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Backups.RestoreBackupRequest request = new Sony.PS4.SaveData.Backups.RestoreBackupRequest();

                        request.UserId = userId;
                        request.DirName = dirName;
                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Backups.RestoreBackup(request, restoreResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (restoreResponse.ReturnCodeValue < 0)
                        {
                            errorCode = restoreResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }


                internal static int WriteIcon(int userId, Sony.PS4.SaveData.EmptyResponse iconResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.Dialogs.NewItem newItem)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Mounting.SaveIconRequest request = new Sony.PS4.SaveData.Mounting.SaveIconRequest();

                        if (mp == null) return errorCode;

                        request.UserId = userId;
                        request.MountPointName = mp.PathName;
                        request.RawPNG = newItem.RawPNG;
                        request.IconPath = newItem.IconPath;
                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Mounting.SaveIcon(request, iconResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (iconResponse.ReturnCodeValue < 0)
                        {
                            errorCode = iconResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int WriteParams(int userId, Sony.PS4.SaveData.EmptyResponse paramsResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.SaveDataParams saveDataParams)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Mounting.SetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.SetMountParamsRequest();

                        if (mp == null) return errorCode;

                        request.UserId = userId;
                        request.MountPointName = mp.PathName;
                        request.IgnoreCallback = true;

                        request.Params = saveDataParams;

                        Sony.PS4.SaveData.Mounting.SetMountParams(request, paramsResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (paramsResponse.ReturnCodeValue < 0)
                        {
                            errorCode = paramsResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static bool ErrorDialog(int userId, Sony.PS4.SaveData.Dialogs.DialogType displayType, Sony.PS4.SaveData.Dialogs.OpenDialogResponse errorResponse, int errorCode)
                {
                    try
                    {
                        Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

                        request.UserId = userId;
                        request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.ErrorCode;
                        request.DispType = displayType;

                        Sony.PS4.SaveData.Dialogs.ErrorCodeParam errorParam = new Sony.PS4.SaveData.Dialogs.ErrorCodeParam();
                        errorParam.ErrorCode = errorCode;

                        request.ErrorCode = errorParam;

                        request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Dialogs.OpenDialog(request, errorResponse);

                        return true;
                    }
                    catch //(Sony.PS4.SaveData.SaveDataException e)
                    {

                    }

                    return false;
                }

                internal static int ConfirmDialog(int userId, Sony.PS4.SaveData.Dialogs.DialogType displayType, Sony.PS4.SaveData.Dialogs.OpenDialogResponse sysMesgResponse, Sony.PS4.SaveData.Dialogs.SystemMessageType msgType,
                                                  Sony.PS4.SaveData.DirName selectedDirName,
                                                  Sony.PS4.SaveData.Dialogs.Animation okAnim = Sony.PS4.SaveData.Dialogs.Animation.Off, Sony.PS4.SaveData.Dialogs.Animation cancelAnim = Sony.PS4.SaveData.Dialogs.Animation.On,
                                                  Sony.PS4.SaveData.Dialogs.NewItem newItem = null, ulong systemMsgValue = 0)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

                        request.UserId = userId;
                        request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.SystemMsg;
                        request.DispType = displayType;

                        Sony.PS4.SaveData.Dialogs.SystemMessageParam msg = new Sony.PS4.SaveData.Dialogs.SystemMessageParam();
                        msg.SysMsgType = msgType;
                        msg.Value = systemMsgValue;

                        request.SystemMessage = msg;

                        request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(okAnim, cancelAnim);

                        if (selectedDirName.IsEmpty == false)
                        {
                            Sony.PS4.SaveData.DirName[] dirNames = new Sony.PS4.SaveData.DirName[1];
                            dirNames[0] = selectedDirName;

                            Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();
                            items.DirNames = dirNames;

                            request.Items = items;
                        }

                        request.NewItem = newItem;

                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Dialogs.OpenDialog(request, sysMesgResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (sysMesgResponse.ReturnCodeValue < 0)
                        {
                            errorCode = sysMesgResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int FullSearch(int userId, Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Searching.DirNameSearchRequest request = new Sony.PS4.SaveData.Searching.DirNameSearchRequest();

                        request.UserId = userId;
                        request.Key = Sony.PS4.SaveData.Searching.SearchSortKey.DirName;
                        request.Order = Sony.PS4.SaveData.Searching.SearchSortOrder.Ascending;
                        request.IncludeBlockInfo = true;
                        request.IncludeParams = true;
                        request.MaxDirNameCount = Sony.PS4.SaveData.Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;

                        // For coroutine don't want callback. Will just poll response object
                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Searching.DirNameSearch(request, searchResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (searchResponse.ReturnCodeValue < 0)
                        {
                            errorCode = searchResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int ListDialog(int userId, Sony.PS4.SaveData.Dialogs.DialogType displayType, Sony.PS4.SaveData.Dialogs.OpenDialogResponse openDialogResponse, Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse, Sony.PS4.SaveData.Dialogs.NewItem newItem)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.DirName[] dirNames = null;

                        if (searchResponse.SaveDataItems.Length > 0)
                        {
                            dirNames = new Sony.PS4.SaveData.DirName[searchResponse.SaveDataItems.Length];

                            for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
                            {
                                dirNames[i] = searchResponse.SaveDataItems[i].DirName;
                            }
                        }

                        Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

                        request.UserId = userId;
                        request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.List;
                        request.DispType = displayType;

                        Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();

                        if (dirNames != null)
                        {
                            items.DirNames = dirNames;
                        }

                        items.FocusPos = Sony.PS4.SaveData.Dialogs.FocusPos.DataLatest;
                        items.ItemStyle = Sony.PS4.SaveData.Dialogs.ItemStyle.SubtitleDataSize;

                        request.Items = items;

                        request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.Off, Sony.PS4.SaveData.Dialogs.Animation.On);

                        request.NewItem = newItem;

                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Dialogs.OpenDialog(request, openDialogResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (openDialogResponse.ReturnCodeValue < 0)
                        {
                            errorCode = openDialogResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }

                internal static int ProgressBarDialog(int userId, Sony.PS4.SaveData.Dialogs.DialogType displayType, Sony.PS4.SaveData.Dialogs.OpenDialogResponse progressBarResponse, Sony.PS4.SaveData.Dialogs.NewItem newItem, Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType msgType, Sony.PS4.SaveData.DirName loadDirName)
                {
                    int errorCode = unchecked((int)0x80B8000E);

                    try
                    {
                        Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

                        request.UserId = userId;
                        request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.ProgressBar;
                        request.DispType = displayType;

                        if (loadDirName.IsEmpty == false)
                        {
                            Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();

                            Sony.PS4.SaveData.DirName[] dirName = new Sony.PS4.SaveData.DirName[1];

                            dirName[0] = loadDirName;

                            items.DirNames = dirName;

                            request.Items = items;
                        }

                        Sony.PS4.SaveData.Dialogs.ProgressBarParam progressBar = new Sony.PS4.SaveData.Dialogs.ProgressBarParam();

                        progressBar.BarType = Sony.PS4.SaveData.Dialogs.ProgressBarType.Percentage;
                        progressBar.SysMsgType = msgType;

                        request.ProgressBar = progressBar;

                        request.NewItem = newItem;

                        request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.Off, Sony.PS4.SaveData.Dialogs.Animation.On);

                        request.IgnoreCallback = true;

                        Sony.PS4.SaveData.Dialogs.OpenDialog(request, progressBarResponse);

                        errorCode = 0;
                    }
                    catch
                    {
                        if (progressBarResponse.ReturnCodeValue < 0)
                        {
                            errorCode = progressBarResponse.ReturnCodeValue;
                        }
                    }

                    return errorCode;
                }
            }
        }
    }
}


