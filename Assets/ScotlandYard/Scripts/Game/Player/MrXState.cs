using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[Serializable]
public class MrXState : ISerializable
{   
    public int Moves = 0;
    
    public bool IsUsingDoubleTicketThisTurn;

    public Station LastAppearance;

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("LastAppearanceId", (this.LastAppearance != null) ? this.LastAppearance.Id : 0);
        info.AddValue("Moves", this.Moves);
        info.AddValue("IsUsingDoubleTicketThisTurn", this.IsUsingDoubleTicketThisTurn);
    }

    public MrXState() { }
    public MrXState(SerializationInfo info, StreamingContext context)
    {
        int locationId = (int)info.GetValue("LastAppearanceId", typeof(int));
        this.LastAppearance = (locationId > 0) ? Station.FindStation(locationId) : null;

        this.Moves = (int)info.GetValue("Moves", typeof(int));
        this.IsUsingDoubleTicketThisTurn = (bool)info.GetValue("IsUsingDoubleTicketThisTurn", typeof(bool));
    }

}
