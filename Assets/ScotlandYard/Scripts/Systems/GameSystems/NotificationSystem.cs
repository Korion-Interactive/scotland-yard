using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public class NotificationSystem : BaseSystem<ConnectionEvent, GameEvents, GameGuiEvents, NotificationSystem>
    {

        protected override void RegisterEvents()
        {
            ListenTo<ConnectionArgs>(ConnectionEvent.PeerLeft, PeerLeft);
            ListenTo(GameEvents.PlayerCannotMove, PlayerCannotMove);
            ListenTo(GameGuiEvents.LoadingScene, LoadScene);
        }

        private void LoadScene(BaseArgs obj)
        {
            PopupManager.ShowNotification("wait", "loading_icon", false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void PlayerCannotMove(BaseArgs obj)
        {
            if (!GameState.HasInstance)
                return;

            PlayerBase player = GameState.Instance.CurrentPlayer;

            PopupManager.ShowNotification("player_cannot_move", player.PlayerInfo.Color.GetActorSpriteName(), player.PlayerDisplayName);
        }

        private void PeerLeft(ConnectionArgs args)
        {
            if (!GameState.HasInstance)
                return;

            PlayerSetup player = GameSetupBehaviour.Instance.IterateAllPlayers(true).FirstOrDefault(o => o.ControllingParticipantID == args.ParticipantId);

            PopupManager.ShowPrompt("peer_left_session_header", "peer_left_session_text", null, ((player != null) ? player.DisplayName : Loc.Get("player_human")));

        }

    }

