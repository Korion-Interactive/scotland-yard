using Korion.ScotlandYard.Input;
using Rewired;
using Rewired.Platforms.Switch;
using System.Collections;
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
#if UNITY_SWITCH && !UNITY_EDITOR
        SwitchControllerCheck._onControllerAppletOpened.AddListener(UpdateControllers);
#endif
    }

    private void OnDisable()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        SwitchControllerCheck._onControllerAppletOpened.RemoveListener(UpdateControllers);
#endif
    }

    private void UpdateControllers()
    {
        if (MultiplayerInputManager.Instance == null) { return; }

        StartCoroutine(UpdateCOntrollersDelayed());
    }

    private IEnumerator UpdateCOntrollersDelayed()
    {
        yield return new WaitForSeconds(1);

        for (int i = 0; i < MultiplayerInputManager.Instance.AllPlayers.Count; i++)
        {
            Player player = ReInput.players.GetPlayer(i);

            for (int j = 0; j < 10; j++)
            {
#if UNITY_STANDALONE
                IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, j);
#elif UNITY_SWITCH
                IEnumerable<ControllerMap> cm;
                if (SwitchControllerCheck.IsHandheld)
                {
                    cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, j);
                }
                else
                {
                    cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, j);
                }
#else
                IEnumerable<ControllerMap> cm = player.controllers.maps.GetMaps(ControllerType.Joystick, j);
#endif
                if (cm != null)
                {
                    Debug.Log("CM is not null at Index " + j);
                    foreach (ControllerMap c in cm)                                                                                          //KORION: All players share the same input mapping so it should work fine!
                    {
                        if (c.categoryId == 0 && _controllerMapToSwitchTo == "Default")
                        {
                            c.enabled = true;
                            player.controllers.maps.SetMapsEnabled(true, "Default");
                        }
                        else if (c.categoryId == 1 && _controllerMapToSwitchTo == "UI")
                        {
                            c.enabled = true;
                            player.controllers.maps.SetMapsEnabled(true, "UI");
                        }
                        else if (c.categoryId == 2 && _controllerMapToSwitchTo == "PopUp")
                        {
                            c.enabled = true;
                            player.controllers.maps.SetMapsEnabled(true, "PopUp");
                        }
                        else
                        {
                            Debug.LogWarning("Warning! Could not reset Action Map");
                        }
                    }
                }
            }
        }
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
        /*
        if (SwitchControllerCheck.IsHandheld)
        {
            cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, 0);
        }
        else
        {
            cm = player.controllers.maps.GetMaps(InputDevices.LastActiveController.type, 1);
        }
        */
        if(cm != null)
        {
            for(int i =0; i < 10; i++)
            {
                cm = player.controllers.maps.GetMaps(ControllerType.Joystick, i);

                foreach (ControllerMap c in cm)                                                                                          //KORION: All players share the same input mapping so it should work fine!
                {
                    if (c.enabled)
                    {
                        return c.categoryId;
                    }
                }
            }
        }
        

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
