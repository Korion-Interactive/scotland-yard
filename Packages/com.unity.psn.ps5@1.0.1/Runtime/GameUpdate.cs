
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

#if UNITY_PS5
namespace Unity.PSN.PS5
{

    /// <summary>
    /// Provides methods to check if a title has updates
    /// </summary>
    public class GameUpdate
    {
        enum NativeMethods : UInt32
        {
            GameUpdateCheck = 0x1500001u,
            GameUpdateGetAddcontLatestVersion = 0x1500002u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("GameUpdate");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal FeatureGating queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Update check request
        /// </summary>
        public class GameUpdateRequest : Request
        {
            /// <summary>
            /// Game update request option
            /// </summary>
            public Int32 Option { get; set; }

            /// <summary>
            /// Flag for whether an update for the game was found
            /// </summary>
            public bool Found { get; internal set; }

            /// <summary>
            /// Flag for whether an update for additional content was found
            /// </summary>
            public bool AddcontFound { get; internal set; }

            /// <summary>
            /// The game update version found
            /// </summary>
            public string ContentVersion { get; internal set; }

            protected internal override void Run()
            {
                Found = false;
                AddcontFound = false;
                ContentVersion = "";

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GameUpdateCheck);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(Option);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Found = nativeMethod.Reader.ReadBoolean();
                    AddcontFound = nativeMethod.Reader.ReadBoolean();
                    ContentVersion = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Additional content version request
        /// </summary>
        public class AddcontLatestVersionRequest : Request
        {
            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// Unified entitlement label for the relevant additional content
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Flag for whether an update for additional content was found
            /// </summary>
            public bool Found { get; internal set; }

            /// <summary>
            /// Additional content update version
            /// </summary>
            public string ContentVersion { get; internal set; }

            protected internal override void Run()
            {
                Found = false;
                ContentVersion = "";

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GameUpdateGetAddcontLatestVersion);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Found = nativeMethod.Reader.ReadBoolean();
                    ContentVersion = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

    }

}
#endif
