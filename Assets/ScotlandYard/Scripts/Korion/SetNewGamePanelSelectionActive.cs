using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewGamePanelSelectionActive : MonoBehaviour
{
    [SerializeField]
    public ChangeActionMap changeActionMap;

    public void ActivateActionMap(bool isSet)
    {
        Debug.Log("ActivateActionMap");

        //opposite as expected --> since used by a different variable
        if (!isSet)
            changeActionMap.SetControllerMapState();
        else
            changeActionMap.ResetControllerMaps();
    }
}
