using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDictionary", menuName = "ScriptableObjects/SoundDictionary", order = 2)]
public class SoundDictionary : ScriptableObject
{
    [SerializeField, TableView] private List<SoundData> sounds;
    
    public AudioClip GetClip(Definitions.SoundType soundType)
    {
        return sounds.First(sound => sound.soundType == soundType).audioClip;
    }
}

[Serializable]
public struct SoundData
{
    public Definitions.SoundType soundType;
    public AudioClip audioClip;
}
