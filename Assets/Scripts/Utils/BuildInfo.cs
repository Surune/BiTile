using UnityEngine;

public static class BuildInfo
{
    private const string ResourcePath = "build_info";

    public static string GitHash
    {
        get
        {
            var textAsset = Resources.Load<TextAsset>(ResourcePath);
            return textAsset.text.Trim();
        }
    }
}
