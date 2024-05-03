using System.Collections;
using System.Collections.Generic;
using Ravity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScene : MonoBehaviour 
{
	[SerializeField] private Image _ravensburgerTriangleLogo;
	[SerializeField] private Image _productLogo;
	private List<Image> _logos = new List<Image>();

	private void SetAlpha(Image logo, float alpha)
	{
		Color tmp = logo.color;
		tmp.a = alpha;
		logo.color = tmp;
	}

		
	void Awake()
	{
		_logos.Add(_ravensburgerTriangleLogo);
		_logos.Add(_productLogo);

		foreach (Image logo in _logos)
		{
			SetAlpha(logo,0);
		}

		// scale ravensburger triangle logo according to aspect ratio
		// 4:3 = 1.333 e.g. on iPad take 25%  
		// 16:10 = 1.600 	take 30%
		// 16:9 = 1.777 e.g on iPhone or Android take 33%
		// equation: percentage height = 0.1875 * aspect ratio
		float aspectRatio = HardwareUtils.AspectRatioInLandscape;
		float percentageHeight = 0.1875f * aspectRatio;
		percentageHeight = Mathf.Clamp(percentageHeight, 0.25f, 1f/3f); 
		float logoScale = 3 * percentageHeight;
		//Debug.Log("logo " + percentageHeight + "% logoScale=" + logoScale);
		_ravensburgerTriangleLogo.transform.localScale = new Vector3(logoScale,logoScale,logoScale);
	}

	IEnumerator Start () 
	{
		yield return new WaitForSeconds(8);//yield return null;
		

		Time.timeScale = 1.0f;

		foreach (Image logo in _logos)
		{
			float alpha = 0;
			while (true)
			{
				alpha += Time.deltaTime;
				SetAlpha(logo,alpha);

				if (alpha >= 1.0)
				{
					break;
				}
				yield return null;
			}
		}

		// load next scene
		yield return null;
		SceneManager.LoadScene(1);		
	}
}
