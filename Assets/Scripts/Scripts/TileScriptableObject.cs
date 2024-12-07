using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileScriptableObject", order = 1)]
public class TileScriptableObject : ScriptableObject
{
    public string typeName;
    public Sprite blackSprite;
    public Sprite whiteSprite;

	public Sprite[] blackSkins;
    public Sprite[] whiteSkins;

    public Sprite GetBlackSkinSprite(int skinIndex)
    {
        return blackSkins[skinIndex];
    }

    public Sprite GetWhiteSkinSprite(int skinIndex)
    {
        return whiteSkins[skinIndex];
    }
}