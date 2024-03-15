using UnityEngine;

public class TicketPopupMrX : TicketPopup
{

    public UILabel LblAny, LblDouble;
    public UIButton BtnAny, BtnDouble;
    public bool IsAllowedToUseSpecialTickets = true;
    public bool IsAllowedToUseDoubleTickets = true;


    public void Setup(PlayerBase player, Station target, bool doubleTicketInUse)
    {
        Debug.Log("Setup MrX");
        
        LblAny.text = player.PlayerState.Tickets.BlackTickets.TicketsLeft.ToString();
        LblDouble.text = player.PlayerState.Tickets.DoubleTickets.ToString();

        bool any = (player.PlayerState.Tickets.BlackTickets.TicketsLeft > 0) && IsAllowedToUseSpecialTickets;
        bool doubleT = !doubleTicketInUse && player.PlayerState.Tickets.DoubleTickets > 0 && IsAllowedToUseDoubleTickets;

        BtnAny.isEnabled = any;
        BtnDouble.isEnabled = doubleT;

        base.SetupButtons(player, target);
        
       
            if (any)
            {
                _nextUiElement = BtnAny;
            } else if (doubleT)
            {
                _nextUiElement = BtnDouble;
            }
        
        SelectFirstButton();
        
        OnSetupComplete();
    }

    public void UseAny()
    {
        Debug.Log("UseAny");
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Any));
    }

    public void UseDouble()
    {
        Debug.Log("UseDouble");
        TicketUsed();
        this.Broadcast(GameGuiEvents.DoubleTicketSelected);
    }

    public void ForceHighlight()
    { 
        _nextUiElement.SetState(UIButtonColor.State.Hover, true);
    }
}
