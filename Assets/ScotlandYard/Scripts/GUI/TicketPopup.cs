using UnityEngine;
using System.Collections;

/// <summary>
/// component 'TicketPopup'
/// </summary>
[AddComponentMenu("Scripts/TicketPopup")]
public class TicketPopup : MonoBehaviour
{
    public TransportationType AllowedTransportationTypes = TransportationType.Any;
    public UILabel LblTaxiLeft, LblBusLeft, LblMetroLeft, LblFrom, LblTo;
    public UIButton BtnTaxi, BtnBus, BtnMetro;

    public BoxCollider Bounds;

    void Start()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(PlayerBase player, Station target)
    {
        LblTaxiLeft.text = player.PlayerState.Tickets.TaxiTickets.TicketsLeft.ToString();
        LblBusLeft.text = player.PlayerState.Tickets.BusTickets.TicketsLeft.ToString();
        LblMetroLeft.text = player.PlayerState.Tickets.MetroTickets.TicketsLeft.ToString();

        LblFrom.text = player.Location.Id.ToString();
        LblTo.text = target.Id.ToString();

        bool taxi  = (player.PlayerState.Tickets.TaxiTickets.TicketsLeft > 0)  && player.Location.HasTransportationNeighbour(TransportationType.Taxi, target)  && AllowedTransportationTypes.IsForTaxi();
        bool bus   = (player.PlayerState.Tickets.BusTickets.TicketsLeft > 0)   && player.Location.HasTransportationNeighbour(TransportationType.Bus, target)   && AllowedTransportationTypes.IsForBus();
        bool metro = (player.PlayerState.Tickets.MetroTickets.TicketsLeft > 0) && player.Location.HasTransportationNeighbour(TransportationType.Metro, target) && AllowedTransportationTypes.IsForMetro();

        BtnTaxi.isEnabled = taxi;
        BtnBus.isEnabled = bus;
        BtnMetro.isEnabled = metro;

        this.Broadcast(GameGuiEvents.TicketPopupOpened);
    }

    public void UseTaxi()
    {
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Taxi));
    }

    public void UseBus()
    {
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Bus));
    }

    public void UseMetro()
    {
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Metro));
    }
}
