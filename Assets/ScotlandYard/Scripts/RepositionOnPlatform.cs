using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RepositionOnPlatform : MonoBehaviour
{
    public Vector3 AlternateLocalPosition;

    public bool RepositionOnFtuBuild = false;

    public List<RuntimePlatform> RepositionPlatforms = new List<RuntimePlatform>();

    public void Awake()
    {
#if FTU
        if (RepositionOnFtuBuild)
        {
            this.transform.localPosition = AlternateLocalPosition;
            return;
        }
#endif

        if (RepositionPlatforms.Contains(Application.platform))
            this.transform.localPosition = AlternateLocalPosition;
    }

}
