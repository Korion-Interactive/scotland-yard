using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewGamePanelSelectionActive : MonoBehaviour
{
    [SerializeField]
    public List<ActionReceiver> actionReceiverss = new List<ActionReceiver>();

    [SerializeField]
    public List<UIKeyNavigation> uIKeyNavigations = new List<UIKeyNavigation>();

    public void SetActive(bool isActive)
    {
        //TOOD KORION --> cancelui action map fix
        //PopupManager.Instance.CurrentPopup.yesButton.GetComponent<ActionReceiver>().enabled = isActive;

        for (int i = 0; i < uIKeyNavigations.Count; i++)
        {
            uIKeyNavigations[i].enabled = isActive;
        }

        for (int i = 0; i < actionReceiverss.Count; i++)
        {
            actionReceiverss[i].enabled = isActive;
        }
    }
}
