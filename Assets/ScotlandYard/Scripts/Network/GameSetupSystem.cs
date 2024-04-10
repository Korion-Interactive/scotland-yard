using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSetupSystem : NetworkSystem<GameSetupEvents, GameSetupSystem>
{
    protected override byte Context { get { return Globals.Net_Context_GameSetup; } }
    
    public DrawStartStation cardSelection;
    public GameObject StartObject;
    public GameObject BackObject;
    public GameObject PlayerReadyPrefab;

    public GameObject StationCardSelectionPanel, GameModePanel;

    bool scriptCalledForClick = false;
    GameSetupSettings settings { get { return GameSetupSettings.Instance; } }

    HashSet<string> ReadyPlayers = new HashSet<string>();

    private GameObject cachedSelectedObject;

    public delegate void OnStartClicked();
    public static event OnStartClicked onStartClicked;

    protected override void RegisterEvents()
    {
        ListenTo(GameSetupEvents.RoundTimeChanged, (args) => SendEvent(GameSetupEvents.RoundTimeChanged, args));
        ListenTo(GameSetupEvents.PlayerSetupChanged, PlayerSetupChanged);

        ListenTo(GameSetupEvents.PlayerReady, (args) => SendEvent(GameSetupEvents.PlayerReady, args));
        ListenTo(GameSetupEvents.PlayerNotReady, (args) => SendEvent(GameSetupEvents.PlayerNotReady, args));
        ListenTo(GameSetupEvents.PlayerChoseCard, (args) => SendEvent(GameSetupEvents.PlayerChoseCard, args));
        ListenTo(GameSetupEvents.AllPlayersReady, (args) => SendEvent(GameSetupEvents.AllPlayersReady, args));

        ListenTo<ConnectionArgs>(GameSetupEvents.PlayerNotReady, PlayerNotReady);
        ListenTo<ConnectionArgs>(GameSetupEvents.PlayerReady, PlayerReady);
        ListenTo(GameSetupEvents.AllPlayersReady, AllPlayersReady);

        this.ListenTo(ConnectionEvent.SessionLeft, SessionLeft);
        this.ListenTo(ConnectionEvent.PeerLeft, PeerLeft);
    }

    private void PlayerSetupChanged(BaseArgs args)
    {
        if (!GSP.IsMultiplayerRTAvailable)
            return;

        bool sendToHost = !(GSP.MultiplayerRT.IsHost);
        SendEvent(GameSetupEvents.PlayerSetupChanged, args, sendToHost);
    }


    private void SessionLeft(BaseArgs args)
    {
        PopupManager.ShowPrompt("local_user_left_session_header", "local_user_left_session_text");

        ReadyPlayers.Clear();

        // Little hack: wait for end of animation before returning to mode-screen
        this.WaitAndDo(new WaitForSeconds(1.5f), null, () =>
            {
                if (StationCardSelectionPanel.activeSelf) // card selection
                {
                    StationCardSelectionPanel.SetActive(false);
                    GameModePanel.transform.ResetToIdentity();
                    GameModePanel.SetActive(true);
                }
                else if (settings.IsAvailableForNetwork) // game setup
                {
                    scriptCalledForClick = true;
                    BackObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
                    BackObject.SendMessage("OnClick");
                    GSP.AllowInvites = true;
                    scriptCalledForClick = false;
                }
            });
    }

    private void PeerLeft(BaseArgs args)
    {
        ConnectionArgs conArgs = args as ConnectionArgs;
        PlayerSetup player = GameSetupBehaviour.Instance.IterateAllPlayers(true).FirstOrDefault(o => o.ControllingParticipantID == conArgs.ParticipantId);

        PopupManager.ShowPrompt("peer_left_session_header", "peer_left_session_text", null, ((player != null) ? player.DisplayName : Loc.Get("player_human")));

        settings.ReplaceNetworkPlayerWithAi(conArgs.ParticipantId);
        
        ReadyPlayers.Remove(conArgs.ParticipantId);
        PlayerReady(null);

        GameSetupSettings.Instance.SetCheckBoxContent(Globals.PlayerReady_LeftGame, conArgs.ParticipantId);

        // emergency logout
        if(GSP.IsMultiplayerRTAvailable)
        {
            SessionLeft(args);
        }
    }

    public void StartClicked()
    {
        if (scriptCalledForClick)
            return;

        onStartClicked?.Invoke();

        if (GameSetupBehaviour.Instance.IsNetworkGame)
        {
            StartObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(false);

            // prevent starting a multiplayer game without detectives
            int playerCount = GameSetupBehaviour.Instance.IterateAllPlayers(false).Count();
            if (playerCount <= 1)
            {
                PopupManager.ShowPrompt("access_denied", "too_few_players");
                return;
            }
            
            int connectionCount = GSP.MultiplayerRT.GetNumberOfParticipants(includeSelf: false);
            if (connectionCount > GSP.MultiplayerRT.MaxAllowedConnections)
            {
                PopupManager.ShowPrompt("access_denied", "too_many_connections");
                return;
            }

            var lbl = StartObject.GetComponentInChildren<LabelTranslator>();
            var spr = StartObject.GetComponent<UISprite>();

            if (ReadyPlayers.Contains(GSP.MultiplayerRT.OwnParticipantId))
            {
                this.Broadcast(GameSetupEvents.PlayerNotReady, new ConnectionArgs());
                settings.PlayerReadyCollider.SetActive(false);
                lbl.SetText("start");
                spr.spriteName = "proceed_icon";
            }
            else
            {
                this.Broadcast(GameSetupEvents.PlayerReady, new ConnectionArgs());
                settings.PlayerReadyCollider.SetActive(true);
                settings.SynchState();
                lbl.SetText("cancel");
                spr.spriteName = "dropdown_closed_icon";
            }
        }
        else
        {
            int playerCount = GameSetupBehaviour.Instance.IterateAllPlayers(false).Count();
            if (playerCount <= 3)
            {
                StartObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(false);

                if (playerCount <= 1)
                {
                    //KORION
                    //if(cachedSelectedObject == no button... dann nicht) //breakpoints
                    cachedSelectedObject = UICamera.selectedObject;
                    PopupManager.ShowQuestion("access_denied", "too_few_players", OnClick, null); //KORION POP UP
                    SetNewGamePanelSelectionActive(false);
                    PopupManager.Instance.CurrentPopup.noButton.SetActive(false);
                }
                else
                {
                    //KORION
                    cachedSelectedObject = UICamera.selectedObject;
                    PopupManager.ShowQuestion("unoptimal_game_question_title", "unoptimal_game_question_body",
                        (o) =>
                        {
                            StartObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
                            scriptCalledForClick = true;
                            StartObject.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                            scriptCalledForClick = false;
                        }, OnClick);
                    SetNewGamePanelSelectionActive(false);
                }        
            }
            else
            {
                StartObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
                settings.PlayerReadyCollider.SetActive(false);
                PrepareCardSelection();
            }
        }
    }

    //KORION
    private void OnClick(GameObject go)
    {
        UICamera.selectedObject = cachedSelectedObject;
        cachedSelectedObject = null;
        SetNewGamePanelSelectionActive(true);
    }

    //KORION
    private void SetNewGamePanelSelectionActive(bool isActive)
    {
        gameObject.GetComponent<SetNewGamePanelSelectionActive>().SetActive(isActive);
    }

    public void BackClicked()
    {
        if (scriptCalledForClick)
            return;

        if (GameSetupBehaviour.Instance.Setup.Mode == GameMode.Network) //GSP.IsMultiplayerRTAvailable)
        {
            BackObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(false);
            UIEventListener.VoidDelegate yes = o => 
            {
                BackObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);

                scriptCalledForClick = true;
                GSP.AllowInvites = true;
                BackObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver); // start animation to go to next screen
                scriptCalledForClick = false;

                GSP.MultiplayerRT.Disconnect();
            };

            PopupManager.ShowQuestion("confirm_leave_session_header", "confirm_leave_session_text", yes, null);
        }
        else
        {
            BackObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
        }
    }

    private void PlayerNotReady(ConnectionArgs args)
    {
        ReadyPlayers.Remove(args.ParticipantId);
        GameSetupSettings.Instance.SetCheckBoxContent(null, args.ParticipantId);
        GameSetupSettings.Instance.Countdown.text = "";
    }

    private void PlayerReady(ConnectionArgs args)
    {
        if (args != null)
        {
            ReadyPlayers.Add(args.ParticipantId);
            string childName = (args.ParticipantId == GSP.MultiplayerRT.OwnParticipantId) ? Globals.PlayerReady_SelfReady : Globals.PlayerReady_OtherReady;
            GameSetupSettings.Instance.SetCheckBoxContent(childName, args.ParticipantId);
        }

        if (ReadyPlayers.Count >= ConnectionObserver.Instance.GetParticipantsOrdered().Count())
        {
            StopCoroutine(coAllPlayersReady());
            StartCoroutine(coAllPlayersReady());
        }
    }

    IEnumerator coAllPlayersReady()
    {
        // during a time of 3 seconds players can mark themselves as "unready"
        for (int i = 3; i >= 0; i--)
        {
            GameSetupSettings.Instance.Countdown.text = i + "...";
            yield return new WaitForSeconds(1);

            // If player became not ready: jump out
            if(string.IsNullOrEmpty(GameSetupSettings.Instance.Countdown.text))
            {
                yield break;
            }
        }

        if (ConnectionObserver.Instance.IsHost
            && ReadyPlayers.Count >= ConnectionObserver.Instance.GetParticipantsOrdered().Count()
            && GSP.IsMultiplayerRTAvailable && GSP.MultiplayerRT.IsConnected)
        {
            if(GSP.IsMultiplayerRTAvailable && GSP.MultiplayerRT.IsHost)
            {
                settings.SynchState();
            }

            Broadcast(GameSetupEvents.AllPlayersReady);
        }
    }

    private void AllPlayersReady(BaseArgs args)
    {
        scriptCalledForClick = true;

        StartObject.SetComponentsInChildrenEnabled<UIPlayAnimation>(true);
        StartObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver); // start animation to go to next screen

        scriptCalledForClick = false;

        PrepareCardSelection();
    }

    private void PrepareCardSelection()
    {
        // KORION: In order to correctly get all human players for multiplayer iterations, set the amount of human players here!
        GameSetupBehaviour.Instance.OnPlayerSetupFinalized();

        this.cardSelection.Reset();
    }

    protected override bool MessageReceivedSuccessfully(GameSetupEvents eventType, BaseArgs args)
    {
        this.LogDebug("Message received: <color=green>" + eventType + "</color> - " + args.ToString());
 
        switch(eventType)
        {
            case GameSetupEvents.RoundTimeChanged:
                settings.SetRoundTime((args as SetRoundTimeArgs).RoundTime, true);
                break;

            case GameSetupEvents.PlayerSetupChanged:
                settings.SetPlayer(args as SetPlayerArgs);
                break;

            case GameSetupEvents.PlayerReady:
                this.Broadcast(GameSetupEvents.PlayerReady, args as ConnectionArgs);
                break;
            case GameSetupEvents.PlayerNotReady:
                this.Broadcast(GameSetupEvents.PlayerNotReady, args as ConnectionArgs);
                break;
            case GameSetupEvents.AllPlayersReady:
                this.Broadcast(GameSetupEvents.AllPlayersReady, args);
                break;

            case GameSetupEvents.PlayerChoseCard:

                if (!cardSelection.IsAvailable)
                    return false;

                GameObject card = args.RelatedObject;
                cardSelection.TurnCardFaceUp(card, false);
                break;
        }

        return true;
    }

    protected override BaseArgs ArgsFactory(GameSetupEvents eventType)
    {
        switch(eventType)
        {
            case GameSetupEvents.RoundTimeChanged:
                return new SetRoundTimeArgs();

            case GameSetupEvents.PlayerSetupChanged:
                return new SetPlayerArgs();

            case GameSetupEvents.PlayerReady:
            case GameSetupEvents.PlayerNotReady:
                return new ConnectionArgs();

            default:
                return new BaseArgs();
        }

    }

    protected override bool CanHandleMessage(GameSetupEvents eventType, BaseArgs args)
    {
        switch(eventType)
        {
            case GameSetupEvents.PlayerChoseCard:
                return cardSelection.IsAvailable;

            default:
                return settings.IsAvailableForNetwork;
        }
    }

}
