using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterData", menuName = "ScriptableObjects/ChapterData", order = 3)]
public class ChapterDataList : ScriptableObject
{
    public ChapterData[] Data;
}

[Serializable]
public struct ChapterData
{
    public int Id;
    public string RomanNumber;
    public GameObject TileModel;
    public GameObject NumberModel;
    public Color BackgroundColor;
    public Color TileColor;
    public Sprite[] BackgroundSprites;
    public string NameLKey => $"CHAPTER_{Id}_NAME";
}
