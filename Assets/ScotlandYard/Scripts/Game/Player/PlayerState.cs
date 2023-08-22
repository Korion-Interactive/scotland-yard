using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[Serializable]
public class PlayerState : ISerializable
{
    public TicketCollection Tickets;
    public Station Location;
    public int MovesThisTurn;
    public bool IsCurrentlyMoving;
    
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Location", this.Location.Id);
        info.AddValue("MovesThisTurn", this.MovesThisTurn);
        info.AddValue("Tickets", this.Tickets);
        info.AddValue("IsCurrentlyMoving", this.IsCurrentlyMoving);
    }

    public PlayerState() { }
    public PlayerState(SerializationInfo info, StreamingContext context)
    {
        int locationId = (int)info.GetValue("Location", typeof(int));
        this.Location = Station.FindStation(locationId);

        this.MovesThisTurn = (int)info.GetValue("MovesThisTurn", typeof(int));
        this.Tickets = (TicketCollection)info.GetValue("Tickets", typeof(TicketCollection));
        this.IsCurrentlyMoving = (bool)info.GetValue("IsCurrentlyMoving", typeof(bool));
    }
}
