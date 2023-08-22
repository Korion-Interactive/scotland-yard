using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SubDirectionIndication : SubSystem<GameBoardAnimationSystem>
{
    protected override bool needsUpdate { get { return true; } }

    public BoxCollider Bounds;
    public Camera GameBoardCam, GuiCam;
    public GameObject PlayerDirectionPrefab;
    public GameObject StationDirectionPrefab;

    Dictionary<PlayerBase, Transform> playerIndicators = new Dictionary<PlayerBase, Transform>();
    Dictionary<Station, Transform> stationIndicators = new Dictionary<Station, Transform>();

    internal override void RegisterEvents()
    {
        System.ListenTo(GameEvents.GameStart, GameStart);
        System.ListenTo(GameEvents.GameLoaded, GameStart);

        System.ListenTo(GameEvents.MrXMove, PlayerMove);
        System.ListenTo(GameEvents.DetectiveMove, PlayerMove);
        System.ListenTo(GameEvents.TurnStart, TurnStart);

    }

    public Transform GetPlayerIndicatorTransform(PlayerBase player)
    {
        return playerIndicators[player];
    }

    internal Vector3 GetStationIndicationPosition(int stationId)
    {
        var obj = stationIndicators.FirstOrDefault((o) => o.Key.Id == stationId).Value;
        if (obj == null)
            return new Vector3();

        return obj.position;
    }

    internal bool IsStationIndicatorVisible(int stationId)
    {
        var obj = stationIndicators.FirstOrDefault((o) => o.Key.Id == stationId);
        return obj.Value != null && obj.Value.gameObject.activeSelf;
    }

    private void TurnStart(BaseArgs args)
    {

        if (GameState.Instance.CurrentPlayer.PlayerInfo.Controller != PlayerController.Human)
            return;

        CreateStationIndicators();
    }

    public void CreateStationIndicators()
    {
        ClearStationInidicators();

        foreach(Station s in GameState.Instance.CurrentPlayer.GetReachableNeighbours().ToHashSet())
        {
            GameObject go = NGUITools.AddChild(Bounds.transform.parent.gameObject, StationDirectionPrefab);
            go.name = "station_indicator_" + s.Id;

            var sprite = go.transform.GetChild(0).GetComponent<UISprite>();
            if (s.HasAnyTransportationOption(TransportationType.Metro))
                sprite.spriteName = "station_indicator_tube";
            else if (s.HasAnyTransportationOption(TransportationType.Bus))
                sprite.spriteName = "station_indicator_bus";
            else
                sprite.spriteName = "station_indicator_taxi";

            go.SetActive(false);

            stationIndicators.Add(s, go.transform);
        }
    }

    private void PlayerMove(BaseArgs args)
    {
        ClearStationInidicators();
    }
    private void ClearStationInidicators()
    {
        foreach (Transform t in stationIndicators.Values)
            GameObject.Destroy(t.gameObject);

        stationIndicators.Clear();
    }

    private void GameStart(BaseArgs args)
    {
        foreach (var player in GameState.Instance.PlayerIterator())
        {
            GameObject go = NGUITools.AddChild(Bounds.transform.parent.gameObject, PlayerDirectionPrefab);
            go.name = player.name + "_indicator";

            go.transform.GetChildByName("PlayerSprite").GetComponent<UISprite>().spriteName = player.PlayerInfo.Color.GetDirectionIndicatorSpriteName();
            go.transform.GetChildByName("PlayerSprite").GetComponent<PlayerFocus>().PlayerID = player.PlayerInfo.PlayerId;
            go.transform.GetChildByName("Arrow").GetChildByName("ArrowSprite").GetComponent<UISprite>().color = player.PlayerInfo.Color.GetColor();
            go.SetActive(false);

            playerIndicators.Add(player, go.transform);
        }
    }

    protected override void Update()
    {

        foreach(var player in playerIndicators.Keys)
        {
            Quaternion rotation = new Quaternion();

            if (player.IsMrX 
                && (!(player as MrX).HasAlreadyAppeared()|| GameSetupBehaviour.Instance.Setup.Mode == GameMode.TutorialBasics)) 
                continue;
                

            Transform playerPos = (player.IsMrX) ? (player as MrX).LastAppearance.transform : player.transform;

            if (UpdateIndicator(playerPos, playerIndicators[player], ref rotation))
                playerIndicators[player].GetChildByName("Arrow").rotation = rotation;
        }

        foreach (var station in stationIndicators.Keys)
        {
            Quaternion rotation = new Quaternion();

            if (UpdateIndicator(station.transform, stationIndicators[station], ref rotation))
                stationIndicators[station].rotation = rotation;
        }
    }

    bool UpdateIndicator(Transform target, Transform indicator, ref Quaternion rotation)
    {
        if(GameState.Instance.IsGamePaused && !GameSetupBehaviour.Instance.Setup.Mode.IsTutorial())
        {
            indicator.gameObject.SetActive(false);
            return false;
        }

        Vector3 screenPoint = GameBoardCam.WorldToScreenPoint(target.position);
        Ray screenRay = GuiCam.ScreenPointToRay(screenPoint);
        RaycastHit screenHit;

        if (Bounds.Raycast(screenRay, out screenHit, Mathf.Infinity))
        {
            // make indicator invisible
            indicator.gameObject.SetActive(false);
            return false;
        }
        else
        {
            indicator.gameObject.SetActive(true);

            Vector3 boundsPos = (Bounds.transform.position + Bounds.center);
            Vector2 dir = screenRay.origin - boundsPos;

            Ray ray = new Ray((Vector2)screenRay.origin, -dir);
            RaycastHit hit;
            if (Bounds.Raycast(ray, out hit, Mathf.Infinity))
            {
                rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                indicator.position = new Vector3(hit.point.x, hit.point.y, 5);
            }
            else
            {
                this.LogError(string.Format("No Hit! ray start: {0} -- ray dir: {1}", ray.origin, ray.direction));
            }
            return true;
        }
    }

}
