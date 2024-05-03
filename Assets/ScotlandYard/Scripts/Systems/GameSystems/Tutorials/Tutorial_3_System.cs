using Rewired.Demos;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial_3_System : TutorialSystem<Tutorial_3_System>
{

    public Station[] NeededStations;
    public TicketPopup TicketDetectivePopup;
    
    [SerializeField]
    private TicketPopup _ticketPopup;

    private bool playerStartMoving = false;

    protected override string LocaStringIdPrefix { get { return "tutorial_agent"; } }

    protected override GameMode expectedGameMode { get { return GameMode.TutorialDetective; } }


    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        ListenTo(GameGuiEvents.TransportSelected, ClickedTransport);
        ListenTo(GameEvents.GameStart, StartGame);
        ListenTo(GameEvents.GameEnd, EndOfGame);

        MrXGameStepSystem.instance.SetStep(0, TransportationType.Taxi);
        MrXGameStepSystem.instance.SetStep(1, TransportationType.Taxi);
    }

    private void ClickedTransport(BaseArgs obj)
    {
        playerStartMoving = true;
    }

    private void EndOfGame(BaseArgs obj)
    {
        //Well done! You caught Mr. X! 
        Debug.Log("Case 19");
        ShowNextPopupDelayed(3f, 19);    //Index 19
        PlayerMouseSpriteExample.Instance.SetVisibility(false);
    }

    private void StartGame(BaseArgs obj)
    {
        GameState.Instance.MrX.Moves = 2;
    }

    private bool AlwaysFalse()
    {
        return false;
    }

    private bool ErasePopupOnPlayerMoveStart()
    {
        return playerStartMoving;
    }

    // round is one-based
    protected override void PlayerTurnStarts(PlayerBase player, int round)
    {
        player.AllowedStationConstraints.Clear();
        playerStartMoving = false;

        GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Clear();
        GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Add(player.transform.position);

        switch (round)
        {
            case 2:
                if (tutorialPopupIndex == 0)
                {
                    //Changing sides, are you? Good choice, it´s never too late to work for Scotland Yard
                    ShowNextPopup(0);    //Index 0                    
                }
                if (tutorialPopupIndex == 5 && player.PlayerId == 0)
                {
                    player.AllowedStationConstraints.Add(30);
                    player.GoStep(TransportationType.Taxi, NeededStations[0]);
                    GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Add(player.transform.position - new Vector3(0.2f, 0));
                    //There he is!
                    Debug.Log("Case 5");
                    ShowNextPopupDelayed(NeededStations[0].transform.position, CompassDirection.East, null, null, 4f, true, 5);   //Index 5
                }
                
                break;
            case 3:
                if (tutorialPopupIndex == 8)
                {
                    if (player.PlayerId == 1)
                    {
                        player.AllowedStationConstraints.Add(29);
                        player.GoStep(TransportationType.Taxi, NeededStations[1]);
                    }
                    if (player.PlayerId == 2)
                    {
                        player.AllowedStationConstraints.Add(105);
                        player.GoStep(TransportationType.Taxi, NeededStations[2]);
                    }
                    if (player.PlayerId == 3)
                    {
                        //Your turn! You should move to station 56
                        Debug.Log("Case 8");
                        PlayerMouseSpriteExample.Instance.SetVisibility(true);
                        PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                        ShowNextPopup(GameToGui(NeededStations[9].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 8);   //Index 8
                        player.AllowedStationConstraints.Add(56);
                    }
                }
               

                if (player.IsMrX && tutorialPopupIndex == 9)
                {
                    //Good! Now wait for all others to move.
                    Debug.Log("Case 9");
                    ShowNextPopup(9);
                    PlayerMouseSpriteExample.Instance.SetVisibility(false);
                }

                if (player.IsMrX && tutorialPopupIndex == 10)
                {
                    player.AllowedStationConstraints.Add(17);
                    player.GoStep(TransportationType.Taxi, NeededStations[3]);
                }
                break;
            case 4:
                 if (tutorialPopupIndex == 10)
                 {
                    if (player.PlayerId == 1)
                    {
                        player.AllowedStationConstraints.Add(6);
                        player.GoStep(TransportationType.Taxi,NeededStations[4]);
                    }
                    if (player.PlayerId == 2)
	                {
                        player.AllowedStationConstraints.Add(89);
                        player.GoStep(TransportationType.Bus, NeededStations[5]);
	                }
                    if (player.PlayerId == 3)
                    {
                        //The Mr. X log also shows you what transportation Mr. X used in his past turns
                        Debug.Log("Case 10");
                        ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + new Vector3(-0.1f, -0.1f), CompassDirection.South, null, null, 10);   //Index 10
                        player.AllowedStationConstraints.Add(42);
                    }  
                  }
                 if (player.PlayerId == 0)
                 {
                     player.AllowedStationConstraints.Add(29);
                     player.GoStep(TransportationType.Taxi, NeededStations[6]);
                 }
                break;
            case 5:
                if (tutorialPopupIndex == 12)
                {
                    //When you´re playing a multiplayer game
                    ChatSystem.Instance.ChatWindowButton.SetActive(true);
                    Debug.Log("Case 12");
                    // ShowNextPopup(ChatSystem.Instance.ChatWindowButton.transform.position + new Vector3(0, -0.05f), CompassDirection.North, null, null, 12);    //Index 12
                    PopupClosed(++tutorialPopupIndex);
                }
                  if (tutorialPopupIndex == 16)
                  {
                      if (player.PlayerId == 1)
                      {
                          player.AllowedStationConstraints.Add(7);
                          player.GoStep(TransportationType.Taxi, NeededStations[7]);
                      }
                      if (player.PlayerId == 2)
                      {
                          player.AllowedStationConstraints.Add(55);
                          player.GoStep(TransportationType.Bus, NeededStations[8]);
                      }
                      if (player.PlayerId == 3)
                      {
                          //Your turn again. Mr. X must be almost surrounded by now
                          GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Add(player.transform.position - new Vector3(2f, 0));
                          Debug.Log("Case 16");
                          ShowNextPopup(16);
                          player.AllowedStationConstraints.Add(29);
                          TicketDetectivePopup.AllowedTransportationTypes = TransportationType.Bus;
                          PlayerMouseSpriteExample.Instance.SetVisibility(false);
                      }
                  }
                break;
            //case 2:
            //break;
        }

        GameBoardAnimationSystem.Instance.CamSubSystem.FocusCamera();
    }

    protected override void PopupClosed(int popupProgress)
    {
        switch (popupProgress)
        {
            case 1: //As a detective you don´t know where Mr. X is currently hiding
                Debug.Log("Case 1");
                ShowNextPopup(1);
                break;
            case 2: //See these brighter log spaces? Mr. X will reveal his location in these turns
                Debug.Log("Case 2");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + new Vector3(-0.1f, -0.1f), CompassDirection.South, null, null, 2);
                break;
            case 3: //You also see a ghost pawn at his last known location.
                Debug.Log("Case 3");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + new Vector3(-0.1f, -0.1f), CompassDirection.South, null, null, 3);
                break;
            case 4: //Let´s wait for his next move...
                Debug.Log("Case 4");
                ShowNextPopup(4);
                UnPause();
                break;
            case 5:
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                break;
            case 6: //Unlike Mr. X, you don´t have any special tickets as a detective
                Debug.Log("Case 6");
                ShowNextPopup(6);
                break;
            case 7: //Now wait for the other detectives to move.
                Debug.Log("Case 7");
                ShowNextPopup(7);
                break;
            case 8:
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                break;
            case 11: //Now move to station 42.
                Debug.Log("Case 11");
                ShowNextPopup(GameToGui(NeededStations[10].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 11);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 13: //This icon opens the keyboard
                Debug.Log("Case 13");
                // ShowNextPopup(ChatSystem.Instance.ChatWindowButton.transform.position + new Vector3(0, -0.05f), CompassDirection.North, () => ChatSystem.Instance.ChatWindowContainer.activeSelf, null, 13);
                PopupClosed(++tutorialPopupIndex);
                break;
            case 14: //Type something, then tap "done" to share your message
                Debug.Log("Case 14");
                // ShowNextPopup(Vector3.zero, CompassDirection.Undefined, /*() => ChatSystem.Instance.ChatText.textLabel.text.Length > 0*/null, null, 14);
                PopupClosed(++tutorialPopupIndex);
                // skip "Voice Chat" explanation
                // this feature was removed along with the online multiplayer in the update v2.5 in June 2020 
                break;
            case 15: //Another way to communicate with players is with voice chat
                Debug.Log("Case 15");
                // ShowNextPopup(15);
                PopupClosed(++tutorialPopupIndex);
                break;
            case 16:
                ChatSystem.Instance.ChatWindowButton.SetActive(false);
                ChatSystem.Instance.ChatWindowContainer.SetActive(false);
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                break;
            case 17: //Beware not to get stuck, as you won't get any additional tickets
                Debug.Log("Case 17");
                ShowNextPopup(17);                
                break;
            case 18: //This will be the last move. Go to station 29.
                Debug.Log("Case 18");
                ShowNextPopup(GameToGui(NeededStations[1].transform.position), CompassDirection.East, ErasePopupOnPlayerMoveStart, null, 18);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 20: //If you don´t get him before all detectives are stuck
                Debug.Log("Case 20");
                ShowNextPopup(20);
                break;
            case 21: //You have learned everything there´s to learn now.
                Debug.Log("Case 21");
                ShowNextPopup(21);
                break;
            case 22: //END OF TUTORIAL!
                Stats.CompletedTutorial3 = true;
                this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
				SceneManager.LoadSceneAsync("MainMenu");
                break;
            default:
                break;
        }
    }
}