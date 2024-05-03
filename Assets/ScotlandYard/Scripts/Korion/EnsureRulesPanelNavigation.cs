using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnsureRulesPanelNavigation : MonoBehaviour
{
    //Korion: Hack for Nintendo Switch
    [SerializeField]
    private List<GameObject> _buttons = new List<GameObject>();

    [SerializeField]
    private GameObject _startSelection;

    private GameObject _lastSelection;

    // Start is called before the first frame update
    void Start()
    {
        _lastSelection = _startSelection;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("CurrentSelection: " + UICamera.selectedObject + ", Last Selection: " + UICamera.MLastSelection);
        CheckButtons();
    }

    private void CheckButtons()
    {
        if(UICamera.selectedObject == null || !_buttons.Contains(UICamera.selectedObject))
        {
            UICamera.ForceSetSelection(_lastSelection);
        }
        else if(_buttons.Contains(UICamera.selectedObject) && _lastSelection != UICamera.selectedObject)
        {
            _lastSelection = UICamera.selectedObject;
        }
    }
}
