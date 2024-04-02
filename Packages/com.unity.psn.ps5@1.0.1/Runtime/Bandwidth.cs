
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Bandwidth
{
    /// <summary>
    /// The BandwidthTest system provides the application with a function for measuring the communication bandwidth between the client and the PlayStation™Network server.
    /// </summary>
    public class BandwidthTest
    {
        enum NativeMethods : UInt32
        {
            StartMeasurement = 0x1700001u,
            PollMeasurement = 0x1700002u,
            AbortMeasurement = 0x1700003u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("Bandwidth");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Bandwidth queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Measurement status
        /// </summary>
        public enum Status
        {
            /// <summary> Measurement not started yet </summary>
            None = 0,
            /// <summary> Measuring </summary>
            Running = 1,
            /// <summary> Measurement not started yet </summary>
            Finised = 2,
        }

        public enum Modes
        {
            /// <summary> Get the upload bits per second </summary>
            CalcUploadBps = 1,                          // bit 1
            /// <summary> Get the download bits per second </summary>
            CalcDownloadBps = 2,                   // bit 2
        }

        /// <summary>
        /// Run measure badwitdh test
        /// </summary>
        public class MeasureBandwidthRequest : Request
        {
         
            public Modes Mode { get; set; } = Modes.CalcDownloadBps;

            /// <summary>
            /// Specify a timeout time other than 0 in miliseconds in order to set a timeout time for bandwidth measurement.
            /// Defaults to 0.
            /// </summary>
            public Int32 TimeoutMs { get; set; } = 0;

            /// <summary>
            /// Set the affinity mask. When 0 is specified, the system will set the affinity mask.
            /// By default affinity mask won't use Cores 0 or 1.
            /// </summary>
            public UInt64 CpuAffinityMask { get; set; }

            /// <summary>
            /// The measurement status
            /// This is updated every frame while the measurement is running.
            /// When the status is 
            /// </summary>
            public Status Status { get; internal set; } = Status.None;

            /// <summary>
            /// The Bits per second, either for upload or download, depending on the <see cref="Mode"/>
            /// </summary>
            public double Bps { get; internal set; } = 0;

            internal int CtxId { get; set; } = -1;

            internal Modes InternalMode { get; set; }

            /// <summary>
            /// Create request with default properties
            /// </summary>
            public MeasureBandwidthRequest()
            {
#if UNITY_PS5
                CpuAffinityMask = 0x1FC; // 1111111111100 - Don't use cores 0 and 1
#else // PS4
                CpuAffinityMask = 0x3C; // 111100 - Don't use cores 0 and 1
#endif
            }

            protected internal override void Run()
            {
                CtxId = -1;

                InternalMode = Mode;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.StartMeasurement);

                nativeMethod.Writer.Write((UInt32)Mode);
                nativeMethod.Writer.Write(TimeoutMs);
                nativeMethod.Writer.Write(CpuAffinityMask);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    CtxId = nativeMethod.Reader.ReadInt32();
                    Status = (Status)nativeMethod.Reader.ReadInt32();
                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == Status.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PollMeasurement);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = (Status)nativeMethod.Reader.ReadInt32();

                    if(Status == Status.Finised)
                    {
                        double uploadBps = nativeMethod.Reader.ReadDouble();
                        double downloadBps = nativeMethod.Reader.ReadDouble();

                        if(InternalMode == Modes.CalcUploadBps)
                        {
                            Bps = uploadBps;
                        }
                        else if (InternalMode == Modes.CalcDownloadBps)
                        {
                            Bps = downloadBps;
                        }

                        int ret = nativeMethod.Reader.ReadInt32();
                    }
                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                    Status = Status.Finised;
                }

                return Status == Status.Finised;
            }
        }

    }
}
