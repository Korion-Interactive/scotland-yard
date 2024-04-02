using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Unity.PSN.PS5.Initialization
{
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
        /// Return the SDK version as a string seperated into Major, Minor and Patch values
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Major.ToString("X2") + "." + Minor.ToString("X3") + "." + Patch.ToString("X3");
        }
    }

    /// <summary>
    /// The native initialization state of NpToolkit
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NativeInitResult
    {
        [MarshalAs(UnmanagedType.I1)]
        internal bool initialized;

        internal UInt32 sceSDKVersion; // SCE_ORBIS_SDK_VERSION
    }

    /// <summary>
    /// The initialization state of NpToolkit
    /// </summary>
    public struct InitResult
    {
        internal bool initialized;

        internal UInt32 sceSDKVersion; // SCE_ORBIS_SDK_VERSION

        internal Version dllVersion;

        /// <summary>
        /// Has NpToolkit been initialize correctly
        /// </summary>
        public bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// The current SDK version the native plugin is built with
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
        /// The current SDK version as Major, Minor and Patch values.
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
}
