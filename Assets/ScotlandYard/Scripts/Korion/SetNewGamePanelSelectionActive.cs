using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewGamePanelSelectionActive : MonoBehaviour
{
    [SerializeField]
    public List<ActionReceiver> actionReceiverss = new List<ActionReceiver>();

    //[SerializeField]
    //public List<UIKeyNavigation> uIKeyNavigations = new List<UIKeyNavigation>();

    [SerializeField]
    public ChangeActionMap changeActionMap;

    public void SetActive(bool isActive)
    {
        //for (int i = 0; i < uIKeyNavigations.Count; i++)
        //{
        //    uIKeyNavigations[i].enabled = isActive;
        //}

        for (int i = 0; i < actionReceiverss.Count; i++)
        {
            actionReceiverss[i].enabled = isActive;
        }
    }

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
