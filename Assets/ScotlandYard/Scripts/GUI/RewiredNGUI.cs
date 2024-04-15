using Korion.ScotlandYard.Input;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewiredNGUI : MonoBehaviour
{
    // Rewired Button Actions
    private static string _actionBottomRow1 = "UISubmit";
    private static string _actionBottomRow2 = "UICancel";

    // Rewired Axis Actions
    private static string _leftStickX = "UIHorizontal";
    private static string _leftStickY = "UIVertical";
    
    private static string _leftStickXPop = "HorizontalPopUp";

    private static string _dPadX = "Horizontal";
    private static string _dPadY = "Vertical";


    void Start()
    {
        UICamera.GetKey = GetKey;
        UICamera.GetKeyDown = GetKeyDown;
        UICamera.GetKeyUp = GetKeyUp;
        UICamera.GetAxis = GetAxis;
    }

    static bool GetKeyDown(KeyCode key)
    {
        
        if (key >= KeyCode.JoystickButton0)
        {
            Debug.Log("Getting Key down: " + key);

            switch (key)
            {
                case KeyCode.JoystickButton0: return MultiplayerInputManager.Instance.CurrentPlayer.GetButtonDown(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return MultiplayerInputManager.Instance.CurrentPlayer.GetButtonDown(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static bool GetKey(KeyCode key)
    {
        if (key >= KeyCode.JoystickButton0)
        {
            Debug.Log("Getting Key : " + key);

            switch (key)
            {
                case KeyCode.JoystickButton0: return MultiplayerInputManager.Instance.CurrentPlayer.GetButton(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return MultiplayerInputManager.Instance.CurrentPlayer.GetButton(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static bool GetKeyUp(KeyCode key)
    {
        if (key >= KeyCode.JoystickButton0)
        {
            Debug.Log("Getting Key up: " + key);

            switch (key)
            {
                case KeyCode.JoystickButton0: return MultiplayerInputManager.Instance.CurrentPlayer.GetButtonUp(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return MultiplayerInputManager.Instance.CurrentPlayer.GetButtonUp(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static float GetAxis(string name)
    {
        switch (name)
        {
            case "UIHorizontal": return MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_leftStickX) + MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_dPadX);
            case "UIVertical": return MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_leftStickY) + MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_dPadY);

            case "HorizontalPopUp": return MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_leftStickXPop) + MultiplayerInputManager.Instance.CurrentPlayer.GetAxis(_dPadX);
        }
        return 0;
    }
}
