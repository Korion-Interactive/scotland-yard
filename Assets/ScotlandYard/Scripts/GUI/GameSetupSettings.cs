using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameSetupSettings : BaseBehaviour
{
    static readonly string[] FallbackMrXNames = new string[] { "Mr Incognito", "Mr Evil", "Joker", "Jack Ripper", "Jule Ace", "Moriarty", "Harry Lame", "Scorpion", "Mr Twain" };
    static readonly string[] FallbackDetectiveNames = new string[] 
    { "James Blond", "Sherlock", "Holmes", "Dr Watson", "Lestrade", "Mr Marple", "Holly Martens", "Austin Power", "Ace Venture", "Eddy Variant", 
        "Mr Diamond", "Charleston", "Mr Wang", "Mrs Marbles", "Mr Perrier"  };

    static GameSetupSettings instance;
    public static GameSetupSettings Instance { get { return instance; } }

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }
    void OnDestroy()
    {
        instance = null;
    }

    [HideInInspector]
    public byte Seconds30 = 30;
    [HideInInspector]
    public byte Seconds45 = 45;
    [HideInInspector]
    public byte Seconds60 = 60;
    [HideInInspector]
    public byte Seconds90 = 90;
    [HideInInspector]
    public byte SecondsInfinity = 0;

    public LabelTranslator StartButtonLabel;

    public UIPanel GameSettingsPanel;

    public Dictionary<byte, UIToggle> TimeOptions = new Dictionary<byte, UIToggle>();
    public Dictionary<string, GameObject> PlayerReadySprites = new Dictionary<string, GameObject>();

    public GameObject PlayerReadyCollider;
    public GameObject PlayerReadyParent;
    public GameObject PlayerReadyPrefab;
    public UILabel Countdown;

    public UIStateButton MrXController;
    public List<UIStateButton> DetectiveControllers = new List<UIStateButton>();

    public UIStateButton MrXAiDifficulty;
    public List<UIStateButton> DetectiveAiDifficulties = new List<UIStateButton>();

    public UILabel MrXName;
    public List<UILabel> DetectiveNames = new List<UILabel>();

    public List<UIInput> PlayerNames = new List<UIInput>();

    public bool IsAvailableForNetwork { get { return isInitialized; } }
    bool isInitialized = false;
    byte lastSetRoundTime = 1; // 1 is "unset"

    List<SetPlayerArgs> storedPlayerArgs = new List<SetPlayerArgs>();

    public void StartNetworkGame()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.Network;
        GSP.AllowInvites = false;
        Reset();
    }

    public void StartLocalGame()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.HotSeat;
        Reset();
        Initialize();
        ReplaceNetworkPlayerWithAi(null);
    }

    public void StartLocalGameMultiController()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.MultiController;
        Reset();
        Initialize();
        ReplaceNetworkPlayerWithAi(null);
    }

    #region TUTORIAL SETUP
    public void StartTutorialGeneral()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.TutorialBasics;
        GameSetupBehaviour.Instance.Setup.RoundTime = float.PositiveInfinity;

        GameSetupBehaviour.Instance.Setup.MrXSetup.Controller = PlayerController.Ai;
        GameSetupBehaviour.Instance.Setup.MrXSetup.StartAtStationId = 1;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].Controller = PlayerController.Human;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].StartAtStationId = 173;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[1].Controller = PlayerController.TutorialCPU;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[1].StartAtStationId = 140;

        for (int i = 2; i < GameSetupBehaviour.Instance.Setup.DetectiveSetups.Length; i++)
        {
            GameSetupBehaviour.Instance.Setup.DetectiveSetups[i].Controller = PlayerController.None;
        }

        LoadGame();

    }

    public void StartTutorialMrX()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.TutorialMrX;
        GameSetupBehaviour.Instance.Setup.RoundTime = float.PositiveInfinity;

        GameSetupBehaviour.Instance.Setup.MrXSetup.Controller = PlayerController.Human;
        GameSetupBehaviour.Instance.Setup.MrXSetup.StartAtStationId = 154;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].Controller = PlayerController.TutorialCPU;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].StartAtStationId = 139;

        for (int i = 1; i < GameSetupBehaviour.Instance.Setup.DetectiveSetups.Length; i++)
        {
            GameSetupBehaviour.Instance.Setup.DetectiveSetups[i].Controller = PlayerController.None;
        }

        LoadGame();

    }

    public void StartTutorialDetective()
    {
        GameSetupBehaviour.Instance.Setup.Mode = GameMode.TutorialDetective;
        GameSetupBehaviour.Instance.Setup.RoundTime = float.PositiveInfinity;

        GameSetupBehaviour.Instance.Setup.MrXSetup.Controller = PlayerController.TutorialCPU;
        GameSetupBehaviour.Instance.Setup.MrXSetup.StartAtStationId = 17;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].Controller = PlayerController.TutorialCPU;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[0].StartAtStationId = 41;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[1].Controller = PlayerController.TutorialCPU;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[1].StartAtStationId = 106;

        GameSetupBehaviour.Instance.Setup.DetectiveSetups[2].Controller = PlayerController.Human;
        GameSetupBehaviour.Instance.Setup.DetectiveSetups[2].StartAtStationId = 91;

        for (int i = 3; i < GameSetupBehaviour.Instance.Setup.DetectiveSetups.Length; i++)
        {
            GameSetupBehaviour.Instance.Setup.DetectiveSetups[i].Controller = PlayerController.None;
        }

        LoadGame();

    }
    #endregion

    void Reset()
    {
        Countdown.text = "";
        StartButtonLabel.SetText("start");
        storedPlayerArgs.Clear();
        PlayerReadyCollider.SetActive(false);

        for (int i = 0; i < PlayerReadySprites.Count; i++ )
        {
            GameObject.Destroy(PlayerReadySprites.Values.ElementAt(i));
        }
        
        PlayerReadySprites.Clear();

        foreach(PlayerSetup player in GameSetupBehaviour.Instance.IterateAllPlayers(true))
        {
            if(player.PlayerId == 0) // Mr X
            {
                player.DisplayName = Loc.Get("mister_x");
                MrXName.text = player.DisplayName;

            }
            else // Detective
            {
                player.DisplayName = string.Format("{0} {1}", Loc.Get("detective"), player.PlayerId);
                DetectiveNames[player.PlayerId - 1].text = player.DisplayName;

            }
        }

    }

    public void SetRoundTimeLocal(byte timeInSeconds, bool isSet) { if (isSet) SetRoundTime(timeInSeconds, false); }
    public void SetRoundTime(byte timeInSeconds, bool forceGui)
    {
        if (!isInitialized && GameSetupBehaviour.Instance.IsNetworkGame)
            return;

        this.LogDebug(string.Format("SetRoundTime: {0} ({1})", timeInSeconds, (forceGui) ? "force ui change" : "send message"));
        if (lastSetRoundTime == timeInSeconds)
            return;

        if (timeInSeconds == 255) // donno why it always passes 255 for Infinity on devices...
            timeInSeconds = SecondsInfinity;

        lastSetRoundTime = timeInSeconds;

        this.LogDebug("Set Time: " + timeInSeconds);

        GameSetupBehaviour.Instance.Setup.RoundTime = (timeInSeconds == SecondsInfinity) ? float.PositiveInfinity : (float)timeInSeconds;

        if (forceGui)
            TimeOptions[timeInSeconds].value = true;
        else
            this.Broadcast(GameSetupEvents.RoundTimeChanged, new SetRoundTimeArgs() { RoundTime = timeInSeconds });
    }


    public void SetDetectiveNameLocal(int detectiveId, string name) {  SetPlayerName(detectiveId + 1, name, false); }
    public void SetMrXNameLocal(string name) { SetPlayerName(0, name, false); }
    public void SetPlayerName(int playerId, string name, bool forceGui = true)
    {
        if (!isInitialized && GameSetupBehaviour.Instance.IsNetworkGame)
            return;

        if (string.IsNullOrEmpty(name))
        {
            if (playerId == 0)
                name = FallbackMrXNames.PickRandom();
            else
                name = FallbackDetectiveNames.PickRandom();
        }

        PlayerSetup p = GameSetupBehaviour.Instance.GetPlayer(playerId);
        p.DisplayName = name;

        this.LogInfo(string.Format("Setting Name of player {0} to \"{1}\"", playerId, name));

        if (forceGui)
        {
            if (playerId == 0)
                MrXName.text = name;
            else
                DetectiveNames[playerId - 1].text = name;
        }

        PlayerNames[playerId].value = name;


        if(storedPlayerArgs.Count > playerId)
            this.storedPlayerArgs[playerId].Name = name;
    }



    public void SetMisterXControllerLocal(int controller) { SetPlayerController(0, controller, false); }
    public void SetPlayerControllerLocal(int detectiveID, int controller) { SetPlayerController(detectiveID + 1, controller, false); }
    public void SetPlayerController(int playerId, int controller, bool forceGui = true, bool overrideHumanPlayerNameInNetwork = true)
    {
        if (!isInitialized && GameSetupBehaviour.Instance.IsNetworkGame)
            return;


        int detId = playerId - 1;
        PlayerController c = (PlayerController)(controller);

        UIStateButton btn = (playerId == 0) ? MrXController : DetectiveControllers[detId];
        var difficultyBtn = (playerId == 0) ? MrXAiDifficulty : DetectiveAiDifficulties[detId];
        var playerNameBtn = PlayerNames[playerId];

        difficultyBtn.gameObject.SetActive(c == PlayerController.Ai);
        playerNameBtn.gameObject.SetActive(c == PlayerController.Human || c == PlayerController.Network);

        // little hacky: parent of Name is input. Disable click collider for network players.
        playerNameBtn.enabled = c != PlayerController.Network;
        btn.transform.gameObject.GetComponent<BoxCollider>().enabled = c != PlayerController.Network;
        btn.Button.enabled = c != PlayerController.Network;

        if (forceGui)
        {
            btn.ApplyState(controller);
        }

        switch(c)
        {
            case PlayerController.Human:
                if(GameSetupBehaviour.Instance.IsNetworkGame)
                {
                    if(!forceGui)
                    {
                        this.storedPlayerArgs[playerId].ParticipantId = GSP.MultiplayerRT.OwnParticipantId;
                    }

                    if (overrideHumanPlayerNameInNetwork)
                    {
                        string name = GSP.Status.PlayerName;
                        SetPlayerName(playerId, name, true);
                    }
                }
                else
                {
                    string name = (playerId == 0) ? FallbackMrXNames.PickRandom() : FallbackDetectiveNames.PickRandom();
                    SetPlayerName(playerId, name, true);
                }
                break;

            case PlayerController.Ai:
                {
                    string name = (playerId == 0) ? Loc.Get("mister_x") : string.Format("{0} {1}", Loc.Get("detective"), playerId);
                    SetPlayerName(playerId, name, true);
                }
                break;
        }
        
        GameSetupBehaviour.Instance.GetPlayer(playerId).Controller = c;

        if (storedPlayerArgs.Count > playerId)
            this.storedPlayerArgs[playerId].Controller = c;
    }


    public void SetMisterXCPUDifficultyLocal(int difficulty) { SetCpuPlayerDifficulty(0, difficulty, false); }
    public void SetCPUDifficultyLocal(int detectiveID, int difficulty) { SetCpuPlayerDifficulty(detectiveID + 1, difficulty, false); }
    public void SetCpuPlayerDifficulty(int playerId, int difficulty, bool forceGui = true)
    {
        if (!isInitialized && GameSetupBehaviour.Instance.IsNetworkGame)
            return;

        AiDifficulty d = (AiDifficulty)difficulty;
        UIStateButton btn = (playerId == 0) ? MrXAiDifficulty : DetectiveAiDifficulties[playerId - 1];

        if(forceGui)
            btn.ApplyState(difficulty);

        GameSetupBehaviour.Instance.GetPlayer(playerId).Difficulty = d;

        if (storedPlayerArgs.Count > playerId)
            this.storedPlayerArgs[playerId].Difficulty = d;
    }



    public void LoadGame()
    {
        this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
		SceneManager.LoadSceneAsync("Game");
    }

    internal void Initialize()
    {
        StartCoroutine(coInitialize());
    }

    IEnumerator coInitialize()
    {
        isInitialized = false;
        while(!GameSettingsPanel.gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }
        // wait a bit longer to make sure the start method already has been called.
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        isInitialized = true;

        this.LogDebug(string.Format("Plot Settings to GUI - START ({0})", GSP.IsMultiplayerRTAvailable ? "Multiplayer" : "Local"));
        storedPlayerArgs.Clear();
        for (int i = 0; i < GameSetupBehaviour.Instance.Setup.DetectiveSetups.Length + 1; i++)
        {
            string name = GameSetupBehaviour.Instance.GetPlayer(i).DisplayName;
            PlayerController ctrl = GameSetupBehaviour.Instance.GetPlayer(i).Controller;
            AiDifficulty diff = GameSetupBehaviour.Instance.GetPlayer(i).Difficulty;

            if (i == 0 && (ctrl == PlayerController.None || ctrl == PlayerController.TutorialCPU))
            {
                ctrl = PlayerController.Ai;
            }

            storedPlayerArgs.Add(new SetPlayerArgs(ctrl, name, diff, false, i));

            if (GSP.IsMultiplayerRTAvailable)
            {
                string ptcp = GameSetupBehaviour.Instance.GetPlayer(i).ControllingParticipantID;
                this.storedPlayerArgs[i].ParticipantId = (string.IsNullOrEmpty(ptcp)) ? GSP.MultiplayerRT.OwnParticipantId : ptcp;
                this.storedPlayerArgs[i].IsDirty = false;
            }

            this.SetPlayerController(i, (int)ctrl, true);
            this.SetCpuPlayerDifficulty(i, (int)diff, true);
            //this.SetPlayerName(i, name, true);

        }
        storedPlayerArgs.Sort();

        SynchState(true);

        byte time = (byte)((GameSetupBehaviour.Instance.Setup.RoundTime == float.PositiveInfinity) ? 0 : Mathf.RoundToInt(GameSetupBehaviour.Instance.Setup.RoundTime));
        this.SetRoundTime(time, true);

        this.LogDebug("Plot Settings to GUI - END");

        yield return new WaitForEndOfFrame();
        

        foreach (GameObject item in PlayerReadySprites.Values)
        {
            GameObject.Destroy(item);
        }

        PlayerReadySprites.Clear();

        foreach (string participantID in ConnectionObserver.Instance.GetParticipantsOrdered().Select(o => o.ParticipantId)) 
        {
            PlayerReadySprites.Add(participantID, NGUITools.AddChild(PlayerReadyParent, PlayerReadyPrefab));
        }

        PlayerReadyParent.GetComponent<UIGrid>().repositionNow = true;

    }

    void OnEnable()
    {
        StartCoroutine(coNetworkPushCycle());
    }

    IEnumerator coNetworkPushCycle()
    {
        while(this.enabled)
        {
            yield return new WaitForSeconds(0.5f);

            if (!isInitialized && GameSetupBehaviour.Instance.IsNetworkGame)
                continue;

            foreach(var arg in this.storedPlayerArgs)
            {
                if(arg.IsDirty)
                {
                    arg.IsDirty = false;
                    this.Broadcast(GameSetupEvents.PlayerSetupChanged, arg);
                }
            }
        }
    }

    public void SetCheckBoxContent(string childName, string participantID)
    {
        if (!PlayerReadySprites.ContainsKey(participantID))
            return;

        foreach (Transform child in PlayerReadySprites[participantID].transform)
        {
            child.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(childName))
            PlayerReadySprites[participantID].gameObject.transform.Find(childName).gameObject.SetActive(true);
    }

    internal void ReplaceNetworkPlayerWithAi(string participantId)
    {
        this.LogInfo(string.Format("Replacing Network players of id {0}", participantId));
        foreach (PlayerSetup setup in GameSetupBehaviour.Instance.IterateAllPlayers(true))
        {
            this.LogInfo("Replace Network Player? " + setup.ControllingParticipantID);
            if (setup.Controller == PlayerController.Network && (participantId == null || setup.ControllingParticipantID == participantId))
            {
                this.LogInfo("Replace Network Player with AI: " + setup.PlayerId);

                setup.Controller = PlayerController.Ai;
                this.SetPlayerController(setup.PlayerId, (int)PlayerController.Ai, true);
            }
        }
    }


    internal void SetPlayer(SetPlayerArgs args)
    {
        int idx = args.PlayerSetupIndex;
        if(this.storedPlayerArgs.Count < idx)
        {
            this.LogError("Something went wrong! StoredPlayerArds.Count is lower than args.PlayerSetupIndex");
            return;
        }

        if (args.Equals(this.storedPlayerArgs[idx]))
            return;

        this.storedPlayerArgs[idx] = args;

        this.LogDebug("Set player - participant id: " + args.ParticipantId);
        PlayerController controller = args.Controller;
        if (args.Controller == PlayerController.Human && args.ParticipantId != GSP.MultiplayerRT.OwnParticipantId)
        {
            controller = PlayerController.Network;
            GameSetupBehaviour.Instance.GetPlayer(idx).ControllingParticipantID = args.ParticipantId;
        }
        else if (args.Controller == PlayerController.Network && args.ParticipantId == GSP.MultiplayerRT.OwnParticipantId)
        {
            controller = PlayerController.Human;
            GameSetupBehaviour.Instance.GetPlayer(idx).ControllingParticipantID = GSP.MultiplayerRT.OwnParticipantId;
        }
        SetPlayerController(idx, (int)controller, true, false);
        SetPlayerName(idx, args.Name);
        SetCpuPlayerDifficulty(idx, (int)args.Difficulty);

        args.IsDirty = false; // make sure not to resend
        if(GSP.MultiplayerRT.IsHost)
        {
            args.IsDirty = true; // broadcast next time as host
        }
    }

    public void SynchState(bool ownOnly = false)
    {
        foreach (var arg in this.storedPlayerArgs)
        {
            if (ownOnly && arg.Controller != PlayerController.Human)
                continue;

            arg.IsDirty = false;
            this.Broadcast(GameSetupEvents.PlayerSetupChanged, arg);
        }
    }
}
