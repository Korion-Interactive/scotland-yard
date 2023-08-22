using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

public class MrX : PlayerBase
{
    int[] appearMoves = new int[] { Globals.MrX_Appear_1st_Time, Globals.MrX_Appear_2nd_Time, Globals.MrX_Appear_3rd_Time, Globals.MrX_Appear_4th_Time, Globals.MrX_Appear_Last_Time };
    public override bool IsMrX { get { return true; } }

    public MrXState MrXState = new MrXState();

    public int Moves { get { return MrXState.Moves; } set { MrXState.Moves = value; } }

    public int BlackTicketStartAmount { get; set; }

    public bool IsUsingDoubleTicketThisTurn { get { return MrXState.IsUsingDoubleTicketThisTurn; } set { MrXState.IsUsingDoubleTicketThisTurn = value; } }

    public Station LastAppearance { get { return MrXState.LastAppearance; } }

    protected override TicketCollection CreateTicketCollection()
    {
        return new TicketCollection(
            Globals.MrX_TicketStartAmount_Taxi,
            Globals.MrX_TicketStartAmount_Bus,
            Globals.MrX_TicketStartAmount_Metro,
            BlackTicketStartAmount,
            Globals.MrX_StartAmount_DoubleTickets);
    }

    public override void GoStep(TransportationType transport, Station target)
    {
        //MovesThisTurn++;
        PlayerState.IsCurrentlyMoving = true;

        MoveArgs.MovementType moveType = MoveArgs.MovementType.SingleMove;
        if(IsUsingDoubleTicketThisTurn)
        {
            moveType = (MovesThisTurn == 0) 
                ? MoveArgs.MovementType.DoubleTicketMove1 
                : MoveArgs.MovementType.DoubleTicketMove2;
        }

        this.Broadcast<GameEvents>(GameEvents.MrXMove, new MoveArgs(this, Location, target, transport, moveType));
    }

    internal void UseDoubleTicket()
    {
        IsUsingDoubleTicketThisTurn = true;
        this.Broadcast<GameEvents>(GameEvents.MrXUseDoubleTicket);
    }

    public void Appear(Station appearLocation)
    {
        MrXState.LastAppearance = appearLocation;
        this.Broadcast(GameEvents.MrXAppear, this.gameObject);

    }

    void OnDrawGizmos() // For testing purpose...
    {
        if (Location == null)
            return;

        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(transform.position, 0.09f);
    }

    public int AppearsInXMoves()
    {
        foreach (int appear in appearMoves)
        {
            if (Moves <= appear)
            {
                return appear - Moves;
            }
        }
        return int.MaxValue;
    }

    internal int AppearedXMovesAgo()
    {
        int result = int.MinValue;
        foreach (int appear in appearMoves)
        {
            if (Moves >= appear)
            {
                result = Moves - appear;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    public bool HasAlreadyAppeared()
    {
        return Moves >= Globals.MrX_Appear_1st_Time;
    }

    public override TransportationType GetAvailableTransportTypes()
    {
        if (PlayerState.Tickets.BlackTickets.TicketsLeft > 0)
            return TransportationType.Any;

        TransportationType result = (TransportationType)0;// TransportationType.None;

        if (PlayerState.Tickets.TaxiTickets.TicketsLeft > 0)
            result |= TransportationType.Taxi;

        if (PlayerState.Tickets.BusTickets.TicketsLeft > 0)
            result |= TransportationType.Bus;

        if (PlayerState.Tickets.MetroTickets.TicketsLeft > 0)
            result |= TransportationType.Metro;

        return result;
    }

    /*
    public override void Serialize(JsonSerializer serializer, JsonWriter writer)
    {
        serializer.Serialize(writer, this.moves); ;
        serializer.Serialize(writer, IsUsingDoubleTicketThisTurn);

        serializer.Serialize(writer, (LastAppearance != null) ? LastAppearance.Id : 0);

        base.Serialize(serializer, writer);
    }

    public override void Deserialize(JsonSerializer serializer, JsonReader reader)
    {
        this.moves = serializer.Deserialize<int>(reader);
        this.IsUsingDoubleTicketThisTurn = serializer.Deserialize<bool>(reader);

        int appearId = serializer.Deserialize<int>(reader);
        LastAppearance = (appearId > 0) ? Station.FindStation(appearId) : null;

        base.Deserialize(serializer, reader);
    }
     * */
}
