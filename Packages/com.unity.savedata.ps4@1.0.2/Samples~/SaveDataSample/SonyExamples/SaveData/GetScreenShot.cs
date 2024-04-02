using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveDataSample
{

public class GetScreenShot : MonoBehaviour
{
    private bool doScreenshot = false;
    public byte[] screenShotBytes = null;

    public bool IsWaiting
    {
        get { return doScreenshot == true; }
    }

    public void DoScreenShot()
    {
        screenShotBytes = null;
        doScreenshot = true;
    }

    void LateUpdate()
    {
#if UNITY_PS4
        if (doScreenshot)
        {
            Camera camera = GetComponent<Camera>();

            int iconWidth = Sony.PS4.SaveData.Mounting.SaveIconRequest.DATA_ICON_WIDTH;
            int iconHeight = Sony.PS4.SaveData.Mounting.SaveIconRequest.DATA_ICON_HEIGHT;

            RenderTexture rt = new RenderTexture(iconWidth, iconHeight, 24);

            camera.targetTexture = rt;

            Texture2D screenShot = new Texture2D(iconWidth, iconHeight, TextureFormat.RGB24, false);

            camera.Render();

            RenderTexture.active = rt;

            screenShot.ReadPixels(new Rect(0, 0, iconWidth, iconHeight), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null;

            Destroy(rt);

            screenShotBytes = screenShot.EncodeToPNG();

            doScreenshot = false;

            OnScreenLog.Add("Screenshot Icon size : W = " + iconWidth + " H = " + iconHeight);
        }
#endif
    }
}
}