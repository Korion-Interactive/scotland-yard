
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Auth
{
    /// <summary>
    /// The Authentication system provides a feature to obtain the authorization code required by the application server to access user information on the
    /// server of PlayStation™Network, and it provides a feature for obtaining an ID token.
    /// </summary>
    public class Authentication
    {
        enum NativeMethods : UInt32
        {
            GetAuthorizationCode = 0x0900001u,
            GetIdToken = 0x0900002u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("Authentication");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Authentication queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Get authorization code
        /// </summary>
        public class GetAuthorizationCodeRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Client program's client ID
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// Entitlement scope for obtaining authorization codes
            /// </summary>
            public string Scope { get; set; }

            /// <summary>
            /// The obtained authorization code
            /// </summary>
            public string AuthCode { get; internal set; }

            /// <summary>
            /// The obtained issuer ID
            /// </summary>
            public int IssuerId { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetAuthorizationCode);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(ClientId);
                nativeMethod.Writer.WritePrxString(Scope);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    IssuerId = nativeMethod.Reader.ReadInt32();
                    AuthCode = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Get ID token
        /// </summary>
        public class GetIdTokenRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Client program's client ID
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// Client program's client secret
            /// </summary>
            public string ClientSecret { get; set; }

            /// <summary>
            /// Scope of information requested by the client program
            /// </summary>
            public string Scope { get; set; }

            /// <summary>
            /// The obtained ID token
            /// </summary>
            public string IdToken { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetIdToken);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(ClientId);
                nativeMethod.Writer.WritePrxString(ClientSecret);
                nativeMethod.Writer.WritePrxString(Scope);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    IdToken = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

    }
}
