using UnityEngine;
using System;
using System.Runtime.InteropServices;


#if UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX

namespace Prime31
{
	public class MultiPeerManager : AbstractManager
	{
		/// <summary>
		/// Fired when the peer picker finishes. Possible parameter values are "done" or "cancelled"
		/// </summary>
		public static event Action<string> peerPickerFinishedEvent;

		#region Nearby Service Browser Events

		/// <summary>
		/// Fired when the nearby service browser (MultiPeer.advertiseCurrentDeviceWithNearbyServiceAdvertiser method) finds a peer
		/// </summary>
		public static event Action<MultiPeerDiscoveryInfo> browserFoundPeerEvent;

		/// <summary>
		/// Fired when the nearby service browser (MultiPeer.advertiseCurrentDeviceWithNearbyServiceAdvertiser method) loses a peer
		/// </summary>
		public static event Action<string> browserLostPeerEvent;

		/// <summary>
		/// Fired when the nearby service browser (MultiPeer.advertiseCurrentDeviceWithNearbyServiceAdvertiser method) fails to start browsing for peers
		/// </summary>
		public static event Action<string> browserDidNotStartBrowsingForPeersEvent;

		#endregion


		#region Advertiser Events

		/// <summary>
		/// Fired when an invitation is received from a peer. The bool? returned will indicate if the invite should be accepted (true), rejected (false)
		/// or deferred (null). If the invite is deferred you MUST call MultiPeer.acceptDeferredInvitationFromPeer to avoid memory leaks! Note that if
		/// there is no event handler setup invites will always be accepted.
		/// </summary>
		public static event Func<string,bool?> advertiserReceivedInvitationEvent;

		/// <summary>
		/// Fired when the advertiser (MultiPeer.advertiseCurrentDevice method) fails to start advertising.
		/// </summary>
		public static event Action<string> advertiserDidNotStartAdvertisingPeerEvent;

		#endregion


		#region Session Events

		/// <summary>
		/// Fired when a peer connects
		/// </summary>
		public static event Action<string> peerDidChangeStateToConnectedEvent;

		/// <summary>
		/// Fired when a peer disconnects
		/// </summary>
		public static event Action<string> peerDidChangeStateToNotConnectedEvent;

		/// <summary>
		/// Fired when a raw message is received. Includes the peerId and the raw data
		/// </summary>
		public static event Action<string, byte[]> receivedRawDataEvent;

		#endregion


		#region Internal Setup

#if UNITY_EDITOR
		void OnApplicationQuit()
		{
			// in the editor we always want to disconnect and stop all advertising because native code still runs even when the editor
			// is paused
			MultiPeer.disconnectAndEndSession();
			MultiPeer.stopNearbyServiceBrowser();
			MultiPeer.stopAdvertisingPeer();
		}
#endif


		delegate void receivedDataCallback( string peerId, IntPtr dataBuf, int dataSize);

		[DllImport( MultiPeer.DLL_NAME )]
		static extern void _multiPeerSetReceivedDataCallback( receivedDataCallback callback );


		delegate void sendMessageCallback( string gameObjectName, string method, string param );

		[DllImport( MultiPeer.DLL_NAME)]
		static extern void _multiPeerSetSendMessageCallback( sendMessageCallback callback );


		delegate int invitationHandlerCallback( string peerId );

		[DllImport( MultiPeer.DLL_NAME )]
		static extern int _multiPeerSetInvitationHandlerCallback( invitationHandlerCallback callback );


		static MultiPeerManager()
		{
			initialize( typeof( MultiPeerManager ) );

			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
			{
				_multiPeerSetReceivedDataCallback( didReceivedData );
				_multiPeerSetInvitationHandlerCallback( onInvitationReceived );
			}

			// on macOS we need to mimic UnitySendMessage so we'll use this callback to do so
			if( Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerSetSendMessageCallback( receivedSendMessage );
		}


		[AOT.MonoPInvokeCallback(typeof( receivedDataCallback ) )]
		static void didReceivedData( string peerId, IntPtr dataBuf, int dataSize )
		{
			var data = new byte[dataSize];
			Marshal.Copy( dataBuf, data, 0, dataSize );

			if( receivedRawDataEvent != null )
				receivedRawDataEvent( peerId, data );
		}


		[AOT.MonoPInvokeCallback( typeof( sendMessageCallback ) )]
		static void receivedSendMessage( string gameObjectName, string method, string param )
		{
			if( gameObjectName == "Debug.Log" )
			{
				Debug.Log( method );
				Debug.Log( param );
				return;
			}


			var go = GameObject.Find( gameObjectName );
			if( go == null )
			{
				#if !UNITY_EDITOR
				// we are going to get errors in the editor when pressing stop so no need to spam the console
				Debug.LogError( "could not find GameObject: " + gameObjectName );
				#endif
				return;
			}

			go.SendMessage( method, param );
		}

		[AOT.MonoPInvokeCallback( typeof( invitationHandlerCallback ) )]
		static int onInvitationReceived( string peerId )
		{
			if( advertiserReceivedInvitationEvent != null )
			{
				var val = advertiserReceivedInvitationEvent( peerId );
				if( val.HasValue )
					return val.Value ? 1 : -1;
				return 0;
			}

			return 1;
		}

		#endregion


		void peerPickerFinished( string param )
		{
			peerPickerFinishedEvent.fire( param );
		}


		void browserFoundPeer( string json )
		{
			var info = Json.decode<MultiPeerDiscoveryInfo>( json );

			if( MultiPeer.shouldAutoInviteFoundPeers )
			{
				Debug.Log( "shouldAutoInviteFoundPeers is true so auto inviting the found peer now" );
				MultiPeer.invitePeer( info.peerId );
			}
			
			browserFoundPeerEvent.fire( info );
		}


		void browserLostPeer( string peerId )
		{
			browserLostPeerEvent.fire( peerId );
		}


		void browserDidNotStartBrowsingForPeers( string error )
		{
			browserDidNotStartBrowsingForPeersEvent.fire( error );
		}


		void advertiserDidNotStartAdvertisingPeer( string error )
		{
			advertiserDidNotStartAdvertisingPeerEvent.fire( error );
		}


		void peerDidChangeStateToConnected( string param )
		{
			peerDidChangeStateToConnectedEvent.fire( param );
		}


		void peerDidChangeStateToNotConnected( string param )
		{
			peerDidChangeStateToNotConnectedEvent.fire( param );
		}

	}

}
#endif
