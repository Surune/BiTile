using System.Collections.Generic;
using UnityEngine;

public class Localization
{
    private const string DefaultLocale = "koKR";
    private const string PlayerPrefsLocaleKey = "LanguageLocale";

    public const string Korean = "koKR";
    public const string Japanese = "jaJP";
    public const string ChineseSimplified = "zhHans";
    public const string ChineseTraditional = "zhHant";
    public const string English = "enUS";
    public const string Spanish = "esES";
    public const string PortugueseBrazil = "ptBR";
    public const string Russian = "ruRU";
    public const string German = "deDE";
    public const string French = "frFR";
    public const string Turkish = "trTR";
    public const string Thai = "thTH";

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
    }

    public string Get(string lkey)
    {
        return Get(lkey, currentLocale);
    }

    private string Get(string lkey, string locale)
    {
        if (lkey == string.Empty)
        {
            return string.Empty;
        }

        return textByKey[lkey][locale];
    }

    public static string GetSystemLocale()
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
