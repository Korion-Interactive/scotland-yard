using Cysharp.Threading.Tasks.Triggers;
using Korion.ScotlandYard.Input;
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
        Debug.Log("Better be triggered, or I AM");
        Player player = ReInput.players.GetPlayer(_playerIndex);
        _cachedControllerMap = GetCurrentActionMap();

        foreach(var _player in MultiplayerInputManager.Instance.AllPlayers)     // For now let every player share the same controller map
            SetControllerMap(_player, _controllerMapToSwitchTo);
    }

    private void SetControllerMap(Player player, string controllerMap)
    {
        Debug.Log("Set to ControllerMap: " + controllerMap);

        if (_disableOtherMaps)
        {
            if (controllerMap == "Default")
            {
                player.controllers.maps.SetMapsEnabled(false, "UI");
                player.controllers.maps.SetMapsEnabled(false, "PopUp");

            }
            else if (controllerMap == "UI")
            {
                player.controllers.maps.SetMapsEnabled(false, "Default");
                player.controllers.maps.SetMapsEnabled(false, "PopUp");
            }
            else if (controllerMap == "PopUp")
            {
                player.controllers.maps.SetMapsEnabled(false, "UI");
                player.controllers.maps.SetMapsEnabled(false, "Default");
            }
        }
        player.controllers.maps.SetMapsEnabled(true, controllerMap);
    }

    public void ResetControllerMaps()
    {
        Debug.Log("ResetControllerMaps");
        foreach (var _player in MultiplayerInputManager.Instance.AllPlayers)     // For now let every player share the same controller map
        {
            if (_controllerMapToSwitchTo == "Default")
            {
                _player.controllers.maps.SetMapsEnabled(false, "Default");
                _player.controllers.maps.SetMapsEnabled(false, "PopUp");
                _player.controllers.maps.SetMapsEnabled(true, "UI");
            }
            else if (_controllerMapToSwitchTo == "UI")
            {
                _player.controllers.maps.SetMapsEnabled(true, "Default");
                _player.controllers.maps.SetMapsEnabled(false, "UI");
                _player.controllers.maps.SetMapsEnabled(false, "PopUp");
            }
            else if(_controllerMapToSwitchTo == "PopUp")
            {
                _player.controllers.maps.SetMapsEnabled(true, "Default"); //IS THIS ALWAYS TRUE?! //why not using cached in these cases
                _player.controllers.maps.SetMapsEnabled(false, "UI");
                _player.controllers.maps.SetMapsEnabled(false, "PopUp");
            }
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
