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
            Rewired.Player rewiredSystemPlayer = Rewired.ReInput.players.GetSystemPlayer(); // Done every time this is called???

            switch (key)
            {
                case KeyCode.JoystickButton0: return rewiredSystemPlayer.GetButtonDown(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return rewiredSystemPlayer.GetButtonDown(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static bool GetKey(KeyCode key)
    {
        if (key >= KeyCode.JoystickButton0)
        {
            Rewired.Player rewiredSystemPlayer = Rewired.ReInput.players.GetSystemPlayer(); // Done every time this is called???

            switch (key)
            {
                case KeyCode.JoystickButton0: return rewiredSystemPlayer.GetButton(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return rewiredSystemPlayer.GetButton(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static bool GetKeyUp(KeyCode key)
    {
        if (key >= KeyCode.JoystickButton0)
        {
            Rewired.Player rewiredSystemPlayer = Rewired.ReInput.players.GetSystemPlayer(); // Done every time this is called???

            switch (key)
            {
                case KeyCode.JoystickButton0: return rewiredSystemPlayer.GetButtonUp(_actionBottomRow1);        // Action bottom row 1 (Xbox "A" button, for instance)
                case KeyCode.JoystickButton1: return rewiredSystemPlayer.GetButtonUp(_actionBottomRow2);        // Action bottom row 2 (Xbox "B" button, for instance)
            }
        }
        return false;
    }

    static float GetAxis(string name)
    {
        Rewired.Player rewiredSystemPlayer = Rewired.ReInput.players.GetSystemPlayer(); // Done every time this is called???

        switch (name)
        {
            case "UIHorizontal":
                {
                    if (rewiredSystemPlayer.GetAxis(_leftStickX) + rewiredSystemPlayer.GetAxis(_dPadX) > 0 /* && rewiredSystemPlayer.controllers.Keyboard.*/)
                        return 1f;

                    if (rewiredSystemPlayer.GetAxis(_leftStickX) + rewiredSystemPlayer.GetAxis(_dPadX) < 0 /* && rewiredSystemPlayer.controllers.Keyboard.*/)
                        return -1f;

                    return rewiredSystemPlayer.GetAxis(_leftStickX) + rewiredSystemPlayer.GetAxis(_dPadX);
                }
            case "UIVertical": 
                {
                    if (rewiredSystemPlayer.GetAxis(_leftStickY) + rewiredSystemPlayer.GetAxis(_dPadY) > 0 /* && rewiredSystemPlayer.controllers.Keyboard.*/)
                        return 1f;

                    if (rewiredSystemPlayer.GetAxis(_leftStickY) + rewiredSystemPlayer.GetAxis(_dPadY) < 0 /* && rewiredSystemPlayer.controllers.Keyboard.*/)
                        return -1f;

                    return rewiredSystemPlayer.GetAxis(_leftStickY) + rewiredSystemPlayer.GetAxis(_dPadY);
                }
        }
        return 0;
    }
}
