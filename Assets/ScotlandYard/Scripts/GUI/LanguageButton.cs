using Cysharp.Threading.Tasks;
using Korion.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
public class LanguageButton : MonoBehaviour
{
    public UISprite LanguageSprite;
    public UILabel CreditsText;

    public UILabel HuntingMrX_EN, HuntingMrX_DE, HuntingMrX_ES, HuntingMrX_IT, HuntingMrX_FR;

    void Start()
    {
        SetLanguageSprite(Loc.Language);
        SetCreditsText();
        SetTicker(Loc.Language);
    }

    void OnClick()
    {
        int idx = Array.FindIndex(Loc.SupportedLanguages, (l) => l == Loc.Language);
        idx = (idx + 1) % Loc.SupportedLanguages.Length;



#if UNITY_SWITCH
        WriteDataAsync("language", idx.ToString()).Forget();
#else
        PlayerPrefs.SetInt("Language", idx);
        PlayerPrefs.Save();
#endif

        SystemLanguage lang = Loc.SupportedLanguages[idx];
        Loc.Language = lang;

        SetLanguageSprite(lang);
        TranslateAllLabels();

        SetTicker(lang);
    }

    public void SetLanguageOnSwitch(int id)
    {
        SystemLanguage lang = Loc.SupportedLanguages[id];
        Loc.Language = lang;

        SetLanguageSprite(lang);
        TranslateAllLabels();

        SetTicker(lang);
    }

#if UNITY_SWITCH
    public UniTask WriteDataAsync<T>(string id, T data, CancellationToken cancellationToken = default)
    {
        //Debug.Log("KORION: Start Writing Data");
        var writer = IOSystem.Instance.GetWriter();
        //string json = JsonUtility.ToJson(savedSettings, prettyPrint: true); //now data

        //Debug.Log("Writing Korion IO");

        return writer.WriteAsync(id, data, cancellationToken);
    }

#endif

    public void SetLanguageSprite(SystemLanguage lang)
    {
        string l = Loc.GetLanguageString(lang) + "_language";
        LanguageSprite.spriteName = l;
    }

    void TranslateAllLabels()
    {
        foreach(var l in GameObject.FindObjectsOfType<LabelTranslator>())
        {
            l.Translate();
        }
        SetCreditsText();
    }

    void SetCreditsText()
    {
        CreditsText.text = string.Format(Loc.Get("credits_text"), Globals.VersionNumber);
    }

    void SetTicker(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.German:
                SetLabel(HuntingMrX_DE);
                break;
            case SystemLanguage.French:
                SetLabel(HuntingMrX_FR);
                break;
            case SystemLanguage.Spanish:
                SetLabel(HuntingMrX_ES);
                break;
            case SystemLanguage.Italian:
                SetLabel(HuntingMrX_IT);
                break;
            case SystemLanguage.English:
                SetLabel(HuntingMrX_EN);
                break;
            default:
                SetLabel(HuntingMrX_EN);
                break;
        }
    }

    void SetLabel(UILabel lbl)
    {
        HuntingMrX_EN.gameObject.SetActive(false);
        HuntingMrX_ES.gameObject.SetActive(false);
        HuntingMrX_DE.gameObject.SetActive(false);
        HuntingMrX_FR.gameObject.SetActive(false);
        HuntingMrX_IT.gameObject.SetActive(false);

        lbl.gameObject.SetActive(true);
    }
}
