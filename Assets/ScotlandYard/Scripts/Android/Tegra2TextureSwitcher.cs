using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tegra2TextureSwitcher : MonoBehaviour
{
    enum MaxTextureSize
    {
        NotDetectedYet = 0,
        HigherThan1024 = 1,
        Max1024 = 2,
    }

    static MaxTextureSize detectedDevice = MaxTextureSize.NotDetectedYet;

    void Start()
    {
        if(detectedDevice == MaxTextureSize.NotDetectedYet)
        {
            this.LogInfo(string.Format("graphics device: {0} - vendorID: {1} - graphicsID: {2} - graphicsVersion: {3} - vendorName: {4}, - maxTextureSize: {5}",
                SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceVendorID, SystemInfo.graphicsDeviceID, 
                SystemInfo.graphicsDeviceVersion, SystemInfo.graphicsDeviceVendor, SystemInfo.maxTextureSize));

            detectedDevice = (SystemInfo.maxTextureSize <= 1024)
                ? MaxTextureSize.Max1024
                : MaxTextureSize.HigherThan1024;
        }

        if(detectedDevice == MaxTextureSize.Max1024)
        {
            var texture = this.GetComponent<UITexture>();
            if(texture != null)
            {
                string spriteName = texture.mainTexture.name;

                Texture2D tex = Resources.Load("Tex1024Max/" + spriteName, typeof(Texture2D)) as Texture2D;
                if(tex != null)
                {
                    texture.mainTexture = tex;
                }

            }
        }
    }

}
