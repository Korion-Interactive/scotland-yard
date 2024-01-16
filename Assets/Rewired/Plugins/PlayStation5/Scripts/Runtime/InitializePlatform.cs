#if UNITY_PS5 && !UNITY_EDITOR
// Copyright (c) 2022 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067

namespace Rewired.Utils.Platforms.PS5 {

    public static class InitializePlatform {

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize() {
            Rewired.Utils.ExternalTools.getPlatformInitializerDelegate = InitializePS5;
        }

        private static object InitializePS5() {
            Rewired.Utils.Platforms.PS5.Main.externalTools = new PS5ExternalTools();
            return Rewired.Utils.Platforms.PS5.Main.GetPlatformInitializer();
        }
    }
}
#endif