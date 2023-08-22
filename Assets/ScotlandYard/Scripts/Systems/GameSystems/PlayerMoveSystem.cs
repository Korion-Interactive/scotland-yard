using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerMoveSystem : BaseSystem<GameEvents, PlayerMoveSystem>
{
    protected override void RegisterEvents()
    {
        ListenTo<MoveArgs>(GameEvents.DetectiveMove, DetectiveMove, Globals.Listen_Early);
        ListenTo<MoveArgs>(GameEvents.MrXMove, MrXMove, Globals.Listen_Early);
        ListenTo(GameEvents.MrXUseDoubleTicket, MrXDoubleTicket, Globals.Listen_Early);
        ListenTo(GameEvents.TurnEnd, TurnEnd);

        ListenTo(GameEvents.GameLoaded, GameLoaded, -10); // Listen Early but not first
    }

    private void GameLoaded(BaseArgs args)
    {
        GameState.Instance.MrX.transform.position = GameState.Instance.MrX.Location.transform.position;
        foreach(var det in GameState.Instance.DetectivesIterator())
        {
            det.transform.position = det.Location.transform.position;
        }

        StartCoroutine(WaitForPlayAndStartAfterLoad());
    }

    private IEnumerator WaitForPlayAndStartAfterLoad()
    {
        while (GameState.Instance.IsGamePaused)
            yield return new WaitForSeconds(0.2f);

        PlayerBase player = GameState.Instance.CurrentPlayer;
        bool isMoving = player.PlayerState.IsCurrentlyMoving;
        Broadcast(GameEvents.TurnStart, player.gameObject);

        if (isMoving)
        {
            Broadcast(GameEvents.PlayerMoveFinished, player.gameObject, GameState.Instance.MoveHistory.Entries.Last().MoveHistoryArgs);
        }
    }

    private void TurnEnd(BaseArgs obj)
    {
        PlayerBase player = obj.RelatedObject.GetComponent<PlayerBase>();
        if(player != null)
        {
            player.MovesThisTurn = 0;
            player.PlayerState.IsCurrentlyMoving = false;
        }
        else
        {
            this.LogError("Turn End... player not valid.");
            foreach (var p in GameState.Instance.PlayerIterator())
                p.MovesThisTurn = 0;
        }

        this.WaitAndDo(new WaitForSeconds(0.2f), () => true, () => GameState.Instance.MrX.IsUsingDoubleTicketThisTurn = false);
    }

    private void MrXDoubleTicket(BaseArgs args)
    {
        GameState.Instance.MrX.IsUsingDoubleTicketThisTurn = true;
        GameState.Instance.MrX.PlayerState.Tickets.UseDoubleTicket();
    }

    private void MrXMove(MoveArgs args)
    {
        var mrX = GameState.Instance.MrX;

        mrX.Moves++;
        mrX.MovesThisTurn++;
        mrX.PlayerState.Tickets.UseTicket(args.Ticket);
        mrX.Location = args.To; 
    }

    private void DetectiveMove(MoveArgs args)
    {
        var det = args.RelatedObject.GetComponent<Detective>();

        det.MovesThisTurn++;
        det.PlayerState.Tickets.UseTicket(args.Ticket);
        det.Location = args.To; 
    }
}