using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeActionMap : MonoBehaviour
{
    [SerializeField]
    private bool _executeOnAwake = false;

    [SerializeField]
    private bool _executeOnEnable = false;

    [SerializeField]
    private string _controllerMapToSwitchTo;

    [SerializeField]
    private int _playerIndex = 0;

    [SerializeField]
    private bool _forAllPlayers = false; //KORION Todo: Do that for all players

    [SerializeField]
    private bool _disableOtherMaps = true;

    private string _cachedControllerMap = null;

    // Start is called before the first frame update
    void Awake()
    {
        if(_executeOnAwake)
        {
            SetControllerMapState();
        }
    }

    public void SetControllerMapState()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);
        _cachedControllerMap = GetCurrentActionMap();
        SetControllerMap(player, _controllerMapToSwitchTo);
    }

    private void SetControllerMap(Player player, string controllerMap)
    {
        if (_disableOtherMaps)
        {
            if (controllerMap == "Default")
            {
                player.controllers.maps.SetMapsEnabled(false, "UI");
            }
            else if(controllerMap == "UI")
            {
                player.controllers.maps.SetMapsEnabled(false, "Default");
            }
        }
        player.controllers.maps.SetMapsEnabled(true, controllerMap);
    }

    public void ResetControllerMaps()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);

        if(_controllerMapToSwitchTo == "Default")
        {
            player.controllers.maps.SetMapsEnabled(false, "Default");
            player.controllers.maps.SetMapsEnabled(true, "UI");
        }
        else if(_controllerMapToSwitchTo == "UI")
        {
            player.controllers.maps.SetMapsEnabled(true, "Default");
            player.controllers.maps.SetMapsEnabled(false, "UI");
        }
    }

    private string GetCurrentActionMap()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);
        IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(ControllerType.Joystick, _playerIndex); //KORION Todo: Get proper player Index
        foreach(ControllerMap c in cm)
        {
            if(c.enabled)
            {
                return c.name;
            }
        }

        Debug.LogError("Error, no controller map active");
        return "Error, no controller map active";
    }
}
