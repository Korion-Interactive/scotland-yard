using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using Ravity;

#if UNITY_ANDROID 
    using BTMP = Bluetooth.Android.MultiplayerRT;
#elif UNITY_IPHONE
    using BTMP = Bluetooth.iOS.Multipeer.MultiplayerRT;
#endif
public class MatchMakingSettings : BaseBehaviour
{
    [HideInInspector]
    public bool ServerClientMode = true;
    [HideInInspector]
    public bool P2PMode = false;

    //public bool IsServerClientMode;

    public GameObject ServerButton, ClientButton;

    public GameObject PlayerCountLabel;
    public UIGrid PlayerCountGrid;
    public List<Tuple<UIToggle, int>> PlayerCount;
    public UILabel StatusText;
    int playerCountSelection = 2;

    public void EnterScreen()
    {
        if(!GSP.IsMultiplayerRTAvailable)
        {
            this.LogError("EnterScreen(): no multiplayer available.");
            return;
        }

        StartCoroutine(CoEnterScreen());
    }

    IEnumerator CoEnterScreen()
    {
        yield return new WaitForEndOfFrame();

        string loc = "";
#if UNITY_IPHONE
        playerCountSelection = Math.Min(playerCountSelection, (GSP.MultiplayerRT is BTMP) ? 2 : 4);
        loc = (GSP.MultiplayerRT is BTMP) ? "matchmaking_start_bluetooth_ios_state" : "matchmaking_start_net_state";
#elif UNITY_ANDROID 
        loc = (GSP.MultiplayerRT is BTMP) ? "matchmaking_start_bluetooth_android_state" : "matchmaking_start_net_state";
#endif
        if (StatusText != null)
            StatusText.text = Loc.Get(loc);
        else
            this.LogError("Status Text is null!");

        int maxPlayers = 6;
#if UNITY_IPHONE
        // HACK: max 4 players with Game Center, max 2 players with bluetooth
        maxPlayers = 2;
#elif UNITY_ANDROID
        if (GSP.MultiplayerRT is BTMP)
            maxPlayers = 2;
#endif
        bool isPlayerGridVisible = maxPlayers > 2;
        PlayerCountLabel.SetActive(isPlayerGridVisible);
        PlayerCountGrid.gameObject.SetActive(isPlayerGridVisible);

        try
        {
            if (isPlayerGridVisible)
            {
                foreach (var toggle in PlayerCount)
                {
                    toggle.A.transform.parent.gameObject.SetActive(toggle.B <= maxPlayers);

                    toggle.A.GetComponent<BoxCollider>().enabled = true;
                    toggle.A.value = toggle.B == playerCountSelection;
                }

		        PlayerCountGrid.Reposition();
            }
        }
        catch (Exception ex) { this.LogError(ex); }


        //IsServerClientMode = serverClientMode;
#if UNITY_ANDROID
        switch(GSP.MultiplayerRT.Mode)
        {
            case MultiplayerMode.ServerClient:
                ServerButton.SetActive(true);
                ServerButton.GetComponentInChildren<LabelTranslator>().SetText("start_server");

                break;
            case MultiplayerMode.PeerToPeer:
                ServerButton.SetActive(true);
                ServerButton.GetComponentInChildren<LabelTranslator>().SetText("show_invitations");
                break;
        }

#else
        ServerButton.SetActive(false);
#endif
        ClientButton.SetActive(true);

        if(isPlayerGridVisible)
        {
            ServerButton.transform.localPosition = new Vector3(0, -400, 0);
            ClientButton.transform.localPosition = new Vector3(0, -650, 0);
        }
        else
        {
            ServerButton.transform.localPosition = new Vector3(0, -100, 0);
            ClientButton.transform.localPosition = new Vector3(0, -350, 0);
        }
    }

    public void SetPlayerCount(GameObject sender)
    {
        UIToggle toggle = sender.GetComponent<UIToggle>();
        if (toggle != null && toggle.value)
        {
            playerCountSelection = PlayerCount.First((o) => o.A == toggle).B;
        }
    }

    public void StartServerOrShowInvites()
    {
        if (InteractionSpacer.IsTooNarrow()) return;
        if (!GSP.IsMultiplayerRTAvailable) return;
        
        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            if (HardwareUtils.IsAndroid && Android.GetSDKLevel() >= Android.ANDROID_12_SDK_LEVEL)
            {
                var hasPermissions = new BoolResult();
                yield return Android.GetPermissions(Android.HostPermissions, hasPermissions);
                if (hasPermissions.Value == false)
                {
                    Debug.Log($"User has not granted the required permissions.");
                    yield break;
                }
            }

            GSP.MultiplayerRT.SetHost(true);
            GSP.MultiplayerRT.StartQuickMatch(playerCountSelection);

            foreach (UIToggle toggle in PlayerCount.Select((o) => o.A))
                toggle.GetComponent<BoxCollider>().enabled = false;

            ServerButton.SetActive(false);
            ClientButton.SetActive(false);

            StatusText.text += "\n\n" + Loc.Get("wait");
        }
    }

    public void ConnectAsClient()
    {
        if (InteractionSpacer.IsTooNarrow()) return;
        if (!GSP.IsMultiplayerRTAvailable) return;
        
        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            if (HardwareUtils.IsAndroid && Android.GetSDKLevel() >= Android.ANDROID_12_SDK_LEVEL)
            {
                var hasClientPermissions = new BoolResult();
                yield return Android.GetPermissions(Android.ClientPermissions, hasClientPermissions);
                if (hasClientPermissions.Value == false)
                {
                    Debug.Log($"User has not granted the required permissions.");
                    yield break;
                }
            }
            
            GSP.MultiplayerRT.SetHost(false);
            GSP.MultiplayerRT.StartQuickMatch(playerCountSelection);

            ServerButton.SetActive(false);
            ClientButton.SetActive(false);

            StatusText.text += "\n\n" + Loc.Get("wait");
        }
    }

    private static class Android
    {
        public static IEnumerator GetPermissions(string[] permissions, BoolResult result)
        {
            var results = new Dictionary<string, bool?>();
            foreach (string permission in permissions) results[permission] = null;

            var callbacks = new UnityEngine.Android.PermissionCallbacks();
            callbacks.PermissionGranted += permission => results[permission] = true;
            callbacks.PermissionDenied += permission => results[permission] = false;
            callbacks.PermissionDeniedAndDontAskAgain += permission => results[permission] = false;
                
            UnityEngine.Android.Permission.RequestUserPermissions(results.Keys.ToArray(), callbacks);
            yield return new WaitUntil(() => results.Values.All(permissionResult => permissionResult.HasValue));
        
            result.Value = results.Values.All(permissionResult => permissionResult.Value);
        }
    
        // https://gamedev.stackexchange.com/a/103116
        public static int GetSDKLevel()
        {
            IntPtr clazz = AndroidJNI.FindClass("android/os/Build$VERSION");
            IntPtr fieldID = AndroidJNI.GetStaticFieldID(clazz, "SDK_INT", "I");
            int sdkLevel = AndroidJNI.GetStaticIntField(clazz, fieldID);
            return sdkLevel;
        }

        public static  string[] HostPermissions => new[] { CONNECT_PERMISSION, SCAN_PERMISSION, ADVERTISE_PERMISSION };
        public static string[] ClientPermissions => new[] { CONNECT_PERMISSION, SCAN_PERMISSION };

        private const string PERMISSION_PREFIX = "android.permission.";
        private const string CONNECT_PERMISSION = PERMISSION_PREFIX + "BLUETOOTH_CONNECT";
        private const string SCAN_PERMISSION = PERMISSION_PREFIX + "BLUETOOTH_SCAN";
        private const string ADVERTISE_PERMISSION = PERMISSION_PREFIX + "BLUETOOTH_ADVERTISE";

        public const int ANDROID_12_SDK_LEVEL = 31;
    }
    
    private class BoolResult { public bool Value; }
}
