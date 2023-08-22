using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public struct TicketCollection
{
    [SerializeField] [JsonProperty]
    TicketPool taxi, bus, metro, black;

    [JsonIgnore]
    public TicketPool TaxiTickets { get { return taxi; } }
    [JsonIgnore]
    public TicketPool BusTickets { get { return bus; } }
    [JsonIgnore]
    public TicketPool MetroTickets { get { return metro; } }
    [JsonIgnore]
    public TicketPool BlackTickets { get { return black; } }

    public int DoubleTickets ;//{ get; private set; }


    //private TicketCollection() { }
    public TicketCollection(int taxiTicketCount, int busTicketCount, int metroTicketCount)
        : this(taxiTicketCount, busTicketCount, metroTicketCount, 0, 0)
    { }
    public TicketCollection(int taxiTicketCount, int busTicketCount, int metroTicketCount, int blackTicketCount, int doubleTicketCount)
    {
        taxi = new TicketPool(TransportationType.Taxi, taxiTicketCount);
        bus = new TicketPool(TransportationType.Bus, busTicketCount);
        metro = new TicketPool(TransportationType.Metro, metroTicketCount);
        black = new TicketPool(TransportationType.Any, blackTicketCount);
        DoubleTickets = doubleTicketCount;
    }

    internal void AddTicket(TransportationType transportation)
    {
        ChangeTicketCount(transportation, +1);
    }


    internal void UseTicket(TransportationType transportation)
    {
        ChangeTicketCount(transportation, -1);
    }
    internal void UseDoubleTicket()
    {
        DoubleTickets--;
    }
    private void ChangeTicketCount(TransportationType transportation, int change)
    {
        switch (transportation)
        {
            case TransportationType.Taxi:
                taxi.TicketsLeft += change;
                break;

            case TransportationType.Bus:
                bus.TicketsLeft += change;
                break;

            case TransportationType.Metro:
                metro.TicketsLeft += change;
                break;

            case TransportationType.Ferry:
            case TransportationType.Any:
                black.TicketsLeft += change;
                break;

            default:
                throw new NotImplementedException();
        }
    }
    public TicketPool GetTicketPoolCopy(TransportationType transportation)
    {
        switch (transportation)
        {
            case TransportationType.Taxi:
                return taxi;

            case TransportationType.Bus:
                return bus;

            case TransportationType.Metro:
                return metro;

            case TransportationType.Ferry:
            case TransportationType.Any:
                return black;

            default:
                throw new NotImplementedException();
        }
    }


    public TicketCollection GetCopy()
    {
        return new TicketCollection() {
                taxi = this.TaxiTickets,
                bus = this.BusTickets,
                metro = this.MetroTickets,
                black = this.BlackTickets,
                DoubleTickets = this.DoubleTickets,
            };
    }

    internal bool CanUse(TransportationType transportationType)
    {
        TicketPool pool = GetTicketPoolCopy(transportationType);

        return pool.TicketsLeft > 0 || BlackTickets.TicketsLeft > 0;
    }

    public override int GetHashCode()
    {
        return taxi.GetHashCode() ^ bus.GetHashCode() ^ metro.GetHashCode() ^ black.GetHashCode() ^ DoubleTickets;
    }
}