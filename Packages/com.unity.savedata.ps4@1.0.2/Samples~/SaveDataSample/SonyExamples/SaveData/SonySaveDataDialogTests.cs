using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataDialogTests : IScreen
{
    MenuLayout m_MenuDialogTests;

    float progress = 0.0f;
    bool showingProgressBar = false;

    public SonySaveDataDialogTests()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuDialogTests;
    }

    bool isActive = false;

    public void OnEnter()
    {
        isActive = true;
    }

    public void OnExit()
    {
        isActive = false;
    }

    public void Process(MenuStack stack)
    {
        if(showingProgressBar == true)
        {
            if (Sony.PS4.SaveData.Dialogs.DialogIsReadyToDisplay() == true)
            {
                progress += 0.25f;
                Sony.PS4.SaveData.Dialogs.ProgressBarSetValue((UInt32)progress);

                if (progress >= 100.0f)
                {
                    showingProgressBar = false;
                    progress = 0;

                    Sony.PS4.SaveData.Dialogs.CloseParam closeParam = new Sony.PS4.SaveData.Dialogs.CloseParam();
                    closeParam.Anim = Sony.PS4.SaveData.Dialogs.Animation.On;

                    Sony.PS4.SaveData.Dialogs.Close(closeParam);
                }
            }
        }

        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuDialogTests = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuDialogTests.Update();

        if (m_MenuDialogTests.AddItem("User Message Dialog", "Open an example user message dialog"))
        {
            UserMessageDialog();
        }

        if (m_MenuDialogTests.AddItem("System Message Dialog", "Open an example system message dialog"))
        {
            SystemMessageDialog();
        }

        if (m_MenuDialogTests.AddItem("Error Dialog", "Open an example error code dialog"))
        {
            ErrorDialog();
        }

        if (m_MenuDialogTests.AddItem("Progress Bar Dialog", "Open an example progress bar dialog"))
        {
            ProgressBarDialog();
        }

        if (m_MenuDialogTests.AddItem("List Dialog", "Open an example savedata list dialog"))
        {
            ListDialog();
        }

        if (m_MenuDialogTests.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void UserMessageDialog()
    {
        try
        {
            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.UserMsg;
            request.DispType = Sony.PS4.SaveData.Dialogs.DialogType.Save;

            request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

            Sony.PS4.SaveData.Dialogs.UserMessageParam msg = new Sony.PS4.SaveData.Dialogs.UserMessageParam();
            msg.MsgType = Sony.PS4.SaveData.Dialogs.UserMessageType.Normal;
            msg.ButtonType = Sony.PS4.SaveData.Dialogs.DialogButtonTypes.YesNo;
            msg.Message = "This is a test of the user message savedata dialog";

            request.UserMessage = msg;

            Sony.PS4.SaveData.Dialogs.OpenDialogResponse response = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

            int requestId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SystemMessageDialog()
    {
        try
        {
            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.SystemMsg;
            request.DispType = Sony.PS4.SaveData.Dialogs.DialogType.Load;

            request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

            Sony.PS4.SaveData.Dialogs.SystemMessageParam msg = new Sony.PS4.SaveData.Dialogs.SystemMessageParam();
            msg.SysMsgType = Sony.PS4.SaveData.Dialogs.SystemMessageType.CorruptedAndDelete;

            Sony.PS4.SaveData.Searching.DirNameSearchResponse searchItems = FullSearch();

            if (searchItems.SaveDataItems.Length > 0)
            {
                Sony.PS4.SaveData.DirName[] dirNames = new Sony.PS4.SaveData.DirName[1];
                dirNames[0] = searchItems.SaveDataItems[0].DirName;

                Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();
                items.DirNames = dirNames;

                request.Items = items;

                request.SystemMessage = msg;

                Sony.PS4.SaveData.Dialogs.OpenDialogResponse response = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

                int requestId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, response);

                OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
            }

        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ErrorDialog()
    {
        try
        {
            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.ErrorCode;
            request.DispType = Sony.PS4.SaveData.Dialogs.DialogType.Save;

            request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

            Sony.PS4.SaveData.Dialogs.ErrorCodeParam errorParam = new Sony.PS4.SaveData.Dialogs.ErrorCodeParam();
            errorParam.ErrorCode = unchecked((int)0x80B80006);

            request.ErrorCode = errorParam;

            Sony.PS4.SaveData.Dialogs.OpenDialogResponse response = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

            int requestId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ProgressBarDialog()
    {
        try
        {
            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.ProgressBar;
            request.DispType = Sony.PS4.SaveData.Dialogs.DialogType.Save;

            request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

            Sony.PS4.SaveData.Dialogs.ProgressBarParam progressBar = new Sony.PS4.SaveData.Dialogs.ProgressBarParam();
            
            progressBar.BarType = Sony.PS4.SaveData.Dialogs.ProgressBarType.Percentage;
            progressBar.SysMsgType = Sony.PS4.SaveData.Dialogs.ProgressSystemMessageType.Progress;

            request.ProgressBar = progressBar;

            Sony.PS4.SaveData.Dialogs.NewItem newItem = new Sony.PS4.SaveData.Dialogs.NewItem();

            newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
            newItem.Title = "Testing new item title with progress bar";

            request.NewItem = newItem;

            Sony.PS4.SaveData.Dialogs.OpenDialogResponse response = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

            int requestId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ListDialog()
    {
        try
        {
            Sony.PS4.SaveData.Searching.DirNameSearchResponse searchItems = FullSearch();

            Sony.PS4.SaveData.DirName[] dirNames = null;

            if (searchItems.SaveDataItems.Length > 0)
            {
                dirNames = new Sony.PS4.SaveData.DirName[searchItems.SaveDataItems.Length];

                for (int i = 0; i < searchItems.SaveDataItems.Length; i++)
                {
                    dirNames[i] = searchItems.SaveDataItems[i].DirName;
                }
            }

            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = User.GetActiveUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.List;
            request.DispType = Sony.PS4.SaveData.Dialogs.DialogType.Save;

            request.Animations = new Sony.PS4.SaveData.Dialogs.AnimationParam(Sony.PS4.SaveData.Dialogs.Animation.On, Sony.PS4.SaveData.Dialogs.Animation.On);

            Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();
          
            if(dirNames != null)
            {
                items.DirNames = dirNames;
            }

            items.FocusPos = Sony.PS4.SaveData.Dialogs.FocusPos.DataLatest;
            items.ItemStyle = Sony.PS4.SaveData.Dialogs.ItemStyle.SubtitleDataSize;

            request.Items = items;

            Sony.PS4.SaveData.Dialogs.NewItem newItem = new Sony.PS4.SaveData.Dialogs.NewItem();

            newItem.IconPath = "/app0/Media/StreamingAssets/SaveIcon.png";
            newItem.Title = "Testing new item title with save list";

            request.NewItem = newItem;

            Sony.PS4.SaveData.Dialogs.OpenDialogResponse response = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

            int requestId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("OpenDialog Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public Sony.PS4.SaveData.Searching.DirNameSearchResponse FullSearch()
    {
        try
        {
            Sony.PS4.SaveData.Searching.DirNameSearchRequest request = new Sony.PS4.SaveData.Searching.DirNameSearchRequest();

            request.UserId = User.GetActiveUserId;
            request.Key = Sony.PS4.SaveData.Searching.SearchSortKey.DirName;
            request.Order = Sony.PS4.SaveData.Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = true;
            request.IncludeParams = true;
            request.MaxDirNameCount = Sony.PS4.SaveData.Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;
            request.Async = false;

            Sony.PS4.SaveData.Searching.DirNameSearchResponse response = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();

            Sony.PS4.SaveData.Searching.DirNameSearch(request, response);

            return response;
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return null;
    }

    public void OnAsyncEvent(Sony.PS4.SaveData.SaveDataCallbackEvent callbackEvent)
    {
        if (isActive == false) return;

        switch (callbackEvent.ApiCalled)
        {
            case Sony.PS4.SaveData.FunctionTypes.NotificationDialogOpened:
                {
                    Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = callbackEvent.Request as Sony.PS4.SaveData.Dialogs.OpenDialogRequest;

                    if (request.Mode == Sony.PS4.SaveData.Dialogs.DialogMode.ProgressBar)
                    {
                        showingProgressBar = true;
                        progress = 0;
                    }
                }
                break;
            case Sony.PS4.SaveData.FunctionTypes.OpenDialog:
                {
                    OpenDialogResponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Dialogs.OpenDialogResponse);
                }
                break;
        }
    }

    public void OpenDialogResponseOutput(Sony.PS4.SaveData.Dialogs.OpenDialogResponse response)
    {
        if (response != null)
        {
            Sony.PS4.SaveData.Dialogs.DialogResult result = response.Result;

            if(result == null)
            {
                OnScreenLog.Add("Error occured when opening dialog");
                return;
            }

            OnScreenLog.Add("Mode : " + result.Mode);
            OnScreenLog.Add("Result : " + result.CallResult);
            OnScreenLog.Add("ButtonId : " + result.ButtonId);
            OnScreenLog.Add("DirName : " + result.DirName.Data);
            OnScreenLog.Add("Params :");
            OnScreenLog.Add("   Title : " + result.Param.Title);
            OnScreenLog.Add("   SubTitle : " + result.Param.SubTitle);
            OnScreenLog.Add("   Detail : " + result.Param.Detail);
            OnScreenLog.Add("   UserParam : " + result.Param.UserParam);
            OnScreenLog.Add("   Time : " + result.Param.Time.ToString("d/M/yyyy HH:mm:ss"));
        }
    }

}
}
#endif
