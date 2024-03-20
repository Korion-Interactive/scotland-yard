
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Checks
{
    /// <summary>
    /// Checks to test if a user can perform certain online actions on the PS5 system
    /// </summary>
    public class OnlineSafety
    {
        enum NativeMethods : UInt32
        {
            GetCRS = 0x0800001u,  // Communication Restriction Status
            FilterProfanity = 0x0800002u,
            TestProfanity = 0x0800003u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("OnlineSafety");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal OnlineSafety queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Indicates whether or not the account is restricted from communicating on the platform.
        /// Also shows if the account can't access PSN for another reason
        /// </summary>
        public enum CRStatus : UInt32  // Communication Restriction Status
        {
            /// <summary> The status hasn't been checked. </summary>
            NotChecked = 0,
            /// <summary> The checked failed for some reason during a SDK method call. </summary>
            CheckFailed = 1,
            /// <summary> The user is not signed up to use PSN and has no PSN account. </summary>
            NotSignedUp = 2,
            /// <summary> The user was not found of the system. </summary>
            UserNotFound = 3,
            /// <summary> The user is signed out from PSN. </summary>
            SignedOut = 4,
            /// <summary> The account is restricted from communicating on the platform. </summary>
            Restricted = 5,
            /// <summary> The account is not restricted from communicating on the platform. </summary>
            Unrestricted = 6,
            /// <summary> The user is signed in but is not registered with the system. Use <see cref="Users.UserSystem.AddUserRequest"/> to add a user to the system then use <see cref="GetCommunicationRestrictionStatusRequest"/> again.</summary>
            SignedInNotRegistered = 7
        }

        /// <summary>
        /// Returns the users communication restriction status
        /// Examples of such features include: Parental Controls, Posting or viewing of user-generated content (such as text, image, audio, video clip, broadcasting, etc.)
        /// </summary>
        public class GetCommunicationRestrictionStatusRequest : Request
        {
            /// <summary>
            /// The local user id to check
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The returned status of the user
            /// </summary>
            public CRStatus Status { get; internal set; } = CRStatus.NotChecked;

            protected internal override void Run()
            {
                Status = CRStatus.NotChecked;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetCRS);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Status = (CRStatus)nativeMethod.Reader.ReadUInt32();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// The type of profanity filter to use
        /// </summary>
        public enum ProfanityFilterType
        {
            /// <summary>Replace any profanity with * characters </summary>
            ReplaceProfanity,
            /// <summary>Any profanity is surrounded by "[]" characters. </summary>
            MarkProfanity,
        }

        /// <summary>
        /// Filter profanity in text
        /// </summary>
        public class FilterProfanityRequest : Request
        {
            /// <summary>
            ///User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The language locale to use
            /// </summary>
            public string Locale { get; set; } = "en-US";

            /// <summary>
            /// The string to filter
            /// </summary>
            public string TextToFilter { get; set; }

            /// <summary>
            /// The filter type
            /// </summary>
            public ProfanityFilterType FilterType { get; set; } = ProfanityFilterType.ReplaceProfanity;

            /// <summary>
            /// The result of filtered text
            /// </summary>
            public string FilteredText { get; internal set; }

            protected internal override void Run()
            {
                FilteredText = "";

                MarshalMethods.MethodHandle nativeMethod;

                if (FilterType == ProfanityFilterType.ReplaceProfanity)
                {
                    nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FilterProfanity);
                }
                else // FilterType == ProfanityFilterType.MarkProfanity)
                {
                    nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.TestProfanity);
                }

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(Locale);
                nativeMethod.Writer.WritePrxString(TextToFilter);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    FilteredText = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

    }
}
