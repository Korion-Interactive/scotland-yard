using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ravensburger.ScotlandYard
{
	public class AppStoreReview : MonoBehaviour 
	{
		const string KEY_APP_STARTS = "AppStoreReview_AppStarts";

		private static bool _initialized = false;
		void Awake () 
		{
			if (!_initialized)
			{
				_initialized = true;

				// show review after several app starts
				int appStarts = PlayerPrefs.GetInt(KEY_APP_STARTS,0) + 1;
				if (appStarts >= 5)
				{
					StoreReviewNativeBindings.RequestAppReview();
					appStarts = 0;
				}
				PlayerPrefs.SetInt(KEY_APP_STARTS,appStarts);
				PlayerPrefs.Save();
			}
		}
	}
}
