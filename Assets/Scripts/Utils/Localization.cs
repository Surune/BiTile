using System;
using System.Collections.Generic;
using UnityEngine;

public class Localization
{
    private const string DefaultLocale = "koKR";
    private const string PlayerPrefsLocaleKey = "LanguageLocale";

    private const string Korean = "koKR";
    private const string Japanese = "jaJP";
    private const string ChineseSimplified = "zhHans";
    private const string ChineseTraditional = "zhHant";
    private const string English = "enUS";
    private const string Spanish = "esES";
    private const string PortugueseBrazil = "ptBR";
    private const string Russian = "ruRU";
    private const string German = "deDE";
    private const string French = "frFR";
    private const string Turkish = "trTR";
    private const string Thai = "thTH";

    public static readonly string[] SupportedLocales =
    {
        English,
        Korean,
        Japanese,
        ChineseSimplified,
        ChineseTraditional,
        Spanish,
        PortugueseBrazil,
        Russian,
        German,
        French,
        Turkish,
        Thai
    };

    public static readonly string[] SupportedLocaleLabels =
    {
        "English",
        "한국어",
        "日本語",
        "简体中文",
        "繁體中文",
        "Español",
        "Português",
        "Русский",
        "Deutsch",
        "Français",
        "Türkçe",
        "ภาษาไทย"
    };

    private Dictionary<string, Dictionary<string, string>> textByKey;
    private string currentLocale;

    public event Action LocaleChanged;

    public void Init()
    {
        textByKey = LoadText();
        currentLocale = LoadLocale();
    }

    public string CurrentLocale => currentLocale;

    public void SetLocale(string locale)
    {
        currentLocale = locale;
        PlayerPrefs.SetString(PlayerPrefsLocaleKey, locale);
        PlayerPrefs.Save();
        LocaleChanged?.Invoke();
    }

    public string Get(Definitions.LKey lkey)
    {
        if (lkey == Definitions.LKey.None)
        {
            return string.Empty;
        }

        return Get(lkey.ToString());
    }

    public string Get(string lkey)
    {
        return textByKey[lkey][currentLocale];
    }

    private static string GetSystemLocale()
    {
        return Application.systemLanguage switch
        {
            SystemLanguage.Korean => Korean,
            SystemLanguage.Japanese => Japanese,
            SystemLanguage.ChineseSimplified => ChineseSimplified,
            SystemLanguage.ChineseTraditional => ChineseTraditional,
            SystemLanguage.Chinese => ChineseSimplified,
            SystemLanguage.English => English,
            SystemLanguage.Spanish => Spanish,
            SystemLanguage.Portuguese => PortugueseBrazil,
            SystemLanguage.Russian => Russian,
            SystemLanguage.German => German,
            SystemLanguage.French => French,
            SystemLanguage.Turkish => Turkish,
            SystemLanguage.Thai => Thai,
            _ => DefaultLocale
        };
    }

    private static string LoadLocale()
    {
        var locale = PlayerPrefs.HasKey(PlayerPrefsLocaleKey)
            ? PlayerPrefs.GetString(PlayerPrefsLocaleKey)
            : GetSystemLocale();

        for (var i = 0; i < SupportedLocales.Length; i++)
        {
            if (SupportedLocales[i] == locale)
            {
                return locale;
            }
        }

        return GetSystemLocale();
    }

    private Dictionary<string, Dictionary<string, string>> LoadText()
    {
        var rows = CSVReader.Read("localization");
        var localization = new Dictionary<string, Dictionary<string, string>>(rows.Count);

        foreach (var row in rows)
        {
            localization[row["LKEY"]] = row;
        }

        return localization;
    }
}
