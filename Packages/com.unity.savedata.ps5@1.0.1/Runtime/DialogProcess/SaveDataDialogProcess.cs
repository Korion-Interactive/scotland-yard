using System;
using System.Collections;

using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;
using Unity.SaveData.PS5.Files;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
using Unity.SaveData.PS5.Search;

namespace Unity.SaveData.PS5.Dialog
{
    /// <summary>
    /// Dialog state machine for running the save, load, and delete save data processes.
    /// </summary>
    public class SaveDataDialogProcess
    {
        static DirName emptyDirName = new DirName();

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

            ShowErrorNoSpaceStart,
            ShowErrorNoSpace,
            ShowErrorStart,
            ShowError,

            Finished,

            Exit,
        }

        /// <summary>
        /// Returns true if a dialog can show the New Item button. 
        /// </summary>
        /// <param name="response">The response from the search. Use this to determine if there are too many save data items.</param>
        /// <returns>Returns true if the New Item button should be shown, false otherwise.</returns>
        public delegate bool AllowNewItemTest(Searching.DirNameSearchResponse response);

        /// <summary>
        /// Start the save process as a Unity coroutine.
        /// </summary>
        /// <param name="userId">The ID of the user who will save data.</param>
        /// <param name="newItem">The new item details displayed in the save list.</param>
        /// <param name="newDirName">The folder name of a new save data if new save is selected from the list.</param>
        /// <param name="newSaveDataBlocks">The size of a new save data if new save is selected from the list.</param>
        /// <param name="saveDataParams">The save data parameters for a new or overwritten save data.</param>
        /// <param name="fileRequest">The custom file IO operations for the actual files inside the save data.</param>
        /// <param name="fileResponse">The custom file IO response containing the results of the file operation.</param>
        /// <param name="backup">Indicates whether the save data should be backed up when saving completes. This generates a notification after the backup completes.</param>
        /// <param name="allowNewItemCB">A callback allows the caller to specify whether the dialog should allow a new save data, or that the user must overwrite an existing one.</param>
        /// <returns>An enumerator that can be used in a Unity coroutine.</returns>
        public static IEnumerator StartSaveDialogProcess(int userId, Dialogs.NewItem newItem, DirName newDirName, UInt64 newSaveDataBlocks, SaveDataParams saveDataParams,
                                                          FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, bool backup, AllowNewItemTest allowNewItemCB = null)
        {
            SaveState currentState = SaveState.Begin;

            Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
            Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse overwriteResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse confirmRestoreResponse = new Dialogs.OpenDialogResponse();
            EmptyResponse restoreResponse = new EmptyResponse();
            Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse noSpaceResponse = new Dialogs.OpenDialogResponse();
            Mounting.MountResponse mountResponse = new Mounting.MountResponse();

            int errorCode = 0;

            DirName selectedDirName = new DirName();

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

                        // A callback allows the caller to specify whether the dialog should allow a new save data,
                        // or that the user must overwrite an existing one.
                        bool allowNew = allowNewItemCB != null ? allowNewItemCB(searchResponse) : true;

                        errorCode = ListDialog(userId, Dialogs.DialogType.Save, openDialogResponse, searchResponse, allowNew ? newItem : null);

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
                                Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                if (dialogResult.CallResult == Dialogs.DialogCallResults.OK)
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
                            Dialogs.NewItem useNewItem = newItem;
                            DirName dirName = newDirName;

                            // If an existing directory name has been selected then use that and don't use the newItem
                            if (selectedDirName.IsEmpty == false)
                            {
                                useNewItem = null;
                                dirName = selectedDirName;
                            }

                            errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Save, progressBarResponse, useNewItem, Dialogs.ProgressSystemMessageType.Progress, dirName);

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
                            var dialogStatus = Dialogs.DialogGetStatus();

                            if (dialogStatus == Dialogs.DialogStatus.Running)
                            {
                                if (Dialogs.DialogIsReadyToDisplay() == true)
                                {
                                    currentState = SaveState.ShowSave;
                                }
                            }
                        }
                        break;
                    case SaveState.ShowSave:
                        {
                            // At this point the save list dialog is displayed and its safe to mount a save data in read/write mode
                            Mounting.MountModeFlags flags = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;

                            DirName dirName;

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
                                    if (mountResponse.ReturnCode == ReturnCodes.DATA_ERROR_NO_SPACE_FS)
                                    {
                                        currentState = SaveState.ShowErrorNoSpaceStart;
                                    }
                                    else if (mountResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                    {
                                        //Backups.CheckBackupResponse backupResponse = new Backups.CheckBackupResponse();
                                        //// Test if backup save data exists
                                        //errorCode = CheckBackup(userId, backupResponse, dirName);

                                        //if (errorCode < 0)
                                        //{
                                        //    currentState = SaveState.ShowErrorStart;
                                        //}
                                        //else
                                        //{
                                        //    while (backupResponse.Locked == true)
                                        //    {
                                        //        yield return null;
                                        //    }

                                        //    if (backupResponse.IsErrorCode == true)
                                        //    {
                                        //        currentState = SaveState.ShowErrorStart;
                                        //        errorCode = mountResponse.ReturnCodeValue;
                                        //    }
                                        //    else
                                        //    {
                                        //        currentState = SaveState.ConfirmRestoreStart;
                                        //    }
                                        //}

                                        currentState = SaveState.ShowErrorStart;
                                        errorCode = mountResponse.ReturnCodeValue;
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
                                    Mounting.MountPoint mp = mountResponse.MountPoint;

                                    // Do actual saving
                                    fileRequest.MountPointName = mp.PathName;
                                    fileRequest.Async = true;
                                    fileRequest.UserId = userId;

                                    errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.ShowErrorStart;
                                    }
                                    else
                                    {
                                        while (fileResponse.Locked == true)
                                        {
                                            Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                            yield return null;
                                        }

                                        Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                        // Write the icon and any detail parmas set here.
                                        EmptyResponse iconResponse = new EmptyResponse();

                                        errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                                        if (errorCode < 0)
                                        {
                                            currentState = SaveState.ShowErrorStart;
                                        }
                                        else
                                        {
                                            EmptyResponse paramsResponse = new EmptyResponse();

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
                                                EmptyResponse unmountResponse = new EmptyResponse();

                                                errorCode = UnmountSaveData(userId, unmountResponse, mp);

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

                        errorCode = ConfirmDialog(userId, Dialogs.DialogType.Save, overwriteResponse, Dialogs.SystemMessageType.Overwrite, selectedDirName);

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
                                Dialogs.DialogResult dialogResult = overwriteResponse.Result;

                                if (dialogResult.CallResult == Dialogs.DialogCallResults.OK)
                                {
                                    if (dialogResult.ButtonId == Dialogs.DialogButtonIds.Yes)
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
                    case SaveState.ShowErrorNoSpaceStart:
                        {
                            // The progress bar dialog will be displayed at this point so close it.
                            ForceCloseDialog(Dialogs.Animation.Off);

                            while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
                            {
                                yield return null;
                            }

                            Dialogs.NewItem useNewItem = newItem;

                            if (selectedDirName.IsEmpty == false)
                            {
                                useNewItem = null;
                            }

                            // Note - This needs to show the RequiredBlocks from the mounting process and not the actual blocks size required for the save data. These numbers can be very different.
                            errorCode = ConfirmDialog(userId, Dialogs.DialogType.Save, noSpaceResponse, Dialogs.SystemMessageType.NoSpace, emptyDirName,
                                                      Dialogs.Animation.On, Dialogs.Animation.On, useNewItem, mountResponse.RequiredBlocks);

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
                            ForceCloseDialog(Dialogs.Animation.Off);

                            while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
                            {
                                yield return null;
                            }

                            if (ErrorDialog(userId, Dialogs.DialogType.Save, errorResponse, errorCode) == false)
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
        /// Starts the load process as a Unity coroutine.
        /// </summary>
        /// <param name="userId">The ID of the user who will save data.</param>
        /// <param name="fileRequest">The custom file IO operations for the actual files inside the save data.</param>
        /// <param name="fileResponse">The custom file IO response containing the results of the file operation.</param>
        /// <returns>An enumerator which can be used in a Unity coroutine.</returns>
        public static IEnumerator StartLoadDialogProcess(int userId, FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse)
        {
            SaveState currentState = SaveState.Begin;

            Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
            Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse confirmRestoreResponse = new Dialogs.OpenDialogResponse();
            EmptyResponse restoreResponse = new EmptyResponse();
            Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse noDataResponse = new Dialogs.OpenDialogResponse();

            int errorCode = 0;

            DirName selectedDirName = new DirName();

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

                        errorCode = ConfirmDialog(userId, Dialogs.DialogType.Load, noDataResponse, Dialogs.SystemMessageType.NoData, emptyDirName,
                                                  Dialogs.Animation.On, Dialogs.Animation.On);

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

                        errorCode = ListDialog(userId, Dialogs.DialogType.Load, openDialogResponse, searchResponse, null);

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
                                Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                if (dialogResult.CallResult == Dialogs.DialogCallResults.OK)
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

                        errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Load, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Progress, selectedDirName);

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
                            var dialogStatus = Dialogs.DialogGetStatus();

                            if (dialogStatus == Dialogs.DialogStatus.Running)
                            {
                                if (Dialogs.DialogIsReadyToDisplay() == true)
                                {
                                    currentState = SaveState.ShowLoad;
                                }
                            }
                        }
                        break;
                    case SaveState.ShowLoad:

                        // At this point the save list dialog is displayed and its safe to mount a save data in read/write mode

                        Mounting.MountResponse mountResponse = new Mounting.MountResponse();

                        Mounting.MountModeFlags flags = Mounting.MountModeFlags.ReadOnly;

                        DirName dirName = selectedDirName;

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
                                if (mountResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                {
                                    //Backups.CheckBackupResponse backupResponse = new Backups.CheckBackupResponse();
                                    //// Test if backup save data exists
                                    //errorCode = CheckBackup(userId, backupResponse, dirName);

                                    //if (errorCode < 0)
                                    //{
                                    //    currentState = SaveState.ShowErrorStart;
                                    //}
                                    //else
                                    //{
                                    //    while (backupResponse.Locked == true)
                                    //    {
                                    //        yield return null;
                                    //    }

                                    //    if (backupResponse.IsErrorCode == true)
                                    //    {
                                    //        currentState = SaveState.ShowErrorStart;
                                    //        errorCode = mountResponse.ReturnCodeValue;
                                    //    }
                                    //    else
                                    //    {
                                    //        currentState = SaveState.ConfirmRestoreStart;
                                    //    }
                                    //}
                                    currentState = SaveState.ShowErrorStart;
                                    errorCode = mountResponse.ReturnCodeValue;
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
                                Mounting.MountPoint mp = mountResponse.MountPoint;

                                // Do actual saving
                                fileRequest.MountPointName = mp.PathName;
                                fileRequest.Async = true;
                                fileRequest.UserId = userId;

                                errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.ShowErrorStart;
                                }
                                else
                                {
                                    while (fileResponse.Locked == true)
                                    {
                                        Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                        yield return null;
                                    }

                                    // Update the last progress value as this will have been updated on another thread reading the fileResponse. 
                                    // As long as the developer has set the Progress value to 1.0 the progress dialog will show 100%
                                    Dialogs.ProgressBarSetValue((UInt32)(fileResponse.Progress * 100.0f));

                                    // Yeild for a frame to make sure the progress bar dialog gets a chance to refresh.
                                    yield return null;

                                    // unmount the save data
                                    EmptyResponse unmountResponse = new EmptyResponse();

                                    errorCode = UnmountSaveData(userId, unmountResponse, mp);

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
                    case SaveState.ShowErrorStart:
                        {
                            ForceCloseDialog(Dialogs.Animation.Off);

                            while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
                            {
                                yield return null;
                            }

                            if (ErrorDialog(userId, Dialogs.DialogType.Load, errorResponse, errorCode) == false)
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
        /// Starts the delete process as a Unity coroutine.
        /// </summary>
        /// <param name="userId">The ID of the user who will save data.</param>
        /// <returns>An enumerator that can be used in a Unity coroutine.</returns>
        public static IEnumerator StartDeleteDialogProcess(int userId)
        {
            SaveState currentState = SaveState.Begin;

            Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
            Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse confirmDeleteResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
            Dialogs.OpenDialogResponse noDataResponse = new Dialogs.OpenDialogResponse();

            int errorCode = 0;

            DirName selectedDirName = new DirName();

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

                        errorCode = ConfirmDialog(userId, Dialogs.DialogType.Delete, noDataResponse, Dialogs.SystemMessageType.NoData, emptyDirName,
                                                  Dialogs.Animation.On, Dialogs.Animation.On);

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

                        errorCode = ListDialog(userId, Dialogs.DialogType.Delete, openDialogResponse, searchResponse, null);

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
                                Dialogs.DialogResult dialogResult = openDialogResponse.Result;

                                if (dialogResult.CallResult == Dialogs.DialogCallResults.OK)
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

                        errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Delete, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Progress, selectedDirName);

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
                            var dialogStatus = Dialogs.DialogGetStatus();

                            if (dialogStatus == Dialogs.DialogStatus.Running)
                            {
                                if (Dialogs.DialogIsReadyToDisplay() == true)
                                {
                                    currentState = SaveState.ShowDelete;
                                }
                            }
                        }
                        break;
                    case SaveState.ShowDelete:
                        {
                            EmptyResponse deleteResponse = new EmptyResponse();

                            Progress.ClearProgress();

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
                                    float progress = Progress.GetProgress();

                                    Dialogs.ProgressBarSetValue((UInt32)(progress * 100.0f));

                                    yield return null;
                                }

                                Dialogs.ProgressBarSetValue(100);

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
                            ForceCloseDialog(Dialogs.Animation.Off);

                            while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
                            {
                                yield return null;
                            }

                            errorCode = ConfirmDialog(userId, Dialogs.DialogType.Delete, confirmDeleteResponse, Dialogs.SystemMessageType.Confirm, selectedDirName);

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
                                    Dialogs.DialogResult dialogResult = confirmDeleteResponse.Result;

                                    if (dialogResult.CallResult == Dialogs.DialogCallResults.OK)
                                    {
                                        if (dialogResult.ButtonId == Dialogs.DialogButtonIds.Yes)
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
                            ForceCloseDialog(Dialogs.Animation.Off);

                            while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
                            {
                                yield return null;
                            }

                            if (ErrorDialog(userId, Dialogs.DialogType.Delete, errorResponse, errorCode) == false)
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

        internal static void ForceCloseDialog(Dialogs.Animation anim = Dialogs.Animation.On)
        {
            try
            {
                var dialogStatus = Dialogs.DialogGetStatus();

                if (dialogStatus == Dialogs.DialogStatus.Running)
                {
                    Dialogs.CloseParam closeParam = new Dialogs.CloseParam();
                    closeParam.Anim = anim;
                    Dialogs.Close(closeParam);
                }
            }
            catch
            {

            }
        }

        internal static int DeleteSaveData(int userId, EmptyResponse deleteResponse, DirName dirName)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Deleting.DeleteRequest request = new Deleting.DeleteRequest();

                request.UserId = userId;
                request.IgnoreCallback = true;
                request.DirName = dirName;

                Deleting.Delete(request, deleteResponse);
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

        internal static int MountSaveData(int userId, UInt64 blocks, Mounting.MountResponse mountResponse, DirName dirName, Mounting.MountModeFlags flags)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.MountRequest request = new Mounting.MountRequest();

                request.UserId = userId;
                request.IgnoreCallback = true;
                request.DirName = dirName;

                request.MountMode = flags;

                if (blocks < Mounting.MountRequest.BLOCKS_MIN)
                {
                    blocks = Mounting.MountRequest.BLOCKS_MIN;
                }

                request.Blocks = blocks;

                Mounting.Mount(request, mountResponse);
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

        internal static int UnmountSaveData(int userId, EmptyResponse unmountResponse, Mounting.MountPoint mp)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.UnmountRequest request = new Mounting.UnmountRequest();

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.IgnoreCallback = true;

                Mounting.Unmount(request, unmountResponse);

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

        //internal static int CheckBackup(int userId, Backups.CheckBackupResponse backupResponse, DirName dirName)
        //{
        //    int errorCode = unchecked((int)0x80B8000E);

        //    try
        //    {
        //        Backups.CheckBackupRequest request = new Backups.CheckBackupRequest();

        //        request.UserId = userId;
        //        request.DirName = dirName;
        //        request.IncludeParams = false;
        //        request.IncludeIcon = false;
        //        request.IgnoreCallback = true;

        //        Backups.CheckBackup(request, backupResponse);

        //        errorCode = 0;
        //    }
        //    catch
        //    {
        //        if (backupResponse.ReturnCodeValue < 0)
        //        {
        //            errorCode = backupResponse.ReturnCodeValue;
        //        }
        //    }

        //    return errorCode;
        //}

        //internal static int RestoreBackup(int userId, EmptyResponse restoreResponse, DirName dirName)
        //{
        //    int errorCode = unchecked((int)0x80B8000E);

        //    try
        //    {
        //        Backups.RestoreBackupRequest request = new Backups.RestoreBackupRequest();

        //        request.UserId = userId;
        //        request.DirName = dirName;
        //        request.IgnoreCallback = true;

        //        Backups.RestoreBackup(request, restoreResponse);

        //        errorCode = 0;
        //    }
        //    catch
        //    {
        //        if (restoreResponse.ReturnCodeValue < 0)
        //        {
        //            errorCode = restoreResponse.ReturnCodeValue;
        //        }
        //    }

        //    return errorCode;
        //}


        internal static int WriteIcon(int userId, EmptyResponse iconResponse, Mounting.MountPoint mp, Dialogs.NewItem newItem)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.RawPNG = newItem.RawPNG;
                request.IconPath = newItem.IconPath;
                request.IgnoreCallback = true;

                Mounting.SaveIcon(request, iconResponse);

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

        internal static int WriteParams(int userId, EmptyResponse paramsResponse, Mounting.MountPoint mp, SaveDataParams saveDataParams)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.IgnoreCallback = true;

                request.Params = saveDataParams;

                Mounting.SetMountParams(request, paramsResponse);

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

        internal static bool ErrorDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse errorResponse, int errorCode)
        {
            try
            {
                Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

                request.UserId = userId;
                request.Mode = Dialogs.DialogMode.ErrorCode;
                request.DispType = displayType;

                Dialogs.ErrorCodeParam errorParam = new Dialogs.ErrorCodeParam();
                errorParam.ErrorCode = errorCode;

                request.ErrorCode = errorParam;

                request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);

                request.IgnoreCallback = true;

                Dialogs.OpenDialog(request, errorResponse);

                return true;
            }
            catch //(SaveDataException e)
            {

            }

            return false;
        }

        internal static int ConfirmDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse sysMesgResponse, Dialogs.SystemMessageType msgType,
                                          DirName selectedDirName,
                                          Dialogs.Animation okAnim = Dialogs.Animation.Off, Dialogs.Animation cancelAnim = Dialogs.Animation.On,
                                          Dialogs.NewItem newItem = null, ulong systemMsgValue = 0)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

                request.UserId = userId;
                request.Mode = Dialogs.DialogMode.SystemMsg;
                request.DispType = displayType;

                Dialogs.SystemMessageParam msg = new Dialogs.SystemMessageParam();
                msg.SysMsgType = msgType;
                msg.Value = systemMsgValue;

                request.SystemMessage = msg;

                request.Animations = new Dialogs.AnimationParam(okAnim, cancelAnim);

                if (selectedDirName.IsEmpty == false)
                {
                    DirName[] dirNames = new DirName[1];
                    dirNames[0] = selectedDirName;

                    Dialogs.Items items = new Dialogs.Items();
                    items.DirNames = dirNames;

                    request.Items = items;
                }

                request.NewItem = newItem;

                request.IgnoreCallback = true;

                Dialogs.OpenDialog(request, sysMesgResponse);

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

        internal static int FullSearch(int userId, Searching.DirNameSearchResponse searchResponse)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

                request.UserId = userId;
                request.Key = Searching.SearchSortKey.DirName;
                request.Order = Searching.SearchSortOrder.Ascending;
                request.IncludeBlockInfo = true;
                request.IncludeParams = true;
                request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;

                // For coroutine don't want callback. Will just poll response object
                request.IgnoreCallback = true;

                Searching.DirNameSearch(request, searchResponse);

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

        internal static int ListDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse openDialogResponse, Searching.DirNameSearchResponse searchResponse, Dialogs.NewItem newItem)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                DirName[] dirNames = null;

                if (searchResponse.SaveDataItems.Length > 0)
                {
                    dirNames = new DirName[searchResponse.SaveDataItems.Length];

                    for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
                    {
                        dirNames[i] = searchResponse.SaveDataItems[i].DirName;
                    }
                }

                Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

                request.UserId = userId;
                request.Mode = Dialogs.DialogMode.List;
                request.DispType = displayType;

                Dialogs.Items items = new Dialogs.Items();

                if (dirNames != null)
                {
                    items.DirNames = dirNames;
                }

                items.FocusPos = Dialogs.FocusPos.DataLatest;
                items.ItemStyle = Dialogs.ItemStyle.SubtitleDataSize;

                request.Items = items;

                request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.Off, Dialogs.Animation.On);

                request.NewItem = newItem;

                request.IgnoreCallback = true;

                Dialogs.OpenDialog(request, openDialogResponse);

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

        internal static int ProgressBarDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse progressBarResponse, Dialogs.NewItem newItem, Dialogs.ProgressSystemMessageType msgType, DirName loadDirName)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

                request.UserId = userId;
                request.Mode = Dialogs.DialogMode.ProgressBar;
                request.DispType = displayType;

                if (newItem == null)
                {
                    Dialogs.Items items = new Dialogs.Items();

                    DirName[] dirName = new DirName[1];

                    dirName[0] = loadDirName;

                    items.DirNames = dirName;

                    request.Items = items;
                }

                Dialogs.ProgressBarParam progressBar = new Dialogs.ProgressBarParam();

                progressBar.BarType = Dialogs.ProgressBarType.Percentage;
                progressBar.SysMsgType = msgType;

                request.ProgressBar = progressBar;

                request.NewItem = newItem;

                request.Animations = new Dialogs.AnimationParam(Dialogs.Animation.Off, Dialogs.Animation.On);

                request.IgnoreCallback = true;

                Dialogs.OpenDialog(request, progressBarResponse);

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


