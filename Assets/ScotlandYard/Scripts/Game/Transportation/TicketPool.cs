using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public struct TicketPool
{
    public TransportationType TicketType;// { get; private set; }
    public int StartAmount;// { get; private set; }
    public int TicketsLeft;// { get; private set; }

    public float TicketPercentAmount { get { return (float)TicketsLeft / StartAmount; } }
    
    public TicketPool(TransportationType ticketType, int startAmount) : this(ticketType, startAmount, startAmount)
    { }
    public TicketPool(TransportationType ticketType, int startAmount, int ticketsLeft)
    {
        TicketType = ticketType;
        StartAmount = startAmount;
        TicketsLeft = ticketsLeft;
    }


    public TicketPool GetOneTicketUsedCopy()
    {
        return new TicketPool(TicketType, StartAmount, TicketsLeft - 1);
    }

    public override int GetHashCode()
    {
        return (int)TicketType ^ StartAmount ^ TicketsLeft;
    }
}

