﻿using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField]
    private UnityEvent _onPopupBuilt;

    [SerializeField]
    private UnityEvent _onTicketUsed;

    [SerializeField]
    private UnityEvent _onPopupClosed;
    
    private SelectUIElement _selectUIElement;

    private bool _enableClosingBehaviour = false;

    void Awake()
    {
        _selectUIElement = GetComponent<SelectUIElement>();
        gameObject.SetActive(false);
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

        if (taxi)
        {
            SelectTaxi();
        } else if (bus)
        {
            SelectBus();
        } else if (metro)
        {
            SelectMetro();
        }

        this.Broadcast(GameGuiEvents.TicketPopupOpened);

        _enableClosingBehaviour = true;

        _onPopupBuilt?.Invoke();
    }

    public void SelectTaxi()
    {
        _selectUIElement.SelectSpecificObject(BtnTaxi.gameObject);
    }

    public void SelectBus()
    {
        _selectUIElement.SelectSpecificObject(BtnBus.gameObject);
    }

    public void SelectMetro()
    {
        _selectUIElement.SelectSpecificObject(BtnMetro.gameObject);
    }

    public void UseTaxi()
    {
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Taxi));
    }

    public void UseBus()
    {
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Bus));
    }

    public void UseMetro()
    {
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Metro));
    }

    public void TicketUsed()
    {
        _onTicketUsed?.Invoke();
    }

    private void OnDisable()
    {
        _onPopupClosed?.Invoke();
    }
}
