using UnityEngine;
using System;

public class SoundManager
{
    private readonly AudioSource[] audioSources = new AudioSource[(int)Definitions.Sound.MaxCount];
    private SoundDictionary soundDictionary;
    private bool bgmOn;
    private bool sfxOn;

    public void Init()
    {
        var root = GameObject.Find("@Sound");
        if (root != null)
        {
            return;
        }
        
        root = new GameObject { name = "@Sound" };
        UnityEngine.Object.DontDestroyOnLoad(root);
        soundDictionary = Utils.Load<SoundDictionary>("SoundDictionary");
             
        var soundNames = Enum.GetNames(typeof(Definitions.Sound));
        for (var i = 0; i < soundNames.Length - 1; i++)
        {
            var go = new GameObject { name = soundNames[i] };
            audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = root.transform;
        }

        audioSources[(int)Definitions.Sound.Bgm].loop = true;
            
        bgmOn = PlayerPrefs.GetInt("BGM_MUTE", 0) == 1;
        audioSources[(int)Definitions.Sound.Bgm].mute = bgmOn;
            
        sfxOn = PlayerPrefs.GetInt("SFX_MUTE", 0) == 1;
        audioSources[(int)Definitions.Sound.Effect].mute = sfxOn;
    }

    public void PlayBGM(Definitions.SoundType soundType)
    {
        var audioClip = soundDictionary.GetClip(soundType);
        
        var audioSource = audioSources[(int)Definitions.Sound.Bgm];
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        audioSource.volume = 0.7f;
        audioSource.clip = audioClip; 
        audioSource.Play();
    }

    public void PlaySFX(Definitions.SoundType soundType)
    {
        var audioClip = soundDictionary.GetClip(soundType);
        
        var audioSource = audioSources[(int)Definitions.Sound.Effect];
        audioSource.volume = 0.3f;
        audioSource.PlayOneShot(audioClip);
    }

    public void ToggleBGMMute()
    {
        bgmOn = !bgmOn;
        PlayerPrefs.SetInt("BGM_MUTE", Convert.ToInt32(bgmOn));
        audioSources[(int)Definitions.Sound.Bgm].mute = bgmOn;
    }
    
    public void ToggleSFXMute()
    {
        sfxOn = !sfxOn;
        PlayerPrefs.SetInt("SFX_MUTE", Convert.ToInt32(sfxOn));
        audioSources[(int)Definitions.Sound.Effect].mute = sfxOn;
    }
}
