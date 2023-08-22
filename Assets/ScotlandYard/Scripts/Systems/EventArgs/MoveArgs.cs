using System;
using System.Runtime.Serialization;

[Serializable]
public class MoveArgs : BaseArgs, ISerializable
{
    public enum MovementType
    {
        SingleMove = 0,
        DoubleTicketMove1 = 1,
        DoubleTicketMove2 = 2,
    }

    public PlayerBase MovingPlayer;
    public Station From { get; private set; }
    public Station To { get; private set; }
    public TransportationType Ticket { get; private set; }

    public MovementType MoveType { get; private set; }


    public MoveArgs()
    { 
		this.LogDebug("ctor");
	}

    public MoveArgs(PlayerBase movingPlayer, Station from, Station to, TransportationType ticket, MovementType moveType = MoveArgs.MovementType.SingleMove)
    {
        this.MovingPlayer = movingPlayer;
        this.From = from;
        this.To = to;
        this.Ticket = ticket;
        this.MoveType = moveType;
    }


    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write(MovingPlayer.GetComponent<Identifier>().UniqueID);
        writer.Write(From.Id);
        writer.Write(To.Id);
        writer.Write((int)Ticket);
        writer.Write((int)MoveType);
    }

    public override void Deserialize(System.IO.BinaryReader reader)
    {
        base.Deserialize(reader);

		var identifier = Identifier.Find(reader.ReadInt32());

        MovingPlayer = identifier.GetComponent<PlayerBase>();
		
		
        int from = reader.ReadInt32();
        From = Station.FindStation(from);
		
        int to = reader.ReadInt32();
        To = Station.FindStation(to);
		
        Ticket = (TransportationType)reader.ReadInt32();
        MoveType = (MovementType)reader.ReadInt32();
		
    }

    public override string ToString()
    {
		if(MovingPlayer == null)
			return "MoveArgs (empty)";

        return string.Format("MoveArgs [player: {0}, ticket: {1}, move: {2} > {3}]", MovingPlayer.name, Ticket, 
#if UNITY_EDITOR
            From.Id, To.Id);
#else
            (MovingPlayer.IsMrX) ? "?" : From.Id.ToString(), (MovingPlayer.IsMrX) ? "?": To.Id.ToString());
#endif
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("MovingPlayerId", MovingPlayer.GetComponent<Identifier>().UniqueID);
        info.AddValue("Ticket", this.Ticket);
        info.AddValue("From", this.From.Id);
        info.AddValue("To", this.To.Id);
    }

    public MoveArgs(SerializationInfo info, StreamingContext context)
    {
        int playerId = (int)info.GetValue("MovingPlayerId", typeof(int));
        MovingPlayer = Identifier.Find(playerId).GetComponent<PlayerBase>();

        Ticket = (TransportationType)info.GetValue("Ticket", typeof(TransportationType));

        int fromId = (int)info.GetValue("From", typeof(int));
        From = Station.FindStation(fromId);

        int toId = (int)info.GetValue("To", typeof(int));
        To = Station.FindStation(toId);
    }

    public bool Contains(int stationId)
    {
        return From.Id == stationId || To.Id == stationId;
    }
}
