using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    [SerializeField]
    UILabel label;

    // Update is called once per frame
    void Update()
    {
        Debug.Log("I am active: " + this.gameObject.name + ", Text: " + label.text + ", position: " + transform.position);

        if(ReInput.players.GetPlayer(0).GetButtonUp("UIStart"))
        {
            label.enabled = !label.enabled;
        }
    }
}
