using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum InternalMessageID : byte
{
    ServerToClientMatchReady = 0,
    PlayerReady = 1,

    ReliableMessage = 5,
    ReliableMessageResponse = 6,

    Ping = 10,
    PeerKicked = 11,
}
