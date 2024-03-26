
using System;
using System.IO;
using System.Threading;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Requests for Sockets
    /// </summary>
    public class Sockets
    {
        internal enum NativeMethods : UInt32
        {
            SetupUdpP2PSocket = 0x1300001u,
            TerminateSocket = 0x1300002u,
            SendTo = 0x1300003u,
            RecvThreadUpdate = 0x1300004u,
        }

        internal static void Start()
        {
            recvThread = new RecvSocketThread();
            recvThread.Start("SocketRecv");

            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            Main.OnSystemUpdate -= Update;
            recvThread.Stop();
        }

        internal class SocketThread
        {
            Thread workerThread;
            bool stopThread = false;

            EventWaitHandle pollingEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

            public void Start(string name)
            {
                stopThread = false;
                workerThread = new Thread(new ThreadStart(RunProc));
                workerThread.Name = name;
                workerThread.Start();
            }

            private void RunProc()
            {
                while (!stopThread)
                {
                    pollingEvent.WaitOne();

                    try
                    {
                        if (!stopThread)
                        {
                            Execute();
                        }
                    }
#pragma warning disable CS0168
                    catch (Exception e)
                    {
#if DEBUG
                        UnityEngine.Debug.LogError("SocketThread Exception : " + e.Message + "\n" + e.StackTrace);
#endif
                    }
#pragma warning restore CS0168
                }
            }

            internal virtual void Execute()
            {

            }

            public void Stop()
            {
                stopThread = true;
                pollingEvent.Set();
            }

            public void ReleaseEvent()
            {
                pollingEvent.Set();
            }

        }

        /// <summary>
        /// Delegate for receive socket.
        /// </summary>
        /// <param name="reqEvent">The event data</param>
        public delegate void RecvSocketNotification(byte[] ReceiveBuffer, UInt32 dataLen, UInt32 FromAddr, UInt16 FromPort, UInt16 FromVirtualPort );

        /// <summary>
        /// Event called when data is received by a socket
        /// </summary>
        public static event RecvSocketNotification OnRecvSocketNotification;

        public static void SetNetReceiveData(Int32 recvSocketId, byte[] receiveBuffer )
        {
            recvThread.SocketId = recvSocketId;
            recvThread.ReceiveBuffer = receiveBuffer;
        }

        internal class RecvSocketThread : SocketThread
        {
            public Int32 SocketId { get; set; } = -1;

            public byte[] ReceiveBuffer { get; set; }

            internal override void Execute()
            {
                if (SocketId < 0 || ReceiveBuffer == null)
                {
                    return;
                }

                while (true) // Allowing for the possibility of >1 packets arriving per call to this method...
                {
                    using (MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.RecvThreadUpdate))
                    {
                        nativeMethod.Writer.Write(SocketId);
                        nativeMethod.Writer.Write(ReceiveBuffer.Length);
                        nativeMethod.Call();

                        bool ok = nativeMethod.callResult.apiResult == APIResultTypes.Success;
                        if (!ok)
                        {
                            break; // This includes SCE_NET_ERROR_EAGAIN / SCE_NET_ERROR_WOULDBLOCK when there is no received packet.
                        }

                        bool hasData = nativeMethod.Reader.ReadBoolean();
                        if (!hasData)
                        {
                            continue; // A packet with zero bytes of data?  It's possible but unlikely.  (See Sockets::RecvThreadUpdateImpl)
                        }

                        UInt32 len = nativeMethod.Reader.ReadData(ReceiveBuffer);
                        UInt32 fromAddr = nativeMethod.Reader.ReadUInt32();
                        UInt16 fromPort = nativeMethod.Reader.ReadUInt16();
                        UInt16 fromVirtualPort = nativeMethod.Reader.ReadUInt16();

                        OnRecvSocketNotification?.Invoke(ReceiveBuffer, len, fromAddr, fromPort, fromVirtualPort);
                    } // Implicit MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }
        }

        static RecvSocketThread recvThread;

        private static void Update()
        {
            recvThread.ReleaseEvent();
        }

        /// <summary>
        ///
        /// </summary>
        public class SetupUdpP2PSocketRequest : Request
        {
            /// <summary>
            /// Socket Name
            /// </summary>
            public string SocketName { get; set; }

            /// <summary>
            /// IP address. Use 0 if no IP address is required.
            /// </summary>
            public UInt32 NetAddress { get; set; } = 0;

            /// <summary>
            /// The virtual port number
            /// </summary>
            public UInt16 VirtualPort { get; set; }

            /// <summary>
            /// The created socket
            /// </summary>
            public Int32 SocketId { get; internal set; } = -1;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.SetupUdpP2PSocket);

                nativeMethod.Writer.WritePrxString(SocketName);
                nativeMethod.Writer.Write(VirtualPort);
                nativeMethod.Writer.Write(NetAddress);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    SocketId = nativeMethod.Reader.ReadInt32();
                }
                else
                {
                    SocketId = -1;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class TerminateSocketRequest : Request
        {
            /// <summary>
            /// The socket to terminate
            /// </summary>
            public Int32 SocketId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.TerminateSocket);

                nativeMethod.Writer.Write(SocketId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class SendRequest : Request
        {
            /// <summary>
            /// Socket Id
            /// </summary>
            public Int32 SocketId { get; set; }

            /// <summary>
            /// Send to IP address
            /// </summary>
            public UInt32 SendToAddress { get; set; }

            /// <summary>
            /// Send to IP address
            /// </summary>
            public UInt16 SendToPort { get; set; }

            /// <summary>
            /// The virtual port number
            /// </summary>
            public UInt16 ReceiveVirtualPort { get; set; }

            /// <summary>
            /// Encrypt the data over the UDPP2P socket
            /// </summary>
            public bool Encrypt { get; set; }

            /// <summary>
            /// The data to send
            /// </summary>
            public byte[] Data { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.SendTo);

                nativeMethod.Writer.Write(SocketId);

                nativeMethod.Writer.Write(Data.Length);
                nativeMethod.Writer.Write(Data);

                nativeMethod.Writer.Write(ReceiveVirtualPort);

                nativeMethod.Writer.Write(SendToAddress);
                nativeMethod.Writer.Write(SendToPort);

                nativeMethod.Writer.Write(Encrypt);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
