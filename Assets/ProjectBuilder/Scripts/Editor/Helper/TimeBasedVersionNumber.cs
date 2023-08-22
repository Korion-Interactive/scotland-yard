#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Ravity.ProjectBuilder
{
    public static class TimeBasedVersionNumber
    {
        public static void SetBuildVersionForMobilePlatforms(DateTime relative)
        {
            int bundleVersion = ComputeBundleVersionCode(relative, DateTime.UtcNow);
            PlayerSettings.Android.bundleVersionCode = bundleVersion;
            PlayerSettings.iOS.buildNumber = CreateFullBuildNumberWithFourParts(bundleVersion);
        }

        // returns full build number with 4 parts: (Major version).(Minor version).(Revision number).(Build number)
        public static string CreateFullBuildNumberWithFourParts(int bundleVersion)
        {
            string appVersion = PlayerSettings.bundleVersion;
            // The version should have 2 or 3 parts separated by full stops, like '2021.0' or '2021.0.1'
            int appVersionParts = appVersion.Split('.').Length;
            switch (appVersionParts)
            {
                case 2: return $"{appVersion}.0.{bundleVersion}";
                case 3: return $"{appVersion}.{bundleVersion}";
            }
            throw new ArgumentException($"{nameof(PlayerSettings)}.{nameof(PlayerSettings.bundleVersion)} is '{appVersion}', " +
                                        "but must have 2 or 3 parts separated by full stops (like '2021.0' or '2021.0.1').");
        }
        
        // Version number relative to a specific date e.g 23.11.2020
        // -  totalDays: e.g. 100 i.e. 100 days since relative date
        // -  time of day (hours + minutes) e.g 1413 i.e 14 hours and 13 minutes
        // will result
        //     - on iOS to 2.0.0.[totalDays][time] e.g 2.0.0.1001413
        //     - Android to VersionCode totalDays * 10000 + time e.g: 1001413
        // this solves the issue of having several build configs with separate unity cloud build number counters, but...
        // issue: we want to have a develop and production version from the same commit
        //       if they are build automatically and in parallel
        //       the production version could finish earlier and have a lower build number
        // possible workarounds: 
        // 	   - manually trigger production builds (not on auto build)
        //     - trigger development and production builds in sequence by Jenkins
        public static int ComputeBundleVersionCode(DateTime relative, DateTime now)
        {
            TimeSpan timeSpan = now.Subtract(relative);

            if (timeSpan.TotalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException($"time span is negative: {timeSpan}");
            }

            long totalDays = (long) timeSpan.TotalDays;
            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int timeOfDay = hours * 100 + minutes;
            
            long longBundleVersionCode = 10000 * totalDays + timeOfDay;
            if (longBundleVersionCode > 2000000000)
            {
                // Warning: The greatest value Google Play allows for versionCode is 2 100 000 000. 
                throw new ArgumentOutOfRangeException($"bundle version code to high with totalDays={totalDays} longBundleVersion={longBundleVersionCode}");
            }
            return (int) longBundleVersionCode;
        }
    }
}
#endif
