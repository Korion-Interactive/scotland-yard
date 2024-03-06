using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Toggle Navigation")]
public class UIToggleNavigation : MonoBehaviour
{
    [SerializeField]
    private UIToggle _toggleUp;

    [SerializeField]
    private UIToggle _toggleDown;
    
    public static event System.Action<UIToggleNavigation> AnyToggleClicked;

    protected virtual void OnKey(KeyCode key)
    {
        if (!NGUITools.GetActive(this)) return;
        switch (key)
        {
            case KeyCode.UpArrow:
                Toggle(_toggleUp);
                break;
            case KeyCode.DownArrow:
                Toggle(_toggleDown);
                break;
        }
    }

    private void Toggle(UIToggle toggle)
    {
        if (toggle)
        {
            Debug.Log("Toggle: " + toggle.transform.parent.name, toggle.gameObject);
            toggle.value = true;
            AnyToggleClicked?.Invoke(this);
        }
    }
}