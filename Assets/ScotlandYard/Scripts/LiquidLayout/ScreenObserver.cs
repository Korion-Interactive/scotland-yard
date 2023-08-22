using UnityEngine;

public abstract class ScreenObserver : MonoBehaviour
{
    public virtual void LateUpdate()
    {
        bool screenChanged = Screen.width != LastScreenWidht
                             || Screen.height != LastScreenHeight
                             || Screen.safeArea != LastSafeArea;
        if(screenChanged)
        {
            OnScreenSizeChange();  
            
            LastScreenWidht = Screen.width;
            LastScreenHeight = Screen.height;
            LastSafeArea = Screen.safeArea;
        }
    }
    
    private float LastScreenWidht { get; set; }
    private float LastScreenHeight { get; set; }
    private Rect LastSafeArea { get; set; }

    protected abstract void OnScreenSizeChange();
}
