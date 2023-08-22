#if UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LostPolygon.AndroidBluetoothMultiplayer;

namespace Bluetooth.Android
{
    public class MultiplayerRT : RealtimeMultiplayerBaseProvider
    {
        public override bool IsAvailable => AndroidBluetoothMultiplayer.GetIsBluetoothAvailable();
        public override MultiplayerMode Mode => MultiplayerMode.ServerClient;

        private int numberOfPeers;
        HashSet<string> readyPlayers = new HashSet<string>();

        public override int MaxAllowedConnections => matchPlayerCount - 1;

        private const int matchPlayerCount = 2;
        BluetoothMultiplayerMode mode = BluetoothMultiplayerMode.None;
        public override bool IsHost => UnetNetworking.IsServer;

        public override void SetHost(object host)
        {
            if (mode != BluetoothMultiplayerMode.None)
                return;

            mode = host is bool value && value ? BluetoothMultiplayerMode.Server : BluetoothMultiplayerMode.Client;
        }

        private UnetTransmission _transmission;

        private static MessageHandler _messageHandler = new MessageHandler(255, new PlayerReady(), new ServerToClientMatchReady());
        internal override MessageHandler InternalMessageHandler => _messageHandler;

        NoVoiceChat voip = new NoVoiceChat();
        public override VoiceChatBaseProvider VoiceChat => voip;


        internal override void Init()
        {
            base.Init();

            AndroidBluetoothMultiplayer.SetVerboseLog(Debug.isDebugBuild);

            if (!IsAvailable)
                return;
            
            AndroidBluetoothMultiplayer.ClientConnected += PeerJoined;
            AndroidBluetoothMultiplayer.ClientDisconnected += PeerLeft;
            AndroidBluetoothMultiplayer.ListeningStopped += BluetoothListeningCanceledEvent;
            AndroidBluetoothMultiplayer.DisconnectedFromServer += BluetoothDisconnectedFromServerEvent;
            UnetNetworking.SynchCreatedEvent += RpcSynch_SynchCreatedEvent;
        }

        protected override void DoCleanUp()
        {
            ResetLocalData();

            AndroidBluetoothMultiplayer.ClientConnected -= PeerJoined;
            AndroidBluetoothMultiplayer.ClientDisconnected -= PeerLeft;
            AndroidBluetoothMultiplayer.ListeningStopped -= BluetoothListeningCanceledEvent;
            AndroidBluetoothMultiplayer.DisconnectedFromServer -= BluetoothDisconnectedFromServerEvent;
            UnetNetworking.SynchCreatedEvent -= RpcSynch_SynchCreatedEvent;
        }

        void ResetLocalData()
        {
            numberOfPeers = 0;
            readyPlayers.Clear();
            mode = BluetoothMultiplayerMode.None;
        }

        void RpcSynch_SynchCreatedEvent()
        {
            if(!IsHost)
            {
                string id = this.OwnParticipantId;
                this.LogInfo("sending PlayerReady with ID="+id);
                _transmission.SendInternalNetworkMessage(new PlayerReady(id));
            }
        }

        void BluetoothListeningCanceledEvent()
        {
            this.LogInfo("Bluetooth Listening Canceled");

            AndroidBluetoothMultiplayer.Stop();
            events.CallSessionLeft();
        }

        void BluetoothDisconnectedFromServerEvent(BluetoothDevice obj)
        {
            ResetLocalData();

            AndroidBluetoothMultiplayer.Stop();
            UnetNetworking.Disconnect();
            events.CallSessionLeft();
        }

        private static string _ownParticipantId = null;
        public override string OwnParticipantId => _ownParticipantId ??= Guid.NewGuid().ToString();

        public override int GetNumberOfParticipants(bool includeSelf)
        {
            int result = this.numberOfPeers;
            if (includeSelf)
            {
                result++;
            }
            return result;
        }

        public override IEnumerable<string> AllParticipantIds(bool includeSelf)
        {
            if (includeSelf)
            {
                if(string.IsNullOrEmpty(OwnParticipantId))
                    this.LogError("OwnParticipantId not set!");
                else
                    yield return OwnParticipantId;
            }

            foreach(string p in this.readyPlayers)
            {
                if (p == OwnParticipantId)
                    continue;

                if(string.IsNullOrEmpty(p))
                {
                    this.LogError("Participant ID not set...");
                    continue;
                }
                
                yield return p;
            }
        }

        public override void StartQuickMatch(int playerCount)
        {
            if (!IsAvailable)
            {
                return;
            }
            StartMatchMaking();
        }

        public override void ShowMatchMaker(int minPlayerCount, int maxPlayerCount)
        {
            this.LogWarn("ShowMatchMaker(): No match maker for Android Bluetooth available. Starting quick match instead...");

            StartQuickMatch(minPlayerCount);
        }

        void StartMatchMaking()
        {
            UnetNetworking.Disconnect(); // Just to be sure
            if(mode == BluetoothMultiplayerMode.Server)
            {
                this.LogInfo("Start Server...");
                readyPlayers.Add(OwnParticipantId);
                _transmission = UnetNetworking.StartHost();
            }
            else
            {
                this.LogInfo("Connect as Client...");
                _transmission = UnetNetworking.StartClient();
            }
        }

        private void PeerJoined(BluetoothDevice peer)
        {
            numberOfPeers++;
            int cnt = GetNumberOfParticipants(includeSelf:true);
            
            this.LogInfo($"peer joined: {peer.Name} ({cnt}/{matchPlayerCount}) with address={peer.Address}");
            if (cnt >= matchPlayerCount)
            {
                AndroidBluetoothMultiplayer.StopDiscovery();
            }
        }

        private void StartMatch()
        {
            if (UnetNetworking.IsServer)
            {
                // call match found event!
                IEnumerable<string> participants = AllParticipantIds(true);
                string[] pArray = participants.ToArray();
                foreach (string p in pArray)
                    this.LogInfo("PEER CHECK: " + p);

                _transmission.SendInternalNetworkMessage(new ServerToClientMatchReady(pArray));
                ConnectedToMatch();
            }
        }

        void PeerLeft(BluetoothDevice obj)
        {
            numberOfPeers--;
            string leavingParticipant = GetLeavingParticipant();
            events.CallPeerLeft(leavingParticipant);
        }

        private string GetLeavingParticipant()
        {
            if (_transmission.IsClientConnected())
            {
                // if we are still connected the non local participant left the game
                return GetNonLocalParticipantID(); 
            }
            return OwnParticipantId;
        }

        public string GetNonLocalParticipantID()
        {
            foreach (string player in this.readyPlayers)
            {
                if (!player.Equals(OwnParticipantId))
                {
                    return player;
                }
            }
            return OwnParticipantId; // this should not happen
        }

        public override void SendMessageToOthers(byte[] message, bool reliable)
        {
            if (!reliable)
                this.LogInfo("sending unreliable message not possible with android bluetooth. It will be sent reliably.");

            if (_transmission != null)
                _transmission.SendNetworkMessageToAll(message); // always reliable
            else
                this.LogError("SendMessageToAll(): Synch is null!");
        }

        public override void SendMessageToHost(byte[] message, bool reliable)
        {
            _transmission.SendNetworkMessageToServer(message);
        }

        public override void SendMessageToPeers(byte[] message, bool reliable, params string[] participantIds) { }

        protected override void DisconnectFromSession()
        {
            base.DisconnectFromSession();

            ResetLocalData();

            try
            {
                AndroidBluetoothMultiplayer.StopDiscovery();
                AndroidBluetoothMultiplayer.Stop();
                UnetNetworking.Disconnect();
            }
            catch (Exception ex)
            {
                this.LogError($"exception catched: {ex}");
            }
        }

        internal override void ProcessInternalMessage(IMessage msg)
        {
            InternalMessageID id = (InternalMessageID)msg.MessageTypeId;
            this.LogInfo($"processing message: {id}");

            switch(id)
            {
                case InternalMessageID.ServerToClientMatchReady:
                    {
                        ServerToClientMatchReady m = msg as ServerToClientMatchReady;
                        this.readyPlayers = m.Participants.ToHashSet();
                        Debug.Log($"ServerToClientMatchReady with peers={string.Join(",", m.Participants)}");

                        ConnectedToMatch();
                    }
                    break;

                case InternalMessageID.PlayerReady:
                    {
                        if(IsHost)
                        {
                            PlayerReady m = msg as PlayerReady;
                            readyPlayers.Add(m.SenderID);
                            
                            Debug.Log($"received ready player ={m.SenderID}");

                            int count = GetNumberOfParticipants(true);
                            if(readyPlayers.Count >= count)
                            {
                                StartMatch();
                            }
                        }
                    }
                    break;

                default:
                    this.LogError($"Unexpected Message: {id}");
                    break;
            }
        }
    }
}
#endif
