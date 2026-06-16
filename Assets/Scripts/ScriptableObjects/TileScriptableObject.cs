using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileScriptableObject", order = 1)]
public class TileScriptableObject : ScriptableObject
{
    public string typeName;
    public GameObject model;
}
