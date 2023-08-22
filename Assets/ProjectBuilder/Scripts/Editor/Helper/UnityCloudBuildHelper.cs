#if UNITY_CLOUD_BUILD
using System.Globalization;
using UnityEngine;
using UnityEngine.CloudBuild;
#endif

namespace Ravity.ProjectBuilder
{
    public static class UnityCloudBuildHelper
    {
        public static int GetBuildNumber(int fallbackBuildNumber)
        {
            // https://docs.unity3d.com/Manual/UnityCloudBuildManifest.html
            #if UNITY_CLOUD_BUILD
            BuildManifestObject manifest = Resources.Load<BuildManifestObject>("UnityCloudBuildManifest.scriptable");
            if (manifest != null)
            {
                if(manifest.TryGetValue<string>("buildNumber", out string buildNumberString))
                {
                    int buildNumber = int.Parse(buildNumberString, CultureInfo.InvariantCulture);
                    return buildNumber;
                }
            }
            #endif
            return fallbackBuildNumber;
        }
    }
}