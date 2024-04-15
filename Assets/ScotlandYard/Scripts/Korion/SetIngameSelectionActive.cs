using Korion.ScotlandYard.Input;
using Rewired;
using Rewired.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIngameSelectionActive : MonoBehaviour
{
    [SerializeField]
    private ChangeActionMap changeActionMap;

    public void SetProperActionMap(bool isSet)
    {
        //opposite as expected --> since used by a different variable
        if(!isSet)
            changeActionMap.SetControllerMapState();
        else
            changeActionMap.ResetControllerMaps();
    }
}
