using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

    public class MrXVisualizationSystem : BaseSystem<GameEvents, MrXVisualizationSystem>
    {
        public Transform GhostContainer;
        public UISprite GhostSprite;
        public bool CheatShowMrX;

        protected override void RegisterEvents()
        {
            ListenTo(GameEvents.GameStart, GameStart);
            ListenTo(GameEvents.TurnStart, PlayerTurnStart);
            ListenTo(GameEvents.TurnEnd, PlayerTurnEnd);


            ListenTo(GameEvents.MrXAppear, Appear);


            ListenTo(GameEvents.NewRound, NewRound);
            ListenTo(GameEvents.GameLoaded, NewRound);
        }

        private void Appear(BaseArgs args)
        {
            var pos = GameState.Instance.MrX.LastAppearance.transform.position;
            GhostContainer.position = pos;
            GhostSprite.alpha = 0;

            iTween.ValueTo(this.gameObject,
               iTween.Hash(
               "from", 0,
               "to", 1f,
               "delay", 2f,
               "time", 2.5f,
               "easetype", "easeOutCubic",
               "onupdate", "UpdateAlpha",
               "onupdatetarget", this.gameObject
               ));
        }

        void UpdateAlpha(float val)
        {
            GhostSprite.alpha = val;
        }

        private void NewRound(BaseArgs args)
        {
            var mrX = GameState.Instance.MrX;
            if (!mrX.HasAlreadyAppeared() || GameSetupBehaviour.Instance.Setup.Mode == GameMode.TutorialBasics)
                return;

            var pos = GameState.Instance.MrX.LastAppearance.transform.position;
            GhostContainer.position = pos;

            float t = (float)mrX.AppearedXMovesAgo() / 5f;//Math.Max((mrX.AppearsInXMoves() + 5) % 6 + 1, 5); // little hack because method returns 0 on first round

            UpdateAlpha(Mathf.Lerp(1, 0, t));

        }


        private void GameStart(BaseArgs args)
        {
            ShowMrX(GameState.Instance.MrX.gameObject, CheatShowMrX || false);
        }

        private void PlayerTurnStart(BaseArgs args)
        {
            ShowMrX(args.RelatedObject, true);
        }
        private void PlayerTurnEnd(BaseArgs args)
        {
            ShowMrX(args.RelatedObject, CheatShowMrX || false);
        }

        private void ShowMrX(GameObject player, bool show)
        {
            var p = player.GetComponent<PlayerBase>();
            if(p != null && p.IsMrX && (p.PlayerInfo.Controller == PlayerController.Human || !show || CheatShowMrX))
            {
                var sprite = player.GetComponentInChildren<UISprite>();
                if(sprite != null)
                {
                    sprite.enabled = show;
                }
            }
        }
    }
