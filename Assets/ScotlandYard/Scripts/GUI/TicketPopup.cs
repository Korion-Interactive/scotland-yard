using UnityEngine;
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
    
    protected UIButton _nextUiElement;

    private bool _enableClosingBehaviour = false;
    
    private bool forceFocus = false;

    void Awake()
    {
        _selectUIElement = GetComponent<SelectUIElement>();
        gameObject.SetActive(false);
    }

    public void Setup(PlayerBase player, Station target)
    {
        Debug.Log("Setup");
        
        SetupButtons(player, target);
        
        SelectFirstButton();

        OnSetupComplete();
    }
    
    protected void SetupButtons(PlayerBase player, Station target)
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
            _nextUiElement = BtnTaxi;
        }
        else if (bus)
        {
            _nextUiElement = BtnBus;
        }
        else if (metro)
        {
            _nextUiElement = BtnMetro;
        }
    }
    
    protected void OnSetupComplete()
    {
        this.Broadcast(GameGuiEvents.TicketPopupOpened);
        _enableClosingBehaviour = true;
        _onPopupBuilt?.Invoke();
    }

    protected void SelectFirstButton()
    {
        if (!_nextUiElement)
        {
            return;
        }
        Debug.Log("SelectFirstButton: " + _nextUiElement.name);
        if (forceFocus)
        {
            Debug.Log("ForceFocus2: " + _nextUiElement.name);
            UICamera.ForceSetSelection(_nextUiElement.gameObject);
            _nextUiElement.SetState(UIButtonColor.State.Hover, true);
            forceFocus = false;
        }
        else
        {
            UICamera.selectedObject = _nextUiElement.gameObject;
        }
    }

    public void UseTaxi()
    {
        Debug.Log("UseTaxi");
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Taxi));
    }

    public void UseBus()
    {
        Debug.Log("UseBus");
        TicketUsed();
        this.Broadcast(GameGuiEvents.TransportSelected, this.gameObject, new TransportArgs(TransportationType.Bus));
    }

    public void UseMetro()
    {
        Debug.Log("UseMetro");
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

    public void ForceFocus()
    { 
        if(!NGUITools.GetActive(this))
        {
            Debug.Log("ForceFocus to soon." );
            forceFocus = true;
            return;
        }
        UICamera.ForceSetSelection(_nextUiElement.gameObject);
        _nextUiElement.SetState(UIButtonColor.State.Hover, true);
        Debug.Log("ForceFocus: " + _nextUiElement.name);
    }
}
