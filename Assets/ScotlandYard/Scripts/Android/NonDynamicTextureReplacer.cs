using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(UITexture))]
public class NonDynamicTextureReplacer : BaseBehaviour
{
    public List<string> BadGraphicsCardsIds = new List<string>();
    public Texture2D FallbackTexture16To9; 


    void OnEnable()
    {
        if(BadGraphicsCardsIds.Contains(SystemInfo.graphicsDeviceName))
        {
            // TODO: check for different aspect ratios
            this.GetComponent<UITexture>().mainTexture = FallbackTexture16To9;
        }
    }

}
