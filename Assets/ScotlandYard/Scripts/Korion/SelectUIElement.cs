using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUIElement : MonoBehaviour
{
    Coroutine _cr = null;

    public void SelectSpecificObject(GameObject go)
    {
        UICamera.selectedObject = go;
    }

    public void SelectCurrentObject()
    {
        UICamera.selectedObject = this.gameObject;
    }

    public void SelectSSpecificObjectDelayed(GameObject go)
    {
        if (_cr == null)
        {
            _cr = StartCoroutine(SelectObjectDelayed(go));
        }
        else
        {
            Debug.LogError("Cannot Select Object as it is already being selected");
        }
    }

    IEnumerator SelectObjectDelayed(GameObject go)
    {
        yield return new WaitUntil(() => go.activeInHierarchy == true);

        Debug.Log("Object " + go.name + " selected");
        UICamera.selectedObject = go;
        go.GetComponent<UIButton>().SetState(UIButtonColor.State.Hover, true);
        _cr = null;
    }
}
