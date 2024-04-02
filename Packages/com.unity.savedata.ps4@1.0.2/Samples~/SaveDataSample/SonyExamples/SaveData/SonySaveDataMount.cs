using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataMount : IScreen
{
    MenuLayout m_MenuMount;

    Sony.PS4.SaveData.Icon currentIcon = null;
    bool updateIcon = false;

    public Material iconMaterial;
    public SaveIconWithScreenShot screenShotHelper;

    public void SetIconTexture(Sony.PS4.SaveData.Icon icon)
    {
        currentIcon = icon;
        updateIcon = true;
    }

    public SonySaveDataMount()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuMount;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        if (updateIcon == true)
        {
            updateIcon = false;

            if (currentIcon != null)
            {
                // This will create the texture if it is not already cached in the currentIcon.           
                UnityEngine.Texture2D iconTexture = new UnityEngine.Texture2D(currentIcon.Width, currentIcon.Height);

                iconTexture.LoadImage(currentIcon.RawBytes);

                iconMaterial.mainTexture = iconTexture;

                OnScreenLog.Add("Updating icon material : W = " + iconTexture.width + " H = " + iconTexture.height);
            }
        }

        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuMount = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        List<Sony.PS4.SaveData.Mounting.MountPoint> mountPoints = Sony.PS4.SaveData.Mounting.ActiveMountPoints;

        m_MenuMount.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";
        
        if ( isEnabled == true )
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuMount.AddItem("Create Random Directory Name", "This will generate a new directory name and mount it for read/write access."))
        {
            SaveDataDirNames.GenerateNewDirName(OnScreenLog.FrameCount);

            if (SaveDataDirNames.HasCurrentDirName() == true)
            {
                Mount(true, true);
            }
        }

        if (m_MenuMount.AddItem("Mount Read/Write", "Mount a save data for read/write access." + dirNameToolTip, isEnabled))
        {
            Mount(true, true);
        }

        if (m_MenuMount.AddItem("Mount R/W (synchronous)", "Mount a save data for read/write access using synchronous request." + dirNameToolTip, isEnabled))
        {
            Mount(false, true);
        }

        if (m_MenuMount.AddItem("Mount Read Only", "Mount a read only save data." + dirNameToolTip, isEnabled))
        {
            Mount(true, false);
        }

        Sony.PS4.SaveData.Mounting.MountPoint mp = SonySaveDataMain.GetMountPoint();

        isEnabled = (mp != null);

        string moutPointToolTip = "";

        if (mp != null)
        {
            moutPointToolTip = " Use this on mount point \"" + mp.PathName.Data + "\".";
        }

        if (m_MenuMount.AddItem("Get Mount Info", "Get mount info." + moutPointToolTip, isEnabled))
        {
            GetMountInfo(mp);
        }

        if (m_MenuMount.AddItem("Get Mount Params", "Get mount params." + moutPointToolTip, isEnabled))
        {
            GetMountParams(mp);
        }

        if (m_MenuMount.AddItem("Set Mount Params", "Set mount params." + moutPointToolTip, isEnabled))
        {
            SetMountParams(mp);
        }

        if (m_MenuMount.AddItem("Save Icon (From File)", "Save the save data icon. This will read PNG data from a file and use that as the save data icon." + moutPointToolTip, isEnabled))
        {
            SaveIconFromFile(mp);
        }

        if (m_MenuMount.AddItem("Save Icon (Screenshot)", "Save the save data icon. Take a screenshot and use it for the save data icon. This will not include the OnGUI text in the sample." + moutPointToolTip, isEnabled))
        {
            screenShotHelper.DoScreenShot(mp);
        }
               
        if (m_MenuMount.AddItem("Load Icon", "Load the save data icon." + moutPointToolTip, isEnabled))
        {
            LoadIcon(mp);
        }

        //if(m_MenuMount.AddItem("Test", "" + moutPointToolTip, isEnabled))
        //{
        //    byte[] fileBytes = File.ReadAllBytes("/app0/Media/StreamingAssets/SaveIcon.png");
        //    OnScreenLog.Add("File bytes = " + fileBytes.Length);

        //    string outpath = mp.PathName.Data + "/SaveIcon.dat";
        //    OnScreenLog.Add("Output Path = " + outpath);
        //    File.WriteAllBytes(outpath, fileBytes);
        //}

        if (m_MenuMount.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Mount(bool async, bool readWrite)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.MountRequest request = new Sony.PS4.SaveData.Mounting.MountRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            OnScreenLog.Add("Mounting Directory : " + dirName.Data);

            request.UserId = User.GetActiveUserId;
            request.Async = async;
            request.DirName = dirName;

            if (readWrite == true)
            {
                request.MountMode = Sony.PS4.SaveData.Mounting.MountModeFlags.Create2 | Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite;
            }
            else
            {
                request.MountMode = Sony.PS4.SaveData.Mounting.MountModeFlags.ReadOnly;
            }

            request.Blocks = SonySaveDataMain.TestBlockSize;

            Sony.PS4.SaveData.Mounting.MountResponse response = new Sony.PS4.SaveData.Mounting.MountResponse();

            int requestId = Sony.PS4.SaveData.Mounting.Mount(request, response);

            if (async == true)
            {
                OnScreenLog.Add("Mount Async : Request Id = " + requestId);
            }
            else
            {
                if (response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                else
                {
                    OnScreenLog.Add("Mount Sync : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                MountReponseOutput(response);
                OnScreenLog.AddNewLine();
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetMountInfo(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.GetMountInfoRequest request = new Sony.PS4.SaveData.Mounting.GetMountInfoRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Sony.PS4.SaveData.Mounting.MountInfoResponse response = new Sony.PS4.SaveData.Mounting.MountInfoResponse();

            int requestId = Sony.PS4.SaveData.Mounting.GetMountInfo(request, response);

            OnScreenLog.Add("GetMountInfo Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetMountParams(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.GetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.GetMountParamsRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Sony.PS4.SaveData.Mounting.MountParamsResponse response = new Sony.PS4.SaveData.Mounting.MountParamsResponse();

            int requestId = Sony.PS4.SaveData.Mounting.GetMountParams(request, response);

            OnScreenLog.Add("GetMountParams Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetMountParams(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.SetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.SetMountParamsRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Sony.PS4.SaveData.SaveDataParams sdParams = new Sony.PS4.SaveData.SaveDataParams();

            sdParams.Title = "My Save Data " + OnScreenLog.FrameCount;
            sdParams.SubTitle = "My Save Data Subtitle " + OnScreenLog.FrameCount;
            sdParams.Detail = "This is the long descrition of the save data.";
            sdParams.UserParam = (UInt32)OnScreenLog.FrameCount;

            request.Params = sdParams;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Mounting.SetMountParams(request, response);

            OnScreenLog.Add("GetMountParams Async : Request Id = " + requestId);
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

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Mounting.SaveIcon(request, response);

            OnScreenLog.Add("SaveIcon Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void LoadIcon(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        try
        {
            Sony.PS4.SaveData.Mounting.LoadIconRequest request = new Sony.PS4.SaveData.Mounting.LoadIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;

            Sony.PS4.SaveData.Mounting.LoadIconResponse response = new Sony.PS4.SaveData.Mounting.LoadIconResponse();

            int requestId = Sony.PS4.SaveData.Mounting.LoadIcon(request, response);

            OnScreenLog.Add("LoadIcon Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.PS4.SaveData.SaveDataCallbackEvent callbackEvent)
    {
        switch (callbackEvent.ApiCalled)
        {
            case Sony.PS4.SaveData.FunctionTypes.Mount:
                {
                    MountReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Mounting.MountResponse);
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.GetMountInfo:
                {
                    MountInfoReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Mounting.MountInfoResponse);
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.GetMountParams:
                {
                    MountParamsReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Mounting.MountParamsResponse);
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.LoadIcon:
                {
                    LoadIconReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Mounting.LoadIconResponse);
                }
                break;
        }
    }

    public void MountReponseOutput(Sony.PS4.SaveData.Mounting.MountResponse response)
    {
        if (response != null)
        {
            OnScreenLog.Add("MountPoint : " + response.MountPoint.PathName.Data);
            OnScreenLog.Add("RequiredBlocks : " + response.RequiredBlocks);
            OnScreenLog.Add("WasCreated : " + response.WasCreated);
        }
    }

    public void MountInfoReponseOutput(Sony.PS4.SaveData.Mounting.MountInfoResponse response)
    {
        if (response != null)
        {
            Sony.PS4.SaveData.SaveDataInfo sdInfo = response.Info; 
            OnScreenLog.Add("Blocks : " + sdInfo.Blocks);
            OnScreenLog.Add("FreeBlocks : " + sdInfo.FreeBlocks);
        }
    }

    public void MountParamsReponseOutput(Sony.PS4.SaveData.Mounting.MountParamsResponse response)
    {
        if (response != null)
        {
            Sony.PS4.SaveData.SaveDataParams sdParams = response.Params;

            OnScreenLog.Add("Title : " + sdParams.Title);
            OnScreenLog.Add("SubTitle : " + sdParams.SubTitle);
            OnScreenLog.Add("Detail : " + sdParams.Detail);
            OnScreenLog.Add("UserParam : " + sdParams.UserParam);
            OnScreenLog.Add("Time : " + sdParams.Time.ToString("d/M/yyyy HH:mm:ss"));
        }
    }

    public void LoadIconReponseOutput(Sony.PS4.SaveData.Mounting.LoadIconResponse response)
    {
        if (response != null)
        {
            if ( response.Icon != null)
            {
                OnScreenLog.Add("Icon loaded");
                SetIconTexture(response.Icon);
            }
            else
            {
                OnScreenLog.Add("No Icon saved");
            }       
        }
    }
}
}
#endif
