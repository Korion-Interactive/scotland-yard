using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class LanguageButton : MonoBehaviour
{
    public UISprite LanguageSprite;
    public UILabel CreditsText;

    void Start()
    {
        SetLanguageSprite(Loc.Language);
        SetCreditsText();
    }

    void OnClick()
    {
        int idx = Array.FindIndex(Loc.SupportedLanguages, (l) => l == Loc.Language);
        idx = (idx + 1) % Loc.SupportedLanguages.Length;

        SystemLanguage lang = Loc.SupportedLanguages[idx];
        Loc.Language = lang;
        SetLanguageSprite(lang);
        TranslateAllLabels();
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
}
