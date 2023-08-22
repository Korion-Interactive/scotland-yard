using UnityEngine;

/// <summary>
/// This class is meant to be attached to the Footer Panel of the Scotland Yard GameScene.
/// It takes care of offsetting or resizing elements of the Footer according to the Screen.safeArea
/// </summary>
public class FooterLayouter : ScreenObserver
{
    public Camera footerCam;
    public Transform bottomAlignedContainer;
    public UISprite indicatorsViewBounds;
    public int indicatorsOffset;

    public void Start()
    {
        OriginalIndidcatorsOffset = indicatorsViewBounds.bottomAnchor.absolute;
    }

    protected override void OnScreenSizeChange()
    {
        PositionBottomElements();
        OffsetIndicators();
    }

    private void PositionBottomElements()
    {
        float screenX = Screen.safeArea.position.x + Screen.safeArea.width / 2f;
        float screenY = Screen.safeArea.position.y;
        Vector3 position = footerCam.ScreenToWorldPoint(new Vector2(screenX, screenY));
        bottomAlignedContainer.position = new Vector3(position.x, position.y, bottomAlignedContainer.position.z);
    }

    private void OffsetIndicators()
    {
        if(Screen.safeArea.height != Screen.height)
        {
            indicatorsViewBounds.bottomAnchor.absolute = OriginalIndidcatorsOffset + indicatorsOffset;
        }
        else
        {
            indicatorsViewBounds.bottomAnchor.absolute = OriginalIndidcatorsOffset;
        }
    }

    private int OriginalIndidcatorsOffset { get; set; }
}