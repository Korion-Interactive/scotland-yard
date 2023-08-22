using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Prime31;

#if UNITY_IPHONE
namespace Bluetooth.iOS.Multipeer
{
    public class MultiplayerRT : RealtimeMultiplayerBaseProvider
    {
		public static readonly string ADVERTISE_NAME = "ScotlandYardHD".ToLower(); // name must be 1-15 characters long.
		static readonly bool ADVERTISE_WITH_SERVICE = false;

        public override bool IsAvailable { get { return true; } }

        public override int MaxAllowedConnections => 1;

        public override string OwnParticipantId { get { return MultiPeer.getLocalPeerId(); } }

        string hostId;
        public override bool IsHost { get { return hostId == OwnParticipantId; }}
        public override void SetHost(object host)
        {
            if (host is string)
			{
                hostId = (string)host;
			}
        }

        NoVoiceChat voip = new NoVoiceChat();
        public override VoiceChatBaseProvider VoiceChat { get { return voip; } }

        int expectedPlayerCount;

        MessageHandler msgHandler = new MessageHandler(253);
        internal override MessageHandler InternalMessageHandler { get { return msgHandler; } }

        ReliableNetworkLayer reliableLayer;

        public MultiplayerRT()
        {
            reliableLayer = new ReliableNetworkLayer(this);
        }

        internal override void Init()
        {
            base.Init();

            reliableLayer.Activate();

            MultiPeerManager.peerDidChangeStateToConnectedEvent += PeerConnectedEvent;
			MultiPeerManager.peerDidChangeStateToNotConnectedEvent += DisconnectNotify;
			MultiPeerManager.peerPickerFinishedEvent += BrowserFinishedEvent;
            MultiPeerManager.receivedRawDataEvent += ReceiveRawMessage;
        }

        protected override void DoCleanUp()
        {
            reliableLayer.Deactivate();

            MultiPeerManager.peerDidChangeStateToConnectedEvent -= PeerConnectedEvent;
            MultiPeerManager.peerDidChangeStateToNotConnectedEvent -= DisconnectNotify;
            MultiPeerManager.peerPickerFinishedEvent -= BrowserFinishedEvent;

            MultiPeerManager.receivedRawDataEvent -= ReceiveRawMessage;
        }

        private void PeerDisconnectDetected(string participantId)
        {
            this.LogDebug("PeerDisconnectedDetected() IsConnected: " + IsConnected);

            if (IsConnected)
			{
                events.CallPeerLeft(participantId);
			}
        }

		private void DisconnectNotify(string peerId)
        {
			this.LogWarn ("Disconnect ID: " + peerId);
            this.LogWarn("DisconnectNotify() IsConnected: " + IsConnected);

            if(IsConnected)
			{
			    events.CallPeerLeft(peerId);
			}

			MultiPeer.stopAdvertisingPeer();
		}

        public override IEnumerable<string> AllParticipantIds(bool includeSelf)
        {
            if (includeSelf)
			{
                yield return OwnParticipantId;
			}

            foreach(string p in MultiPeer.getConnectedPeers())
            {
                if (p == OwnParticipantId)
				{
                    continue;
				}
                yield return p;
            }
        }

        public override void StartQuickMatch(int playerCount)
        {
            expectedPlayerCount = playerCount;

			Debug.Log ("### StartQuickMatch ###");
			if (ADVERTISE_WITH_SERVICE)
			{
				MultiPeer.advertiseCurrentDeviceWithNearbyServiceAdvertiser(ADVERTISE_NAME);
			}
			else
			{
				MultiPeer.advertiseCurrentDevice(ADVERTISE_NAME); 
			}

			Debug.Log ("### Host: " + hostId);
			Debug.Log ("### Is " + OwnParticipantId + " a host: " + IsHost);

			// show iOS UI 
			const int numberOfPlayers = 2;
            MultiPeer.showPeerPicker(peerId:null, minimumNumberOfPeers:numberOfPlayers, maximumNumberOfPeers:numberOfPlayers);
        }

        public override void ShowMatchMaker(int minPlayerCount, int maxPlayerCount)
        {
			throw new NotImplementedException("ShowMatchMaker(): No match maker for iOS Bluetooth available. Use quick match instead!");
        }

        void PeerConnectedEvent(string obj)
        {
			int cnt = AllParticipantIds(true).Count();
			this.LogWarn("Peer connected. Count of all players: " + cnt);

			if(cnt >= expectedPlayerCount)
			{
				this.LogWarn ("Player: " + OwnParticipantId + " is connected and joins the match");
				ConnectedToMatch ();
			}
        }

        public override void SendMessageToOthers(byte[] message, bool reliable)
        {
			Debug.Log ("### Sending MESSAGE: " + message);
			string[] participantIds = AllParticipantIds (false).ToArray();

			if (participantIds.Any())
			{
				Debug.Log("to those participants: " + string.Join(",",participantIds));
			}
			else
			{
				Debug.Log ("!!! No participants found, except host! !!!");
			}
			MultiPeer.sendRawMessageToPeers(participantIds, message, reliable);
	    }

        public override void SendMessageToHost(byte[] message, bool reliable)
        {
			Debug.Log ("### HOSTID: " + hostId);
			MultiPeer.sendRawMessageToPeers(new string[] { hostId }, message, reliable);
        }

        public override void SendMessageToPeers(byte[] message, bool reliable, params string[] participantIds)
        {
            if(reliable)
            {
                this.LogError("SendMessageToPeers: TODO: integrate reliable layer!");
            }
            MultiPeer.sendRawMessageToPeers(participantIds, message, reliable);
        }

        protected override void DisconnectFromSession()
        {
            base.DisconnectFromSession();
            reliableLayer.Deactivate();
            
            MultiPeer.stopAdvertisingPeer();
            MultiPeer.disconnectAndEndSession();
        }

        void BrowserFinishedEvent(string state)
        {
			switch(state)
            {
			case "done":
                this.LogDebug("MultiPeer Browser: Done");
				break;

			case "cancelled":
				this.LogDebug("MultiPeer Browser: Cancelled");
                DisconnectFromSession();
                events.CallMatchMakerCancelled();
                break;

            default:
                this.LogError("unexpected browser finished event: " + state);
                break;
            }
        }

        private void ReceiveRawMessage(string peerId, byte[] msg)
        {
            events.ReceiveMessage(msg);
        }

        internal override void ProcessInternalMessage(IMessage msg)
        {
            base.ProcessInternalMessage(msg);
            reliableLayer.InternalMessageReceived(msg);
        }
    }
}
#endif
