using System.Collections.Generic;

public class Localization
{
    private const string DefaultLocale = "koKR";

    private Dictionary<string, Dictionary<string, string>> textByKey;

    public void Init()
    {
        textByKey = LoadText();
    }

    public string Get(string lkey)
    {
        return Get(lkey, DefaultLocale);
    }

    private string Get(string lkey, string locale)
    {
        if (lkey == string.Empty)
        {
            return string.Empty;
        }

        return textByKey[lkey][locale];
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
