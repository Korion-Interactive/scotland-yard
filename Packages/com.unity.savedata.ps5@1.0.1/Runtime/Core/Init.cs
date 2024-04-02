using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;

namespace Unity.SaveData.PS5.Initialization
{
    /// <summary>
    /// The native initialization state of SaveData
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct NativeInitResult
    {
        [MarshalAs(UnmanagedType.I1)]
        internal bool initialized;

        internal UInt32 sceSDKVersion; // SCE_ORBIS_SDK_VERSION
    }

    /// <summary>
    /// Sce SDK Version
    /// </summary>
    public struct SceSDKVersion
    {
        /// <summary>
        /// Major version
        /// </summary>
        public UInt32 Major;

        /// <summary>
        /// Minor verson
        /// </summary>
        public UInt32 Minor;

        /// <summary>
        /// Patch version
        /// </summary>
        public UInt32 Patch;

        /// <summary>
        /// Returns the SDK version as a string separated into Major, Minor, and Patch values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Major.ToString("X2") + "." + Minor.ToString("X3") + "." + Patch.ToString("X3");
        }
    }

    /// <summary>
    /// The initialisation state of SaveData
    /// </summary>
    public struct InitResult
    {
        internal bool initialized;

        internal UInt32 sceSDKVersion; // SCE_ORBIS_SDK_VERSION

        internal Version dllVersion;

        /// <summary>
        /// Returns true if NpToolkit has been initialized correctly, false otherwise.
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// The current SDK version the native plug-in is built with
        /// </summary>
        public UInt32 SceSDKVersionValue
        {
            get { return sceSDKVersion; }
        }

        /// <summary>
        /// The current Version number for the SonyNp assembly
        /// </summary>
        public Version DllVersion
        {
            get { return dllVersion; }
        }

        /// <summary>
        /// The current SDK version as Major, Minor, and Patch values.
        /// </summary>
        public SceSDKVersion SceSDKVersion
        {
            get
            {
                SceSDKVersion version;

                version.Patch = sceSDKVersion & 0x00000FFF;
                version.Minor = (sceSDKVersion >> 12) & 0x00000FFF;
                version.Major = (sceSDKVersion >> 24);

                return version;
            }
        }

        internal void Initialize(NativeInitResult nativeResult)
        {
            initialized = nativeResult.initialized;
            sceSDKVersion = nativeResult.sceSDKVersion;

            dllVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }
    }

    /// <summary>
    /// Set the affinity mask to enable NpToolkit to run on multiple cores.
    /// Important - Core0 and Core1 and not provided, as these are the main Update and Gfx cores and should not be used.
    /// </summary>
    [Flags]
    public enum ThreadAffinity
    {
        /// <summary>Allow native NpToolkit plug-in to run on Core 2</summary>
        Core2 = (1 << 2),
        /// <summary>Allow native NpToolkit plug-in to run on Core 3</summary>
        Core3 = (1 << 3),
        /// <summary>Allow native NpToolkit plug-in to run on Core 4</summary>
        Core4 = (1 << 4),
        /// <summary>Allow native NpToolkit plug-in to run on Core 5</summary>
        Core5 = (1 << 5),

        /// <summary>Allow native NpToolkit plug-in to run on Core 2,3,4, and 5</summary>
        AllCores = Core2 | Core3 | Core4 | Core5,
    }

    /// <summary>
    /// Configures initialization settings for the Save Data plug-in.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InitSettings
    {
        /// <summary>
        /// The affinity mask.
        /// </summary>
        internal ThreadAffinity affinity;

        /// <summary>
        /// The current Version number for the SonyNp assembly.
        /// </summary>
        public ThreadAffinity Affinity
        {
            get { return affinity; }
            set { affinity = value; }
        }

        /// <summary>
        /// By default, initializes the thread settings to use cores 2, 3, 4, and 5.
        /// </summary>
        public void Init()
        {
            affinity = ThreadAffinity.AllCores;
        }
    }

    internal struct ThreadSettingsNative
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        internal string name; // 31 characters plus null

        internal UInt64 affinityMask;

        internal ThreadSettingsNative(ThreadAffinity affinityMask, string name)
        {
            this.name = name;
            this.affinityMask = (UInt64)affinityMask;
        }
    };

    internal class Init
    {
        #region DLL Imports

        [DllImport("__Internal")]
        private static extern int PrxSaveDataSetThreadAffinity(ThreadSettingsNative settings, out APIResult result);

        #endregion

        internal static void SetThreadAffinity(ThreadSettingsNative settings)
        {
            APIResult result;

            PrxSaveDataSetThreadAffinity(settings, out result);
        }
    }

}
