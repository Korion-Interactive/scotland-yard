
using System.Runtime.InteropServices;

using Unity.PSN.PS5.GameIntent;
using Unity.PSN.PS5.Initialization;
using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
using Unity.PSN.PS5.Checks;
using Unity.PSN.PS5.Auth;
using Unity.PSN.PS5.Sessions;
using Unity.PSN.PS5.Dialogs;
using Unity.PSN.PS5.Matches;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.TCS;
using Unity.PSN.PS5.Leaderboard;
using Unity.PSN.PS5.Bandwidth;

#if UNITY_PS5
using Unity.PSN.PS5.Trophies;
using Unity.PSN.PS5.UDS;
using Unity.PSN.PS5.PremiumFeatures;
using Unity.PSN.PS5.Entitlement;
using Unity.PSN.PS5.Commerce;
#endif

namespace Unity.PSN.PS5
{
    /// <summary>
    /// PSN Platform types
    /// </summary>
    public enum NpPlatformType
    {
        /// <summary> None </summary>
        None = 0, // SCE_NP_PLATFORM_TYPE_NONE      (0)
        /// <summary> PlayStation 3 </summary>
        PS3 = 1, // SCE_NP_PLATFORM_TYPE_PS3        (1)
        /// <summary> PlayStation Vita </summary>
        Vita = 2, // SCE_NP_PLATFORM_TYPE_VITA      (2)
        /// <summary> PlayStation 4 </summary>
        PS4 = 3, // SCE_NP_PLATFORM_TYPE_PS4        (3)
        /// <summary> PlayStation 5 </summary>
        PS5 = 5, // SCE_NP_PLATFORM_TYPE_PROSPERO   (5)
    }

    /// <summary>
    /// Main entry point to the PSN system and initialization
    /// </summary>
    public class Main
    {
#region DLL Import
        [DllImport("__Internal")]
        private static extern void PSN_PrxInitialize(out NativeInitResult initResult, out APIResult result);

        // Termination
        [DllImport("__Internal")]
        private static extern void PSN_PrxShutDown(out APIResult result);

        // House keeping.
        [DllImport("__Internal")]
        private static extern int PSN_PrxUpdate();

#if UNITY_PS5
        [DllImport("__Internal")]
        private static extern int PrxInjectSystemServiceEvent(UnityEngine.PS5.Utility.SystemServiceEventType type, System.IntPtr paramData);
#endif
        [DllImport("__Internal")] [return:MarshalAs(UnmanagedType.I1)]
        private static extern bool PrxIsSystemEventQueueInitialized();

#endregion

        // A global struct showing if NpToolkit has been initialized and the SDK version number for the native plugin.
        static internal InitResult initResult;

        internal delegate void SystemUpdate();
        internal static event SystemUpdate OnSystemUpdate;

        internal delegate void SystemShutdown();
        internal static event SystemShutdown OnSystemShutdown;

#if UNITY_PS5
        public static void OnSystemServiceEvent(UnityEngine.PS5.Utility.SystemServiceEventType type, System.IntPtr paramData)
        {
            PrxInjectSystemServiceEvent(type, paramData);
        }
#endif

        /// <summary>
        /// Initialise the Unity PSN system
        /// </summary>
        /// <param name="nativeWriteBufferK"> Size of write buffers measured in Kbytes </param>
        /// <param name="nativeReadBufferK"> Size of read buffers measured in Kbytes </param>
        /// <returns>Returns the results of initialization include the SDK version. <see cref="InitResult"/></returns>
        public static InitResult Initialize(uint nativeWriteBufferK = 2048, uint nativeReadBufferK = 2048)
        {
            APIResult result;

            NativeInitResult nativeResult = new NativeInitResult();

            PSN_PrxInitialize(out nativeResult, out result);

            if (result.RaiseException == true) throw new PSNException(result);
#if UNITY_PS5
            if (PrxIsSystemEventQueueInitialized() == false )
            {
                System.Console.WriteLine($"no system event queue initialised ... using fallback managed delegate onSystemServiceEvent");
                UnityEngine.PS5.Utility.onSystemServiceEvent+= OnSystemServiceEvent;
            }
#endif
            initResult.Initialize(nativeResult);

            MarshalMethods.Initialize(nativeWriteBufferK, nativeReadBufferK);

            UserSystem.Start();
            GameIntentSystem.Start();
            WebApiNotifications.Start();
            OnlineSafety.Start();
            Authentication.Start();
            SessionsManager.Start();
            SessionSignalling.Start();
            Sockets.Start();

            DialogSystem.Start();

            TitleCloudStorage.Start();
            Leaderboards.Start();

            BandwidthTest.Start();

#if UNITY_PS5
            UniversalDataSystem.Start();
            TrophySystem.Start();
            FeatureGating.Start();
            GameUpdate.Start();
            Entitlements.Start();
            CommerceDialogSystem.Start();
            CommerceSystem.Start();
            MatchesDialogSystem.Start();
            InvitationDialogSystem.Start();
#endif

            return initResult;
        }

        /// <summary>
        /// Update function. Must be called each frame from the main Unity thread
        /// </summary>
        public static void Update()
        {
            PSN_PrxUpdate();

            WorkerThread.ReleasePollingThreads();

            if (OnSystemUpdate != null)
            {
                OnSystemUpdate();
            }
        }

        /// <summary>
        /// Shutdown the Unity PSN system
        /// </summary>
        public static void ShutDown()
        {
            if (OnSystemShutdown != null)
            {
                OnSystemShutdown();
            }

#if UNITY_PS5
            InvitationDialogSystem.Stop();
            MatchesDialogSystem.Stop();
            CommerceDialogSystem.Stop();
            CommerceSystem.Stop();
            Entitlements.Stop();
            FeatureGating.Stop();
            GameUpdate.Stop();
            TrophySystem.Stop();
            UniversalDataSystem.Stop();
#endif
            BandwidthTest.Stop();
            Leaderboards.Stop();
            TitleCloudStorage.Stop();
            DialogSystem.Stop();
            Sockets.Stop();
            SessionSignalling.Stop();
            SessionsManager.Stop();
            Authentication.Stop();
            OnlineSafety.Stop();
            WebApiNotifications.Stop();
            GameIntentSystem.Stop();
            UserSystem.Stop();
            APIResult result;

            PSN_PrxShutDown(out result);

            if (result.RaiseException == true) throw new PSNException(result);
        }
    }
}
