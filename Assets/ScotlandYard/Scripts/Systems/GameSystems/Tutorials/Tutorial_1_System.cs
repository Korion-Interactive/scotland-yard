using System.Linq;
using Rewired.Demos;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_1_System : TutorialSystem<Tutorial_1_System>
{

    public BoxCollider FooterViewBounds;
    public Station[] NeededStations;
    public GameObject HistoryDropDown;
    public TicketPopup TicketDetectivePopup;
    public GameObject Clock;
    
    [SerializeField]
    private TicketPopup _ticketPopup;

    private bool playerStartMoving = false;
    

    protected override string LocaStringIdPrefix { get { return "tutorial_general"; } }

    protected override GameMode expectedGameMode { get { return GameMode.TutorialBasics; } }

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        ListenTo<VectorEventArgs>(GameGuiEvents.KeepCamInBounds, KeepCamInBounds);
        ListenTo(GameGuiEvents.FocusPosition, FocusPosition);
        ListenTo(GameGuiEvents.PlayerFocusClicked, ClickedTab);
        ListenTo(GameGuiEvents.StationClicked, StationClicked);
        ListenTo(GameGuiEvents.TransportSelected, ClickedTransport);

        ListenTo(GameEvents.GameStart, GameStart);

        GameBoardAnimationSystem.Instance.AutoFocusEnabled = false;
    }

    private void KeepCamInBounds(VectorEventArgs args)
    {
        if (tutorialPopupIndex == 3 && (Mathf.Abs(args.Vector.x) > 0.000001f || Mathf.Abs(args.Vector.y) > 0.000001f))
        {
            //Now it´s time to zoom.           
            Debug.Log("Case 3");
            ShowNextPopup(Vector3.zero, CompassDirection.Undefined, AlwaysFalse, null, 3);
            UnPause();
        }
    }

    private void FocusPosition(BaseArgs obj)
    {
        if(tutorialPopupIndex == 29)
        {
            iTween.Stop();
            GameBoardAnimationSystem.Instance.CamSubSystem.FocusCam(0.65f, GameState.Instance.CurrentPlayer.transform.position); //KORION: Standard Zoom Changed from 1
        }
    }

    private void GameStart(BaseArgs obj)
    {
        PlayerTabSystem.Instance.PlayerTabs[0].gameObject.SetActive(false);

        for (int i = 0; i < 8; i++)
            GameState.Instance.DetectivesIterator().ElementAt(0).PlayerState.Tickets.UseTicket(TransportationType.Taxi);
    }

    private void ClickedTransport(BaseArgs obj)
    {
        this.LogDebug("Clicked Transport");
        if (tutorialPopupIndex == 10)
        {
            this.LogDebug("You did your first move");
            //You did your first move
            Debug.Log("Case 10");
            ShowNextPopupDelayed(2.5f, 10);
            PlayerMouseSpriteExample.Instance.SetVisibility(false);
            
        }
        playerStartMoving = true;

    }

    private void StationClicked(BaseArgs obj)
    {
        if (tutorialPopupIndex == 9)
        {
            if (obj.RelatedObject.transform.GetComponent<Station>().Id == 174)
            {
                this.LogDebug("This is the ticket menu");
                //This is the ticket menu.
                Debug.Log("Case 9");
                ShowNextPopup(GameToGui(NeededStations[5].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 9);
                _ticketPopup.ForceFocus();
            }
        }

        if (tutorialPopupIndex == 20)
        {
            if (obj.RelatedObject.transform.GetComponent<Station>().Id == 135)
            {
                //... and select the bus ticket.
                Debug.Log("Case 20");
                this.WaitAndDo(new WaitForEndOfFrame(), () => true, () =>
                {
                    ShowNextPopup(TicketDetectivePopup.BtnBus.transform.position, CompassDirection.East,
                        ErasePopupOnPlayerMoveStart, null, 20);
                });
                this.WaitAndDo(new WaitForSeconds(0.5f), () => true, () =>
                {
                    // _ticketPopup.ForceFocus();
                });
            }
        }
    }

    private bool AlwaysFalse()
    {
        return false;
    }

    private bool ErasePopupOnPlayerMoveStart()
    {
        return playerStartMoving;
    }

    private void ClickedTab(BaseArgs obj)
    {
        PlayerTab tab = obj.RelatedObject.GetComponent<PlayerTab>();
        if(tab == null)
            return;

        if (tutorialPopupIndex == 7)
        {
            if (tab.PlayerID == 1)
            {
                NeededStations[5].SetFakeHighlight(true);
                // Look at this yellow glowing station.
                Debug.Log("Case 7");
                ShowNextPopupDelayed(NeededStations[5].transform.position, CompassDirection.East, null, null, 1f, true, 7);   //Index 7
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
            }
        }

        if (tutorialPopupIndex == 23)
        {
            if (tab.PlayerID == 2)
            {
                //Now tap your pawn color.
                Debug.Log("Case 23");
                ShowNextPopup(PlayerTabSystem.Instance.PlayerTabs[1].transform.position, CompassDirection.South, AlwaysFalse, null, 23);   //Index 23
                UnPause();
            }
        }

        if (tutorialPopupIndex == 24)
        {
            if (tab.PlayerID == 1)
            {
                //And now tap the other pawn´s color again.
                Debug.Log("Case 24");
                ShowNextPopup(PlayerTabSystem.Instance.PlayerTabs[2].transform.position, CompassDirection.South, AlwaysFalse, null, 24);   //Index 24
                UnPause();
            }
        }

        if (tutorialPopupIndex == 25)
        {
            if (tab.PlayerID == 2)
            {
                // Now just watch the pawn move.
                Debug.Log("Case 25");
                ShowNextPopupDelayed(1f, 25);   //Index 25
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
            }
        }
    }

    // round is one-based
    protected override void PlayerTurnStarts(PlayerBase player, int round)
    {
        GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = false;
        GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = player.PlayerId == 1;
        GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = true;


        var focusPoints = GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints;
        focusPoints.Clear();
        focusPoints.Add(player.Location.transform.position);
        //if (round > 2 && round != 4)
        //{
        //    player.AllowedStationConstraints.Clear();            
        //}
        if (player.PlayerId == 1)
        {
            playerStartMoving = false;        
        }

        switch(round)
        {
            case 1:
                if (player.PlayerId == 1)
	            {
                    player.AllowedStationConstraints.Add(1);
                    if (tutorialPopupIndex == 0)
                    {
                        focusPoints.Add(player.Location.transform.position + new Vector3(-0.3f, 0));
                        this.WaitAndDo(new WaitForEndOfFrame(), () => true, () => GameState.Instance.DetectivesIterator().ElementAt(0).SetCurrentPlayerHighlights(true));
                        //Welcome to the general tutorial
                        ShowNextPopup(0);    //Index 0                               
                    }
	            }
                
                
                break;
            case 2:
                    if (player.PlayerId == 1)
                    {
                        if (tutorialPopupIndex == 13)
                        {
                            focusPoints.Add(player.Location.transform.position + new Vector3(-0.5f, 0));
                            //Let´s take a look at the message box
                            Vector3 shift = new Vector3(0, 0.3f);
                            Debug.Log("Case 13");
                            ShowNextPopup(HistoryDropDown.transform.position + shift, CompassDirection.North, AlwaysFalse, null, 13);
                            PlayerMouseSpriteExample.Instance.SetVisibility(true);
                        PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                        player.AllowedStationConstraints.Clear();
                            player.AllowedStationConstraints.Add(1);
                        }
                        else if(tutorialPopupIndex == 16)
                        {
                            focusPoints.Add(GameState.Instance.CurrentPlayer.transform.position + new Vector3(-0.5f, 0));
                        }
                    }
                    if (player.PlayerId == 2)
                    {
                        // Just a few more things left and we´re done
                        Debug.Log("Case 17");
                        ShowNextPopup(17);    //Index 17
                        PlayerMouseSpriteExample.Instance.SetVisibility(false);
                    }
                break;
            case 3:
                    if (player.PlayerId == 1)
                    {
                        if (tutorialPopupIndex == 18)
                        {
                            //Did you notice this icon? It shows that there is another pawn
                            Vector3 pos = GameBoardAnimationSystem.Instance.DirectionIndicationSubSystem.GetPlayerIndicatorTransform(GameState.Instance.CurrentPlayer).position;
                            Debug.Log("Case 18");
                            ShowNextPopup(pos, CompassDirection.East, null, null, 18); // Index 18 
                            player.AllowedStationConstraints.Clear();
                            player.AllowedStationConstraints.Add(135);  
                            TicketDetectivePopup.AllowedTransportationTypes = TransportationType.Bus;
                            PauseNextFrame();
                        }
                        else if(tutorialPopupIndex == 19)
                        {
                            focusPoints.Add(player.Location.transform.position + new Vector3(-1f, 0));
                        }

                    }
                    if (player.PlayerId == 2)
                    {
                        //You just took one of London´s famous red busses
                        focusPoints.Clear();
                        Debug.Log("Case 21");
                        ShowNextPopup(21);    //Index 21
                        PlayerMouseSpriteExample.Instance.SetVisibility(false);
                    }

                
                break;
            case 4:
                    if (player.PlayerId == 1)
                    {
                        //Time to move to an underground station
                        if (tutorialPopupIndex == 26)
                        {
                            focusPoints.Add(player.Location.transform.position + new Vector3(-1f, 0));
                            Debug.Log("Case 26");
                            ShowNextPopup(GameToGui(NeededStations[8].transform.position), CompassDirection.East, null, null, 26); // Index 26
                            player.AllowedStationConstraints.Clear();
                            player.AllowedStationConstraints.Add(128);
                            this.BroadcastDelayed(GameEvents.TurnStart, player.gameObject, 0.1f);
                            PlayerMouseSpriteExample.Instance.SetVisibility(true);
                        PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                        //UnPause();
                    }
                        if (tutorialPopupIndex == 27)
                        {
                            
                            focusPoints.Add(player.Location.transform.position + new Vector3(-1f, 0));
                        }
                    }
                    if (player.PlayerId == 2)
                    {
                        //... and watch the other pawn do his move.
                        Debug.Log("Case 28");
                        ShowNextPopup(28);    //Index 28
                        PlayerMouseSpriteExample.Instance.SetVisibility(false);
                    }
                break;
            case 5:
                    if (player.PlayerId == 1)
                    {
                        if (tutorialPopupIndex == 29)
                        {

                            player.AllowedStationConstraints.Clear();
                            player.AllowedStationConstraints.Add(89);
                            TicketDetectivePopup.AllowedTransportationTypes = TransportationType.Metro;
                            GameBoardAnimationSystem.Instance.DirectionIndicationSubSystem.CreateStationIndicators();
                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = false;
                            GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = false;
                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = true;
                            this.WaitAndDo(new WaitForSeconds(1.5f), () => true, () =>
                                {
                                    //arrow at the border of the screen? It indicates that there´s a station you can move to outside of the screen.
                                    Debug.Log("Case 29");
                                    ShowNextPopup(GameBoardAnimationSystem.Instance.DirectionIndicationSubSystem.GetStationIndicationPosition(89), CompassDirection.North,
                                        null, (o) =>
                                        {
                                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = false;
                                            GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = true;
                                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = true;
                                        }, 29);
                                    this.BroadcastDelayed(GameEvents.TurnStart, player.gameObject, 0.1f);
                                });
                        }
                        else if(tutorialPopupIndex == 30)
                        {
                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = false;
                            GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = false;
                            GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = true;
                        }
                        //else if(tutorialPopupIndex == 31)
                        //{
                        //    focusPoints.Clear();
                        //    focusPoints.Add(player.transform.position);
                        //    this.LogError("!!!");
                        //}
                    }
                    if (player.PlayerId == 2)
                    {
                        focusPoints.Clear();
                        // Good job! You´ve completed the general tutorial
                        Debug.Log("Case 32");
                        ShowNextPopup(32);
                        PlayerMouseSpriteExample.Instance.SetVisibility(false);
                    }
                break;
            default:
                player.AllowedStationConstraints.Clear();
                break;
        }

        GameBoardAnimationSystem.Instance.CamSubSystem.FocusCamera();
    }

    protected override void PopupClosed(int popupProgress)
    {
        PlayerBase player = GameState.Instance.CurrentPlayer;
        switch (popupProgress)
        {
            case 1:
                if (GameState.Instance.CurrentPlayer.PlayerId == 1)
                {
                    this.Broadcast(GameGuiEvents.FocusPosition, GameState.Instance.CurrentPlayer.gameObject);
                    //This is your pawn for now
                    Debug.Log("Case 1");
                    ShowNextPopup(GameToGui(player.transform.position), CompassDirection.East, null, null, 1);
                }
                break;
            case 2:  //Let´s try to move the map
                GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Clear();
                Debug.Log("Case 2");
                ShowNextPopup(Vector3.zero, CompassDirection.Undefined, AlwaysFalse, null, 2);
                UnPause();
                break;
            case 6:  //tap on the highlighted color
                Debug.Log("Case 6");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[1].transform.position, CompassDirection.South, AlwaysFalse, null, 6);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                UnPause();
                break;
            case 8: //This is a taxi station
                this.LogDebug("This is a taxi station");
                GameState.Instance.CurrentPlayer.AllowedStationConstraints.Add(174);
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                Debug.Log("Case 8");
                ShowNextPopup(GameToGui(NeededStations[5].transform.position), CompassDirection.East, AlwaysFalse, null, 8);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 11: //every move you make consumes 1 ticket
                Debug.Log("Case 11");
                ShowNextPopup(11);
                break;
            case 12: //Your turn ends after you´ve made a move
                Debug.Log("Case 12");
                ShowNextPopup(12);
                UnPause();
                break;
            case 13:
                this.WaitAndDo(new WaitForSeconds(0.1f), () => GameState.Instance.CurrentPlayer.PlayerId == 2, () =>
                {
                    GameState.Instance.CurrentPlayer.AllowedStationConstraints.Add(154);
                    GameState.Instance.CurrentPlayer.GoStep(TransportationType.Bus, NeededStations[0]);
                });
                break;
            case 16: //Ok, let´s take the taxi again
                player.AllowedStationConstraints.Add(161);
                this.Broadcast(GameEvents.TurnStart, player.gameObject);
                Debug.Log("Case 16");
                ShowNextPopup(GameToGui(NeededStations[6].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 16);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                _ticketPopup.ForceFocus();
                break;
            case 18:
                if (player.PlayerId == 2)
                {
                    player.AllowedStationConstraints.Add(155);
                    player.GoStep(TransportationType.Taxi, NeededStations[1]);
                }
                break;
            case 19: // Tap this red glowing station...
                this.Broadcast(GameEvents.TurnStart, player.gameObject);
                Debug.Log("Case 19");
                ShowNextPopup(GameToGui(NeededStations[7].transform.position), CompassDirection.East, AlwaysFalse, null, 19);
                UnPause();
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 20:
                break;
            case 22: //Now´s the other pawn´s turn. Tap this pawn´s color
                Debug.Log("Case 22");
                ShowNextPopup(PlayerTabSystem.Instance.PlayerTabs[2].transform.position, CompassDirection.South, AlwaysFalse, null, 22);
                UnPause();
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 26:
                if (player.PlayerId == 2)
                {
                    player.AllowedStationConstraints.Add(167);
                    player.GoStep(TransportationType.Taxi, NeededStations[2]);
                }
                break;
            case 27: //Let´s take the bus to this underground station...
                //Click 
                //Ticket PopUp
                Debug.Log("Case 27");
                ShowNextPopup(GameToGui(NeededStations[8].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 27);                   
                _ticketPopup.ForceFocus();
                break;
            case 29:
                    if (player.PlayerId == 2)
                    {
                        this.WaitAndDo(new WaitForEndOfFrame(), () => true, () =>
                        {
                            player.AllowedStationConstraints.Add(153);
                            player.GoStep(TransportationType.Taxi, NeededStations[3]);
                        });
                    }
                    break;
            case 30: //Move the map in order to make the station appear.
                    GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Clear();
                    GameBoardAnimationSystem.Instance.CamSubSystem.FocusCamera(GameState.Instance.CurrentPlayer.transform.position);
                    Debug.Log("Case 30");
                    ShowNextPopup(new Vector3(), CompassDirection.Undefined, AlwaysFalse, null, 30);
                    UnPause();
                break;
            case 33: //END OF TUTORIAL!
                Debug.Log("Case 33");
                Stats.CompletedTutorial1 = true;
                this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
				SceneManager.LoadSceneAsync("MainMenu");
                break;
            default:
                break;
        }
    }


    protected override void Update()
    {
        base.Update();

        if (GameCamera == null)
        {
            GameCamera = GameObject.Find("Camera_GameBoard").GetComponent<Camera>();
        }

        switch (tutorialPopupIndex)
        {
            case 4:
                {
                    if (GameCamera.orthographicSize == 1.6f)
                    {
                        Debug.Log("Case 4");
                        //Now, zoom in again
                        ShowNextPopup(Vector3.zero, CompassDirection.Undefined, AlwaysFalse, null, 4);
                        UnPause();
                    }
                }
                break;
            case 5:
                {
                    if (GameCamera.orthographicSize == 0.5f)
                    {
                        Debug.Log("Case 5");
                        // Look at the player bar with 6 different colors
                        ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[1].transform.position, CompassDirection.South, null, null, 5);
                    }

                }
                break;
            case 14:
                {
                    if (HistoryDropDown.activeInHierarchy)
                    {
                        Debug.Log("Case 14");
                        GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = false;
                        //... and tap it again to make it disappear.
                        ShowNextPopup(HistoryDropDown.transform.position - new Vector3(0, 0.4f, 0), CompassDirection.North, AlwaysFalse, null, 14);
                    }
                }
                break;
            case 15:
                {
                    if (!HistoryDropDown.activeInHierarchy)
                    {
                        Debug.Log("Case 15");
                        GameBoardAnimationSystem.Instance.CamSubSystem.LockGestureEnabledStatus = true;
                        //Check out the timer!
                        ShowNextPopup(Clock.transform.position - new Vector3(0, 0.05f, 0), CompassDirection.North, null, null, 15);
                        PlayerMouseSpriteExample.Instance.SetVisibility(false);
                    }
                }
                break;
            case 31:
                {
                    Vector3 screenPoint = GameCamera.WorldToScreenPoint(NeededStations[4].transform.position);
                    Vector3 transPoint = FooterCamera.ScreenToWorldPoint(screenPoint);
                    if (FooterViewBounds.bounds.Contains(transPoint))
                    {
                    Debug.Log("Case 31");
                        //This is an underground station, it has a blue glow
                        ShowNextPopup(GameToGui(NeededStations[4].transform.position), CompassDirection.North, () => TicketDetectivePopup.gameObject.activeSelf, null, 31);
                        UnPause();
                        PlayerMouseSpriteExample.Instance.SetVisibility(true);
                        PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                    }
                }
                break;
        }
    }

    
}