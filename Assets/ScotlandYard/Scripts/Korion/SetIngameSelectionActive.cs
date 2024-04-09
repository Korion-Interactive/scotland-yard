using Korion.ScotlandYard.Input;
using Rewired;
using Rewired.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIngameSelectionActive : MonoBehaviour
{
    [SerializeField]
    public List<ActionReceiver> actionReceiverss = new List<ActionReceiver>();

    [SerializeField]
    private ChangeActionMap changeActionMap;

    Player player;

    //[SerializeField]
    //public List<UIKeyNavigation> uIKeyNavigations = new List<UIKeyNavigation>();

    public void SetActive(bool isActive)
    {
        PlayerMouseSpriteExample.Instance.SetVisibility(isActive);

        for (int i = 0; i < actionReceiverss.Count; i++)
        {
            actionReceiverss[i].enabled = isActive;
        }
    }

    public void SetProperActionMap(bool isSet)
    {
        //opposite as expected --> since used by a different variable
        if(!isSet)
            changeActionMap.SetControllerMapState();
        else
            changeActionMap.ResetControllerMaps();
    }
}
