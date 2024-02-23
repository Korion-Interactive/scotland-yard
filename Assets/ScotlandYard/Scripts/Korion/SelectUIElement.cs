using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUIElement : MonoBehaviour
{

    public void SelectSpecificObject(GameObject go)
    {
        UICamera.selectedObject = go;
    }

    public void SelectCurrentObject()
    {
        UICamera.selectedObject = this.gameObject;
    }
}
