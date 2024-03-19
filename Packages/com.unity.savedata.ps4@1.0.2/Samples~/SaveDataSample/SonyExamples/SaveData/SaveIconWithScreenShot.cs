using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveDataSample
{

public class SaveIconWithScreenShot : MonoBehaviour
{
    private bool doScreenshot = false;
    private Sony.PS4.SaveData.Mounting.MountPoint currentMP;

    public void DoScreenShot(Sony.PS4.SaveData.Mounting.MountPoint mp)
    {
        currentMP = mp;

        if (mp != null)
        {
            doScreenshot = true;
        }
    }

    void LateUpdate()
    {
        if (doScreenshot)
        {
#if UNITY_PS4
            doScreenshot = false;

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

            byte[] bytes = screenShot.EncodeToPNG();

            OnScreenLog.Add("Screenshot Icon size : W = " + iconWidth + " H = " + iconHeight);

            SaveIcon(currentMP, bytes);
#endif
        }      
    }

    public void SaveIcon(Sony.PS4.SaveData.Mounting.MountPoint mp, byte[] pngBytes)
    {
#if UNITY_PS4
        try
        {
            Sony.PS4.SaveData.Mounting.SaveIconRequest request = new Sony.PS4.SaveData.Mounting.SaveIconRequest();

            if (mp == null) return;

            request.UserId = User.GetActiveUserId;
            request.MountPointName = mp.PathName;
            request.RawPNG = pngBytes;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Mounting.SaveIcon(request, response);

            OnScreenLog.Add("SaveIcon Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
#endif
    }
}
}