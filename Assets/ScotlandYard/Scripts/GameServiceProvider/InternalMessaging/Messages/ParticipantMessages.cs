using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class ParticipantMessage : IMessage
{
    string senderId;
    public string SenderID { get { return senderId; } }

    protected ParticipantMessage() { }
    protected ParticipantMessage(string senderId)
    {
        this.senderId = senderId;
    }


    public virtual void Serialize(System.IO.BinaryWriter writer)
    {
        writer.Write(senderId);
    }

    public virtual void Deserialize(System.IO.BinaryReader reader)
    {
        senderId = reader.ReadString();
    }

    public abstract byte MessageTypeId { get; }
    public abstract IMessage CreateEmptyInstance();
}

public class PlayerReady : ParticipantMessage
{
    public override byte MessageTypeId { get { return (byte)InternalMessageID.PlayerReady; } }

    public PlayerReady() : base() { }
    public PlayerReady(string participantId) : base(participantId) { }

    public override IMessage CreateEmptyInstance()
    {
        return new PlayerReady();
    }
}
