using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkSystem : NetworkSystem<GameEvents, GameNetworkSystem>
{
    protected override byte Context { get { return Globals.Net_Context_Ingame; } }

    private bool isMrX { get { return Is(GameState.Instance.MrX); } }

    private bool Is(PlayerBase player)
    {
        return GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleFor(player);
    }

    protected override void Start()
    {
        //base.RequireOrderedMessages = true;

        this.ListenTo(GameEvents.GameStart, GameStart);

        this.WaitAndDo(null, () => GameState.HasInstance && GameState.Instance.IsInitialized, () => 
            base.Start());
    }

    protected override void RegisterEvents()
    {
        //ListenTo(GameEvents.GameStart,          (args) => SendEvent(GameEvents.GameStart, args));
        //ListenTo(GameEvents.NewRound,           (args) => SendEvent(GameEvents.NewRound, args));
        //ListenTo(GameEvents.PlayerCannotMove,   (args) => SendEvent(GameEvents.PlayerCannotMove, args));
        //ListenTo(GameEvents.TurnStart,          (args) => SendEvent(GameEvents.TurnStart, args));
        //ListenTo(GameEvents.TurnEnd,            (args) => SendEvent(GameEvents.TurnEnd, args));
        //ListenTo(GameEvents.TurnTimeOut,        (args) => SendEvent(GameEvents.TurnTimeOut, args));


        ListenTo(GameEvents.MrXAppear,                      (args) => { if (isMrX)                  SendEvent(GameEvents.MrXAppear, args); });
        //ListenTo(GameEvents.MrXUseDoubleTicket, (args) => SendEvent(GameEvents.MrXUseDoubleTicket, args));

        ListenTo<MoveArgs>(GameEvents.MrXMove,              (args) => { if (isMrX)                  SendEvent(GameEvents.MrXMove, args); });
        ListenTo<MoveArgs>(GameEvents.DetectiveMove,        (args) => { if (Is(args.MovingPlayer))  SendEvent(GameEvents.DetectiveMove, args); });

        ListenTo<GameOverArgs>(GameEvents.GameEnd,          (args) => SendEvent(GameEvents.GameEnd, args, false, false));

        //ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished,   (args) => SendEvent(GameEvents.PlayerMoveFinished, args));
        this.ListenTo(ConnectionEvent.PeerLeft, PeerLeft);
        this.ListenTo(ConnectionEvent.SessionLeft, SessionLeft);

        this.ListenTo(GameEvents.YouAreInTheSocietyNow, GainSocietyAchievement);
    }

    private void GameStart(BaseArgs obj)
    {
        if(Stats.IsInSociety)
        {
            SendEvent(GameEvents.YouAreInTheSocietyNow, new BaseArgs(), false, false);
        }
    }

    private void SessionLeft(BaseArgs obj)
    {
        PopupManager.ShowPrompt("local_user_left_session_header", "local_user_left_session_text", (o) =>
        {
            if (GSP.IsMultiplayerRTAvailable && GSP.MultiplayerRT.Mode == MultiplayerMode.ServerClient)
            {
                this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
				SceneManager.LoadSceneAsync("MainMenu");
            }
            else
            {
                ReplaceNetPlayersWithAi(null);
            }
        });
    }

    private void PeerLeft(BaseArgs args)
    {
        string participantId = (args as ConnectionArgs).ParticipantId;

        ReplaceNetPlayersWithAi((o) => o == participantId);

    }

    void ReplaceNetPlayersWithAi(Predicate<string> participantIdPredicate)
    {
        foreach (var player in GameState.Instance.PlayerIterator())
        {
            if (player.PlayerInfo.Controller == PlayerController.Network)
            {
                if (participantIdPredicate == null || participantIdPredicate(player.PlayerInfo.ControllingParticipantID))
                {
                    this.LogInfo("Replacing Network Player with AI");

                    player.PlayerInfo.Controller = PlayerController.Ai;
                    if (GameState.Instance.CurrentPlayer == player)
                    {
                        if (((player.MovesThisTurn == 0)
                            || (player.IsMrX && player.MovesThisTurn == 1 && GameState.Instance.MrX.IsUsingDoubleTicketThisTurn))
                            && GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleFor(player))
                        {
                            this.LogDebug("replaced player: do random move!");
							var storePlayer = player;
							this.WaitAndDo(new WaitForSeconds(1f), null, () => storePlayer.GoRandomStep());
                        }
                    }
                }
            }
        }
    }


    protected override bool MessageReceivedSuccessfully(GameEvents eventType, BaseArgs args)
    {
        switch (eventType)
        {
            case GameEvents.MrXAppear:
                if (GameState.Instance.MrX.AppearsInXMoves() != 0)
                {
                    this.LogError(string.Format("Message received out of order: MrXAppear, but but mr x appears in {0} moves", GameState.Instance.MrX.AppearsInXMoves()));
                    return false;
                }
                break;

            //case GameEvents.MrXUseDoubleTicket: break;
            case GameEvents.MrXMove:

                if (GameState.Instance.CurrentPlayer.IsDetective)
                {
                    this.LogError(string.Format("Message received out of order: MrXMove, but current player is detective ({0})", GameState.Instance.CurrentPlayer.PlayerDisplayName));
                    return false;
                }

                var mx = args as MoveArgs;
                if (mx.MoveType == MoveArgs.MovementType.DoubleTicketMove2 && !GameState.Instance.MrX.IsUsingDoubleTicketThisTurn)
                {
                    this.LogError("Message received out of order: MrXMove, tries to do second move but not using double ticket.");
                    return false;
                }

                if (mx.MoveType == MoveArgs.MovementType.DoubleTicketMove1 && !GameState.Instance.MrX.IsUsingDoubleTicketThisTurn)
                    GameState.Instance.MrX.UseDoubleTicket();

                break;

            case GameEvents.DetectiveMove:

                if (GameState.Instance.CurrentPlayer.IsMrX)
                {
                    this.LogError(string.Format("Message received out of order: Detective Move, but current player is mr. X ({0})", GameState.Instance.CurrentPlayer.PlayerDisplayName));
                    return false;
                }

                var md = args as MoveArgs;
                if (md.MovingPlayer != GameState.Instance.CurrentPlayer)
                {
                    this.LogError(string.Format("Message received out of order: Detective Move, but current player is not moving player (current: {0}, moving: {1})",  
                        GameState.Instance.CurrentPlayer.PlayerDisplayName, 
                        (md.MovingPlayer != null) ? md.MovingPlayer.PlayerDisplayName : "null"));

                    return false;
                }

                break;
        }

        Broadcast(eventType, args);
        return true;
    }

    protected override BaseArgs ArgsFactory(GameEvents eventType)
    {
        switch(eventType)
        {
            case GameEvents.DetectiveMove:
            case GameEvents.MrXMove:
            case GameEvents.PlayerMoveFinished:
                return new MoveArgs();

            case GameEvents.GameEnd:
                return new GameOverArgs();

            default:
                return new BaseArgs();
        }
    }


    private void GainSocietyAchievement(BaseArgs obj)
    {
        Achievements.Unlock(Achievements.society);
    }
}