using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SubPlayerMove : SubSystem<GameBoardAnimationSystem>
{
    #region nested class: Movement
    public class Movement
    {
        public MoveArgs Move;
        public Vector3[] Path;
        public PlayerBase Player { get { return Move.MovingPlayer; } }


        public Movement(MoveArgs args)
        {
            this.Move = args;

            var connection = StationConnection.Find(args.From, args.To, args.Ticket);
            if (connection != null)
            {
                var p = connection.GetComponent<iTweenPath>();
                Path = (connection.StationA == args.From) ? iTweenPath.GetPath(p) : iTweenPath.GetPathReversed(p);
            }
            else
            {
                this.LogError("no connection found for: " + args.From.Id + " > " + args.To.Id + " per " + args.Ticket);

                Path = new Vector3[] { args.From.transform.position, args.To.transform.position };
            }
        }
    }
    #endregion

    Movement currentMovement;

    Queue<MoveArgs> pendingMoves = new Queue<MoveArgs>();

    protected override bool needsUpdate { get { return false; } }

    internal override void RegisterEvents()
    {
        System.ListenTo<MoveArgs>(GameEvents.DetectiveMove, DetectiveMove);
        System.ListenTo<MoveArgs>(GameEvents.MrXMove, MrXMove);
    }

    void DetectiveMove(MoveArgs args)
    {
        PlayerMove(args);

    }
    void MrXMove(MoveArgs args)
    {
        PlayerMove(args);

        //var mrX = GameState.Instance.MrX;
    }

    private void PlayerMove(MoveArgs args)
    {
        if (currentMovement == null || GameSetupBehaviour.Instance.Setup.Mode.IsTutorial())
        {
            currentMovement = new Movement(args);
            iTween.MoveTo(currentMovement.Player.gameObject,
               iTween.Hash(
                    "path", currentMovement.Path,
                    "time", 2f,
                    "delay", 0.5f,
                    "easetype", "easeInOutQuad",
                    "oncomplete", "PlayerAnimationCompleted",
                    "oncompletetarget", System.gameObject
                    ));
        }
        else
        {
            pendingMoves.Enqueue(args);
        }
    }

    internal void PlayerAnimationCompleted()
    {

        var args = currentMovement.Move;
        var obj = currentMovement.Player.gameObject;

        System.BroadcastDelayed(GameEvents.PlayerMoveFinished, obj, args, 0.5f);
        //System.Broadcast(GameEvents.PlayerMoveFinished, obj, args);

        currentMovement = null;

        if (pendingMoves.Count > 0)
        {
            var newArgs = pendingMoves.Dequeue();
            PlayerMove(newArgs);
        }
    }

}