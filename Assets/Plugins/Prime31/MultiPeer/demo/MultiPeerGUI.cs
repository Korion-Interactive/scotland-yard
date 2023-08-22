using System.Collections.Generic;
using UnityEngine;


namespace Prime31
{
	public class MultiPeerGUI : MonoBehaviourGUI
	{
		string _lastReceivedMessage;

#if UNITY_IOS || UNITY_TVOS

		void OnEnable()
		{
			// listen to the receivedRawDataEvent so we know when we got a message
			MultiPeerManager.receivedRawDataEvent += multiPeerRawMessageReceiver;
		}


		void OnDisable()
		{
			MultiPeerManager.receivedRawDataEvent -= multiPeerRawMessageReceiver;
		}


		void OnGUI()
		{
			beginColumn();


			GUILayout.Label( "First we need to advertise ourself" );

			if( GUILayout.Button( "Advertise Device" ) )
			{
				MultiPeer.advertiseCurrentDevice( "prime31-MyGame" );
			}


			if( GUILayout.Button( "Advertise Device (no UI)" ) )
			{
				MultiPeer.advertiseCurrentDeviceWithNearbyServiceAdvertiser( "prime31Game" );
			}


			if( GUILayout.Button( "Stop Advertising" ) )
			{
				MultiPeer.stopAdvertisingPeer();
			}


			GUILayout.Label( "Then we can use Apple's built in UI on iOS" );

			if( GUILayout.Button( "Show Peer Picker" ) )
			{
				MultiPeer.showPeerPicker();
			}


			GUILayout.Label( "Or no UI at all" );

			if( GUILayout.Button( "Start Service Browser" ) )
			{
				MultiPeer.startNearbyServiceBrowser();
			}


			if( GUILayout.Button( "Stop Service Browser" ) )
			{
				MultiPeer.stopNearbyServiceBrowser();
			}


			GUILayout.Label( "Once connected, we can do more" );

			if( GUILayout.Button( "Get Connected Peers" ) )
			{
				var peers = MultiPeer.getConnectedPeers();
				Utils.logObject( peers );
			}


			if( GUILayout.Button( "Get Local PeerID" ) )
			{
				Debug.Log( "local peerID: " + MultiPeer.getLocalPeerId() );
			}


			if( GUILayout.Button( "Disconnect and End Session" ) )
			{
				MultiPeer.disconnectAndEndSession();
			}


			endColumn( true );


			GUILayout.Label( "Here are the different ways to send data" );

			if( GUILayout.Button( "Send Time to All Peers" ) )
			{
				var result = MultiPeer.sendMessageToAllPeers( "ui", "multiPeerMessageReceiver", Time.timeSinceLevelLoad.ToString() );
				Debug.Log( "send result: " + result );
			}


			if( GUILayout.Button( "Send Time to First Peer" ) )
			{
				var peers = MultiPeer.getConnectedPeers();
				if( peers.Count == 0 )
				{
					Debug.Log( "aborting send since there are no connected peers" );
					return;
				}

				var result = MultiPeer.sendMessageToPeers( new string[] { peers[0] }, "ui", "multiPeerMessageReceiver", Time.timeSinceLevelLoad.ToString() );
				Debug.Log( "send result: " + result );
			}


			if( GUILayout.Button( "Send Raw Message to All Peers" ) )
			{
				// we will just send some text across the wire encoded into a byte array for demonstration purposes
				var theStr = "im a string sent by MultiPeer magic";
				var bytes = System.Text.Encoding.UTF8.GetBytes( theStr );

				var result = MultiPeer.sendRawMessageToAllPeers( bytes );
				Debug.Log( "send result: " + result );
			}


			if( GUILayout.Button( "Send Raw Message to First Peers" ) )
			{
				var peers = MultiPeer.getConnectedPeers();
				if( peers.Count == 0 )
				{
					Debug.Log( "aborting send since there are no connected peers" );
					return;
				}

				// we will just send some text across the wire encoded into a byte array for demonstration purposes
				var theStr = "im a string sent by MultiPeer magic";
				var bytes = System.Text.Encoding.UTF8.GetBytes( theStr );

				var result = MultiPeer.sendRawMessageToPeers( new string[] { peers[0] }, bytes );
				Debug.Log( "send result: " + result );
			}

			if( _lastReceivedMessage != null )
			{
				GUILayout.Label( "Last received message:" );
				GUILayout.Label( _lastReceivedMessage );
			}

			endColumn( false );
		}


		#region Message receiver

		// this method will be called when we receive a non-raw message
		void multiPeerMessageReceiver( string message )
		{
			_lastReceivedMessage = message;
			Debug.Log( "received message: " + message );
		}


		void multiPeerRawMessageReceiver( string peerId, byte[] bytes )
		{
			_lastReceivedMessage = System.Text.Encoding.UTF8.GetString( bytes );
			Debug.Log( "received raw message from peer: " + peerId + ": " + _lastReceivedMessage );
		}

		#endregion

#endif
	}

}
