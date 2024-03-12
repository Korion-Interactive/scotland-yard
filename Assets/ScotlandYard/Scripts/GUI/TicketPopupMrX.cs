using UnityEngine;
using System.Collections;

public class TicketPopupMrX : TicketPopup
{

    public UILabel LblAny, LblDouble;
    public UIButton BtnAny, BtnDouble;
    public bool IsAllowedToUseSpecialTickets = true;
    public bool IsAllowedToUseDoubleTickets = true;


    public void Setup(PlayerBase player, Station target, bool doubleTicketInUse)
    {
        LblAny.text = player.PlayerState.Tickets.BlackTickets.TicketsLeft.ToString();
        LblDouble.text = player.PlayerState.Tickets.DoubleTickets.ToString();

        bool any = (player.PlayerState.Tickets.BlackTickets.TicketsLeft > 0) && IsAllowedToUseSpecialTickets;
        bool doubleT = !doubleTicketInUse && player.PlayerState.Tickets.DoubleTickets > 0 && IsAllowedToUseDoubleTickets;

        BtnAny.isEnabled = any;
        BtnDouble.isEnabled = doubleT;

        base.Setup(player, target);
    }
    
    public void SelectAny()
    {
        _nextUiElement = BtnAny.gameObject;
    }
    
    public void SelectDouble()
    {
        _nextUiElement = BtnDouble.gameObject;
    }

    public void UseAny()
    {
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Any));
    }

    public void UseDouble()
    {
        TicketUsed();
        this.Broadcast(GameGuiEvents.DoubleTicketSelected);
    }
}
