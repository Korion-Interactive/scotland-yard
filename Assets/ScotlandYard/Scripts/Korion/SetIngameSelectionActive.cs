using Rewired.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIngameSelectionActive : MonoBehaviour
{
    [SerializeField]
    public List<ActionReceiver> actionReceiverss = new List<ActionReceiver>();

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
}
