using Korion.ScotlandYard.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerTurnSystem : BaseSystem<GameEvents, PlayerTurnSystem>
{
    public BlurCam BlurCamera;

    private int round { get { return GameState.Instance.Round; } }
    private MrX mrX { get { return GameState.Instance.MrX; } }
    private PlayerBase curPlayer { get { return GameState.Instance.CurrentPlayer; } }

    private GameObject m_cachedSelectedObj = null;

    //private bool doubleTicketInUse;

    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.GameStart, GameStart);
        ListenTo<GameOverArgs>(GameEvents.GameEnd, GameEnd);

        ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished, PlayerHasMoved, Globals.Listen_Late);

        ListenTo(GameEvents.MrXUseDoubleTicket, UseDoubleTicket, Globals.Listen_Early);
        ListenTo<MoveArgs>(GameEvents.DetectiveMove, PlayerMoves, Globals.Listen_Early);
        ListenTo<MoveArgs>(GameEvents.MrXMove, PlayerMoves, Globals.Listen_Early);

        ListenTo(GameEvents.GameLoaded, GameLoaded, Globals.Listen_Early);
        //ListenTo(GameEvents.PlayerCannotMove, MoveImpossible);
    }

    private void GameLoaded(BaseArgs obj)
    {
        GameState.Instance.IsGameOver = false;
        PlayerBase player = GameState.Instance.CurrentPlayer;
        if (IsMixedHotSeatGame() && player.IsMrX && !player.PlayerState.IsCurrentlyMoving)
        {
            PauseAndBlur(true); 
            PopupManager.ShowPrompt("pass_device", "mr_x_turn_starts", (o) => { PauseAndBlur(false);}, GameState.Instance.MrX.PlayerDisplayName);
        }

        //MrX mrX = GameState.Instance.MrX;

        //if (curPlayer == mrX && mrX.IsUsingDoubleTicketThisTurn)
        //{
        //    doubleTicketInUse = true;
        //}
        
        //Broadcast(GameEvents.TurnStart, curPlayer.gameObject);
    }

    private void GameStart(BaseArgs args)
    {
		this.LogInfo("GameStart()");
        GameState.Instance.IsGameOver = false;

        Broadcast(GameEvents.NewRound);
        Action firstTurn = () =>
        {
            BroadcastDelayed(GameEvents.TurnStart, curPlayer.gameObject, 1f);
        };

        if (IsMixedHotSeatGame())
        {
            PauseAndBlur(true);

            PopupManager.ShowQuestion("pass_device", "mr_x_turn_starts", (o) => { PauseAndBlur(false); firstTurn(); SetIngameSelectionActive(true); PopupManager.Instance.CachedButton = null; }, null); //KORION POP UP
            PopupManager.Instance.CachedButton = PopupManager.Instance.CurrentPopup.yesButton; //used to activate when receiving uiCancelAction //popupkill
            PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text = string.Format(PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text, GameState.Instance.MrX.PlayerDisplayName); //KORION IMPROVE --> NEXT LINE IN BETWEEN
            PopupManager.Instance.CurrentPopup.noButton.SetActive(false);
            SetIngameSelectionActive(false);
        }
        else
        {
            firstTurn();
        }
    }

    private void SetIngameSelectionActive(bool isActive)
    {
        gameObject.GetComponent<SetIngameSelectionActive>().SetProperActionMap(isActive);
    }

    private void PauseAndBlur(bool enable)
    {
        BlurCamera.SetBlurred(enable);
        GameState.Instance.SetPausing(enable, false);

    }

    //private void MoveImpossible(GameObject player, EventArgs args)
    //{
    //    this.Assert(player == curPlayer.gameObject);

    //    curPlayer.EndTurn();
    //    Broadcast(GameEvents.TurnEnd, curPlayer.gameObject);

    //    GameState.Instance.NextPlayer();

    //    curPlayer.StartTurn();
    //    BroadcastDelayed(GameEvents.TurnStart, curPlayer.gameObject, 0.5f); // delay is necessary to make sure mrX can appear before start of turn.
    //}

    private void PlayerMoves(MoveArgs args)
    {
        if (args.MovingPlayer.IsMrX)
        {
            var mrX = args.MovingPlayer as MrX;

            GameState.Instance.MoveHistory.MrXTurnStart();

            if(mrX.AppearsInXMoves() == 1)
            {
                Log.info(this, "Mr X appears: " + mrX.Location.Id);
                mrX.Appear(args.To);
            }
        }
        GameState.Instance.MoveHistory.AddMove(args);
    }
    private void UseDoubleTicket(BaseArgs args)
    {
        GameState.Instance.MoveHistory.UsingDoubleTicket();

        //doubleTicketInUse = true;
    }

    private void GameEnd(GameOverArgs args)
    {
        this.LogInfo(string.Format("Game ended in round {0} (Mr X moves: {1})", round, mrX.Moves), Color.yellow);
        GameState.Instance.IsGameOver = true;
    }

    private void PlayerHasMoved(MoveArgs move)
    {
        GameObject player = move.RelatedObject;

        this.Assert(player == curPlayer.gameObject);


        if (curPlayer.IsDetective)
        {
            mrX.PlayerState.Tickets.AddTicket(move.Ticket);

            // GAME END?
            if (curPlayer.Location == mrX.Location)
            {
                Broadcast(GameEvents.GameEnd, new GameOverArgs(GameOverReason.MrXCaught));
                return;
            }
        }
        else
        {
            if(mrX.IsUsingDoubleTicketThisTurn && mrX.MovesThisTurn <= 1) //doubleTicketInUse)
            {
                //doubleTicketInUse = false;
                Broadcast(GameEvents.TurnStart, curPlayer.gameObject);
                return;
            }
        }
            
        curPlayer.EndTurn();

		var lastMovedPlayer = GameState.Instance.MoveHistory.Entries.Last ().MoveHistoryArgs.MovingPlayer;
		//Debug.LogError ("LastMovedPlayer: " + lastMovedPlayer.PlayerDisplayName + " CurrentPlayer: " + curPlayer.PlayerDisplayName);
		if(curPlayer == lastMovedPlayer)
            GameState.Instance.NextPlayer();

        var lastPlayer = curPlayer;

        // SKIP PLAYERS
        while (curPlayer.IsDetective && !curPlayer.CanMove())
        {
            Broadcast(GameEvents.PlayerCannotMove, curPlayer.gameObject);
            GameState.Instance.NextPlayer();

        }

        if (curPlayer.IsMrX)
        {
            // GAME END? (MR X WINS)
            if (mrX.Moves >= Globals.MrX_Appear_Last_Time
                || (mrX.CanMove() && GameState.Instance.DetectivesIterator().FirstOrDefault((o) => o.CanMove()) == null)) // no detective can move?
            {
                Broadcast(GameEvents.GameEnd, new GameOverArgs(GameOverReason.EscapeOfMrX));
                return;
            }

            // GAME END? (DETECTIVES WIN)
            if(!mrX.CanMove())
            {
                // GAME END? (TIE)
                GameState.Instance.NextPlayer();
                if (lastPlayer == curPlayer)
                {
                    Broadcast(GameEvents.GameEnd, new GameOverArgs(GameOverReason.NobodyCanMove));
                    return;
                }

                Broadcast(GameEvents.GameEnd, new GameOverArgs(GameOverReason.MrXSurrounded));
                return;
            }

            // NEW ROUND!
            Broadcast(GameEvents.NewRound);

        }


        Action nextPlayer = () =>
            {
                curPlayer.StartTurn();

                if(curPlayer.PlayerInfo.Controller == PlayerController.Human)
                    MultiplayerInputManager.Instance.NextPlayer();
            };

        if(IsMixedHotSeatGame())
        {
            PauseAndBlur(true);

            PopupManager.ShowQuestion("pass_device", "mr_x_turn_starts", (o) => { PauseAndBlur(false); nextPlayer(); SetIngameSelectionActive(true); PopupManager.Instance.CachedButton = null; }, null); //KORION POP UP
            PopupManager.Instance.CachedButton = PopupManager.Instance.CurrentPopup.yesButton; //used to activate when receiving uiCancelAction //popupkill
            PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text = string.Format(PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text, GameState.Instance.MrX.PlayerDisplayName); //KORION IMPROVE --> NEXT LINE IN BETWEEN
            PopupManager.Instance.CurrentPopup.noButton.SetActive(false);
            SetIngameSelectionActive(false);
        }
        else
        {
            nextPlayer();
        }

        this.WaitAndDo(null, null, () => 
        {
            if (GameSetupBehaviour.Instance.Setup.Mode == GameMode.HotSeat
                && !GameState.Instance.IsGameOver)
            {
                AppSetup.Instance.SaveGame();
            }
        });
    }

    private bool IsMixedHotSeatGame()
    {       
        return curPlayer.IsMrX && curPlayer.PlayerInfo.Controller == PlayerController.Human
            && GameState.Instance.DetectivesIterator().FirstOrDefault((o) => o.PlayerInfo.Controller == PlayerController.Human) != null;
    }


}
