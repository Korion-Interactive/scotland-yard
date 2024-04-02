using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataBackup : IScreen
{
    MenuLayout m_MenuBackup;

    public SonySaveDataBackup()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuBackup;
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
        m_MenuBackup = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuBackup.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";

        if (isEnabled == true)
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuBackup.AddItem("Backup", "Backup the current save data directory." + dirNameToolTip, isEnabled))
        {
            Backup();
        }

        if (m_MenuBackup.AddItem("Check Backup", "Test to check if a backup exist. Also returns params and icon for backup save data." + dirNameToolTip, isEnabled))
        {
            CheckBackup();
        }

        if (m_MenuBackup.AddItem("Restore Backup", "Restore the backup of the current save data directory." + dirNameToolTip, isEnabled))
        {
            RestoreBackup();
        }

        if (m_MenuBackup.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Backup()
    {
        try
        {
            Sony.PS4.SaveData.Backups.BackupRequest request = new Sony.PS4.SaveData.Backups.BackupRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Backups.Backup(request, response);

            OnScreenLog.Add("Backup Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void CheckBackup()
    {
        try
        {
            Sony.PS4.SaveData.Backups.CheckBackupRequest request = new Sony.PS4.SaveData.Backups.CheckBackupRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;
            request.IncludeParams = true;
            request.IncludeIcon = true;

            Sony.PS4.SaveData.Backups.CheckBackupResponse response = new Sony.PS4.SaveData.Backups.CheckBackupResponse();

            int requestId = Sony.PS4.SaveData.Backups.CheckBackup(request, response);

            OnScreenLog.Add("CheckBackup Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RestoreBackup()
    {
        try
        {
            Sony.PS4.SaveData.Backups.RestoreBackupRequest request = new Sony.PS4.SaveData.Backups.RestoreBackupRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Backups.RestoreBackup(request, response);

            OnScreenLog.Add("Backup Async : Request Id = " + requestId);
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
            case Sony.PS4.SaveData.FunctionTypes.CheckBackup:
                {
                    CheckBackupReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Backups.CheckBackupResponse);
                }
                break;
        }
    }

    public void CheckBackupReponseOutput(Sony.PS4.SaveData.Backups.CheckBackupResponse response)
    {
        if (response != null)
        {
            if (response.ReturnCode == Sony.PS4.SaveData.ReturnCodes.SUCCESS)
            {
                bool hasParams = response.HasParams;
                bool hasIcon = response.HasIcon;

                if (hasParams == true)
                {
                    var sdParams = response.Params;

                    OnScreenLog.Add("   Title : " + sdParams.Title);
                    OnScreenLog.Add("   SubTitle : " + sdParams.SubTitle);
                    OnScreenLog.Add("   Detail : " + sdParams.Detail);
                    OnScreenLog.Add("   UserParam : " + sdParams.UserParam);
                    OnScreenLog.Add("   Time : " + sdParams.Time.ToString("d/M/yyyy HH:mm:ss"));
                }
                else
                {
                    OnScreenLog.Add("   No Parmas returned for backup save data");
                }

                if (hasIcon == true)
                {
                    Sony.PS4.SaveData.Icon icon = response.Icon;

                    OnScreenLog.Add("Icon size : W = " + icon.Width + " H = " + icon.Height);
                }
                else
                {
                    OnScreenLog.Add("   No Icon returned for backup save data");
                }
            }
        }
    }

}
}
#endif
