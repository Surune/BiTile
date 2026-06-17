using System;
using UnityEngine;

public class SoundManager
{
    private const string BgmMuteKey = "BGM_MUTE";
    private const string SfxMuteKey = "SFX_MUTE";

    private readonly AudioSource[] audioSources = new AudioSource[(int)Definitions.Sound.MaxCount];
    private SoundDictionary soundDictionary;
    private bool bgmOn;
    private bool sfxOn;

    public void Init(SoundDictionary dictionary)
    {
        var root = GameObject.Find("@Sound");
        if (root != null)
        {
            return;
        }
        
        root = new GameObject { name = "@Sound" };
        UnityEngine.Object.DontDestroyOnLoad(root);
        soundDictionary = dictionary;
             
        var soundNames = Enum.GetNames(typeof(Definitions.Sound));
        for (var i = 0; i < soundNames.Length - 1; i++)
        {
            var go = new GameObject { name = soundNames[i] };
            audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = root.transform;
        }

        audioSources[(int)Definitions.Sound.Bgm].loop = true;
            
        bgmOn = PlayerPrefs.GetInt(BgmMuteKey, 0) == 1;
        audioSources[(int)Definitions.Sound.Bgm].mute = bgmOn;
            
        sfxOn = PlayerPrefs.GetInt(SfxMuteKey, 0) == 1;
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
        PlayerPrefs.SetInt(BgmMuteKey, Convert.ToInt32(bgmOn));
        PlayerPrefs.Save();
        audioSources[(int)Definitions.Sound.Bgm].mute = bgmOn;
    }
    
    public void ToggleSFXMute()
    {
        sfxOn = !sfxOn;
        PlayerPrefs.SetInt(SfxMuteKey, Convert.ToInt32(sfxOn));
        PlayerPrefs.Save();
        audioSources[(int)Definitions.Sound.Effect].mute = sfxOn;
    }
}
