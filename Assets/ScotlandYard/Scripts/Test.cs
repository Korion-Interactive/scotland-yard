using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(coPopups());
    }

    IEnumerator coPopups()
    {
        yield return null;
        yield return null;
        PopupManager.ShowNotification("wait", "loading_icon", false);
        yield return new WaitForSeconds(2);
        PopupManager.CloseNotification();
        yield return new WaitForSeconds(1);
        PopupManager.ShowQuestion("header", "text", (o) => Start(), null);
    }



    //public UIPlayAnimation anim;
    //public void StartAnimation()
    //{
    //    StartCoroutine(coStartAnim());
    //}

    //IEnumerator coStartAnim()
    //{
    //    //foreach (var comp in obj.GetComponents<UIPlayAnimation>())
    //    //    comp.trigger = AnimationOrTween.Trigger.OnSelect;
    //    yield return new WaitForSeconds(1);

    //    gameObject.SetComponentsEnabled<UIPlayAnimation>(true);
    //    gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
    //    //foreach (var anim in obj.GetComponents<UIPlayAnimation>())
    //    //{         //anim.trigger = AnimationOrTween.Trigger.

    //    //    anim.target.gameObject.SetActive(true);
    //    //    ActiveAnimation.Play(anim.target, AnimationOrTween.Direction.Reverse);
            
    //    //}
    //}
}


