using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sunbow.Util.IO;
using UnityEngine;

public static class Loc
{
    public const SystemLanguage DEFAULT_LANGUAGE = SystemLanguage.English;

    public static SystemLanguage[] SupportedLanguages = { SystemLanguage.English, SystemLanguage.German, SystemLanguage.French, SystemLanguage.Italian, SystemLanguage.Spanish };

    private static List<Tuple<SystemLanguage, string>> languageMapping = new List<Tuple<SystemLanguage, string>>() 
    {
        new Tuple<SystemLanguage, string>(SystemLanguage.Czech, "cz"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Dutch, "nl"),
        new Tuple<SystemLanguage, string>(SystemLanguage.English, "en"),
        new Tuple<SystemLanguage, string>(SystemLanguage.French, "fr"),
        new Tuple<SystemLanguage, string>(SystemLanguage.German, "de"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Italian, "it"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Japanese, "jp"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Korean, "ko"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Polish, "pl"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Portuguese, "pt"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Russian, "ru"),
        new Tuple<SystemLanguage, string>(SystemLanguage.Spanish, "es"),
    };

    public static Table Table { get { return AppSetup.Instance.LocaTable; } }

    public static event Action<SystemLanguage> LanguageHasChanged;

    static string language = GetLanguageString(DEFAULT_LANGUAGE);
    public static SystemLanguage Language
    {
        get { return GetLanguageEnum(language); }
        set
        {
            string val = GetLanguageString(value);

            if (val == language)
                return;

            if (Table.ContainsColumn(val))
            {
                language = val;

                if (LanguageHasChanged != null)
                    LanguageHasChanged(value);
            }
            else
            {
                Log.error("Loc", string.Format("Language {0} not supported by localization file.", value));
            }

        }
    }

    public static void SetLanguage(string id)
    {
        if (language == id)
            return;

        language = id;

        if (LanguageHasChanged != null)
            LanguageHasChanged(GetLanguageEnum(id));
    }

    public static string GetLanguageString(SystemLanguage language)
    {
        var result = languageMapping.FirstOrDefault(o => o.A == language);

        if (result == null)
        {
            Log.error("Loc", string.Format("The language {0} is not (yet) supported", language));
            return languageMapping.First(o => o.A == DEFAULT_LANGUAGE).B;
        }

        return result.B;
    }

    static SystemLanguage GetLanguageEnum(string language)
    {
        var result = languageMapping.FirstOrDefault(o => o.B == language);

        if (result == null)
        {
            Log.error("Loc", string.Format("The language {0} is not (yet) supported", language));
            return DEFAULT_LANGUAGE;
        }

        return result.A;
    }

    public static string Get(string stringID)
    {
        stringID = stringID.Trim(' ', '\r', '\n', '\t');
        if (string.IsNullOrEmpty(stringID))
        {
            Log.error("LOC", "string is null");
            return "";
        }

        if (Table.ContainsRow(stringID))
        {
            string result = Table[language, stringID];
            if (!string.IsNullOrEmpty(result))
                return result.Replace('~', '\n').Replace("…", "...");
            else
                return string.Format("[{0}]", stringID);
        }
        else
        {
            Table.AppendRow(stringID);
            return string.Format("[{0}]", stringID);
        }
    }

    public static string Get(string stringId, params string[] args)
    {
        for (int i = 0; i < args.Length; i++)
            args[i] = Loc.Get(args[i]);

        return string.Format(Loc.Get(stringId), args);
    }
}
