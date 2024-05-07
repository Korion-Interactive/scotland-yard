using System.Linq;
using Rewired.Demos;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Tutorial_2_System : TutorialSystem<Tutorial_2_System>
{
    protected override string LocaStringIdPrefix { get { return "tutorial_mrx"; } }
    public TicketPopupMrX TicketsMrXPopup;
    public Station[] NeededSations;

    private bool playerStartMoving = false;

    protected override GameMode expectedGameMode { get { return GameMode.TutorialMrX; } }
    
    [SerializeField]
    private TicketPopupMrX _ticketPopup;

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        ListenTo(GameGuiEvents.StationClicked, StationClicked);
        ListenTo(GameGuiEvents.DoubleTicketSelected, DTClicked);
        ListenTo(GameGuiEvents.TransportSelected, ClickedTransport);
        ListenTo(GameEvents.GameStart, StartGame);
    }

    private void ClickedTransport(BaseArgs obj)
    {
        playerStartMoving = true;
    }

    private void StartGame(BaseArgs obj)
    {
        GameState.Instance.MrX.PlayerState.Tickets.AddTicket(TransportationType.Any);
        PlayerMouseSpriteExample.Instance.ResetCursorPosition();
    }

    private void DTClicked(BaseArgs obj)
    {
        //The double move ticket allows you to move twice in one turn
        Debug.Log("Case 9");
        ShowNextPopup(9);
        TicketsMrXPopup.AllowedTransportationTypes = TransportationType.Taxi;
    }

    private void StationClicked(BaseArgs obj)
    {
        if (obj.RelatedObject == NeededSations[0].gameObject && tutorialPopupIndex == 8)
        {
            //Let´s get started with the double move ticket
            Debug.Log("Case 8");
            ShowNextPopup(GameToGui(NeededSations[0].transform.position), CompassDirection.East, AlwaysFalse, null, 8);
            _ticketPopup.ForceFocus();
        }

        if (obj.RelatedObject == NeededSations[4].gameObject && tutorialPopupIndex == 23)
        {
            playerStartMoving = true;
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

    // round is one-based
    protected override void PlayerTurnStarts(PlayerBase player, int round)
    {
        if (round > 0)
        {
            player.AllowedStationConstraints.Clear();            
        }

        var focusPoints = GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints;

        playerStartMoving = false;
        
        Debug.Log("Round: " + round);
        
        switch (round)
        {
            case 0:
                if (player.IsMrX)
                {
                    if(tutorialPopupIndex >= 7)
                        focusPoints.Add(player.transform.position - new Vector3(0.35f, 1));
                    else
                        focusPoints.Add(player.transform.position - new Vector3(0.35f, 0));
                    TicketsMrXPopup.AllowedTransportationTypes = 0;
                    TicketsMrXPopup.IsAllowedToUseSpecialTickets = false;
                    player.AllowedStationConstraints.Add(1);
                    if (tutorialPopupIndex == 0)
                    {
                        //This is the Mr. X tutorial
                        Debug.Log("Case 0");
                        ShowNextPopup(0); // Index 0
                    }
                }
                break;
            
            case 1:
                if (player.IsMrX)
                {
                    player.AllowedStationConstraints.Add(156);
                    TicketsMrXPopup.AllowedTransportationTypes = TransportationType.Taxi;
                    TicketsMrXPopup.IsAllowedToUseDoubleTickets = false;
                }
                //Now comes your second move. Tap on station 156 and move there
                Debug.Log("Case 11");
                PopupClosed(++tutorialPopupIndex);
                ShowNextPopup(11); 
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;

            case 2:
            if (tutorialPopupIndex == 12)
            {
                //That´s it! See how you evaded the detective
                Debug.Log("Case 12");
                ShowNextPopup(12); // Index 12
            }
            if (player.IsDetective && tutorialPopupIndex == 13)
            {
                player.AllowedStationConstraints.Add(154);
                player.GoStep(TransportationType.Taxi, NeededSations[1]);
                //Oh, he´s trying to follow you
                Debug.Log("Case 13");
                this.WaitAndDo(new WaitForSeconds(0.2f), () => !PopupManager.IsTutorialPopupOpen, 
                    () => ShowNextPopup(13));
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
            }
            if (player.IsMrX)
            {
                player.AllowedStationConstraints.Add(157);
                TicketsMrXPopup.IsAllowedToUseDoubleTickets = false;
                TicketsMrXPopup.IsAllowedToUseSpecialTickets = true;
                TicketsMrXPopup.AllowedTransportationTypes = 0;
            }

            break;

            case 3:
            if (tutorialPopupIndex == 18)
            {
                // Now there´s only a black ticket in the message box
                Vector3 logShift = new Vector3(-0.4f, -0.1f);
                Debug.Log("Case 18");
                    focusPoints.Add(player.transform.position + new Vector3(0.35f, 2f));
                    ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 18); // Index 18
                _ticketPopup.ForceFocus();
            }
            if (player.IsDetective && tutorialPopupIndex == 19)
            {
                //Oh, seems he´s smelling a rat
                player.AllowedStationConstraints.Add(155);
                player.GoStep(TransportationType.Taxi, NeededSations[0]);       
                Debug.Log("Case 19");
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
                this.WaitAndDo(new WaitForSeconds(0.2f), () => !PopupManager.IsTutorialPopupOpen, 
                    () => ShowNextPopup(19));
            }
            if(player.IsMrX)
            {
                player.AllowedStationConstraints.Add(115);
                TicketsMrXPopup.AllowedTransportationTypes = TransportationType.Any;
            }
            break;

            case 4:
            if (player.IsDetective)
            {
                //Well done! You escaped the detective
                Debug.Log("Case 23");
                ShowNextPopup(23); // Index 23
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
            }
            break;
        }
    }

    protected override void PopupClosed(int popupProgress)
    {
        Vector3 logShift = new Vector3(-0.4f, -0.1f);

        this.LogDebug(tutorialPopupIndex.ToString());
        switch (popupProgress)
        {
            case 1: // When you´re playing as Mr. X, you try to escape the detectives
                Debug.Log("Case 1");
                ShowNextPopup(1);
                break;
            case 2: //Check out the Mr. X log at the bottom of the screen
                Debug.Log("Case 2");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 2);
                break;
            case 3: //In these turns, the detectives will find out your current position
                Debug.Log("Case 3");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 3);
                break;
            case 4: //Before we start, always keep your display covered if you´re playing with friends
                Debug.Log("Case 4");
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 4);
                break;
            case 5: //As Mr. X, you always play as the white pawn.
                Debug.Log("Case 5");
                ShowNextPopup((go) => {
                    this.Broadcast(GameGuiEvents.FocusPosition, GameState.Instance.CurrentPlayer.gameObject);
                }, 5);
                break;
            case 6: //Beware! There´s a detective on a station next to yours
                Debug.Log("Case 6");
                ShowNextPopupDelayed(GameState.Instance.DetectivesIterator().ElementAt(0).transform.position, CompassDirection.West, null, null, 0.1f, true, 6);
                break;
            case 7: //Tap on station 155. You´ll see that you have 2 new tickets
                GameState.Instance.CurrentPlayer.AllowedStationConstraints.Add(155);
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                Debug.Log("Case 7");
                ShowNextPopup(GameToGui(NeededSations[0].transform.position), CompassDirection.East, AlwaysFalse, null, 7);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 10: //As the ticket´s activated now, select station 155 again and move there.
                Debug.Log("Case 10");
                ShowNextPopup(10);
                PlayerMouseSpriteExample.Instance.SetVisibility(false);
                break;
            case 14: //When you use a ticket, the detectives will know WHICH 
                Debug.Log("Case 14");
                ShowNextPopup(14);
                break;
            case 15: //Always try to use this to your advantage
                Debug.Log("Case 15");
                ShowNextPopup(15);
                break;
            case 16: //The black ticket will not reveal the transportation method you´re using
                Debug.Log("Case 16");
                ShowNextPopup(16);
                break;
            case 17: //Select station 157 and use the black ticket
                Debug.Log("Case 17");
                ShowNextPopup(17);
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 20: //By the way, notice that Mr. X gets every ticket the detectives are using
                Debug.Log("Case 20");
                ShowNextPopup(20);
                break;
            case 21: // Do you see the white line in the Thames
                Debug.Log("Case 21");
                ShowNextPopup(21);
                break;
            case 22: //Follow it to the north, tap on station 115 and use the black ticket again.
                Debug.Log("Case 22");
                ShowNextPopup(GameToGui(NeededSations[4].transform.position), CompassDirection.North, ErasePopupOnPlayerMoveStart, null, 22);
                UnPause();
                PlayerMouseSpriteExample.Instance.SetVisibility(true);
                PlayerMouseSpriteExample.Instance.ResetCursorPosition();
                break;
            case 24: //But take care when you use your special tickets
                Debug.Log("Case 24");
                ShowNextPopup(24);
                break;
            case 25: //You win as Mr. X if you don´t get caught
                Debug.Log("Case 25");
                ShowNextPopup(25);
                break;
            case 26: //You´re ready to escape them again without my help now
                Debug.Log("Case 26");
                ShowNextPopup(26);
                break;
            case 27: //END OF TUTORIAL!
                Stats.CompletedTutorial2 = true;
                this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
				SceneManager.LoadSceneAsync("MainMenu");
                break;
            default:
                break;
        }
    }


}