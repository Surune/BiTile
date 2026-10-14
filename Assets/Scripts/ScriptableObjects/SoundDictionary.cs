using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDictionary", menuName = "ScriptableObjects/SoundDictionary", order = 2)]
public class SoundDictionary : ScriptableObject
{
    [SerializeField] private List<SoundData> sounds;
    
    public AudioClip GetClip(Definitions.SoundType soundType)
    {
        var clips = sounds.First(sound => sound.soundType == soundType).audioClips;
        return clips[UnityEngine.Random.Range(0, clips.Count)];
    }
}

[Serializable]
public struct SoundData
{
    public Definitions.SoundType soundType;
    public List<AudioClip> audioClips;
}
