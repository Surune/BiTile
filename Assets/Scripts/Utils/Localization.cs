using System.Collections.Generic;

public class Localization
{
    private const string DefaultLocale = "koKR";

    private Dictionary<string, Dictionary<string, object>> textByKey;

    public void Init()
    {
        textByKey = LoadText();
    }

    public string Get(string lkey)
    {
        return Get(lkey, DefaultLocale);
    }

    public string Get(string lkey, string locale)
    {
        if (lkey == string.Empty)
        {
            return string.Empty;
        }

        return textByKey[lkey][locale].ToString();
    }

    private Dictionary<string, Dictionary<string, object>> LoadText()
    {
        var rows = CSVReader.Read("localization");
        var localization = new Dictionary<string, Dictionary<string, object>>(rows.Count);

        foreach (var row in rows)
        {
            localization[row["LKEY"].ToString()] = row;
        }

        return localization;
    }
}
