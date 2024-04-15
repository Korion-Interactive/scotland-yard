using UnityEngine;

public class ClickByKey : MonoBehaviour
{
    public static bool IsBlockedGlobally = false;

    public KeyCode Key = KeyCode.Escape; // Escape is "Back" on Android


#if !UNITY_EDITOR && !UNITY_PS4 && !UNITY_PS5 && !UNITY_STANDALONE
    void Update()
    {
        if (Input.GetKeyUp(Key) && ! IsBlockedGlobally)
        {
            SendMessage("OnClick");
        }
    }
#endif
}