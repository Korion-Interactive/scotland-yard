using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlyphDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _glyphSwitch, _glyphPCXBOX, _glyphPS4, _glyphPS5;

    // Start is called before the first frame update
    void Start()
    {
        DisableAllGlyphs();
        
#if UNITY_SWITCH
        _glyphSwitch.SetActive(true);
#elif UNITY_PS4
        _glyphPS4.SetActive(true);
#elif UNITY_PS5
        _glyphPS5.SetActive(true);
#else
        _glyphPCXBOX.SetActive(true);
#endif
        
    }


    private void DisableAllGlyphs()
    {
        _glyphSwitch.SetActive(false);
        _glyphPCXBOX.SetActive(false);
        _glyphPS4.SetActive(false);
        _glyphPS5.SetActive(false);
    }
}
