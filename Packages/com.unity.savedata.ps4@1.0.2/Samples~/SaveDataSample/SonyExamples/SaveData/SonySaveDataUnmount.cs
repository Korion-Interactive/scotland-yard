using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataUnmount : IScreen
{
    MenuLayout m_MenuUnmount;

    public SonySaveDataUnmount()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuUnmount;
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
        m_MenuUnmount = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        List<Sony.PS4.SaveData.Mounting.MountPoint> mountPoints = Sony.PS4.SaveData.Mounting.ActiveMountPoints;

        m_MenuUnmount.Update();

        Sony.PS4.SaveData.Mounting.MountPoint mp = SonySaveDataMain.GetMountPoint();

        bool isEnabled = (mp != null);

        string moutPointToolTip = "";

        if (mp != null)
        {
            moutPointToolTip = " Use this on mount point \"" + mp.PathName.Data + "\".";
        }

        if (m_MenuUnmount.AddItem("Unmount", "Unmount the last mounted save data." + moutPointToolTip, isEnabled))
        {
            Unmount(mp, false);
        }

        if (m_MenuUnmount.AddItem("Unmount with Backup", "Unmount the last mounted save data and backup. An additional backup notification will occur once the backup is complete." + moutPointToolTip, isEnabled))
        {
            Unmount(mp, true);
        }

        int userId = User.GetActiveUserId;

        for (int i = 0; i < mountPoints.Count; i++)
        {
            if (mountPoints[i].IsMounted == true && mountPoints[i].UserId == userId)
            {
                string menuName = "Unmount ";
                menuName += mountPoints[i].PathName.Data;

                if (m_MenuUnmount.AddItem(menuName, "Unmount this save data"))
                {
                    Unmount(mountPoints[i], false);
                }
            }
        }

        if (m_MenuUnmount.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
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

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            OnScreenLog.Add("Unmounting = " + request.MountPointName.Data);

            int requestId = Sony.PS4.SaveData.Mounting.Unmount(request, response);

            OnScreenLog.Add("Unmount Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    //public void Unmount(Sony.PS4.SaveData.Mounting.MountPoint mountPoint)
    //{
    //    try
    //    {
    //        Sony.PS4.SaveData.Mounting.UnmountRequest request = new Sony.PS4.SaveData.Mounting.UnmountRequest();

    //        request.MountPointName = mountPoint.PathName;

    //        Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

    //        OnScreenLog.Add("Unmounting = " + request.MountPointName.Data);

    //        int requestId = Sony.PS4.SaveData.Mounting.Unmount(request, response);

    //        OnScreenLog.Add("Unmount Async : Request Id = " + requestId);
    //    }
    //    catch (Sony.PS4.SaveData.SaveDataException e)
    //    {
    //        OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
    //    }
    //}

    public void OnAsyncEvent(Sony.PS4.SaveData.SaveDataCallbackEvent callbackEvent)
    {
        switch (callbackEvent.ApiCalled)
        {
            case Sony.PS4.SaveData.FunctionTypes.Unmount:
                {
                    Sony.PS4.SaveData.EmptyResponse response = callbackEvent.Response as Sony.PS4.SaveData.EmptyResponse;

                    if (response != null && response.ReturnCode == Sony.PS4.SaveData.ReturnCodes.SUCCESS)
                    {

                    }
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.NotificationUnmountWithBackup:
                {
                    Sony.PS4.SaveData.UnmountWithBackupNotification response = callbackEvent.Response as Sony.PS4.SaveData.UnmountWithBackupNotification;

                    if (response != null)
                    {
                        OnScreenLog.Add("UserId : 0x" + response.UserId.ToString("X8"));
                        OnScreenLog.Add("DirName : " + response.DirName.Data);
                    }
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.NotificationBackup:
                {
                    Sony.PS4.SaveData.BackupNotification response = callbackEvent.Response as Sony.PS4.SaveData.BackupNotification;

                    if (response != null)
                    {
                        OnScreenLog.Add("UserId : 0x" + response.UserId.ToString("X8"));
                        OnScreenLog.Add("DirName : " + response.DirName.Data);
                    }
                }
                break;
        }
    }

}
}
#endif
