using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReselectDetectiveNameButton : MonoBehaviour
{
    [SerializeField]
    private GameObject ButtonToSelect;

    public void SelectButton()
    {
        if(ButtonToSelect != null)
        {
            UICamera.selectedObject = ButtonToSelect;
        }
    }
}
