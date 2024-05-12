using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.Events;
using System.Linq;
#if UNITY_SWITCH //&& !UNITY_EDITOR
using nn.hid;
#endif


namespace Korion.ScotlandYard.Input
{

    public class SwitchControllerCheck : MonoBehaviour
    {
#if UNITY_SWITCH //&& !UNITY_EDITOR
		private NpadId[] npadIds = { NpadId.Handheld, NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
		private NpadStyle[] states = new NpadStyle[7];
		private NpadState npadState = new NpadState();
		private long[] preButtons;
		private ControllerSupportArg controllerSupportArg = new ControllerSupportArg();
		private nn.Result result = new nn.Result();
		private bool isHandheld = false;
		private bool showingControllerSupport = false;
		private Coroutine appletDelay = null;

		public static bool IsHandheld;

		public static UnityEvent _onControllerAppletOpened = new UnityEvent();

		// Start is called before the first frame update
		void Start()
		{
			// Initial Check
			for (int i = 0; i < npadIds.Length; i++)
			{
				NpadId npadId = npadIds[i];
				NpadStyle npadStyle = Npad.GetStyleSet(npadId);
				if (npadStyle == NpadStyle.None) { continue; }
				if (npadStyle == NpadStyle.Handheld)
				{
					isHandheld = true;
					IsHandheld = true;
				}

				Npad.GetState(ref npadState, npadId, npadStyle);
				if (i < states.Length)
				{
					states[i] = npadStyle;
				}
			}
		}

		// Update is called once per frame
		void Update()
		{
            checkNPads();
		}

		void checkNPads()
		{

			for (int i = 0; i < npadIds.Length; i++)
			{
				NpadId npadId = npadIds[i];
				NpadStyle npadStyle = Npad.GetStyleSet(npadId);
				bool wasHandheldBefore = isHandheld;

                if (i == 0 && npadStyle == NpadStyle.Handheld)
                {
                    isHandheld = true;
                    IsHandheld = true;
                }
				else if(i == 0 && npadStyle != NpadStyle.Handheld)
				{
					isHandheld = false;
                    IsHandheld = false;
                }

				if(isHandheld != wasHandheldBefore)
				{
					//ReInput.players.GetPlayer(0)
				}

                IEnumerable<ControllerMap> cm = ReInput.players.GetPlayer(0).controllers.maps.GetAllMaps();

				int count = 0;

                foreach (ControllerMap c in cm)                                                                                        
                {
					count++;
                }

				if(count == 0)
				{
					Debug.Log("SwitchControllerCheck: Could not find a Controllermap for player 0");
				}

                Npad.GetState(ref npadState, npadId, npadStyle);
				if (i < states.Length && states[i] != npadStyle)
				{

					if (!showingControllerSupport)
					{
						ShowControllerSupport();
						showingControllerSupport = true;
						if (appletDelay != null)
						{
							StopCoroutine(appletDelay);
						}

						appletDelay = StartCoroutine(delayControllerApplet(1));
					}


					states[i] = npadStyle;
				}

			}

		}

		public void ShowControllerApplet()
		{
			if (!showingControllerSupport)
			{
				ShowControllerSupport();
				showingControllerSupport = true;
				if (appletDelay != null)
				{
					StopCoroutine(appletDelay);
				}

				appletDelay = StartCoroutine(delayControllerApplet(1));
			}
		}

		void ShowControllerSupport()
		{
			controllerSupportArg.SetDefault();
			controllerSupportArg.enableIdentificationColor = true;

			Debug.Log(controllerSupportArg);
			UnityEngine.Switch.Applet.Begin();
			result = ControllerSupport.Show(controllerSupportArg);
			UnityEngine.Switch.Applet.End();
			if (!result.IsSuccess()) 
			{ 
				Debug.Log(result); 
			}
			//ReInput.Reset();
			_onControllerAppletOpened?.Invoke();
		}

		IEnumerator delayControllerApplet(float delay)
		{
			yield return new WaitForSeconds(delay);
			showingControllerSupport = false;

			appletDelay = null;
		}
#endif
    }

}