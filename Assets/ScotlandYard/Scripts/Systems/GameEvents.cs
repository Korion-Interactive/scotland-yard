using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameEvents : byte
{
    GameStart = 0,
    GameEnd = 1,

    NewRound = 2,

    TurnStart = 3,
    TurnTimeOut = 4,
    TurnEnd = 5,

    PlayerCannotMove = 6,
    PlayerMoveFinished = 7,

    DetectiveMove = 8,

    MrXUseDoubleTicket = 9,
    MrXMove = 10,
    MrXAppear = 11,

    ChangeGamePausing = 12,
    GameLoaded = 13,

    // Special event for achievement "Society" - "Play with or against someone who already has gained this achievement"
    YouAreInTheSocietyNow = 14,
}