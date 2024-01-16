#if UNITY_PS4 && !UNITY_EDITOR
// Copyright (c) 2022 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#if UNITY_2020 || UNITY_2021 || UNITY_2022 || UNITY_2023 || UNITY_2024 || UNITY_2025
#define UNITY_2020_PLUS
#endif

#if UNITY_2019 || UNITY_2020_PLUS
#define UNITY_2019_PLUS
#endif

#if UNITY_2018 || UNITY_2019_PLUS
#define UNITY_2018_PLUS
#endif

#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067

namespace Rewired.Utils.Platforms.PS4 {

    public static class InitializePlatform {

#if UNITY_2019_PLUS
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void Initialize() {
            Rewired.Utils.ExternalTools.getPlatformInitializerDelegate = InitializePS4;
        }

        private static object InitializePS4() {
            Rewired.Utils.Platforms.PS4.Main.externalTools = new PS4ExternalTools();
            return Rewired.Utils.Platforms.PS4.Main.GetPlatformInitializer();
        }
    }
}
#endif