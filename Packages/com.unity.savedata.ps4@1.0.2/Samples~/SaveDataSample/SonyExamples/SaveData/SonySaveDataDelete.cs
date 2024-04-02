using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataDelete : IScreen
{
    MenuLayout m_MenuDelete;

    public SonySaveDataDelete()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDelete;
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
        m_MenuDelete = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDelete.Update();

        bool isEnabled = SaveDataDirNames.HasCurrentDirName();

        string dirName = SaveDataDirNames.GetCurrentDirName();

        string dirNameToolTip = "";

        if (isEnabled == true)
        {
            dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
        }

        if (m_MenuDelete.AddItem("Delete", "Delete the save data."+ dirNameToolTip, isEnabled))
        {
            Delete();
        }

        if (m_MenuDelete.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Delete()
    {
        try
        {
            Sony.PS4.SaveData.Deleting.DeleteRequest request = new Sony.PS4.SaveData.Deleting.DeleteRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = SaveDataDirNames.GetCurrentDirName();

            request.UserId = User.GetActiveUserId;
            request.DirName = dirName;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Deleting.Delete(request, response);

            OnScreenLog.Add("Delete Async : Request Id = " + requestId);
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
            case Sony.PS4.SaveData.FunctionTypes.Delete:
                {
                    Sony.PS4.SaveData.EmptyResponse response = callbackEvent.Response as Sony.PS4.SaveData.EmptyResponse;

                    if (response != null && response.ReturnCode == Sony.PS4.SaveData.ReturnCodes.SUCCESS)
                    {
                        Sony.PS4.SaveData.Deleting.DeleteRequest request = callbackEvent.Request as Sony.PS4.SaveData.Deleting.DeleteRequest;

                        if (request != null)
                        {
                            SaveDataDirNames.RemoveDirName(request.DirName.Data);
                        }
                    }
                }
                break;
        }
    }

}
}
#endif
