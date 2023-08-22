using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Ping : ParticipantMessage
{
    public override byte MessageTypeId { get { return (byte)InternalMessageID.Ping; } }

    public Ping() : base() { }
    public Ping(string senderId) : base(senderId) { }

    public override IMessage CreateEmptyInstance()
    {
        return new Ping();
    }
}