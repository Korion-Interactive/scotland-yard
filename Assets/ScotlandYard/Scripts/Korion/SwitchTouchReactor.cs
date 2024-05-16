using Korion.ScotlandYard.Input;
using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTouchReactor : MonoBehaviour
{
    [SerializeField]
    private GameObject ObjectToSelectAfterTouch;

    [SerializeField]
    private GameObject RootObject;

#if UNITY_SWITCH

    // Update is called once per frame
    void Update()
    {        
        if ((UICamera.selectedObject == null || UICamera.selectedObject == RootObject) && (MultiplayerInputManager.Instance.CurrentPlayer.GetAnyButton() || MultiplayerInputManager.Instance.CurrentPlayer.GetAnyNegativeButton()))
        {
            UICamera.selectedObject = ObjectToSelectAfterTouch;
        }
    }

#endif
}
