
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using System.Collections.Generic;

#if UNITY_PS5
namespace Unity.PSN.PS5.Entitlement
{
    /// <summary>
    /// Entitlements provides features for accessing additional content, consumable entitlements, and subscription entitlements.
    /// </summary>
    public class Entitlements
    {
        enum NativeMethods : UInt32
        {
            GetAdditionalContentEntitlementList = 0xFA00001u,
            GetSkuFlag = 0xFA00002u,
            GetAdditionalContentEntitlementInfo = 0xFA00003u,
            GetEntitlementKey = 0xFA00004u,

            AbortRequest = 0xFA00005u,
            DeleteRequest = 0xFA00006u,
            GenerateTransactionId = 0xFA00007u,
            PollUnifiedEntitlementInfo = 0xFA00008u,
            PollUnifiedEntitlementInfoList = 0xFA00009u,
            PollServiceEntitlementInfo = 0xFA00010u,
            PollServiceEntitlementInfoList = 0xFA00011u,
            PollConsumeEntitlement = 0xFA00012u,
            RequestUnifiedEntitlementInfo = 0xFA00013u,
            RequestUnifiedEntitlementInfoList = 0xFA00014u,
            RequestServiceEntitlementInfo = 0xFA00015u,
            RequestServiceEntitlementInfoList = 0xFA00016u,
            RequestConsumeUnifiedEntitlement = 0xFA00017u,
            RequestConsumeServiceEntitlement = 0xFA00018u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("Entitlements");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Entitlements queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Package type
        /// </summary>
        public enum EntitlementAccessPackageType
        {
            /// <summary> Undefined type </summary>
            None = 0,
            /// <summary> Application </summary>
            PSGD = 1,
            /// <summary> Additional content with extra data </summary>
            PSAC = 2,
            /// <summary> Additional content without extra data </summary>
            PSAL = 3,
            /// <summary> Consumable </summary>
            PSCONS = 4,
            /// <summary> In-game currency </summary>
            PSVC = 5,
            /// <summary> Subscription </summary>
            PSSUBS = 6,
        }

        /// <summary>
        /// Entitlement type
        /// </summary>
        public enum EntitlementTypes
        {
            /// <summary> Undefined type </summary>
            None = 0,
            /// <summary> Service entitlement </summary>
            Service = 1,
            /// <summary> Unified entitlement </summary>
            Unified = 2,
        }

        /// <summary>
        /// Sort type
        /// </summary>
        public enum SortTypes
        {
            /// <summary> None </summary>
            None = 0,
            /// <summary> Service entitlement </summary>
            ActiveData = 1,
        }

        /// <summary>
        /// Sort order
        /// </summary>
        public enum SortOrders
        {
            /// <summary> None </summary>
            None = 0,
            /// <summary> Ascending order </summary>
            Ascending = 1,
            /// <summary> Descending order </summary>
            Descending = 2,
        }

        /// <summary>
        /// Download status
        /// </summary>
        public enum EntitlementAccessDownloadStatus
        {
            /// <summary> This additional content does not include any data to be downloaded </summary>
            NoExtraData = 0,
            /// <summary> There is data to be downloaded, but it has not been installed </summary>
            NoInQueue,
            /// <summary> There is data to be downloaded, and it is currently being downloaded </summary>
            Downloading,
            /// <summary> There is data to be downloaded, and the download is currently paused </summary>
            DownloadSuspended,
            /// <summary> There is data to be downloaded, and installation is complete </summary>
            Installed,
        }

        /// <summary>
        /// Additional content information
        /// </summary>
        public class AdditionalContentEntitlementInfo
        {
            /// <summary>
            /// Unified entitlement label
            /// </summary>
            public string EntitlementLabel { get; internal set; }

            /// <summary>
            /// Package type
            /// </summary>
            public EntitlementAccessPackageType PackageType { get; internal set; }

            /// <summary>
            /// Download status
            /// </summary>
            public EntitlementAccessDownloadStatus DownloadStatus { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                EntitlementLabel = reader.ReadPrxString();
                PackageType = (EntitlementAccessPackageType)reader.ReadUInt32();
                DownloadStatus = (EntitlementAccessDownloadStatus)reader.ReadUInt32();
            }
        }

        /// <summary>
        /// Gets a list of additional content information for which the entitlement is valid
        /// </summary>
        public class GetAdditionalContentEntitlementListRequest : Request
        {
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// List of additional content entitlements.
            /// </summary>
            public AdditionalContentEntitlementInfo[] Entitlements { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetAdditionalContentEntitlementList);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(ServiceLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    int numEntitlements = nativeMethod.Reader.ReadInt32();

                    if (numEntitlements == 0)
                    {
                        Entitlements = null;
                    }
                    else
                    {
                        Entitlements = new AdditionalContentEntitlementInfo[numEntitlements];

                        for (int i = 0; i < numEntitlements; i++)
                        {
                            Entitlements[i] = new AdditionalContentEntitlementInfo();

                            Entitlements[i].Deserialise(nativeMethod.Reader);
                        }
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Sku type
        /// </summary>
        public enum SkuTypes
        {
            /// <summary> Undefined type </summary>
            NotSet = 0,
            /// <summary> Trial version </summary>
            Trial = 1,
            /// <summary> Full version </summary>
            Full = 3,
        }

        /// <summary>
        /// Gets the SKU type
        /// </summary>
        public class GetSkuFlagRequest : Request
        {
            public SkuTypes SkuType { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetSkuFlag);

                // Write the data to match the expected format in the native code

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    SkuType = (SkuTypes)nativeMethod.Reader.ReadInt32();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Gets additional content information
        /// </summary>
        public class GetAdditionalContentEntitlementInfoRequest : Request
        {
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Unified entitlement label of the target additional content
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Additional content entitlement.
            /// </summary>
            public AdditionalContentEntitlementInfo Entitlement { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetAdditionalContentEntitlementInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Entitlement.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Gets the entitlement key of additional content
        /// </summary>
        public class GetEntitlementKeyRequest : Request
        {
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Unified entitlement label of the target additional content
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Entitlement key of additional content
            /// </summary>
            public string EntitlementKey { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetEntitlementKey);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    EntitlementKey = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Unified entitlement
        /// </summary>
        public class UnifiedEntitlementInfo
        {
            /// <summary>
            /// Unified entitlement label
            /// </summary>
            public string EntitlementLabel { get; internal set; }

            /// <summary>
            /// End date
            /// </summary>
            public DateTime InactiveDate { get; internal set; }

            /// <summary>
            /// Start date
            /// </summary>
            public DateTime ActiveDate { get; internal set; }

            /// <summary>
            /// Entitlement type
            /// </summary>
            public EntitlementTypes EntitlementType { get; internal set; }

            /// <summary>
            /// Number of consumed entitlement counts
            /// </summary>
            public Int32 UseCount { get; internal set; }

            /// <summary>
            /// Number of remaining entitlement counts that can be consumed
            /// </summary>
            public Int32 UseLimit { get; internal set; }

            /// <summary>
            /// Package type
            /// </summary>
            public EntitlementAccessPackageType PackageType { get; internal set; }

            /// <summary>
            /// Active state
            /// </summary>
            public bool IsActive { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                InactiveDate = default;
                ActiveDate = default;

                bool isDateSet = reader.ReadBoolean();
                if (isDateSet)
                {
                    InactiveDate = reader.ReadRtcTicks();
                }

                isDateSet = reader.ReadBoolean();
                if (isDateSet)
                {
                    ActiveDate = reader.ReadRtcTicks();
                }

                EntitlementLabel = reader.ReadPrxString();
                EntitlementType = (EntitlementTypes)reader.ReadUInt32();
                UseCount = reader.ReadInt32();
                UseLimit = reader.ReadInt32();
                PackageType = (EntitlementAccessPackageType)reader.ReadUInt32();
                IsActive = reader.ReadBoolean();
            }
        }

        internal enum PollStatus
        {
            None,
            Running,
            Finished,
            Failed
        }

        /// <summary>
        /// Requests to obtain a list of unified entitlement information
        /// </summary>
        public class GetUnifiedEntitlementInfoListRequest : Request
        {
            /// <summary>
            /// Entitlement Info List Max
            /// </summary>
            public const UInt32 MaxEntitlementsLimit = 100;

            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Index (starting from 0) to a set of paged results. Responses start from the results at the specified offset.
            /// Result sets can be paged by using <see cref="Offset"/> and <see cref="Limit"/> in combination
            /// </summary>
            public Int32 Offset { get; set; }

            /// <summary>
            /// Maximum number of entitlements to retrieve in this request
            /// </summary>
            public UInt32 Limit { get; set; } = MaxEntitlementsLimit;

            /// <summary>
            /// Type of Unified package to retrieve. Only valid values are:
            /// <see cref="EntitlementAccessPackageType.PSCONS"/>,
            /// <see cref="EntitlementAccessPackageType.PSVC"/> and
            /// <see cref="EntitlementAccessPackageType.PSSUBS"/>
            /// </summary>
            public EntitlementAccessPackageType PackageType { get; set; }

            /// <summary>
            /// Sort type
            /// </summary>
            public SortTypes Sort { get; set; } = SortTypes.None;

            /// <summary>
            /// Sort order
            /// </summary>
            public SortOrders SortDirection { get; set; }

            /// <summary>
            /// List of unified entitlements.
            /// </summary>
            public UnifiedEntitlementInfo[] Entitlements { get; internal set; }

            /// <summary>
            /// Offset value for obtaining the next paged set of entitlements passed to succeeding requests.
            /// </summary>
            public Int32 NextOffset { get; internal set; }

            /// <summary>
            /// Offset value for obtaining the previous paged set of entitlements passed to succeeding requests.
            /// </summary>
            public Int32 PreviousOffset { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestUnifiedEntitlementInfoList);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.Write(Offset);
                nativeMethod.Writer.Write(Limit);
                nativeMethod.Writer.Write((Int32)PackageType);
                nativeMethod.Writer.Write((Int32)Sort);
                nativeMethod.Writer.Write((Int32)SortDirection);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollUnifiedEntitlementInfoList);

                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Writer.Write(Limit);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        NextOffset = nativeMethod.Reader.ReadInt32();
                        PreviousOffset = nativeMethod.Reader.ReadInt32();

                        // Read the data from the call
                        int numEntitlements = nativeMethod.Reader.ReadInt32();

                        if (numEntitlements == 0)
                        {
                            Entitlements = null;
                        }
                        else
                        {
                            Entitlements = new UnifiedEntitlementInfo[numEntitlements];

                            for (int i = 0; i < numEntitlements; i++)
                            {
                                Entitlements[i] = new UnifiedEntitlementInfo();

                                Entitlements[i].Deserialise(nativeMethod.Reader);
                            }
                        }

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Requests to obtain unified entitlement information.
        /// </summary>
        public class GetUnifiedEntitlementInfoRequest : Request
        {
            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Unified entitlement label
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Unified entitlement.
            /// </summary>
            public UnifiedEntitlementInfo Entitlement { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestUnifiedEntitlementInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollUnifiedEntitlementInfo);

                nativeMethod.Writer.Write(RequestId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        // Read the data from the call
                        Entitlement = new UnifiedEntitlementInfo();
                        Entitlement.Deserialise(nativeMethod.Reader);

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Service entitlement.
        /// </summary>
        public class ServiceEntitlementInfo
        {
            /// <summary>
            /// Service entitlement label.
            /// </summary>
            public string EntitlementLabel { get; internal set; }

            /// <summary>
            /// End date
            /// </summary>
            public DateTime InactiveDate { get; internal set; }

            /// <summary>
            /// Start date.
            /// </summary>
            public DateTime ActiveDate { get; internal set; }

            /// <summary>
            /// Entitlement type.
            /// </summary>
            public EntitlementTypes EntitlementType { get; internal set; }

            /// <summary>
            /// Number of consumed entitlement counts.
            /// </summary>
            public Int32 UseCount { get; internal set; }

            /// <summary>
            /// Number of remaining entitlement counts that can be consumed.
            /// </summary>
            public Int32 UseLimit { get; internal set; }

            /// <summary>
            /// Active state.
            /// </summary>
            public bool IsActive { get; internal set; }

            /// <summary>
            /// Whether the service entitlement is consumable.
            /// </summary>
            public bool IsConsumable { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                InactiveDate = default;
                ActiveDate = default;

                bool isDateSet = reader.ReadBoolean();
                if (isDateSet)
                {
                    InactiveDate = reader.ReadRtcTicks();
                }

                isDateSet = reader.ReadBoolean();
                if (isDateSet)
                {
                    ActiveDate = reader.ReadRtcTicks();
                }

                EntitlementLabel = reader.ReadPrxString();
                EntitlementType = (EntitlementTypes)reader.ReadUInt32();
                UseCount = reader.ReadInt32();
                UseLimit = reader.ReadInt32();
                IsActive = reader.ReadBoolean();
                IsConsumable = reader.ReadBoolean();
            }
        }

        /// <summary>
        /// Requests to obtain a list of service entitlement information
        /// </summary>
        public class GetServiceEntitlementInfoListRequest : Request
        {
            /// <summary>
            /// Entitlement Info List Max
            /// </summary>
            public const UInt32 MaxEntitlementsLimit = 100;

            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Index (starting from 0) to a set of paged results. Responses start from the results at the specified offset.
            /// Result sets can be paged by using <see cref="Offset"/> and <see cref="Limit"/> in combination
            /// </summary>
            public Int32 Offset { get; set; }

            /// <summary>
            /// Maximum number of entitlements to retrieve in this request
            /// </summary>
            public UInt32 Limit { get; set; } = MaxEntitlementsLimit;

            /// <summary>
            /// Sort type
            /// </summary>
            public SortTypes Sort { get; set; } = SortTypes.None;

            /// <summary>
            /// Sort order
            /// </summary>
            public SortOrders SortDirection { get; set; }

            /// <summary>
            /// List of service entitlements.
            /// </summary>
            public ServiceEntitlementInfo[] Entitlements { get; internal set; }

            /// <summary>
            /// Offset value for obtaining the next paged set of entitlements passed to succeeding requests.
            /// </summary>
            public Int32 NextOffset { get; internal set; }

            /// <summary>
            /// Offset value for obtaining the previous paged set of entitlements passed to succeeding requests.
            /// </summary>
            public Int32 PreviousOffset { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestServiceEntitlementInfoList);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.Write(Offset);
                nativeMethod.Writer.Write(Limit);
                nativeMethod.Writer.Write((Int32)Sort);
                nativeMethod.Writer.Write((Int32)SortDirection);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollServiceEntitlementInfoList);

                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Writer.Write(Limit);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        NextOffset = nativeMethod.Reader.ReadInt32();
                        PreviousOffset = nativeMethod.Reader.ReadInt32();

                        // Read the data from the call
                        int numEntitlements = nativeMethod.Reader.ReadInt32();

                        if (numEntitlements == 0)
                        {
                            Entitlements = null;
                        }
                        else
                        {
                            Entitlements = new ServiceEntitlementInfo[numEntitlements];

                            for (int i = 0; i < numEntitlements; i++)
                            {
                                Entitlements[i] = new ServiceEntitlementInfo();

                                Entitlements[i].Deserialise(nativeMethod.Reader);
                            }
                        }

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Requests to obtain service entitlement information.
        /// </summary>
        public class GetServiceEntitlementInfoRequest : Request
        {
            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Service entitlement label
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Service entitlement.
            /// </summary>
            public ServiceEntitlementInfo Entitlement { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestServiceEntitlementInfo);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollServiceEntitlementInfo);

                nativeMethod.Writer.Write(RequestId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        // Read the data from the call
                        Entitlement = new ServiceEntitlementInfo();
                        Entitlement.Deserialise(nativeMethod.Reader);

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Generates a transaction ID
        /// </summary>
        public class GenerateTransactionIdRequest : Request
        {
            public string TransactionId { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GenerateTransactionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    TransactionId = nativeMethod.Reader.ReadPrxString();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Requests to consume a unified entitlement
        /// </summary>
        public class ConsumeUnifiedEntitlementRequest : Request
        {
            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Unified entitlement label
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Number of entitlement counts to consume
            /// </summary>
            public Int32 UseCount { get; set; }

            /// <summary>
            /// Transaction Id
            /// </summary>
            public string TransactionId { get; set; } = "";

            /// <summary>
            /// The number of remaining entitlement counts that can be consumed
            /// </summary>
            public Int32 UseLimit { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                if(string.IsNullOrEmpty(TransactionId))
                {
                    MarshalMethods.MethodHandle nativeTransactionMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GenerateTransactionId);

                    nativeTransactionMethod.Call();

                    Result = nativeTransactionMethod.callResult;

                    if (Result.apiResult == APIResultTypes.Success)
                    {
                        TransactionId = nativeTransactionMethod.Reader.ReadPrxString();
                    }

                    MarshalMethods.ReleaseHandle(nativeTransactionMethod);
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestConsumeUnifiedEntitlement);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.Write(UseCount);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);
                nativeMethod.Writer.WritePrxString(TransactionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollConsumeEntitlement);

                nativeMethod.Writer.Write(RequestId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        // Read the data from the call
                        UseLimit = nativeMethod.Reader.ReadInt32();

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Requests to consume a service entitlement
        /// </summary>
        public class ConsumeServiceEntitlementRequest : Request
        {
            /// <summary>
            /// User id
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Service entitlement label
            /// </summary>
            public string EntitlementLabel { get; set; }

            /// <summary>
            /// Number of entitlement counts to consume
            /// </summary>
            Int32 UseCount { get; set; }

            /// <summary>
            /// Transaction Id
            /// </summary>
            public string TransactionId { get; set; } = "";

            /// <summary>
            /// The number of remaining entitlement counts that can be consumed
            /// </summary>
            public Int32 UseLimit { get; internal set; }

            internal Int64 RequestId { get; set; } = 0;
            internal PollStatus Status { get; set; } = PollStatus.None;

            protected internal override void Run()
            {
                Status = PollStatus.None;

                if (string.IsNullOrEmpty(TransactionId))
                {
                    MarshalMethods.MethodHandle nativeTransactionMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GenerateTransactionId);

                    nativeTransactionMethod.Call();

                    Result = nativeTransactionMethod.callResult;

                    if (Result.apiResult == APIResultTypes.Success)
                    {
                        TransactionId = nativeTransactionMethod.Reader.ReadPrxString();
                    }

                    MarshalMethods.ReleaseHandle(nativeTransactionMethod);
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RequestConsumeServiceEntitlement);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.Write(UseCount);
                nativeMethod.Writer.WritePrxString(EntitlementLabel);
                nativeMethod.Writer.WritePrxString(TransactionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                RequestId = 0;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadInt64();
                    Status = PollStatus.Running;
                }
                else
                {
                    Status = PollStatus.Failed;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

            protected internal override bool HasUpdate()
            {
                return Status == PollStatus.Running;
            }

            // return true when finished
            protected internal override bool Update()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollConsumeEntitlement);

                nativeMethod.Writer.Write(RequestId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Polling worked but it may need to be called again.
                    bool isFinished = nativeMethod.Reader.ReadBoolean();

                    // If its finished then read the data from the call, otherwise
                    if (isFinished == true)
                    {
                        // Read the data from the call
                        UseLimit = nativeMethod.Reader.ReadInt32();

                        DeleteRequestId();
                        Status = PollStatus.Finished;
                    }
                    // Otherwise it has finished yet so this update method will need to be called again next frame.
                }
                else
                {
                    // polling failed
                    Status = PollStatus.Failed;
                    DeleteRequestId();
                    return true;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

                // Return true if finished
                return Status == PollStatus.Finished;
            }

            internal void DeleteRequestId()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteRequest);
                nativeMethod.Writer.Write(RequestId);
                nativeMethod.Call();
                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
#endif
