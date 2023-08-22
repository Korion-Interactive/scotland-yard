using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ClickByKey : MonoBehaviour
{
    public static bool IsBlockedGlobally = false;

    public KeyCode Key = KeyCode.Escape; // Escape is "Back" on Android

    void Update()
    {
        if (Input.GetKeyUp(Key) && ! IsBlockedGlobally)
        {
            SendMessage("OnClick");
        }
    }

}