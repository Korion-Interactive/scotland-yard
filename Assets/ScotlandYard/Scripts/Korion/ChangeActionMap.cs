using Korion.ScotlandYard.Input;
using Rewired;
using Rewired.Platforms.Switch;
using System.Collections.Generic;
using System.Linq;
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
    private string _specifiedControllerMapToResetTo;

    [SerializeField]
    private int _playerIndex = 0;

    [SerializeField]
    private bool _forAllPlayers = false; //KORION Todo: Do that for all players

    [SerializeField]
    private bool _disableOtherMaps = true;

    //private string _cachedControllerMap = null;
    private int _cachedControllerMap = -1;

    // Start is called before the first frame update
    void Awake()
    {
        if(_executeOnAwake)
        {
            SetControllerMapState();
        }
    }

    private void OnEnable()
    {
        if(_executeOnEnable)
        {
            SetControllerMapState();
        }

        SwitchControllerCheck._onControllerAppletOpened.AddListener(UpdateControllers);
    }

    private void OnDisable()
    {
        SwitchControllerCheck._onControllerAppletOpened.RemoveListener(UpdateControllers);
    }

    private void UpdateControllers()
    {
        if (MultiplayerInputManager.Instance == null) { return; }

        foreach (var _player in MultiplayerInputManager.Instance.AllPlayers)     // For now let every player share the same controller map
            SetControllerMap(_player, _controllerMapToSwitchTo);
    }

    public void SetControllerMapState()
    {
        //Debug.Log("Better be triggered, or I AM");
        Player player = ReInput.players.GetPlayer(_playerIndex);
        _cachedControllerMap = GetCurrentActionMap();

        Debug.Log("_cachedControllerMap: " + _cachedControllerMap);
        Debug.Log("_controllerMapToSwitchTo_: " + _controllerMapToSwitchTo);

        if(MultiplayerInputManager.Instance == null) { return; }

        foreach (var _player in MultiplayerInputManager.Instance.AllPlayers)     // For now let every player share the same controller map
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
            //double Ticket fix, since the flow of an event fires after we already switched to ui and therefore cache ui to switch back to when we actually expect default
            if(_specifiedControllerMapToResetTo != "")
            {
                Debug.Log("reset to specified controller map: " + _specifiedControllerMapToResetTo);
                _player.controllers.maps.SetAllMapsEnabled(false);
                _player.controllers.maps.SetMapsEnabled(true, _specifiedControllerMapToResetTo);
            }
            else if(_cachedControllerMap != -1)
            {
                _player.controllers.maps.SetAllMapsEnabled(false);

                _player.controllers.maps.SetMapsEnabled(true, _cachedControllerMap);
            }
        }
    }

    private int GetCurrentActionMap()
    {
        Player player = ReInput.players.GetPlayer(_playerIndex);

#if UNITY_SWITCH
        Debug.Log("GetCurrentActionMap: Joystick Count: " + player.controllers.joystickCount + ", Player ID: " + player.id);
        SwitchGamepadExtension ext = player.controllers.Joysticks[0].GetExtension<SwitchGamepadExtension>();
        if(ext != null)
        {
            Debug.Log("Npad ID of player " + _playerIndex + " is " + ext.npadId + ", npadStyle: " + ext.npadStyle);
        }
        Controller contr = player.controllers.GetLastActiveController();

        IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, _playerIndex);

#else
        IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, _playerIndex); 
#endif

        //IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, _playerIndex);  //KORION Todo: Get proper player Index
        foreach (ControllerMap c in cm)                                                                                          //KORION: All players share the same input mapping so it should work fine!
        {
            if(c.enabled)
            {
                return c.categoryId;
            }
        }
        /*
        if (cm.Count<ControllerMap>() == 0)
        {
            UpdateControllers();
        }
        */
        Debug.LogError("Error, no controller map active");
        return -1;
    }
}
