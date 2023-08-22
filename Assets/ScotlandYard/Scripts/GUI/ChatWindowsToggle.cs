using UnityEngine;
using System.Collections;

public class ChatWindowsToggle : MonoBehaviour {

    public void ToggleChatWindow(GameObject go)
    {
        NGUITools.SetActive(go, !NGUITools.GetActive(go));
    }
}
