
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Requests for Session Signalling
    /// </summary>
    public class SessionSignalling
    {
        public static readonly UInt32 InvalidId = 0;   // Same as SCE_NP_SESSION_SIGNALING_INVALID_ID

        internal enum NativeMethods : UInt32
        {
            FetchSignallingEvent = 0x1200001u,
            UserToUserSignalling = 0x1200002u,
            ActivateUser = 0x1200003u,
            Deactivate = 0x1200004u,
            GetLocalNetInfo = 0x1200005u,
            GetConnectionStatus = 0x1200006u,
            GetConnectionInfo = 0x1200007u,
            CreateUserContext = 0x1200008u,
            DestroyUserContext = 0x1200009u,
            GetNatRouterInfo = 0x120000au,
            ActivateSession = 0x1200010u,
        }

        internal static void Start()
        {
            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            Main.OnSystemUpdate -= Update;
        }

        /// <summary>
        /// Delegate for notifications about request events.
        /// </summary>
        /// <param name="reqEvent">The event data</param>
        public delegate void RequestNotification(RequestEvent reqEvent);

        /// <summary>
        /// Event called when a request event occurs.
        /// </summary>
        public static event RequestNotification OnRequestNotification;

        /// <summary>
        /// Delegate for notifications about group events.
        /// </summary>
        /// <param name="groupEvent">The event data</param>
        public delegate void GroupNotification(GroupEvent groupEvent);

        /// <summary>
        /// Event called when a group event occurs.
        /// </summary>
        public static event GroupNotification OnGroupNotification;

        /// <summary>
        /// Delegate for notifications about connection events.
        /// </summary>
        /// <param name="connEvent">The event data</param>
        public delegate void ConnectionNotification(ConnectionEvent connEvent);

        /// <summary>
        /// Event called when a connection event occurs.
        /// </summary>
        public static event ConnectionNotification OnConnectionNotification;

        public class SignallingEvent
        {
            public Int32 UserId { get; internal set; }

            public UInt32 CtxId { get; internal set; } = InvalidId;

            public Int32 ErrorCode { get; internal set; } = 0;

            public UInt32 Id { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                UserId = reader.ReadInt32();
                CtxId = reader.ReadUInt32();
                ErrorCode = reader.ReadInt32();

                Id = reader.ReadUInt32();
            }
        }

        public class RequestEvent : SignallingEvent
        {
            public enum Reasons
            {
                Unknown = -1,
                Prepare = 0    // SCE_NP_SESSION_SIGNALING_REQUEST_EVENT_PREPARE
            }

            public Reasons Reason { get; internal set; } = Reasons.Unknown;

            internal void DeserialiseReason(BinaryReader reader)
            {
                Reason = (Reasons)reader.ReadInt32();
            }
        }

        public class GroupEvent : SignallingEvent
        {
            public enum Reasons
            {
                Unknown = -1,
                Activate = 0,    // SCE_NP_SESSION_SIGNALING_GROUP_EVENT_ACTIVATED
                ActivateError = 1,    //  SCE_NP_SESSION_SIGNALING_GROUP_EVENT_ACTIVATE_ERROR
                PeerActivated = 2, // SCE_NP_SESSION_SIGNALING_GROUP_EVENT_PEER_ACTIVATED
            }

            /// <summary>
            ///
            /// </summary>
            public Reasons Reason { get; internal set; } = Reasons.Unknown;

            /// <summary>
            ///
            /// </summary>
            public PeerAddress PeerActivatedData { get; internal set; }

            internal void DeserialiseReason(BinaryReader reader)
            {
                Reason = (Reasons)reader.ReadInt32();

                if (Reason == Reasons.PeerActivated && ErrorCode == 0)
                {
                    PeerActivatedData = new PeerAddress();
                    PeerActivatedData.AccountId = reader.ReadUInt64();
                    PeerActivatedData.Platform = (NpPlatformType)reader.ReadInt32();
                }
            }

        }

        public class ConnectionEvent : SignallingEvent
        {
            public enum Reasons
            {
                Unknown = -1,
                Dead = 0,    // SCE_NP_SESSION_SIGNALING_CONNECTION_EVENT_DEAD
                Established = 1,    //  SCE_NP_SESSION_SIGNALING_CONNECTION_EVENT_ESTABLISHED
                Activated = 2,    // SCE_NP_SESSION_SIGNALING_CONNECTION_EVENT_ACTIVATED
            }

            public Reasons Reason { get; internal set; } = Reasons.Unknown;

            public UInt32 GroupId { get; private set; }

            internal void DeserialiseReason(BinaryReader reader)
            {
                Reason = (Reasons)reader.ReadInt32();
            }

            internal void DeserializeExtras (BinaryReader reader)
            {
                GroupId = reader.ReadUInt32();
            }
        }

        internal enum EventType
        {
            Request = 0,
            Group = 1,
            Connection = 2
        };

        internal static SignallingEvent FetchNext()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchSignallingEvent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            if (nativeMethod.ResultsSize == 0)
            {
                return null;
            }


            EventType type = (EventType)nativeMethod.Reader.ReadInt32();

            // Work out the type of event first
            if(type == EventType.Request)
            {
                RequestEvent reqEvent = new RequestEvent();

                reqEvent.Deserialise(nativeMethod.Reader);
                reqEvent.DeserialiseReason(nativeMethod.Reader);

                return reqEvent;
            }
            else if (type == EventType.Group)
            {
                GroupEvent grpEvent = new GroupEvent();

                grpEvent.Deserialise(nativeMethod.Reader);
                grpEvent.DeserialiseReason(nativeMethod.Reader);

                return grpEvent;
            }
            else if (type == EventType.Connection)
            {
                ConnectionEvent connEvent = new ConnectionEvent();

                connEvent.Deserialise(nativeMethod.Reader);
                connEvent.DeserialiseReason(nativeMethod.Reader);
                connEvent.DeserializeExtras(nativeMethod.Reader);

                return connEvent;
            }

            return null;
        }

        private static void Update()
        {
            SignallingEvent se = FetchNext();

            if (se != null)
            {
                if(se is RequestEvent)
                {
                    if (OnRequestNotification != null)
                    {
                        OnRequestNotification(se as RequestEvent);
                    }
                }
                else if (se is GroupEvent)
                {
                    if (OnGroupNotification != null)
                    {
                        OnGroupNotification(se as GroupEvent);
                    }
                }
                else if (se is ConnectionEvent)
                {
                    if (OnConnectionNotification != null)
                    {
                        OnConnectionNotification(se as ConnectionEvent);
                    }
                }
            }
        }


        /// <summary>
        /// Peer address of the user
        /// </summary>
        public class PeerAddress
        {
            /// <summary>
            /// The account id of the user
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// The platform for the user
            /// </summary>
            public NpPlatformType Platform { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class CreateUserContextRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Created Context ID
            /// </summary>
            public UInt32 CtxId { get; internal set; } = InvalidId;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.CreateUserContext);

                nativeMethod.Writer.Write(UserId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    CtxId = nativeMethod.Reader.ReadUInt32();
                }
                else
                {
                    CtxId = InvalidId;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class DestroyUserContextRequest : Request
        {
            /// <summary>
            /// Context Id to destroy
            /// </summary>
            public UInt32 CtxId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.DestroyUserContext);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        ///
        /// </summary>
        public class UserToUserRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The users request id
            /// </summary>
            public UInt32 RequestId { get; internal set; } = InvalidId;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.UserToUserSignalling);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    RequestId = nativeMethod.Reader.ReadUInt32();
                }
                else
                {
                    RequestId = 0;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class ActivateUserRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// Peer address to make a connection
            /// </summary>
            public PeerAddress PeerAddr { get; set; }

            /// <summary>
            /// The P2P groud id
            /// </summary>
            public UInt32 GroupId { get; internal set; } = InvalidId;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.ActivateUser);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Writer.Write(PeerAddr.AccountId);
                nativeMethod.Writer.Write((Int32)PeerAddr.Platform);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    GroupId = nativeMethod.Reader.ReadUInt32();
                }
                else
                {
                    GroupId = 0;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        public enum SignalingSessionTypes
        {
            PlayerSession = 0, // SCE_NP_SESSION_SIGNALING_SESSION_TYPE_PLAYER_SESSION,
            GameSession = 1, // SCE_NP_SESSION_SIGNALING_SESSION_TYPE_GAME_SESSION
        }

        public enum SignalingTopologyTypes
        {
            Mesh = 1, // SCE_NP_SESSION_SIGNALING_TOPOLOGY_TYPE_MESH
            Star = 2  // SCE_NP_SESSION_SIGNALING_TOPOLOGY_TYPE_STAR
        }

        public enum SignalingHostTypes
        {
            None = 0, // SCE_NP_SESSION_SIGNALING_HOST_TYPE_NONE
            Auto = 1, // SCE_NP_SESSION_SIGNALING_HOST_TYPE_AUTO
            Me = 2 // SCE_NP_SESSION_SIGNALING_HOST_TYPE_ME
        }

        /// <summary>
        ///
        /// </summary>
        public class ActivateSessionRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The Player or Game session ID
            /// </summary>
            public string SessionID { get; set; }

            /// <summary>
            /// Session type (Player Session/Game Session)
            /// </summary>
            public SignalingSessionTypes SessionType { get; set; }

            /// <summary>
            /// Topology type
            /// </summary>
            public SignalingTopologyTypes TopologyType { get; set; }

            /// <summary>
            /// Specify the host type based on the topology type.
            /// If the topology type is <see cref="SignalingTopologyTypes.Mesh"/>, specify <see cref="SignalingHostTypes.None"/>
            /// If the topology type is <see cref="SignalingTopologyTypes.Star"/>, specify <see cref="SignalingHostTypes.Auto"/> or <see cref="SignalingHostTypes.Me"/>
            /// </summary>
            public SignalingHostTypes HostType { get; set; }

            /// <summary>
            /// The P2P groud id
            /// </summary>
            public UInt32 GroupId { get; internal set; } = InvalidId;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.ActivateSession);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Writer.WritePrxString(SessionID);
                nativeMethod.Writer.Write((Int32)SessionType);
                nativeMethod.Writer.Write((Int32)TopologyType);
                nativeMethod.Writer.Write((Int32)HostType);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    GroupId = nativeMethod.Reader.ReadUInt32();
                    TopologyType = (SignalingTopologyTypes)nativeMethod.Reader.ReadInt32();
                    HostType = (SignalingHostTypes)nativeMethod.Reader.ReadInt32();
                }
                else
                {
                    GroupId = 0;
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class DeactivateRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The P2P groud id
            /// </summary>
            public UInt32 GroupId { get; set; } = InvalidId;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.Deactivate);

                nativeMethod.Writer.Write(CtxId);
                nativeMethod.Writer.Write(GroupId);

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
        public class NetInfo
        {
            public UInt32 LocalAddr { get; internal set; }

            public UInt32 MappedAddr { get; internal set; }

            public Int32 NatStatus { get; internal set; }

            public string LocalAddrStr { get; internal set; }

            public string MappedAddrStr { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                LocalAddr = reader.ReadUInt32();
                MappedAddr = reader.ReadUInt32();
                NatStatus = reader.ReadInt32();
                LocalAddrStr = reader.ReadPrxString();
                MappedAddrStr = reader.ReadPrxString();
            }
        }

        public enum NatInfoStunStatus
        {
            Unchecked = 0, // SCE_NET_CTL_NATINFO_STUN_UNCHECKED
            Failed = 1, // SCE_NET_CTL_NATINFO_STUN_FAILED
            Ok = 2 // SCE_NET_CTL_NATINFO_STUN_OK
        }

        /// <summary>
        ///
        /// </summary>
        public class NatRouterInfo
        {
            public NatInfoStunStatus StunStatus { get; internal set; }

            public Int32 NatType { get; internal set; }

            public UInt32 MappedAddr { get; internal set; }
            public string MappedAddrStr { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                StunStatus = (NatInfoStunStatus)reader.ReadInt32();
                NatType = reader.ReadInt32();
                MappedAddr = reader.ReadUInt32();
                MappedAddrStr = reader.ReadPrxString();
            }
        }
        /// <summary>
        ///
        /// </summary>
        public class GetLocalNetInfoRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The P2P groud id
            /// </summary>
            public NetInfo LocalNetInfo { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.GetLocalNetInfo);

                nativeMethod.Writer.Write(CtxId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    LocalNetInfo = new NetInfo();

                    LocalNetInfo.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtain NAT router information
        /// </summary>
        public class GetNatRouterInfoRequest : Request
        {
            /// <summary>
            /// NAT router information
            /// </summary>
            public NatRouterInfo NatRouterInfo { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.GetNatRouterInfo);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    NatRouterInfo = new NatRouterInfo();
                    NatRouterInfo.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        ///
        /// </summary>
        public class ConnectionStatus
        {
            public enum Statuses
            {
                Inactive,
                Pending,
                Active
            }

            public Statuses Status { get; internal set; }

            public UInt32 PeerAddr { get; internal set; }

            public string PeerAddrStr { get; internal set; }

            public UInt16 Port { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                Status = (Statuses)reader.ReadInt32();
                PeerAddr = reader.ReadUInt32();
                PeerAddrStr = reader.ReadPrxString();
                Port = reader.ReadUInt16();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class GetConnectionStatusRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The connection id
            /// </summary>
            public UInt32 ConnectionId { get; set; }

            /// <summary>
            /// Connection Status
            /// </summary>
            public ConnectionStatus Status { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.GetConnectionStatus);

                nativeMethod.Writer.Write(CtxId);
                nativeMethod.Writer.Write(ConnectionId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Status = new ConnectionStatus();

                    Status.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Information codes for connection targeted for obtainment
        /// </summary>
        public enum ConnectionInfoCodes
        {
            /// <summary> Round-trip time (microseconds). Measurement is made upon the exchange of the keep-alive packet, which takes place every 10 seconds. The average of the last 6 measurements is returned. </summary>
            RoundTripTime = 1,
            /// <summary> IP address and port number of the communication peer. This is the IP address and port number of the connected peer.  </summary>
            NetAddress = 4,
            /// <summary> Application's own IP address and port number as seen from the communication peer. This is your own IP address and port number as seen from your connected peer. </summary>
            MappedAddress = 5,
            /// <summary> Packet loss percentage. This is the packet loss percentage when making a round-trip of a UDPP2P packet to a connected peer and back. Measurement is made upon the exchange of the keep-alive packet, which takes place every 10 seconds. The value of the last 6 measurements is returned. </summary>
            PacketLoss = 6,
            /// <summary> This is the peer address of the connected peer (account ID and platform type). </summary>
            PeerAddress = 7,
        }

        /// <summary>
        ///
        /// </summary>
        public class ConnectionInfo
        {
            /// <summary>
            /// The type of connection info retireved from <see cref="GetConnectionInfoRequest"/>
            /// </summary>
            public ConnectionInfoCodes InfoCode { get; internal set; }

            /// <summary>
            /// Round-trip time (microseconds). Only valid is <see cref="InfoCode"/> is <see cref="ConnectionInfoCodes.RoundTripTime"/>
            /// </summary>
            public UInt32 RoundTripTime { get; internal set; }

            /// <summary>
            /// Peer address of the communication peer. Only valid is <see cref="InfoCode"/> is <see cref="ConnectionInfoCodes.PeerAddress"/>
            /// </summary>
            public PeerAddress PeerAddress { get; internal set; }

            /// <summary>
            /// IP address of the peer or mapped address. Only valid is <see cref="InfoCode"/> is either <see cref="ConnectionInfoCodes.NetAddress"/> or <see cref="ConnectionInfoCodes.MappedAddress"/>
            /// </summary>
            public UInt32 NetAddress { get; internal set; }

            /// <summary>
            /// IP address string of the peer or mapped address. Only valid is <see cref="InfoCode"/> is either <see cref="ConnectionInfoCodes.NetAddress"/> or <see cref="ConnectionInfoCodes.MappedAddress"/>
            /// </summary>
            public string NetAddressStr { get; internal set; }

            /// <summary>
            /// Port number of the peer or mapped address. Only valid is <see cref="InfoCode"/> is either <see cref="ConnectionInfoCodes.NetAddress"/> or <see cref="ConnectionInfoCodes.MappedAddress"/>
            /// </summary>
            public UInt16 Port { get; internal set; }

            /// <summary>
            /// Packet loss percentage. Only valid is <see cref="InfoCode"/> is <see cref="ConnectionInfoCodes.PacketLoss"/>
            /// </summary>
            public UInt32 PacketLoss { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                InfoCode = (ConnectionInfoCodes)reader.ReadInt32();

                if(InfoCode == ConnectionInfoCodes.RoundTripTime)
                {
                    RoundTripTime = reader.ReadUInt32();
                }
                else if (InfoCode == ConnectionInfoCodes.PeerAddress)
                {
                    PeerAddress = new PeerAddress();
                    PeerAddress.AccountId = reader.ReadUInt64();
                    PeerAddress.Platform = (NpPlatformType)reader.ReadInt32();
                }
                else if (InfoCode == ConnectionInfoCodes.NetAddress || InfoCode == ConnectionInfoCodes.MappedAddress)
                {
                    NetAddress = reader.ReadUInt32();
                    NetAddressStr = reader.ReadPrxString();
                    Port = reader.ReadUInt16();
                }
                else if (InfoCode == ConnectionInfoCodes.PacketLoss)
                {
                    PacketLoss = reader.ReadUInt32();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class GetConnectionInfoRequest : Request
        {
            /// <summary>
            /// Context Id
            /// </summary>
            public UInt32 CtxId { get; set; }

            /// <summary>
            /// The connection id
            /// </summary>
            public UInt32 ConnectionId { get; set; }

            /// <summary>
            /// Information codes for connection targeted for obtainment
            /// </summary>
            public ConnectionInfoCodes InfoCode { get; set; }

            /// <summary>
            /// Connection Info
            /// </summary>
            public ConnectionInfo Info { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((uint)NativeMethods.GetConnectionInfo);

                nativeMethod.Writer.Write(CtxId);
                nativeMethod.Writer.Write(ConnectionId);
                nativeMethod.Writer.Write((Int32)InfoCode);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Info = new ConnectionInfo();

                    Info.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
