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
        if(_cachedControllerMap != string.Empty)
        {
            player.controllers.maps.SetMapsEnabled(false, _cachedControllerMap);
        }
        player.controllers.maps.SetMapsEnabled(true, controllerMap);
    }

    public void ResetControllerMaps()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);

        player.controllers.maps.SetMapsEnabled(false, _controllerMapToSwitchTo);
        if (_cachedControllerMap != string.Empty)
        {
            player.controllers.maps.SetMapsEnabled(true, _cachedControllerMap);
        }
    }

    private string GetCurrentActionMap()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);
        IEnumerable<ControllerMap> cm = player.controllers.maps.GetAllMaps();
        foreach(ControllerMap c in cm)
        {
            if(c.enabled)
            {
                return c.name;
            }
        }

        return "Error, no controller map active";
    }
}
