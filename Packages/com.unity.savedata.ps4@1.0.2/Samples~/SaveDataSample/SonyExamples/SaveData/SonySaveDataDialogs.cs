using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataDialogs : IScreen
{
    MenuLayout m_MenuDialogs;

    SonySaveDataDialogTests m_DialogTests;
    public GetScreenShot screenShotHelper;

    public SonySaveDataDialogs()
    {
        Initialize();

        m_DialogTests = new SonySaveDataDialogTests();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDialogs;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuDialogs = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDialogs.Update();


        if (m_MenuDialogs.AddItem("Create Test Save Data", "Create some test save data items used for testing the dialog save process."))
        {
            SonySaveDataMain.StartSaveDataCoroutine(SonySaveDataDialogs.CreateTestSaveData(this));
        }

        if (m_MenuDialogs.AddItem("Start Save Dialog Process", "Start the save dialog process."))
        {
            SaveProcess();
        }

        if (m_MenuDialogs.AddItem("Start Load Dialog Process", "Start the load dialog process."))
        {
            LoadProcess();
        }

        if (m_MenuDialogs.AddItem("Start Delete Dialog Process", "Start the delete dialog process."))
        {
            DeleteProcess();
        }

        if (m_MenuDialogs.AddItem("Test Dialog Methods", "Test each dialog type without doing any real file IO."))
        {
            menuStack.PushMenu(m_DialogTests.GetMenu());
        }

        if (m_MenuDialogs.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public static IEnumerator CreateTestSaveData(SonySaveDataDialogs dialogs)
    {
        OnScreenLog.AddNewLine();
        OnScreenLog.AddWarning("Creating Test Save Data");
        OnScreenLog.AddNewLine();
        OnScreenLog.AddNewLine();
        OnScreenLog.Add("This will create a series of savedata directories for testing the various different states for the savdata dialog state machine.");
        OnScreenLog.Add("It will create a normal savedata, without backup, and one with a backup. Each of these can be overwritten.");
        OnScreenLog.Add("It will also create a couple of savedata which has details showing they should be flagged as corrupted. ");
        OnScreenLog.Add("One of these has a backup to test what happen when trying to overwrite a corrupted savedata with backup");

        OnScreenLog.AddNewLine();
        OnScreenLog.Add("This will block for a short time while all the test savedata is created.");
        OnScreenLog.AddNewLine();

        OnScreenLog.AddWarning("This will also delete anysave data beginning with \"Example\". Make sure no other savedata is mounted otherwise errors might occur.");
        OnScreenLog.AddWarning("Wait until the next warning message is displayed indicating the test data has been created.");

        OnScreenLog.AddNewLine();

        yield return null;

        OnScreenLog.Add("Delete any current 'Example' savedata...");

        Sony.PS4.SaveData.Searching.DirNameSearchResponse searchResponse = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();

        dialogs.TestSaveDataSearch(searchResponse);

        if (searchResponse.SaveDataItems != null)
        {
            for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
            {
                Sony.PS4.SaveData.DirName toDelete = searchResponse.SaveDataItems[i].DirName;
                OnScreenLog.Add("   Deleting " + toDelete);

                yield return null;

                dialogs.Delete(toDelete);
            }
        }

        // Test1
        OnScreenLog.Add("Creating Example1 savedata...");
        dialogs.CreateTestSaveData("Example1", false, false); // No backup

        yield return null;
        // Test2
        OnScreenLog.Add("Creating Example2 savedata...");
        dialogs.CreateTestSaveData("Example2", true, false); // Backup.

        yield return null;
        // Test3
        OnScreenLog.Add("Creating Example3 savedata...");
        dialogs.CreateTestSaveData("Example3", true, true); // Backup, Mark as corrupt test.

        yield return null;

        // Test4
        OnScreenLog.Add("Creating Example4 savedata...");
        dialogs.CreateTestSaveData("Example4", false, true); // No backup. Mark as corrupt test.

        yield return null;

        OnScreenLog.AddNewLine();
        OnScreenLog.Add("Finished creating test savedata...");

        OnScreenLog.AddNewLine();
        OnScreenLog.AddWarning("To create the corrupt saves follow these steps: ");
        OnScreenLog.AddWarning("   For an installed package build: ");
        OnScreenLog.AddWarning("      1) Press the 'PS' button. ");
        OnScreenLog.AddWarning("      2) Press the 'Options' button on the sample app.");
        OnScreenLog.AddWarning("      3) Select the '*Saved Data Management' option and press 'OK' if a warning message is shown.");
        OnScreenLog.AddWarning("      4) Select the 'Saved Data in System Storage' option");
        OnScreenLog.AddWarning("      5) Select the '+Upload to Online Storage' option");
        OnScreenLog.AddWarning("      6) Select the sample app");
        OnScreenLog.AddWarning("      7) Find the first save whose details show it should be flagged as corrupted and press the options button");
        OnScreenLog.AddWarning("      8) Select the '*Fake Save Data Broken Status' option");
        OnScreenLog.AddWarning("      9) Select the 'On' option");
        OnScreenLog.AddWarning("     10) Repeat for the other save data that is labeled as being corrupt.");
        OnScreenLog.AddWarning("     11) Both save data should now be shown as 'Corrupted Data'");
        OnScreenLog.AddWarning("     12) Important : One of the corrupted saves has a backup which changes the flow of the dialogs, the other doesn't.");
    }

    public void CreateTestSaveData(string dirName, bool backup, bool flagAsCorrupt)
    {
        Sony.PS4.SaveData.Mounting.MountPoint mp = Mount(dirName);

        WriteFiles(mp);
        SetMountParams(mp, backup, flagAsCorrupt);
        SaveIconFromFile(mp);

        Unmount(mp, backup);
    }

    public void TestSaveDataSearch(Sony.PS4.SaveData.Searching.DirNameSearchResponse response)
    {
        try
        {
            Sony.PS4.SaveData.Searching.DirNameSearchRequest request = new Sony.PS4.SaveData.Searching.DirNameSearchRequest();

            Sony.PS4.SaveData.DirName searchDirName = new Sony.PS4.SaveData.DirName();
            searchDirName.Data = "Example%";

            request.UserId = User.GetActiveUserId;
            request.Key = Sony.PS4.SaveData.Searching.SearchSortKey.DirName;
            request.Order = Sony.PS4.SaveData.Searching.SearchSortOrder.Ascending;
            request.DirName = searchDirName;
            request.MaxDirNameCount = Sony.PS4.SaveData.Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.Async = false;

            int errorCode = Sony.PS4.SaveData.Searching.DirNameSearch(request, response);

            if (errorCode != (int)Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Search Error : " + errorCode.ToString("X8"));
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void Delete(Sony.PS4.SaveData.DirName dirName)
    {
        try
        {
            Sony.PS4.SaveData.Deleting.DeleteRequest request = new Sony.PS4.SaveData.Deleting.DeleteRequest();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;
            request.Async = false;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int errorCode = Sony.PS4.SaveData.Deleting.Delete(request, response);

            if (errorCode != (int)Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Delete Error : " + errorCode.ToString("X8"));
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public Sony.PS4.SaveData.Mounting.MountPoint Mount(string directoryName)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.MountRequest request = new Sony.PS4.SaveData.Mounting.MountRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = directoryName;

            request.UserId = User.GetActiveUserId;
            request.Async = false;
            request.DirName = dirName;
            request.MountMode = Sony.PS4.SaveData.Mounting.MountModeFlags.Create2 | Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite;
            request.Blocks = SonySaveDataMain.TestBlockSize;
            // request.IgnoreCallback = true;

            Sony.PS4.SaveData.Mounting.MountResponse response = new Sony.PS4.SaveData.Mounting.MountResponse();

            int errorCode = Sony.PS4.SaveData.Mounting.Mount(request, response);

            if (errorCode != (int)Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Mount Error : " + errorCode.ToString("X8"));
                return null;
            }

            return response.MountPoint;
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return null;
    }

    public void Unmount(Sony.PS4.SaveData.Mounting.MountPoint mp, bool backup)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.UnmountRequest request = new Sony.PS4.SaveData.Mounting.UnmountRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.Backup = backup;
            request.Async = false;
            //  request.IgnoreCallback = true;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int errorCode = Sony.PS4.SaveData.Mounting.Unmount(request, response);

            if (errorCode != (int)Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Unmount Error : " + errorCode.ToString("X8"));
            }

            if (backup == true)
            {
                while (lastBackupDirname.Data != mp.DirName.Data)
                {

                }
                //float progress = Sony.PS4.SaveData.Backups.GetProgress();
                //while(progress < 1.0f)
                //{
                //    progress = Sony.PS4.SaveData.Backups.GetProgress();
                //}
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void WriteFiles(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            ExampleWriteFilesRequest request = new ExampleWriteFilesRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.Async = false;

            request.myTestData = "This is some text test data which will be written to a file. " + OnScreenLog.FrameCount;
            request.myOtherTestData = "This is some more text which is written to another save file. " + OnScreenLog.FrameCount;

            ExampleWriteFilesResponse response = new ExampleWriteFilesResponse();

            int errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(request, response);

            if (errorCode != (int)Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                OnScreenLog.AddError("Write files Error : " + errorCode.ToString("X8"));
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetMountParams(Sony.PS4.SaveData.Mounting.MountPoint mp, bool backup, bool flagAsCorrupt)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.SetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.SetMountParamsRequest();

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Sony.PS4.SaveData.SaveDataParams sdParams = new Sony.PS4.SaveData.SaveDataParams();

            if (flagAsCorrupt == false)
            {
                sdParams.Title = "My Save Data " + OnScreenLog.FrameCount;
                sdParams.SubTitle = "My Save Data Subtitle " + OnScreenLog.FrameCount;
                sdParams.Detail = "This is the long descrition of the save data.";
            }
            else
            {
                sdParams.Title = "Corrupt savedata test " + OnScreenLog.FrameCount;
                sdParams.SubTitle = "Corrupt savedata test ";
                sdParams.Detail = "This is the long descrition of the corrupt savedata test.";

                if (backup == true)
                {
                    sdParams.SubTitle = sdParams.SubTitle + ", with backup ";
                }

                sdParams.SubTitle = sdParams.SubTitle + OnScreenLog.FrameCount;
            }

            sdParams.UserParam = (UInt32)OnScreenLog.FrameCount;

            request.Params = sdParams;
            request.Async = false;
            //  request.IgnoreCallback = true;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            Sony.PS4.SaveData.Mounting.SetMountParams(request, response);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SaveIconFromFile(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.SaveIconRequest request = new Sony.PS4.SaveData.Mounting.SaveIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            request.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";

            request.Async = false;
            //  request.IgnoreCallback = true;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            Sony.PS4.SaveData.Mounting.SaveIcon(request, response);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SaveProcess()
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        // Create the new item for the saves dialog list
        Sony.PS4.SaveData.Dialogs.NewItem newItem = new Sony.PS4.SaveData.Dialogs.NewItem();

        //newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
        newItem.Title = "Testing new item title with save list " + OnScreenLog.FrameCount;

        // The directory name for a new savedata
        Sony.PS4.SaveData.DirName newDirName = new Sony.PS4.SaveData.DirName();
        newDirName.Data = "Example" + OnScreenLog.FrameCount;

        // Should the savedata be backed up automaitcally when the savedata is unmounted.
        bool backup = true;

        // What size should a new save data be created.
        UInt64 newSaveDataBlocks = SonySaveDataMain.TestBlockSize;

        // Parameters to use for the savedata
        Sony.PS4.SaveData.SaveDataParams saveDataParams = new Sony.PS4.SaveData.SaveDataParams();

        saveDataParams.Title = newItem.Title;
        saveDataParams.SubTitle = "Subtitle for savedata " + OnScreenLog.FrameCount;
        saveDataParams.Detail = "Details for savedata " + OnScreenLog.FrameCount;
        saveDataParams.UserParam = (uint)OnScreenLog.FrameCount;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        ExampleWriteFilesRequest fileRequest = new ExampleWriteFilesRequest();
        fileRequest.myTestData = "This is some text test data which will be written to a file. " + OnScreenLog.FrameCount;
        fileRequest.myOtherTestData = "This is some more text which is written to another save file. " + OnScreenLog.FrameCount;
        fileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete

        ExampleWriteFilesResponse fileResponse = new ExampleWriteFilesResponse();

        SonySaveDataMain.StartSaveDataCoroutine(StartSaveDialogProcessWithScreenshot(userId, newItem, newDirName, newSaveDataBlocks, saveDataParams, fileRequest, fileResponse, backup, screenShotHelper));
    }


    // Example SaveDataDialogProcess.AllowNewItemTest callback.
    // Allows five slots.
    //
    // As a user of this library you must make your own decision on how to write this function
    // in order to comply with TRC 4100.
    private static bool AllowFiveSlots(Sony.PS4.SaveData.Searching.DirNameSearchResponse response)
    {
        var items = response.SaveDataItems;
        int num = items != null ? items.Length : 0;
        bool allow = num < 5; // for example.
        return allow;
    }

    public static IEnumerator StartSaveDialogProcessWithScreenshot(int userId, Sony.PS4.SaveData.Dialogs.NewItem newItem, Sony.PS4.SaveData.DirName newDirName, ulong newSaveDataBlocks, Sony.PS4.SaveData.SaveDataParams saveDataParams,
        Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse, bool backup, GetScreenShot screenShotHelper)
    {
        screenShotHelper.DoScreenShot();

        while (screenShotHelper.IsWaiting == true)
        {
            yield return null;
        }

        newItem.RawPNG = screenShotHelper.screenShotBytes;

        SonySaveDataMain.StartSaveDataCoroutine(Sony.PS4.SaveData.SaveDataDialogProcess.StartSaveDialogProcess(userId, newItem, newDirName, newSaveDataBlocks, saveDataParams, fileRequest, fileResponse, backup, AllowFiveSlots));
    }

    public void LoadProcess()
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        ExampleReadFilesRequest fileRequest = new ExampleReadFilesRequest();
        fileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        ExampleReadFilesResponse fileResponse = new ExampleReadFilesResponse();

        SonySaveDataMain.StartSaveDataCoroutine(Sony.PS4.SaveData.SaveDataDialogProcess.StartLoadDialogProcess(userId, fileRequest, fileResponse));
    }

    public void DeleteProcess()
    {
        // Get the user id for the saves
        int userId = User.GetActiveUserId;

        SonySaveDataMain.StartSaveDataCoroutine(Sony.PS4.SaveData.SaveDataDialogProcess.StartDeleteDialogProcess(userId));
    }

    static Sony.PS4.SaveData.DirName lastBackupDirname;

    public void OnAsyncEvent(Sony.PS4.SaveData.SaveDataCallbackEvent callbackEvent)
    {
        m_DialogTests.OnAsyncEvent(callbackEvent);

        switch (callbackEvent.ApiCalled)
        {
            case Sony.PS4.SaveData.FunctionTypes.NotificationUnmountWithBackup:
                {
                    Sony.PS4.SaveData.UnmountWithBackupNotification response = callbackEvent.Response as Sony.PS4.SaveData.UnmountWithBackupNotification;

                    if (response != null)
                    {
                        lastBackupDirname = response.DirName;
                    }
                }
                break;
        }
    }

}
}
#endif
