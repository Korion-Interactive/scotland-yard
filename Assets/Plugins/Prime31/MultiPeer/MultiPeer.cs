using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;



#if UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX

namespace Prime31
{
	public class MultiPeer
	{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR
		public const string DLL_NAME = "P31MultiPeerMac";
#else
		public const string DLL_NAME = "__Internal";
#endif

		/// <summary>
		/// When advertising with the nearby service advertiser this bool determines if a invitePeer should be called automatically
		/// for all found peers.
		/// </summary>
		public static bool shouldAutoInviteFoundPeers = true;


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerAdvertiseCurrentDevice( string serviceType, string peerId, string discoveryInfo );

		/// <summary>
		/// Starts advertising the current device. This method will require the user to accept the connection via an alert.
		/// Note that serviceType must be 1–15 characters long and can contain only ASCII lowercase letters, numbers, and hyphens.
		/// </summary>
		public static void advertiseCurrentDevice( string serviceType, string peerId = null, Dictionary<string,string> discoveryInfo = null )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerAdvertiseCurrentDevice( serviceType, peerId ?? SystemInfo.deviceName, Json.encode( discoveryInfo ) );
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerAdvertiseCurrentDeviceWithNearbyServiceAdvertiser( string serviceType, string peerId, string discoveryInfo );

		/// <summary>
		/// Starts advertising the current device. This method will auto-connect all peers as they are found if shouldAutoInviteFoundPeers is true.
		/// If it is not true, you will be required to call invitePeer manually when the browserFoundPeerEvent fires with an interesting peer. Additionally,
		/// if there is no event listener for advertiserReceivedInvitationEvent invites will be accepted automatically.
		/// Note that serviceType must be 1–15 characters long and can contain only ASCII lowercase letters, numbers, and hyphens.
		/// Results in the advertiserDidNotStartAdvertisingPeerEvent firing if there is an issue starting to advertise.
		/// </summary>
		public static void advertiseCurrentDeviceWithNearbyServiceAdvertiser( string serviceType, string peerId = null, Dictionary<string,string> discoveryInfo = null )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerAdvertiseCurrentDeviceWithNearbyServiceAdvertiser( serviceType, peerId ?? SystemInfo.deviceName, Json.encode( discoveryInfo ) );
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerStopAdvertisingPeer();

		/// <summary>
		/// Stops advertising the current device.
		/// </summary>
		public static void stopAdvertisingPeer()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerStopAdvertisingPeer();
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerShowPeerPicker( string peerId, int minimumNumberOfPeers, int maximumNumberOfPeers );

		/// <summary>
		/// iOS/macOS only. Shows the peer picker browser so users can select peers to connect with. Fires the peerPickerFinishedEvent
		/// on completion.
		/// </summary>
		public static void showPeerPicker( string peerId = null, int minimumNumberOfPeers = -1, int maximumNumberOfPeers = -1 )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerShowPeerPicker( peerId ?? SystemInfo.deviceName, minimumNumberOfPeers, maximumNumberOfPeers );
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerStartNearbyServiceBrowser();

		/// <summary>
		/// Starts up the nearby service browser. This works much like the peer picker except it automatically invites any devices that it finds with no user interation required.
		/// </summary>
		public static void startNearbyServiceBrowser()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerStartNearbyServiceBrowser();
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerStopNearbyServiceBrowser();

		/// <summary>
		/// Stops the nearby service browser
		/// </summary>
		public static void stopNearbyServiceBrowser()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerStopNearbyServiceBrowser();
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerInvitePeer( string peerId, double timeout );

		/// <summary>
		/// Invites a peer to join the session
		/// </summary>
		/// <param name="peerId">peer identifier</param>
		public static void invitePeer( string peerId, double timeout = 0 )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			               || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerInvitePeer( peerId, timeout );
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerAcceptDeferredInvitationFromPeer( string peerId, bool shouldAcceptInvite );

		/// <summary>
		/// Accepts or rejects an invitation that was received and deferred (null returned in the advertiserReceivedInvitationEvent).
		/// You MUST accept or reject all received invitations to avoid memory leaks!
		/// </summary>
		/// <param name="peerId">Peer identifier.</param>
		public static void acceptDeferredInvitationFromPeer( string peerId, bool shouldAcceptInvite )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerAcceptDeferredInvitationFromPeer( peerId, shouldAcceptInvite );
		}


		[DllImport( DLL_NAME )]
		private static extern string _multiPeerGetLocalPeerID();

		/// <summary>
		/// Gets the peerID of the local player
		/// </summary>
		public static string getLocalPeerId()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return _multiPeerGetLocalPeerID();

			return string.Empty;
		}


		[DllImport( DLL_NAME )]
		private static extern string _multiPeerGetConnectedPeers();

		/// <summary>
		/// Gets all the currently connected peers
		/// </summary>
		public static List<string> getConnectedPeers()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return Json.decode<List<string>>( _multiPeerGetConnectedPeers() );

			return new List<string>();
		}


		[DllImport( DLL_NAME )]
		private static extern void _multiPeerDisconnectAndEndSession();

		/// <summary>
		/// Disconnects from the current session and ends it completely
		/// </summary>
		public static void disconnectAndEndSession()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				_multiPeerDisconnectAndEndSession();
		}


		[DllImport( DLL_NAME )]
		private static extern bool _multiPeerSendMessageToPeers( string peerIds, string gameObject, string method, string param, bool reliably );

		/// <summary>
		/// Sends a message to all the peers present in peerIds. Works much like SendMessage with regard to the GameObject, method and parameter
		/// </summary>
		public static bool sendMessageToPeers( string[] peerIds, string gameObject, string method, string param, bool reliably = false )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return _multiPeerSendMessageToPeers( Json.encode( peerIds ), gameObject, method, param, reliably );

			return false;
		}


		[DllImport( DLL_NAME )]
		private static extern bool _multiPeerSendMessageToAllPeers( string gameObject, string method, string param, bool reliably = false );

		/// <summary>
		/// Sends a message to all the connected peers. Works much like SendMessage with regard to the GameObject, method and parameter
		/// </summary>
		public static bool sendMessageToAllPeers( string gameObject, string method, string param, bool reliably = false )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return _multiPeerSendMessageToAllPeers( gameObject, method, param, reliably );

			return false;
		}


		[DllImport( DLL_NAME )]
		private static extern bool _multiPeerSendRawMessageToAllPeers( byte[] bytes, int length, bool reliably );

		/// <summary>
		/// Sends a raw byte array message to all connected devices
		/// </summary>
		public static bool sendRawMessageToAllPeers( byte[] bytes, bool reliably = false )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return _multiPeerSendRawMessageToAllPeers( bytes, bytes.Length, reliably );

			return false;
		}


		[DllImport( DLL_NAME )]
		private static extern bool _multiPeerSendRawMessageToPeers( string peerIds, byte[] bytes, int length, bool reliably );

		/// <summary>
		/// Sends a raw byte array message to all peerIds
		/// </summary>
		public static bool sendRawMessageToPeers( string[] peerIds, byte[] bytes, bool reliably = false )
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS
			   || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor )
				return _multiPeerSendRawMessageToPeers( Json.encode( peerIds ), bytes, bytes.Length, reliably );

			return false;
		}



	}

}
#endif
