using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        PlayerPrefs.SetInt("Language", idx);

        SystemLanguage lang = Loc.SupportedLanguages[idx];
        Loc.Language = lang;

        SetLanguageSprite(lang);
        TranslateAllLabels();

        SetTicker(lang);
    }

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
