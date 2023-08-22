using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Flags]
public enum TransportationType
{
    // 0000 0001
    Taxi = 1,
    // 0000 0010
    Bus = 2,
    // 0000 0100
    Metro = 4,
    // 0000 1000
    Ferry = 8,

    // 0000 1111
    Any = Taxi | Bus | Metro | Ferry,
}

public static class TransportationTypeExtension
{
    public static bool IsForTaxi(this TransportationType self) { return (self & TransportationType.Taxi) == TransportationType.Taxi; }
    public static bool IsForBus(this TransportationType self) { return (self & TransportationType.Bus) == TransportationType.Bus; }
    public static bool IsForMetro(this TransportationType self) { return (self & TransportationType.Metro) == TransportationType.Metro; }
    public static bool IsForFerry(this TransportationType self) { return (self & TransportationType.Ferry) == TransportationType.Ferry; }

    public static string GetTransportSpriteName(this TransportationType self)
    {
        switch (self)
        {
            case TransportationType.Taxi:
                return "ticket_taxi";
            case TransportationType.Bus:
                return "ticket_bus";
            case TransportationType.Metro:
                return "ticket_tube";
            case TransportationType.Ferry:
                return "ticket_invisible";
            case TransportationType.Any:
                return "ticket_invisible";
            default:
                throw new ArgumentOutOfRangeException("self");
        }

    }

}