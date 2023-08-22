using UnityEngine;



namespace Prime31
{
	public class MultiPeerEventListener : MonoBehaviour
	{
#if UNITY_IOS || UNITY_TVOS
		void OnEnable()
		{
			// Listen to all events for illustration purposes
			MultiPeerManager.peerPickerFinishedEvent += browserFinishedEvent;

			MultiPeerManager.browserFoundPeerEvent += browserFoundPeerEvent;
			MultiPeerManager.browserLostPeerEvent += browserLostPeerEvent;
			MultiPeerManager.browserDidNotStartBrowsingForPeersEvent += browserDidNotStartBrowsingForPeersEvent;

			MultiPeerManager.advertiserReceivedInvitationEvent += advertiserReceivedInvitationEvent;
			MultiPeerManager.advertiserDidNotStartAdvertisingPeerEvent += advertiserDidNotStartAdvertisingPeerEvent;

			MultiPeerManager.peerDidChangeStateToConnectedEvent += peerDidChangeStateToConnectedEvent;
			MultiPeerManager.peerDidChangeStateToNotConnectedEvent += peerDidChangeStateToNotConnectedEvent;
			MultiPeerManager.receivedRawDataEvent += receivedRawDataEvent;
		}


		void OnDisable()
		{
			// Remove all event handlers
			MultiPeerManager.peerPickerFinishedEvent -= browserFinishedEvent;

			MultiPeerManager.browserFoundPeerEvent -= browserFoundPeerEvent;
			MultiPeerManager.browserLostPeerEvent -= browserLostPeerEvent;
			MultiPeerManager.browserDidNotStartBrowsingForPeersEvent -= browserDidNotStartBrowsingForPeersEvent;

			MultiPeerManager.advertiserReceivedInvitationEvent -= advertiserReceivedInvitationEvent;
			MultiPeerManager.advertiserDidNotStartAdvertisingPeerEvent -= advertiserDidNotStartAdvertisingPeerEvent;

			MultiPeerManager.peerDidChangeStateToConnectedEvent -= peerDidChangeStateToConnectedEvent;
			MultiPeerManager.peerDidChangeStateToNotConnectedEvent -= peerDidChangeStateToNotConnectedEvent;
			MultiPeerManager.receivedRawDataEvent -= receivedRawDataEvent;
		}



		void browserFinishedEvent( string param )
		{
			Debug.Log( "browserFinishedEvent: " + param );
		}


		void browserFoundPeerEvent( MultiPeerDiscoveryInfo info )
		{
			Debug.Log( "browserFoundPeerEvent: " + info );
		}


		void browserLostPeerEvent( string peerId )
		{
			Debug.Log( "browserLostPeerEvent: " + peerId );
		}


		void browserDidNotStartBrowsingForPeersEvent( string error )
		{
			Debug.Log( "browserDidNotStartBrowsingForPeersEvent: " + error );
		}


		bool? advertiserReceivedInvitationEvent( string peerId )
		{
			Debug.Log( "advertiserReceivedInvitationEvent: " + peerId );
			return true;
		}


		void advertiserDidNotStartAdvertisingPeerEvent( string error )
		{
			Debug.Log( "advertiserDidNotStartAdvertisingPeerEvent: " + error );
		}


		void peerDidChangeStateToConnectedEvent( string peerId )
		{
			Debug.Log( "peerDidChangeStateToConnectedEvent: " + peerId );
		}


		void peerDidChangeStateToNotConnectedEvent( string peerId )
		{
			Debug.Log( "peerDidChangeStateToNotConnectedEvent: " + peerId );
		}


		void receivedRawDataEvent( string peerId, byte[] data )
		{
			Debug.Log( "receivedRawDataEvent from: " + peerId + ", data.Length: " + data.Length );
		}


#endif
	}

}


