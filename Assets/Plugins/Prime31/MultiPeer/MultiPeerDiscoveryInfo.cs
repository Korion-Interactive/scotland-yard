using System.Collections.Generic;


namespace Prime31
{
	public class MultiPeerDiscoveryInfo
	{
		public string peerId;
		public Dictionary<string, string> discoveryInfo;


		public override string ToString()
		{
			return Json.encode( this );
		}
	}
}