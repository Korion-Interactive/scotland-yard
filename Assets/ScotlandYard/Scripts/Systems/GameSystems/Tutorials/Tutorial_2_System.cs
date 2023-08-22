using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Tutorial_2_System : TutorialSystem<Tutorial_2_System>
{
    protected override string LocaStringIdPrefix { get { return "tutorial_mrx"; } }
    public TicketPopupMrX TicketsMrXPopup;
    public Station[] NeededSations;

    private bool playerStartMoving = false;

    protected override GameMode expectedGameMode { get { return GameMode.TutorialMrX; } }


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
    }

    private void DTClicked(BaseArgs obj)
    {
        //The double move ticket allows you to move twice in one turn
        ShowNextPopup(9);
        TicketsMrXPopup.AllowedTransportationTypes = TransportationType.Taxi;
    }

    private void StationClicked(BaseArgs obj)
    {
        if (obj.RelatedObject == NeededSations[0].gameObject && tutorialPopupIndex == 8)
        {
            //Let´s get started with the double move ticket
            ShowNextPopup(GameToGui(NeededSations[0].transform.position), CompassDirection.East, AlwaysFalse, null, 8);
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
        
        switch (round)
        {
            case 0:
                if (player.IsMrX)
                {
                    focusPoints.Add(player.transform.position - new Vector3(0.35f, 0));
                    TicketsMrXPopup.AllowedTransportationTypes = 0;
                    TicketsMrXPopup.IsAllowedToUseSpecialTickets = false;
                    player.AllowedStationConstraints.Add(1);
                    if (tutorialPopupIndex == 0)
                    {
                        //This is the Mr. X tutorial
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
                ShowNextPopup(11); 
            break;

            case 2:
            if (tutorialPopupIndex == 12)
            {
                //That´s it! See how you evaded the detective
                ShowNextPopup(12); // Index 12
            }
            if (player.IsDetective && tutorialPopupIndex == 13)
            {
                player.AllowedStationConstraints.Add(154);
                player.GoStep(TransportationType.Taxi, NeededSations[1]);
                //Oh, he´s trying to follow you
                this.WaitAndDo(new WaitForSeconds(0.2f), () => !PopupManager.IsTutorialPopupOpen, 
                    () => ShowNextPopup(13));
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
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 18); // Index 18
            }
            if (player.IsDetective && tutorialPopupIndex == 19)
            {
                //Oh, seems he´s smelling a rat
                player.AllowedStationConstraints.Add(155);
                player.GoStep(TransportationType.Taxi, NeededSations[0]);              
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
                ShowNextPopup(23); // Index 23
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
                ShowNextPopup(1);
                break;
            case 2: //Check out the Mr. X log at the bottom of the screen
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 2);
                break;
            case 3: //In these turns, the detectives will find out your current position
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 3);
                break;
            case 4: //Before we start, always keep your display covered if you´re playing with friends
                ShowNextPopup(PlayerTabSystem.instance.PlayerTabs[0].transform.position + logShift, CompassDirection.South, null, null, 4);
                break;
            case 5: //As Mr. X, you always play as the white pawn.
                ShowNextPopup((go) => {
                    this.Broadcast(GameGuiEvents.FocusPosition, GameState.Instance.CurrentPlayer.gameObject);
                }, 5);
                break;
            case 6: //Beware! There´s a detective on a station next to yours
                ShowNextPopupDelayed(GameState.Instance.DetectivesIterator().ElementAt(0).transform.position, CompassDirection.West, null, null, 0.1f, true, 6);
                break;
            case 7: //Tap on station 155. You´ll see that you have 2 new tickets
                GameState.Instance.CurrentPlayer.AllowedStationConstraints.Add(155);
                this.Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
                ShowNextPopup(GameToGui(NeededSations[0].transform.position), CompassDirection.East, AlwaysFalse, null, 7);
                break;
            case 10: //As the ticket´s activated now, select station 155 again and move there.
                ShowNextPopup(10);
                break;
            case 14: //When you use a ticket, the detectives will know WHICH 
                ShowNextPopup(14);
                break;
            case 15: //Always try to use this to your advantage
                ShowNextPopup(15);
                break;
            case 16: //The black ticket will not reveal the transportation method you´re using
                ShowNextPopup(16);
                break;
            case 17: //Select station 157 and use the black ticket
                ShowNextPopup(17);
                break;
            case 20: //By the way, notice that Mr. X gets every ticket the detectives are using
                ShowNextPopup(20);
                break;
            case 21: // Do you see the white line in the Thames
                ShowNextPopup(21);
                break;
            case 22: //Follow it to the north, tap on station 115 and use the black ticket again.
                ShowNextPopup(GameToGui(NeededSations[4].transform.position), CompassDirection.North, ErasePopupOnPlayerMoveStart, null, 22);
                UnPause();
                break;
            case 24: //But take care when you use your special tickets
                ShowNextPopup(24);
                break;
            case 25: //You win as Mr. X if you don´t get caught
                ShowNextPopup(25);
                break;
            case 26: //You´re ready to escape them again without my help now
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