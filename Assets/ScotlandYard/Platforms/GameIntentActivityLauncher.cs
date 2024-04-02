using Cysharp.Threading.Tasks;
#if UNITY_PS5
using Korion.PS5.PSN;
using static Unity.PSN.PS5.GameIntent.GameIntentSystem;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
//using Bumblebee.Fishing.Game;
//using Bumblebee.Fishing.Management;
using UnityEngine.EventSystems;

public class GameIntentActivityLauncher : MonoBehaviour
{
    //[SerializeField]
    //private StartGameSetup _startGameSetup;

    //[SerializeField]
    //private GameObject _gameStartSequence;

    //public TMPro.TextMeshProUGUI _tmpText;

    public static bool GameStartReady = false;
    public static int ReadCounter = 0;

//    private void OnEnable()
//    {
//        //TryLoadingGameState("BuTKino").Forget();
//#if UNITY_PS5 && !UNITY_EDITOR
//        GameIntentManager.Instance.NotificationEvent.AddListener(OnNotification);
//#endif
//#if UNITY_EDITOR
//        //TryLoadingGameState().Forget(); //KORION: ONLY FOR TESTING
//#endif
//    }

//    private void OnDisable()
//    {
//#if UNITY_PS5 && !UNITY_EDITOR
//        GameIntentManager.Instance.NotificationEvent.RemoveListener(OnNotification);
//#endif
//    }

//#if UNITY_PS4
//    private void Awake()
//    {
//        //normal Start
//        _gameStartSequence.SetActive(true);
//    }
//#endif

    //private async UniTaskVoid TryLoadingGameState(string activityId)
    //{

    //    if (ActivityManager.Instance == null)
    //    {

    //        //normal Start
    //        _gameStartSequence.SetActive(true);
    //        Debug.Log("noActivityStarted");
    //        return;
    //    }


    //    ActivityManager.Instance.IsAwaitingActivity = true;

    //    //init for save game //GetTotalUnlockedSpots //improve could be a bool
    //    await UniTask.WaitUntil(() => GameStartReady == true);


    //    await UniTask.WaitUntil(() => ReadCounter == 5);


    //    int lastPlayer = PlayerPrefs.GetInt("LastPlayer", 0);


    //    //never started before = we wont create a random character, or do we
    //    if (!GameManager.FlowManager.SetupPlayer(lastPlayer))
    //        GameManager.FlowManager.NewPlayer(lastPlayer, "Hans Huber", 1);

    //    //MhhK erste mal ins game reinladen //progress startet //nicht den zweiten freigeschaltet //auch nach ps systemneustart
    //    if (ActivityManager.Instance.GetTotalUnlockedSpots() <= 0)
    //    {

    //        ActivityManager.Instance.StartActivity("AF_AllLocations");
    //        Load(destroyCancellationToken).Forget();
    //    }
    //    else
    //    {
    //        if (ActivityManager.Instance.GetTotalUnlockedSpots() >= 14)
    //        {

    //            ActivityManager.Instance.EndActivity("AF_AllLocations");

    //            //normal Start
    //            Load(destroyCancellationToken).Forget();
    //            return;
    //        }
    //        else
    //        {

    //            ActivityManager.Instance.ResumeActivities();
    //            Load(destroyCancellationToken).Forget();
    //        }
    //    }
    //}

    //private async UniTaskVoid Load(CancellationToken cancellationToken = default)
    //{
    //    Debug.Log("isLoadingActivity");

    //    //update
    //    ActivityManager.Instance.UpdateResumeableLakeSpotID();
    //    int lakeID = ActivityManager.Instance.LakeID;
    //    int spotID = ActivityManager.Instance.SpotID;

    //    GameManager.GuiReference.LakeSpotSelection.SelectedLakeSpotID = spotID;

    //    GameManager.GuiReference.Loading.Setup(lakeID, spotID);

    //    ActivityManager.Instance.WindZone.SetActive(false);
    //}

//#if UNITY_PS5 && !UNITY_EDITOR
//    private void OnNotification(GameIntent gameIntent)
//    {
//        if (gameIntent is LaunchActivity launchActivity)
//        {
//            string activityID = launchActivity.ActivityId;
//            Debug.Log("KORION: Loaded PS5 Activity: " + activityID);
//            TryLoadingGameState(activityID).Forget();
//        }
//    }
//#endif
}
