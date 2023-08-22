using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonListener : MonoBehaviour
{
    [SerializeField] public List<EventDelegate> _onClickDelegate = new List<EventDelegate>();
    [SerializeField] public List<UIPlayAnimation> _animations = new List<UIPlayAnimation>();
    
    // called by UICamera.Notify() via GameObject.SendMessage()
    public void OnClick()
    {
        if (InteractionSpacer.IsTooNarrow())
        {
            return;
        }
        
        // call delegates first, as they might disable the following UIPlayerAnimations
        EventDelegate.Execute(_onClickDelegate);

        // then trigger the animations
        foreach (UIPlayAnimation clickAnimation in _animations)
        {
            clickAnimation.OnClick();
        }
    }
}
