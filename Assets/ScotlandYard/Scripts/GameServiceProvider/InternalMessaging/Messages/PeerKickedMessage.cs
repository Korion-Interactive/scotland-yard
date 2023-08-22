using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class PeerKickedMessage : ParticipantMessage
{
    public override byte MessageTypeId { get { return (byte)InternalMessageID.PeerKicked; } }

    public string KickedParticipantID;

    public PeerKickedMessage() : base() { }

    public PeerKickedMessage(string senderId, string kickedPeerId)
        : base(senderId)
    {
        this.KickedParticipantID = kickedPeerId;
    }

    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(KickedParticipantID);
    }

    public override void Deserialize(System.IO.BinaryReader reader)
    {
        base.Deserialize(reader);
        KickedParticipantID = reader.ReadString();
    }


    public override IMessage CreateEmptyInstance()
    {
        return new PeerKickedMessage();
    }
}

