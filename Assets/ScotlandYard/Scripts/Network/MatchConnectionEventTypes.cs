using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ConnectionEvent : byte
{
    FindHost = 0,

    Ping = 1,
    Pong = 2,

    HostChanged = 3,
    PeerLeft = 4,
    SessionLeft = 5,

    //ClientServerMatchStarted = 6,
}

public enum GameSetupEvents : byte
{
    PlayerSetupChanged = 0,
    RoundTimeChanged = 1,

    PlayerReady = 2,
    PlayerNotReady = 3,
    AllPlayersReady = 4,

    PlayerChoseCard = 5,
}