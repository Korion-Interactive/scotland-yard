using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using Bumblebee.Fishing.Management;
//using Bumblebee.Fishing.Game;
//using ManagedSteam.SteamTypes;
#if UNITY_PS5
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.UDS;
using UnityEngine.PS5;
#endif
public class ActivityManager : MonoBehaviour
{
    private static ActivityManager _instance;
    private bool _isActivityRunning;
    private bool _activityInitializedResumeOrStart = false;
    public bool IsAwaitingActivity = false;

    //game
    private int _lakeID;
    private int _lakeSpotID;

    [SerializeField]
    private GameObject _gameStartSequence;

    [SerializeField]
    private GameObject _windZone;

    [SerializeField]
    private GameObject _underwaterScene;

    private Dictionary<int, string> activityNames = new Dictionary<int, string>();

    public static ActivityManager Instance
    {
        get => _instance;
    }

#if UNITY_PS5
    public bool IsActivityRunning => _isActivityRunning;
    public bool ActivityInitializedResumeOrStart => _activityInitializedResumeOrStart;
#endif

    public int LakeID { get => _lakeID; set => _lakeID = value; }
    public int SpotID { get => _lakeSpotID; set => _lakeSpotID = value; }

    public GameObject GameStartSequence { get => _gameStartSequence; set => _gameStartSequence = value; }
    public GameObject WindZone { get => _windZone; set => _windZone = value; }
    public GameObject UnderwaterScene { get => _underwaterScene; set => _underwaterScene = value; }

    private void Awake()
    {
        //TODO KORION PS5
        //if (_instance != null)
        //{
        //    Destroy(this);
        //    return;
        //}
        //_instance = this;

        //FillDictionary();
    }

    private void FillDictionary()
    {
        activityNames.Add(0, "RuhrLakeSpot2");
        activityNames.Add(1, "RuhrLakeSpot3");
        activityNames.Add(2, "RuhrLakeSpot4");
        activityNames.Add(3, "RuhrLakeSpot5");
        activityNames.Add(4, "LochNessSpot1");
        activityNames.Add(5, "LochNessSpot2");
        activityNames.Add(6, "LochNessSpot3");
        activityNames.Add(7, "LochNessSpot4");
        activityNames.Add(8, "LochNessSpot5");
        activityNames.Add(9, "AlaskanLakeSpot1");
        activityNames.Add(10, "AlaskanLakeSpot2");
        activityNames.Add(11, "AlaskanLakeSpot3");
        activityNames.Add(12, "AlaskanLakeSpot4");
        activityNames.Add(13, "AlaskanLakeSpot5");
    }

    public void EndInitialization()
    {
        _activityInitializedResumeOrStart = false;
    }

    //[System.Diagnostics.Conditional("UNITY_PS5")]
//    public void StartActivity(string activityId)
//    {
//#if UNITY_PS5
//        if (_isActivityRunning)
//            return;

//        UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();
//        UniversalDataSystem.UDSEvent udsEvent = new UniversalDataSystem.UDSEvent();
//        udsEvent.Create("activityStart");
//        udsEvent.Properties.Set("activityId", activityId);

//        request.UserId = Utility.initialUserId;
//        request.EventData = udsEvent;

//        Debug.Log($"Created Post request: {JsonConvert.SerializeObject(request)}");

//        var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
//        {
//            if (AsyncRequestUtil.CheckAsyncResultRequest(antecedent))
//            {
//                Debug.Log("Event sent. Activity started");
//                _isActivityRunning = true;
//                _activityInitializedResumeOrStart = true;
//            }
//            else
//            {
//                Debug.Log("Result not successful");
//                Debug.Log($"Event result: {antecedent.Request.Result.apiResult}, ErrorCode: {antecedent.Request.Result.sceErrorCode}, Msg: {antecedent.Request.Result.message}");
//            }
//        });

//        Debug.Log("Scheduling request ...");
//        UniversalDataSystem.Schedule(requestOp);
//#endif
//    }


//    [System.Diagnostics.Conditional("UNITY_PS5")]
//    public void EndActivity(string activityId, string outcome = "completed")
//    {
//#if UNITY_PS5

//        if (!_isActivityRunning)
//            return;

//        UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();
//        UniversalDataSystem.UDSEvent udsEvent = new UniversalDataSystem.UDSEvent();
//        udsEvent.Create("activityEnd");
//        udsEvent.Properties.Set("activityId", activityId);
//        udsEvent.Properties.Set("outcome", outcome);

//        request.UserId = Utility.initialUserId;
//        request.EventData = udsEvent;

//        Debug.Log($"Created Post request: {JsonConvert.SerializeObject(request)}");

//        var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
//        {
//            if (AsyncRequestUtil.CheckAsyncResultRequest(antecedent))
//            {
//                Debug.Log("Event sent");
//                _isActivityRunning = false;
//            }
//            else
//            {
//                Debug.Log($"Event result: {antecedent.Request.Result.apiResult}, ErrorCode: {antecedent.Request.Result.sceErrorCode}, Msg: {antecedent.Request.Result.message}");
//            }
//        });

//        Debug.Log("Scheduling request ...");
//        UniversalDataSystem.Schedule(requestOp);
//#endif
//    }

    //Korion - not neccessary for progress activity - why should it be terminated?
//    [System.Diagnostics.Conditional("UNITY_PS5")]
//    public void TerminateActivity(string activityId)
//    {
//#if UNITY_PS5
//            UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();
//            UniversalDataSystem.UDSEvent udsEvent = new UniversalDataSystem.UDSEvent();
//            udsEvent.Create("activityTerminate");
//            udsEvent.Properties.Set("activityId", activityId);

//            request.UserId = Utility.initialUserId;
//            request.EventData = udsEvent;

//            Debug.Log($"Created Post request: {JsonConvert.SerializeObject(request)}");

//            var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
//            {
//                if (AsyncRequestUtil.CheckAsyncResultRequest(antecedent))
//                {
//                    Debug.Log("Event sent");
//                    _isActivityRunning = false;
//                }
//                else
//                {
//                    Debug.Log($"Event result: {antecedent.Request.Result.apiResult}, ErrorCode: {antecedent.Request.Result.sceErrorCode}, Msg: {antecedent.Request.Result.message}");
//                }
//            });

//            Debug.Log("Scheduling request ...");
//            UniversalDataSystem.Schedule(requestOp);
//#endif
//    }

//    public void ResumeActivities()
//    {
        
//#if UNITY_PS5 && !UNITY_EDITOR
//        List<string> active = new List<string>();
//        List<string> complete = new List<string>();

//        bool activityCompleted = false;

//        Debug.Log("Trying to resume activity");

//        UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();
//        UniversalDataSystem.UDSEvent udsEvent = new UniversalDataSystem.UDSEvent();

//        int totalSpots = _instance.GetTotalUnlockedSpots();

//        for (int i = 0; i < 14; i++)
//        {

//            if (i < totalSpots)
//            {
//                complete.Add(activityNames[i]);
//            }
//            else
//            {
//                active.Add(activityNames[i]);
//            }
//        }

//        Debug.Log("Completed Spots: " + complete.Count + ", Active Spots: " + active.Count);

//        if (complete.Count >= 14)
//        {
//            Debug.Log("Ending Activity as all Spots have been unlocked");
//            udsEvent.Create("activityEnd");
//            udsEvent.Properties.Set("activityId", "AF_AllLocations");

//            activityCompleted = true;
//        }
//        else
//        {
//            Debug.Log("Resuming Activity");
//            udsEvent.Create("activityResume");
//            udsEvent.Properties.Set("activityId", "AF_AllLocations");
//            _activityInitializedResumeOrStart = true;

//            UniversalDataSystem.EventPropertyArray activeArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
//            activeArray.AppendValues(active.ToArray());
//            udsEvent.Properties.Set(new UniversalDataSystem.EventProperty("inProgressActivities", activeArray));
//            UniversalDataSystem.EventPropertyArray completedArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
//            completedArray.AppendValues(complete.ToArray());
//            udsEvent.Properties.Set(new UniversalDataSystem.EventProperty("completedActivities", completedArray));
//        }

//        request.UserId = Utility.initialUserId;
//        request.EventData = udsEvent;
//        Debug.Log($"Created Post request: {JsonConvert.SerializeObject(request)}");

//        var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
//        {
//            if (antecedent != null)
//            {

//                if (antecedent.Request != null && antecedent.Request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Success)
//                {
//                    Debug.Log("Event sent");
//                    if(activityCompleted)
//                    {
//                        _isActivityRunning = false;
//                    }
//                    else
//                    {
//                        _isActivityRunning = true;
//                    }
//                }
//                else
//                {
//                    Debug.Log($"Event result: {antecedent.Request.Result.apiResult}, ErrorCode: {antecedent.Request.Result.sceErrorCode}, Msg: {antecedent.Request.Result.message}");
//                }
//            }
//        });

//        Debug.Log("Scheduling request ...");
//        UniversalDataSystem.Schedule(requestOp);
//#endif
        
//    }

    //public int GetTotalUnlockedSpots()
    //{
    //    int totalUnlocked = 0;

    //    //Lake alaska Spot 5
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 75)
    //        totalUnlocked++;

    //    //Lake alaska Spot 4
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 50)
    //        totalUnlocked++;

    //    //Lake alaska Spot 3
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 25)
    //        totalUnlocked++;

    //    //Lake alaska Spot 2
    //    if (GameManager.FlowManager.CurrentSaveGame.PlayerCaughtEel >= 1)
    //        totalUnlocked++;

    //    //Special: Lake alaska Spot 1 //FIX
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 > 60)
    //        totalUnlocked++;

    //    //Lake lochness Spot 5
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Regenbogenforelle) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Hecht) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Aesche) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Schleie))
    //        totalUnlocked++;

    //    //Lake lochness Spot 4
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 75)
    //        totalUnlocked++;

    //    //Lake lochness Spot 3
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 50)
    //        totalUnlocked++;

    //    //Lake lochness Spot 2
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 25)
    //        totalUnlocked++;

    //    //Special: Lake Alaska Spot 1 //FIX
    //    if (GameManager.FlowManager.CurrentSaveGame.PlayerLevel >= 3)
    //        totalUnlocked++;

    //    //Lake ruhr Spot 5
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Flussbarsch) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Karpfen) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Rotauge) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Brasse) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Zander))
    //        totalUnlocked++;

    //    //Lake ruhr Spot 4
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 80)
    //        totalUnlocked++;

    //    //Lake ruhr Spot 3
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 40)
    //        totalUnlocked++;

    //    //Lake ruhr Spot 2
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 20)
    //        totalUnlocked++;

    //    return totalUnlocked;
    //}

    ///// <summary>
    ///// Also updates LakeSpot and Lake id
    ///// </summary>
    ///// <returns></returns>
    //public void UpdateResumeableLakeSpotID()
    //{
    //    //everything finished in sea lochness
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 75 && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Regenbogenforelle) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Hecht) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Aesche) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Schleie))
    //    {  //Lake alaska Spot 5
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 75)
    //        {
    //            LakeID = 1;
    //            SpotID = 4;
    //            return;
    //        }

    //        //Lake alaska Spot 4
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 50)
    //        {
    //            LakeID = 1;
    //            SpotID = 3;
    //            return;
    //        }

    //        //Lake alaska Spot 3
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee3 >= 25)
    //        {
    //            LakeID = 1;
    //            SpotID = 2;
    //            return;
    //        }

    //        //Lake alaska Spot 2
    //        if (GameManager.FlowManager.CurrentSaveGame.PlayerCaughtEel >= 1)
    //        {
    //            LakeID = 1;
    //            SpotID = 1;
    //            return;
    //        }
 
    //        //Special: Lake alaska Spot 1 //FIX
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 60)
    //        {
    //            LakeID = 1;
    //            SpotID = 0;
    //            return;
    //        }
    //    }

    //    //everything finished in ruhr
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 80 && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Flussbarsch) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Karpfen) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Rotauge) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Brasse) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Zander))
    //    {
    //        //Lake lochness Spot 5
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Regenbogenforelle) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Hecht) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Aesche) && GameManager.FlowManager.CurrentSaveGame.CaughtSee2.Contains(FishingManager.Fishs.Schleie))
    //        {
    //            LakeID = 2;
    //            SpotID = 4;
    //            return;
    //        }

    //        //Lake lochness Spot 4
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 75)
    //        {
    //            LakeID = 2;
    //            SpotID = 3;
    //            return;
    //        }

    //        //Lake lochness Spot 3
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 50)
    //        {
    //            LakeID = 2;
    //            SpotID = 2;
    //            return;
    //        }

    //        //Lake lochness Spot 2
    //        if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee2 >= 25)
    //        {
    //            LakeID = 2;
    //            SpotID = 1;
    //            return;
    //        }

    //        //Special: Lake lochness Spot 1 //FIX
    //        if (GameManager.FlowManager.CurrentSaveGame.PlayerLevel >= 3)
    //        {
    //            LakeID = 2;
    //            SpotID = 0;
    //            return;
    //        }
    //    }

    //    //Lake ruhr Spot 5
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Flussbarsch) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Karpfen) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Rotauge) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Brasse) && GameManager.FlowManager.CurrentSaveGame.CaughtSee1.Contains(FishingManager.Fishs.Zander))
    //    {
    //        LakeID = 0;
    //        SpotID = 4;
    //        return;
    //    }

    //    //Lake ruhr Spot 4
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 80)
    //    {
    //        LakeID = 0;
    //        SpotID = 3;
    //        return;
    //    }

    //    //Lake ruhr Spot 3
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 40)
    //    {
    //        LakeID = 0;
    //        SpotID = 2;
    //        return;
    //    }

    //    //Lake ruhr Spot 2
    //    if (GameManager.FlowManager.CurrentSaveGame.CaughtFishSee1 >= 20)
    //    {
    //        LakeID = 0;
    //        SpotID = 1;
    //        return;
    //    }

    //    //Special: Lake ruhr Spot 1
    //    LakeID = 0;
    //    SpotID = 0;
    //}



    //Korion - not neccessary for progress activity - always available, no need to unlock
    //    [System.Diagnostics.Conditional("UNITY_PS5")]
    //    public void UpdateActivityAvailability(string[] availableActivities, string[] unavailableActivities)
    //    {
    //#if UNITY_PS5
    //        UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();
    //        UniversalDataSystem.UDSEvent udsEvent = new UniversalDataSystem.UDSEvent();
    //        udsEvent.Create("activityAvailabilityChange");
    //        udsEvent.Properties.Set("mode", "full");
    //        UniversalDataSystem.EventPropertyArray availableArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
    //        availableArray.AppendValues(availableActivities);

    //        UniversalDataSystem.EventPropertyArray unavailableArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
    //        unavailableArray.AppendValues(unavailableActivities);

    //        udsEvent.Properties.Set("availableActivities", availableArray);
    //        udsEvent.Properties.Set("unavailableActivities", unavailableArray);

    //        request.UserId = Utility.initialUserId;
    //        request.EventData = udsEvent;
    //        Debug.Log($"Created Post request: {JsonConvert.SerializeObject(request)}");

    //        var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
    //        {
    //            if (AsyncRequestUtil.CheckAsyncResultRequest(antecedent))
    //            {
    //                Debug.Log("Event sent");
    //            }
    //            else
    //            {
    //                Debug.Log($"Event result: {antecedent.Request.Result.apiResult}, ErrorCode: {antecedent.Request.Result.sceErrorCode}, Msg: {antecedent.Request.Result.message}");
    //            }
    //        });

    //        Debug.Log("Scheduling request ...");
    //        UniversalDataSystem.Schedule(requestOp);
    //#endif
    //    }
}
