using UnityEngine;

/// <summary>
/// This adds support for notched mobile devices to the app, without meddling with any of the NGUI
/// components. Just add this to any object, that already has NGUI's UIAnchor attached.
/// </summary>
[RequireComponent(typeof(UIAnchor))]
public class SafeAreaOffsetter : ScreenObserver
{
    public void Awake()
    {
        Anchor = gameObject.GetComponent<UIAnchor>();
        OriginalOffset = Anchor.relativeOffset;
    }
    
    protected override void OnScreenSizeChange()
    {
        float offset = 0;
        if(AlignLeft)
        {
            offset = Screen.safeArea.position.x / Screen.width;
        }
        else
        {
            offset = -(Screen.width - Screen.safeArea.position.x - Screen.safeArea.width) / Screen.width;
        }
        
        Anchor.relativeOffset = new Vector2(OriginalOffset.x + offset, OriginalOffset.y);
        Anchor.enabled = true;
    }

    private bool AlignLeft
    {
        get
        {
            switch(Anchor.side)
            {
                case UIAnchor.Side.Right:
                case UIAnchor.Side.BottomRight:
                case UIAnchor.Side.TopRight: return false;
                default: return true;
            }
        }
    }

    
    
    private UIAnchor Anchor { get; set; }
    private Vector2 OriginalOffset { get; set; }
}