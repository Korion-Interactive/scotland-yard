using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace Ravensburger.ScotlandYard
{
	public static class StoreReviewNativeBindings 
	{
		public static void RequestAppReview()
		{
			Debug.Log("StoreReviewNativeBindings.RequestAppReview");
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				#if UNITY_IPHONE
				StoreReviewNativeBindings.RavensburgerRequestAppReview();
				#endif
			}
		}

		#if UNITY_IPHONE
			[DllImport("__Internal")]
			public static extern void RavensburgerRequestAppReview();
		#endif
	}
}
