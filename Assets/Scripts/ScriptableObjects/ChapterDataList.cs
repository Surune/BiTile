using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterData", menuName = "ScriptableObjects/ChapterData", order = 3)]
public class ChapterDataList : ScriptableObject
{
    public ChapterData[] Data;
}

[Serializable]
public struct ChapterData
{
    public GameObject TileModel;
    public GameObject NumberModel;
    public Color BackgroundColor;
    public Color TileColor;
}
