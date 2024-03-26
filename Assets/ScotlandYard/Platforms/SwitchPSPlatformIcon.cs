using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchPSPlatformIcon : MonoBehaviour
{
    //[SerializeField]
    //private Sprite ps4Icon;

#if UNITY_PS4 || UNITY_PS5
    [SerializeField]
    private Sprite ps5Icon;

    private void Awake()
    {
        gameObject.GetComponent<Image>().sprite = ps5Icon;
    }
#endif
}
