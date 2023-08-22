using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.Serialization;


/// <summary>
/// component 'PlayerBase'
/// </summary>
[AddComponentMenu("Scripts/PlayerBase")]
public abstract class PlayerBase : MonoBehaviour
{
    public abstract bool IsMrX { get; }
    public bool IsDetective { get { return !IsMrX; } }

    public WayPartAI[] ConnectionWeight = new WayPartAI[Globals.StationCount + 1]; // [0] is empty: 1-index based
    public List<Station> CurrentWayPlan { get; set; }

    public List<int> AllowedStationConstraints = new List<int>();

    PlayerState state = new PlayerState();
    public PlayerState PlayerState { get { return state; } set { state = value; } }

    public PlayerSetup PlayerInfo;

    public string PlayerDisplayName { get { return PlayerInfo.DisplayName; } set { PlayerInfo.DisplayName = value; } }
    public int PlayerId { get { return PlayerInfo.PlayerId; } }
    
    public Station Location { get { return state.Location; } set { state.Location = value; /*this.transform.position = value.transform.position;*/ } }

    public int MovesThisTurn { get { return state.MovesThisTurn; } set { state.MovesThisTurn = value; } }

    protected GameObject highlightContainer;
    private Transform highlightSmall, highlightBig, highlightTransparent;

    public PlayerBase()
    {

    }

    public virtual void Initialize()
    {
        state.Tickets = CreateTicketCollection();

        
        if (PlayerInfo.StartAtStationId <= 0)
            PlayerInfo.StartAtStationId = UnityEngine.Random.Range(1, Globals.StationCount + 1);

        Location = GameObject.FindObjectsOfType<Station>().FirstOrDefault((o) => o.Id == PlayerInfo.StartAtStationId);
        this.transform.position = Location.transform.position;

        highlightContainer = this.transform.GetChildByName("Highlight").gameObject;
        highlightBig = highlightContainer.transform.GetChildByName("HaloSpriteBig");
        highlightSmall = highlightContainer.transform.GetChildByName("HaloSpriteSmall");
        highlightTransparent = highlightContainer.transform.GetChildByName("HaloSpriteTransparent");
        highlightContainer.SetActive(false);
    }
    protected abstract TicketCollection CreateTicketCollection();
    public abstract TransportationType GetAvailableTransportTypes();

    public bool CanMove()
    {
        foreach (var n in Location.GetAllStationNeighbours())
        {
            // Has apropriate Ticket?
            if (!PlayerState.Tickets.CanUse(n.A))
                continue;

            // Is no detective on location?
            if (GameState.Instance.DetectivesIterator().Select((o) => o.Location).Contains(n.B))
                continue;

            return true;
        }
        return false;
    }
    public abstract void GoStep(TransportationType transport, Station target);

    public IEnumerable<Station> GetReachableNeighbours()
    {
        return Location
            .GetAllStationNeighbours()
            .Where((o) => AllowedStationConstraints.Count == 0 || AllowedStationConstraints.Contains(o.B.Id)) // Tutorial Station Constraints
            .Where((o) => PlayerState.Tickets.CanUse(o.A) && GameState.Instance.DetectivesIterator().FirstOrDefault((p) => p.Location == o.B) == null)
            .Select((o) => o.B);
    }

    public Station GetRandomReachableNeighbour()
    {
        var locs = GetReachableNeighbours().ToArray();
        return locs[UnityEngine.Random.Range(0, locs.Length)];
    }

    public void GoRandomStep()
    {
        var locs = GetReachableNeighbours().ToArray();

        var location = locs[UnityEngine.Random.Range(0, locs.Length)];
        TransportationType[] transports = Location.GetAllTransportationsTo(location).Where((o) => PlayerState.Tickets.CanUse(o)).ToArray();

        if (transports.Length > 0)
        {
            GoStep(transports[0], location);
        }
        else
        {
            this.LogError("Failed to find a ticket... use black");
            GoStep(TransportationType.Any, location);
        }
    }

    internal virtual void EndTurn()
    {
        this.Broadcast(GameEvents.TurnEnd, this.gameObject);
        this.LogInfo(PlayerDisplayName + ": Turn End");
    }

    internal virtual void StartTurn()
    {
        //this.BroadcastDelayed(GameEvents.TurnStart, this.gameObject, 0.5f); // delay is necessary to make sure mrX can appear before start of turn.
        this.Broadcast(GameEvents.TurnStart, this.gameObject);
        this.LogInfo(PlayerDisplayName + ": Turn Start");
    }

    void Update()
    {
        if (highlightContainer.activeSelf)
        {
            highlightSmall.transform.Rotate(0, 0, -100 * Time.deltaTime);
            highlightBig.transform.Rotate(0, 0, 60 * Time.deltaTime);
            highlightTransparent.transform.Rotate(0, 0, 300 * Time.deltaTime);
        }
    }

    public void SetCurrentPlayerHighlights(bool enable)
    {
        highlightContainer.SetActive(enable);
    }

}
