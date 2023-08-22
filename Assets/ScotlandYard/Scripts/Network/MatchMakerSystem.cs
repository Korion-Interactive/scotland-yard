using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_ANDROID 
    using BluetoothMultiplayer = Bluetooth.Android.MultiplayerRT;
#elif UNITY_IPHONE
    using BluetoothMultiplayer = Bluetooth.iOS.Multipeer.MultiplayerRT;
#endif

public class MatchMakerSystem : BaseSystem<ConnectionEvent, GameSetupEvents, MatchMakerSystem>
{
    public MatchMakingSettings Settings;

    public UIPanel WaitingPanel;
    public UniquePanelEnsurer GameSettingsPanel;
    public List<UIPanel> PanelsToKeep = new List<UIPanel>();

    public GameObject StartMatchButton;
    public GameObject BackButton;
    public Dictionary<int, UIToggle> PlayerCount = new Dictionary<int, UIToggle>();

    public BlurCam BlurBG;

    protected override void RegisterEvents()
    {
        this.LogInfo("Register Events");

        ListenTo<ConnectionArgs>(ConnectionEvent.HostChanged, HostChanged);
        //ListenTo(ConnectionEvent.ClientServerMatchStarted, ClientServerMatchStarted);


        GSP.MultiplayerRTEvents.MatchMakerFoundMatch += MatchMakerFoundMatch;
        GSP.MultiplayerRTEvents.MatchMakerCancelled += MatchMakerCancelled;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GSP.MultiplayerRTEvents.MatchMakerFoundMatch -= MatchMakerFoundMatch;
        GSP.MultiplayerRTEvents.MatchMakerCancelled -= MatchMakerCancelled;
    }

    protected override void Start()
    {
        /*
        if (GSP.Status == null)
        {
            GSP.Init(new SocialStatus(), new BluetoothMultiplayer());
        }

        // Deactivate Multiplayer each start to allow local games after a multiplayer game
        GSP.DeactivateMultiplayerRT();

        base.Start();
        */
    }

    // Called from button click
    public void StartQuickMatch(GameObject caller)
    {
        
    }

    // Called from button click
    public void StartBluetoothMatch(GameObject caller)
    {
        #if UNITY_IOS || UNITY_ANDROID
        StartMatch<BluetoothMultiplayer>(caller, null, null);
        #endif
    }

    private void StartMatch<T>(GameObject caller, Action multiplayerAvailableCallback, Action multiplayerNotAvailableCallback)
        where T: RealtimeMultiplayerBaseProvider
    {
        GSP.ActivateMultiplayerRT<T>();

        // Connection Observer listens to MatchMaker->FoundMatch then broadcasts "HostChanged"
        ConnectionObserver.CreateInstance();

        if (GSP.IsMultiplayerRTAvailable)
        {
            caller.SetComponentsEnabled<UIPlayAnimation>(true);
            GameSetupBehaviour.Instance.Reset();

            if (multiplayerAvailableCallback != null)
                multiplayerAvailableCallback();
        }
        else
        {
            caller.SetComponentsEnabled<UIPlayAnimation>(false);

            PopupManager.ShowPrompt("no_multiplayer_available_header", "no_multiplayer_available_text");

            if (multiplayerNotAvailableCallback != null)
                multiplayerNotAvailableCallback();

            this.LogError("Cannot start Quick Match: no multiplayer available.");
        }
    }

    void MatchMakerFoundMatch()
    {
        this.LogInfo("Found Match");
        
        PopupManager.CloseNotification();

        GameSetupBehaviour.Instance.Setup.DetectiveSetups.ForEach(o => o.Controller = PlayerController.None);

        if (WaitingPanel.gameObject.activeSelf)
        {
            int cnt = GSP.MultiplayerRT.GetNumberOfParticipants(true);
            if(PlayerCount.ContainsKey(cnt))
                PlayerCount[cnt].value = true;

            StartMatchButton.SendMessage("OnClick"); // start animation to go to next screen
        }
        else
        {
            foreach (Transform child in WaitingPanel.transform.parent)
            {
                UIPanel panel = child.GetComponent<UIPanel>();
                if(panel != null && !PanelsToKeep.Contains(panel))
                    panel.gameObject.SetActive(false);
            }

            BlurBG.Blur();

            GameSetupSettings.Instance.StartNetworkGame();
            GameSettingsPanel.Activate();
            GameSettingsPanel.gameObject.SetActive(true);
        }
    }
    public void MatchMakerCancelled()
    {
        if (WaitingPanel.gameObject.activeSelf)
        {
            GSP.AllowInvites = true;
            BackButton.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
            BackButton.SendMessage("OnClick"); // start animation to go to previous screen
        }

        if (GSP.IsMultiplayerRTAvailable)
            GSP.MultiplayerRT.Disconnect();
    }


    void HostChanged(ConnectionArgs args)
    {
        this.LogDebug("HostChanged()");
        // Check for non-initial host change
        if (GameSetupBehaviour.Instance.CountDetectiveControlTypes(PlayerController.None) + 1 == Globals.Max_Player_Count)
        {
            this.LogWarn("host found first time");

            RollHostDiceArgs[] participants = ConnectionObserver.Instance.GetParticipantsOrdered().ToArray();

            var setup = GameSetupBehaviour.Instance.Setup;
            SetupPlayerController(setup.MrXSetup, participants[0]);

            for (int i = 0; i < participants.Length - 1; i++)
            {
                RollHostDiceArgs ptcp = participants[i + 1];
                SetupPlayerController(setup.DetectiveSetups[i], ptcp);
            }

            for (int j = participants.Length; j < Globals.Max_Player_Count; j++)
            {
                setup.DetectiveSetups[j - 1].Controller = PlayerController.Ai;
            }

            setup.RoundTime = 45;

            this.LogDebug(GameSetupBehaviour.Instance.PrintSettings());

            GameSetupSettings.Instance.Initialize();
        }
        else
        {
            // Host has changed...
            this.LogWarn("host changed - not the first time");
        }
    }

    void SetupPlayerController(PlayerSetup setup, RollHostDiceArgs args)
    {
        setup.Controller = (args.ParticipantId == GSP.MultiplayerRT.OwnParticipantId) ? PlayerController.Human : PlayerController.Network;
        setup.DisplayName = args.PlayerName;
        setup.ControllingParticipantID = args.ParticipantId;
    }
}
