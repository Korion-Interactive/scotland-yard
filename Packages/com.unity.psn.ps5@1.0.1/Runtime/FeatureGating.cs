
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

#if UNITY_PS5
namespace Unity.PSN.PS5.PremiumFeatures
{

    /// <summary>
    /// Provides methods to check if a user is allowed to access premium features or notify the system
    /// when a premium feature is being used
    /// </summary>
    public class FeatureGating
    {
        enum NativeMethods : UInt32
        {
            CheckPremium = 0x0700001u,
            NotifyPremiumFeature = 0x0700002u,
            StartPremiumEventCallback = 0x0700003u,
            StopPremiumEventCallback = 0x0700004u,
            FetchPremiumEvent = 0x0700005u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("FeatureGating");
            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            workerQueue.Stop();
            Main.OnSystemUpdate -= Update;
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
        /// Delegate for notifications about premium events.
        /// </summary>
        /// <param name="premiumEvent">The premium event data containing the notification data</param>
        public delegate void PremiumNotification(PremiumEvent premiumEvent);

        /// <summary>
        /// Event called when a premium notification occurs.
        /// To start notifications call <see cref="StartPremiumEventCallbackRequest"/>.
        /// To stop notifications call <see cref="StopPremiumEventCallbackRequest"/>.
        /// </summary>
        public static event PremiumNotification OnPremiumNotification;

        /// <summary>
        /// The type of premium event notification
        /// </summary>
        public enum PremiumEventType
        {
            RecheckNeeded = 1
        }

        /// <summary>
        /// The premium notification data containing the type and user
        /// </summary>
        public class PremiumEvent
        {
            /// <summary> UserId for the event </summary>
            public Int32 UserId { get; set; }

            /// <summary> Premium feature event type </summary>
            public PremiumEventType EventType { get; set; }
        }

        private static void Update()
        {
            PremiumEvent pe = FetchNext();

            if (pe != null)
            {
                OnPremiumNotification(pe);
            }
        }

        internal static PremiumEvent FetchNext()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchPremiumEvent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            if (nativeMethod.ResultsSize == 0)
            {
                return null;
            }

            Int32 userId = nativeMethod.Reader.ReadInt32();
            Int32 eventId = nativeMethod.Reader.ReadInt32();

            PremiumEvent newEvent = new PremiumEvent();

            newEvent.UserId = userId;
            newEvent.EventType = (PremiumEventType)eventId;

            return newEvent;
        }

        /// <summary>
        /// Premium feature types
        /// </summary>
        [Flags]
        public enum Features : UInt64    // SCE_NP_PREMIUM_FEATURE
        {
            /// <summary>Real-time multi-play</summary>
            RealtimeMultiplay = 1,     // SCE_NP_PREMIUM_FEATURE_REALTIME_MULTIPLAY
        }

        /// <summary>
        /// Checks eligibility to use a Premium feature
        /// </summary>
        public class CheckPremiumRequest : Request
        {
            /// <summary>
            /// User ID to check
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Premium feature types to check
            /// </summary>
            public Features Features { get; set; } = Features.RealtimeMultiplay;

            /// <summary>
            /// Result of checking eligibility to use a Premium feature
            /// </summary>
            public bool Authorized { get; internal set; }

            protected internal override void Run()
            {
                Authorized = false;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.CheckPremium);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write((UInt64)Features);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Authorized = nativeMethod.Reader.ReadBoolean();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Real-time multi-play properties
        /// </summary>
        [Flags]
        public enum MultiplayProperties : UInt64
        {
            /// <summary>Property not specified</summary>
            None = 0,
            /// <summary>Cross-Platform Play</summary>
            CrossPlatformPlay = 1,
            /// <summary>In-Engine Spectating</summary>
            InEngineSpectating = 2,
        }

        /// <summary>
        /// Notifies the use of a Premium feature
        /// </summary>
        public class NotifyPremiumFeatureRequest : Request
        {
            /// <summary>
            /// User ID to check
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Premium feature types to check
            /// </summary>
            public Features Features { get; set; } = Features.RealtimeMultiplay;

            /// <summary>
            /// Real-time multi-play properties
            /// </summary>
            public MultiplayProperties Properties { get; set; } = MultiplayProperties.None;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.NotifyPremiumFeature);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write((UInt64)Features);
                nativeMethod.Writer.Write((UInt64)Properties);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Start notifying a Premium feature event. <see cref="OnPremiumNotification"/>
        /// </summary>
        public class StartPremiumEventCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartPremiumEventCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Stop Premium feature event notifications
        /// </summary>
        public class StopPremiumEventCallbackRequest : Request
        {
            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StopPremiumEventCallback);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }

}
#endif
