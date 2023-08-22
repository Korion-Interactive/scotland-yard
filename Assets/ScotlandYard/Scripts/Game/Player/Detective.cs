using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Detective : PlayerBase
{
    public override bool IsMrX { get { return false; } }

    protected override TicketCollection CreateTicketCollection()
    {
        return new TicketCollection(
            Globals.Detective_TicketStartAmount_Taxi,
            Globals.Detective_TicketStartAmount_Bus,
            Globals.Detective_TicketStartAmount_Metro);
    }

    public void ClearConnectionWeights()
    {
        //foreach (var waypart in ConnectionWeight)
        for (int i = 0; i < ConnectionWeight.Length; i++)
        {
            ConnectionWeight[i].MakeInvalid();
        }
        //ConnectionWeight[Location.Id].Target = Location;
    }


    internal void GoStep()
    {
        GoStep(ConnectionWeight[CurrentWayPlan[0].Id].BestTransport, CurrentWayPlan[0]);
    }
    public override void GoStep(TransportationType transport, Station target)
    {
        //MovesThisTurn++;
        PlayerState.IsCurrentlyMoving = true;
        Station next = (CurrentWayPlan != null && CurrentWayPlan.Count > 0) ? CurrentWayPlan[0] : null;

        if (next == target)
            CurrentWayPlan.Remove(next);
        else if(CurrentWayPlan != null)
            CurrentWayPlan.Clear();

        this.Broadcast<GameEvents>(GameEvents.DetectiveMove, new MoveArgs(this, Location, target, transport));
    }

    void OnDrawGizmos() // For testing purpose...
    {
        if (Location == null)
            return;

        Gizmos.color = Color.cyan;

        if (GameState.Instance.CurrentPlayer == this)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(transform.position, 0.1f);
    }


    public override TransportationType GetAvailableTransportTypes() { return GetAvailableTransportTypes(false); }
    internal TransportationType GetAvailableTransportTypes(bool ignoreMetroFirstMoves)
    {
        TransportationType result = (TransportationType)0;// TransportationType.None;

        if (PlayerState.Tickets.TaxiTickets.TicketsLeft > 0) 
            result |= TransportationType.Taxi;

        if (PlayerState.Tickets.BusTickets.TicketsLeft > 0) 
            result |= TransportationType.Bus;

        if (!ignoreMetroFirstMoves || GameState.Instance.MrX.HasAlreadyAppeared())
        {
            if (PlayerState.Tickets.MetroTickets.TicketsLeft > 0)
                result |= TransportationType.Metro;
        }

        return result;
    }
}
