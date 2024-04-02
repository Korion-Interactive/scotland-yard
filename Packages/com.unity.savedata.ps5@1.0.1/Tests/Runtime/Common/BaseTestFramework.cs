#if UNITY_PS5
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PS5;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;
using Unity.SaveData.PS5.Search;
using Unity.SaveData.PS5.Initialization;

namespace SaveDataTests
{

    public class BaseTestFramework
    {
        [OneTimeSetUp]
        public void Setup()
        {

        }

    //    static bool initializationForTestComplete = false;

        enum InitState
        {
            Started,
            Initalising,
            SearchingForSaves,
            DeleteingSaves,
            Ready,
            Error,
        }

        static InitState s_CurrentInitState = InitState.Started;

        static public string s_InitOutput = "";

        bool initOK = false;

        // Called by each test, which must make sure the save system is correctly initialized and the
        // devkit is cleaned of all save games first

        // Each test will have its own instance of BaseTestFramework, but the initialisation must be shared.
        // Only one of the tests should actually run it. Only one tests runs anyway at a time

        public bool IsInitialized()
        {
            if (s_CurrentInitState == InitState.Ready)
            {
                return true;
            }

            // Give the app time to start up
            if ( Time.realtimeSinceStartup < 5.0f )
            {
                return false;
            }

            switch(s_CurrentInitState)
            {
                case InitState.Started:
                    initOK = InitializePlugin();
                    s_CurrentInitState = InitState.Initalising;
                    break;
                case InitState.Initalising:
                    if (initOK == true) s_CurrentInitState = InitState.SearchingForSaves;
                    else s_CurrentInitState = InitState.Error;               
                    break;
                case InitState.SearchingForSaves:
                    SearchForSaves();
                    break;
                case InitState.DeleteingSaves:
                    DeletingSaves();
                    break;
                case InitState.Error:

                    break;
            }

            return false;
        }

        static public InitResult initResult;
       
        private bool InitializePlugin()
        {
            Main.OnAsyncEvent += OnAsyncEvent;

            try
            {
                InitSettings settings = new InitSettings();

                settings.Affinity = ThreadAffinity.Core5;

                initResult = Main.Initialize(settings);

                if (initResult.Initialized == true)
                {
                    Debug.Log("Savedata Initialized ");
                }
                else
                {
                    Debug.Log("Savedata not initialized ");
                }

                return initResult.Initialized;
            }
            catch (SaveDataException e)
            {
                Debug.LogError("Exception During Initialization : " + e.ExtendedMessage);
            }
#if UNITY_EDITOR
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("Missing DLL Expection : " + e.Message);
                Debug.LogError("The sample APP will not run in the editor.");
            }
#endif

            return false;
        }

        Searching.DirNameSearchResponse searchResponse;

        private void SearchForSaves()
        {
            if (searchResponse == null)
            {
                // Find if there are any saves
                try
                {
                    PS5Input.LoggedInUser user = GetUser();

                    Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

                    request.UserId = user.userId;

                    request.Key = Searching.SearchSortKey.Time;
                    request.Order = Searching.SearchSortOrder.Ascending;
                    request.IncludeBlockInfo = false;
                    request.IncludeParams = false;
                    request.MaxDirNameCount = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;

                    searchResponse = new Searching.DirNameSearchResponse();

                    int requestId = Searching.DirNameSearch(request, searchResponse);
                }
                catch (SaveDataException)
                {
                    s_CurrentInitState = InitState.Error;
                }

                return;
            }

            if ( searchResponse.Locked == true )
            {
                return;
            }

            // searchResponse has some data
            if ( searchResponse.SaveDataItems.Length > 0)
            {
                // There are saves to delete
                s_InitOutput += "Saves Found " + searchResponse.SaveDataItems.Length + "\n";
                s_CurrentInitState = InitState.DeleteingSaves;
            }
            else
            {
                // There are no saves on this kit for the current user
                s_InitOutput += "No Saves Found\n";
                s_CurrentInitState = InitState.Ready;
            }

            Debug.Log("Search Done ");

           // searchResponse = null;
        }

        EmptyResponse deleteResponse;
        int deleteIndex = 0;

        private void DeletingSaves()
        {
            if (searchResponse == null)
            {
                s_CurrentInitState = InitState.Error;
                return;
            }

            if (deleteResponse == null)
            {
               // Debug.Log("****** Delete" + searchResponse.SaveDataItems[deleteIndex].DirName.Data);

                // Delete the next save data dir in the list.

                try
                {
                    PS5Input.LoggedInUser user = GetUser();

                    Deleting.DeleteRequest request = new Deleting.DeleteRequest();

                    DirName dirName = searchResponse.SaveDataItems[deleteIndex].DirName;

                    request.UserId = user.userId;
                    request.DirName = dirName;

                    deleteResponse = new EmptyResponse();

                    int requestId = Deleting.Delete(request, deleteResponse);
                }
                catch (SaveDataException)
                {
                    s_CurrentInitState = InitState.Error;
                }

                return;
            }

            if (deleteResponse.Locked == true)
            {
                return;
            }

        //    Debug.Log("****** Delete Done " + searchResponse.SaveDataItems[deleteIndex].DirName.Data);
            // Delete has completed

            s_InitOutput += "Deleted " + searchResponse.SaveDataItems[deleteIndex].DirName.Data + "\n";

            deleteIndex++;
            if(deleteIndex >= searchResponse.SaveDataItems.Length)
            {
                // All items deleted
        //        Debug.Log("Delete Ready ");
                s_CurrentInitState = InitState.Ready;
            }

            deleteResponse = null;
        }

        static Dictionary<ResponseBase, SaveDataCallbackEvent> callbackEvents = new Dictionary<ResponseBase, SaveDataCallbackEvent>();
        static System.Object syncObj = new  System.Object();
        static private void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
        {
            lock(syncObj)
            {
                callbackEvents.Add(callbackEvent.Response, callbackEvent);
            }
        }

        public PS5Input.LoggedInUser GetUser()
        {
            PS5Input.LoggedInUser loggedInUser = PS5Input.RefreshUsersDetails(0);

            return loggedInUser;
        }

        public void OutputAsyncResponseEvent(ResponseBase response)
        {
            SaveDataCallbackEvent callbackEvent = null;

            lock (syncObj)
            {
                if (callbackEvents.TryGetValue(response, out callbackEvent) == true)
                {
                    callbackEvents.Remove(response);
                }
            }

            if (callbackEvent != null)
            {
                Debug.Log("Event: API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.RequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");

                Debug.Log("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));

                if (callbackEvent.Response.IsErrorCode)
                {
                    string errorMsg = System.String.Format("Error : returnCode = (0x{0:X})", response.ReturnCode);
                    Debug.Log(errorMsg);
                }
            }
            else
            {
                Debug.Log("No Response object found");
            }
        }

        public void LogException(System.Exception e)
        {
            if (e is SaveDataException)
            {
                SaveDataException sde = e as SaveDataException;

                Debug.LogError("Save Exception : " + sde.ExtendedMessage);
            }
            else
            {
                Debug.LogError("Exception : " + e.Message);
            }
        }
    }
}
#endif