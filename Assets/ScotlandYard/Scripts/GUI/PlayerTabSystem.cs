using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PlayerTabSystem : BaseSystem<GameEvents, GameGuiEvents, PlayerTabSystem>
{
    public GameObject PlayerTabPrefab;

    public ShowTicketsLeft DetectivesTicketsLeft, MrXTicketsLeft;
    public ShowTicketsLeft PermanentDetectivesTicketsLeft, PermanentMrXTicketsLeft;

    public PlayerBase Player;

    public List<PlayerTab> PlayerTabs = new List<PlayerTab>();

    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.GameStart, InitializePlayerTabs);
        ListenTo(GameEvents.GameLoaded, InitializePlayerTabs);

        ListenTo(GameEvents.TurnStart, SetCurrentTabActive);
        ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished, PlayerHasMoved);
        ListenTo(GameGuiEvents.ClickedAnywhere, ClickedSomeWhere);
        
    }

    private void ClickedSomeWhere(BaseArgs obj)
    {
        if (GameState.Instance.IsGamePaused)
            return;

        DetectivesTicketsLeft.gameObject.SetActive(false);
        MrXTicketsLeft.gameObject.SetActive(false);
        Player = null;
    }

    private void PlayerHasMoved(MoveArgs obj)
    {
        if (Player == GameState.Instance.CurrentPlayer)
        {
            UpdateFocusPlayerTickets(obj.MovingPlayer);            
        }
    }

    private void UpdateFocusPlayerTickets(PlayerBase player)
    {
        if (GameState.Instance.CurrentPlayer.IsMrX)
        {
            MrXTicketsLeft.ShowRemainigTickets(player);
        }
        else
        {
            DetectivesTicketsLeft.ShowRemainigTickets(player);
        }
    }

    private void UpdatePermanentPlayerTickets(PlayerBase player)
    {
        if (GameState.Instance.CurrentPlayer.IsMrX)
        {
            PermanentMrXTicketsLeft.ShowRemainigTickets(player);
        }
        else
        {
            PermanentDetectivesTicketsLeft.ShowRemainigTickets(player);
        }
    }
    /// <summary>
    /// Set current player tab to active
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void SetCurrentTabActive(BaseArgs args)
    {
        foreach (var tabTransofrm in GetComponent<UIGrid>().GetChildList())
        {
            PlayerTab tab = tabTransofrm.GetComponent<PlayerTab>();
            PlayerBase player = GameState.Instance.CurrentPlayer;
            bool isCurrent = player.PlayerId == tab.PlayerID;

            tab.SetTabActive(isCurrent);

            if(isCurrent)
            {
                PermanentDetectivesTicketsLeft.gameObject.SetActive(player.IsDetective);
                PermanentMrXTicketsLeft.gameObject.SetActive(player.IsMrX);

                UpdatePermanentPlayerTickets(player);
            }
        }
    }

    /// <summary>
    /// Inititalize all players tabs
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="args"></param>
    private void InitializePlayerTabs(BaseArgs args)
    {
		Log.info(this, "InitializePlayerTabs");
        PlayerTabs.Clear();
        foreach (var detective in GameState.Instance.PlayerIterator())
        {
            //PlayerController playerController = detective.PlayerInfo.Controller;

            GameObject go = NGUITools.AddChild(this.gameObject, PlayerTabPrefab);
			NGUITools.SetLayer (go, 10 );
            PlayerTab playerTab = go.GetComponent<PlayerTab>();

            playerTab.PlayerID = detective.PlayerId;
            playerTab.PlayerColor = detective.PlayerInfo.Color.GetColor();

            //if(playerController == PlayerController.Human)
            //{
                playerTab.PlayerName = detective.PlayerDisplayName;
            //}
            //else
            //{
            //    playerTab.PlayerName = detective.PlayerDisplayName + (detective.PlayerId);
            //}
                PlayerTabs.Add(playerTab);
        }

        GetComponent<UIGrid>().Reposition();
    }    
}
