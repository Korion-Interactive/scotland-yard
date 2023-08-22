using UnityEngine;
using System.Collections;

public class ShowTicketsLeft : MonoBehaviour {

    public UISprite PlayerPawn;
    public UILabel TaxiTicketsLeft;
    public UILabel BusTicketsLeft;
    public UILabel MetroTicketsLeft;
    public UILabel BlackTicketsLeft;
    public UILabel DoubleTicketsLeft;

    public void ShowRemainigTickets(PlayerBase plyr)
    {
        if (plyr == null)
        {
            return;
        }
        if (plyr.IsMrX)
        {
            PlayerPawn.spriteName = plyr.PlayerInfo.Color.GetPawnSpriteName();
            TaxiTicketsLeft.text = plyr.PlayerState.Tickets.TaxiTickets.TicketsLeft.ToString();
            BusTicketsLeft.text = plyr.PlayerState.Tickets.BusTickets.TicketsLeft.ToString();
            MetroTicketsLeft.text = plyr.PlayerState.Tickets.MetroTickets.TicketsLeft.ToString();
            BlackTicketsLeft.text = plyr.PlayerState.Tickets.BlackTickets.TicketsLeft.ToString();
            DoubleTicketsLeft.text = plyr.PlayerState.Tickets.DoubleTickets.ToString();
        }
        else
        {
            PlayerPawn.spriteName = plyr.PlayerInfo.Color.GetPawnSpriteName();
            TaxiTicketsLeft.text = plyr.PlayerState.Tickets.TaxiTickets.TicketsLeft.ToString();
            BusTicketsLeft.text = plyr.PlayerState.Tickets.BusTickets.TicketsLeft.ToString();
            MetroTicketsLeft.text = plyr.PlayerState.Tickets.MetroTickets.TicketsLeft.ToString();
        }
        
    }

}
