using System;
using UnityEngine;

public class SoundManager
{
    private const string BgmMuteKey = "BGM_MUTE";
    private const string SfxMuteKey = "SFX_MUTE";
    private const string BgmVolumeKey = "BGM_VOLUME";
    private const string SfxVolumeKey = "SFX_VOLUME";
    private const float DefaultBgmVolume = 0.5f;
    private const float DefaultSfxVolume = 0.5f;

    private readonly AudioSource[] audioSources = new AudioSource[(int)Definitions.Sound.MaxCount];
    private SoundDictionary soundDictionary;
    private bool bgmOn;
    private bool sfxOn;
    private float bgmVolume;
    private float sfxVolume;

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

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
            
        bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, PlayerPrefs.GetInt(BgmMuteKey, 0) == 1 ? 0f : DefaultBgmVolume);
        audioSources[(int)Definitions.Sound.Bgm].volume = bgmVolume;

        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, PlayerPrefs.GetInt(SfxMuteKey, 0) == 1 ? 0f : DefaultSfxVolume);
        audioSources[(int)Definitions.Sound.Effect].volume = sfxVolume;
    }

    public void PlayBGM(Definitions.SoundType soundType)
    {
        var audioClip = soundDictionary.GetClip(soundType);
        
        var audioSource = audioSources[(int)Definitions.Sound.Bgm];
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        audioSource.volume = bgmVolume;
        audioSource.clip = audioClip; 
        audioSource.Play();
    }

    public void PlaySFX(Definitions.SoundType soundType)
    {
        var audioClip = soundDictionary.GetClip(soundType);
        
        var audioSource = audioSources[(int)Definitions.Sound.Effect];
        audioSource.volume = sfxVolume;
        audioSource.PlayOneShot(audioClip);
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.Save();
        audioSources[(int)Definitions.Sound.Bgm].volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
        audioSources[(int)Definitions.Sound.Effect].volume = sfxVolume;
    }

    public void ToggleBGMMute()
    {
        bgmOn = bgmVolume > 0f;
        PlayerPrefs.SetInt(BgmMuteKey, Convert.ToInt32(bgmOn));
        PlayerPrefs.Save();
        SetBgmVolume(bgmOn ? 0f : DefaultBgmVolume);
    }
    
    public void ToggleSFXMute()
    {
        sfxOn = sfxVolume > 0f;
        PlayerPrefs.SetInt(SfxMuteKey, Convert.ToInt32(sfxOn));
        PlayerPrefs.Save();
        SetSfxVolume(sfxOn ? 0f : DefaultSfxVolume);
    }
}
