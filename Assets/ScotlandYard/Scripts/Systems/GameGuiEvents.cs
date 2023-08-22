using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameGuiEvents : byte
{

    ChatMessage = 0, // <- keep as first entry with value 0!

    StationClicked,
    TransportSelected,
    DoubleTicketSelected,
    ClickedAnywhere,
    FocusPosition,
    MoveHistoryEntryAdded,
    MoveHistoryPanelMouseDown,
    MoveHistoryPanelClick,
    PlayerFocusClicked,
    TimeOutInThreeSeconds,
    TicketPopupOpened,
    KeepCamInBounds,
    MouseUp,

    Undefined,

    // do not change the order of the enum! they are serialized as numbers
    // AND they must have continuous values.
    LoadingScene,

}
