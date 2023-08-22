using UnityEngine;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Ravity
{
	public static class HardwareUtils 
	{
		#region Editor and Desktop
		public static bool IsEditor
		{
			get
			{
				#if UNITY_EDITOR
					return true;
				#else
					return false;
				#endif
			}
		}

		public static bool IsDesktop
		{
			get
			{
				#if UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE
					return true;
				#else
					return false;
				#endif
			}
		}
		#endregion
		
		#region WebGL
		public static bool IsWebGL
		{
			get
			{
				#if UNITY_WEBGL
					return true;
				#else
					return false;
				#endif
			}
		}

		public static bool IsWebGLBuild
		{
			get
			{
				#if UNITY_WEBGL && !UNITY_EDITOR
					return true;
				#else
					return false;
				#endif
			}
		}

		public static bool IsWebGLandSafari => IsWebGLandBrowser("Safari");
		public static bool IsWebGLandFirefox => IsWebGLandBrowser("Firefox");
		public static bool IsWebGLandChrome => IsWebGLandBrowser("Chrome");
		public static bool IsWebGLandEdge => IsWebGLandBrowser("Edge");

		private static bool IsWebGLandBrowser(string browser)
		{
			if (IsWebGLBuild)
			{
				// SystemInfo.deviceModel for WebGL returns values 
				// like "Safari 10.1.1", "Chrome 60.0.3112.113" or "Firefox 54.0" or "Edge 18.17763"
				if (SystemInfo.deviceModel.Contains(browser))
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region iOS
		public static bool IsiOS
		{
			get
			{
				#if UNITY_IPHONE
					return true;
				#else
					return false;
				#endif
			}
		}
		#endregion

		#region Android
		public static bool IsAndroid
		{
			get
			{
				#if UNITY_ANDROID
					return true;
				#else
					return false;
				#endif
			}
		}

		public static bool IsAndroidDevice => IsAndroid && Application.platform == RuntimePlatform.Android;

		public static bool IsAmazon
		{
			get 
			{
				#if UNITY_ANDROID && AMAZON
					return true;
				#else
					return false;
				#endif
			}
		}
		
		public static bool IsGooglePlay
		{
			get 
			{
				#if UNITY_ANDROID && GOOGLE_PLAY
					return true;
				#else
					return false;
				#endif
			}
		}
		#endregion

		#region tvOS
		public static bool IsTVOS
		{
			get 
			{
				#if UNITY_TVOS
					return true;
				#else
					return false;
				#endif
			}
		}
		#endregion
		
		#region Device Model
		public static bool IsDeviceModel(string name)
		{
			string model = SystemInfo.deviceModel.Trim();
			return name.Equals(model);
		}
		#endregion

		#region Device Size
		public static float SmallestScreenDimension => Mathf.Min(Screen.width, Screen.height);

		public static float BiggestScreenDimension => Mathf.Max(Screen.width, Screen.height);

		public static float AspectRatioInLandscape
		{
			get
			{
				float width = Mathf.Max(Screen.width, Screen.height);
				float height = Mathf.Min(Screen.width, Screen.height);
				if (width <= 0 || height <= 0)
				{
					return 1.333f; // educated guess
				}
				return width / height;
			}
		}

		public static float DisplayHeightInInch
		{
			get
			{
				float dpi = Mathf.Max(1f, Screen.dpi);
				float heightInInch = SmallestScreenDimension / dpi;
				return heightInInch;
			}
		}

		private static bool? _usePhoneLayout;
		public static bool UsePhoneLayout
		{
			// you can set this value directly for some special cases where the build-in metric fails
			set => _usePhoneLayout = value;
			get
			{
				if (_usePhoneLayout.HasValue == false)
				{
					// we default to phone if the screen size is unknown
					bool unknownScreenSize = Screen.dpi == 0f;
					_usePhoneLayout = unknownScreenSize || DisplayHeightInInch < 3f;
				}
				return _usePhoneLayout.Value;
			}
		}

		public static bool UseTabletLayout => !UsePhoneLayout;
		#endregion
	}
}
