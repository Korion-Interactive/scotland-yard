using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HiddenOnPlatform : MonoBehaviour
{
    public bool HiddenOnFtuBuild = false;

    public List<RuntimePlatform> HiddenPlatforms = new List<RuntimePlatform>();
    
    public void Awake()
    {
#if FTU
        if(HiddenOnFtuBuild)
        {
            this.gameObject.SetActive(false);
            return;
        }
#endif

        if (HiddenPlatforms.Contains(Application.platform))
            this.gameObject.SetActive(false);
    }
}