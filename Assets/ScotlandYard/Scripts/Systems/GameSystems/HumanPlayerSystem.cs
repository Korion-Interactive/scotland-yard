using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HumanPlayerSystem : BaseSystem<GameEvents, GameGuiEvents, GlobalGuiEvents, HumanPlayerSystem>
{
    List<Station> currentlyHighlighted = new List<Station>();
    public TicketPopup Popup;
    public TicketPopupMrX PopupMrX;
    public Camera GameBoardCamera, UICamera;
    public BoxCollider ViewBounds;

    Station focusedStation;

    int extraMoveCounter = 0;

    protected override void RegisterEvents()
    {

        ListenTo(GameEvents.TurnStart, TurnStart);
        ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished, MoveFinished);
        ListenTo(GameEvents.TurnTimeOut, TimeOut);

        ListenTo(GameGuiEvents.StationClicked, StationClicked);
        ListenTo<TransportArgs>(GameGuiEvents.TransportSelected, DoMove);
        ListenTo(GameGuiEvents.DoubleTicketSelected, UseDoubleTicket);

        ListenTo(GameEvents.GameLoaded, GameLoaded);
    }

    private void GameLoaded(BaseArgs args)
    {
        if(GameState.Instance.CurrentPlayer.PlayerInfo.Controller == PlayerController.Human)
        {
            HighlightReachableStations(GameState.Instance.CurrentPlayer);
        }
    }

    private void TimeOut(BaseArgs args)
    {
        ClearHighlights();
        Popup.gameObject.SetActive(false);
        PopupMrX.gameObject.SetActive(false);

        GameState.Instance.CurrentPlayer.GoRandomStep();
    }

    private void UseDoubleTicket(BaseArgs args)
    {
        GameState.Instance.MrX.UseDoubleTicket();
        PopupMrX.Setup(GameState.Instance.CurrentPlayer, focusedStation.GetComponent<Station>(), true);

        extraMoveCounter = 2;
    }

    private void DoMove(TransportArgs args)
    {
        GameState.Instance.CurrentPlayer.GoStep(args.Transport, focusedStation.GetComponent<Station>());
        Popup.gameObject.SetActive(false);
        PopupMrX.gameObject.SetActive(false);
        ClearHighlights();
    }


    private void TurnStart(BaseArgs args)
    {
        ClearHighlights();

        PlayerBase player = args.RelatedObject.GetComponent<PlayerBase>();
        if (player.PlayerInfo.Controller != PlayerController.Human)
            return;

        player.SetCurrentPlayerHighlights(true);
        HighlightReachableStations(player);

        extraMoveCounter = 0;
    }

    private void HighlightReachableStations(PlayerBase player)
    {
        foreach(Station s in player.GetReachableNeighbours())
        {
            if (currentlyHighlighted.Contains(s))
                continue;

            //UISprite sprite = s.GetComponent<UISprite>();
            //if(sprite != null)
            //{
            //    sprite.spriteName = sprite.spriteName + "_a"; // HACK
            s.SetHighlights(true);
            currentlyHighlighted.Add(s);
            //}
        }
    }

    private void MoveFinished(MoveArgs args)
    {
        ClearHighlights();

        if(extraMoveCounter == 2)
        {
            TurnStart(args);
            extraMoveCounter = 1;
        }
    }

    private void ClearHighlights()
    {
        foreach(var station in currentlyHighlighted)
        {
            station.SetHighlights(false);
            //UISprite sprite = station.GetComponent<UISprite>();
            //if(sprite != null)
            //    sprite.spriteName = sprite.spriteName.Remove(sprite.spriteName.Length - "_a".Length);
        }

        foreach (PlayerBase player in GameState.Instance.PlayerIterator())
            player.SetCurrentPlayerHighlights(false);

        focusedStation = null;
        currentlyHighlighted.Clear();
    }

    private void StationClicked(BaseArgs args)
    {
        this.LogDebug("CLICK");
        var station = args.RelatedObject.GetComponent<Station>();

        if(currentlyHighlighted.Contains(station))
        {
            this.LogDebug("THIS");

            focusedStation = station;

            if (GameState.Instance.CurrentPlayer.IsMrX)
            {
                PopupMrX.Setup(GameState.Instance.CurrentPlayer, station, (GameState.Instance.CurrentPlayer as MrX).IsUsingDoubleTicketThisTurn);
                PopupMrX.gameObject.SetActive(true);
            }
            else
            {
                Popup.Setup(GameState.Instance.CurrentPlayer, station);
                Popup.gameObject.SetActive(true);
                Popup.ForceFocus();
            }
        }
    }


    void Update()
    {
        if (focusedStation == null)
            return;
        
        var screenPos = GameBoardCamera.WorldToScreenPoint(focusedStation.transform.position);
        var pos = UICamera.ScreenToWorldPoint(screenPos);//NGUIMath.ScreenToPixels(focusedStation.transform.position, TicketPopup.transform.parent);
        
        if(Popup.gameObject.activeSelf)
        {
            UpdatePopupPosition(Popup, pos);
        }
        else if(PopupMrX.gameObject.activeSelf)
        {
            UpdatePopupPosition(PopupMrX, pos);
        }
    }

    void UpdatePopupPosition(TicketPopup popup, Vector3 point)
    {
        const float H_SHIFT = -0.1f;

        float w = popup.transform.lossyScale.x * popup.Bounds.size.x;
        float h = popup.transform.lossyScale.y * popup.Bounds.size.y;
        float hOver2 = 0.5f * h;
        float hOver3 = h / 3;

        float x = 0;
        if(point.x + w + H_SHIFT <= ViewBounds.bounds.max.x)
            x = point.x + w + H_SHIFT;
        else if(point.x - w - H_SHIFT >= ViewBounds.bounds.min.x)
            x = point.x - w - H_SHIFT;
        else
        {
            if (Mathf.Abs(point.x - ViewBounds.bounds.min.x) <= Mathf.Abs(point.x - ViewBounds.bounds.max.x))
                x = point.x + w + H_SHIFT;
            else
                x = point.x - w - H_SHIFT;
        }

        float y = 0;
        if(point.y + hOver3 <= ViewBounds.bounds.max.y)
        {
            if (point.y - 2 * hOver3 >= ViewBounds.bounds.min.y)
                y = point.y - hOver2 + hOver3;
            else if (point.y - hOver3 >= ViewBounds.bounds.max.y)
                y = ViewBounds.bounds.min.y + hOver3;
            else
                y = point.y + hOver2 - hOver3;
        }
        else
        {
            y = point.y - hOver2 + hOver3;//ViewBounds.bounds.max.y - hOver2;
        }

        popup.transform.position = new Vector3(x, y);
    }
}
