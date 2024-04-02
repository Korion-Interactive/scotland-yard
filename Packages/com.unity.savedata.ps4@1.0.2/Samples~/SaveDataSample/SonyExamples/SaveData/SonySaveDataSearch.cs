using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
namespace SaveDataSample
{

public class SonySaveDataSearch : IScreen
{
    MenuLayout m_MenuSearch;

    public SonySaveDataSearch()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuSearch;
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
        m_MenuSearch = new MenuLayout(this, 530, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuSearch.Update();

        if (m_MenuSearch.AddItem("Simple Search", "Search for last 10 save data directory names sorted by time."))
        {
            Search();
        }

        if (m_MenuSearch.AddItem("Full Search", "Search for all save data directory names including params and block sizes."))
        {
            FullSearch();
        }

        if (m_MenuSearch.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void Search()
    {
        try
        {
            Sony.PS4.SaveData.Searching.DirNameSearchRequest request = new Sony.PS4.SaveData.Searching.DirNameSearchRequest();

            request.UserId = User.GetActiveUserId;
            request.Key = Sony.PS4.SaveData.Searching.SearchSortKey.Time;
            request.Order = Sony.PS4.SaveData.Searching.SearchSortOrder.Ascending;
            request.IncludeBlockInfo = false;
            request.IncludeParams = false;
            request.MaxDirNameCount = 10;

            Sony.PS4.SaveData.Searching.DirNameSearchResponse response = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();

            int requestId = Sony.PS4.SaveData.Searching.DirNameSearch(request, response);

            OnScreenLog.Add("DirNameSearch Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void FullSearch()
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

            Sony.PS4.SaveData.Searching.DirNameSearchResponse response = new Sony.PS4.SaveData.Searching.DirNameSearchResponse();

            int requestId = Sony.PS4.SaveData.Searching.DirNameSearch(request, response);

            OnScreenLog.Add("DirNameSearch Async : Request Id = " + requestId);
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
            case Sony.PS4.SaveData.FunctionTypes.DirNameSearch:
                {
                    DirNameSearchReponseOutput(callbackEvent.Response as Sony.PS4.SaveData.Searching.DirNameSearchResponse);
                }
                break;
        }
    }

    public void DirNameSearchReponseOutput(Sony.PS4.SaveData.Searching.DirNameSearchResponse response)
    {
        SaveDataDirNames.ClearAllNames();

        if (response != null)
        {
            bool hasParams = response.HasParams;
            bool hasInfo = response.HasInfo;

            var saveDataItems = response.SaveDataItems;

            OnScreenLog.Add("Search Found " + saveDataItems.Length + " saves");

            if ( saveDataItems.Length == 0 )
            {
                OnScreenLog.Add("Search didn't find any saves for this user.");
            }

            for (int i = 0; i < saveDataItems.Length; i++)
            {
                var dirName = saveDataItems[i].DirName;

                SaveDataDirNames.AddDirName(dirName);

                OnScreenLog.Add("DirName : " + dirName.Data);

                if (hasParams == true)
                {
                    var sdParams = saveDataItems[i].Params;

                    OnScreenLog.Add("   Title : " + sdParams.Title);
                    OnScreenLog.Add("   SubTitle : " + sdParams.SubTitle);
                    OnScreenLog.Add("   Detail : " + sdParams.Detail);
                    OnScreenLog.Add("   UserParam : " + sdParams.UserParam);
                    OnScreenLog.Add("   Time : " + sdParams.Time.ToString("d/M/yyyy HH:mm:ss"));
                }

                if (hasInfo == true)
                {
                    var sdInfo = saveDataItems[i].Info;

                    OnScreenLog.Add("   Blocks : " + sdInfo.Blocks);
                    OnScreenLog.Add("   FreeBlocks : " + sdInfo.FreeBlocks);
                }
            }
        }
    }

}
}
#endif
