using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ReliableMessageReceivedResponse : IMessage
{
    public byte MessageTypeId { get { return (byte)InternalMessageID.ReliableMessageResponse; } }

    public string ReliableMessageID;
    public string ParticipantID;

    internal ReliableMessageReceivedResponse() { }

    public ReliableMessageReceivedResponse(string reliableMessageId, string participantId)
    {
        this.ReliableMessageID = reliableMessageId;
        this.ParticipantID = participantId;
    }

    public void Serialize(System.IO.BinaryWriter writer)
    {
        writer.Write(ReliableMessageID);
        writer.Write(ParticipantID);
    }

    public void Deserialize(System.IO.BinaryReader reader)
    {
        ReliableMessageID = reader.ReadString();
        ParticipantID = reader.ReadString();
    }

    public IMessage CreateEmptyInstance()
    {
        return new ReliableMessageReceivedResponse();
    }
}