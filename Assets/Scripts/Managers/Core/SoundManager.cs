using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager
{
    AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    Dictionary<string,AudioClip> _audioClips = new Dictionary<string,AudioClip>();
    public bool bgmOn;
    public bool sfxOn;
    //MP3 Player
    //MP3 ����
    //Listener

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if(root == null)
        {
            root = new GameObject { name = "@Sound" };
            UnityEngine.Object.DontDestroyOnLoad(root);
             
            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for(int i = 0; i < soundNames.Length-1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                _audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            _audioSources[(int)Define.Sound.Bgm].loop = true;
            
            bgmOn = PlayerPrefs.GetInt("BGM_MUTE", 0) == 1;
            _audioSources[(int)Define.Sound.Bgm].mute = bgmOn;
            
            sfxOn = PlayerPrefs.GetInt("SFX_MUTE", 0) == 1;
            _audioSources[(int)Define.Sound.Effect].mute = sfxOn;
        }
    }

    public void Clear()
    {
        foreach(AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    public void Play(string path, Define.Sound type=Define.Sound.Effect, float pitch = 1.0f)
    {
        if (path.Contains("Sounds/") == false)
            path = $"Sounds/{path}";
        if(type == Define.Sound.Bgm)
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>(path);
            if(audioClip == null )
            {
                Debug.Log($"AudioClip Missing ! {path}");
                return;
            }

            AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();
            
            audioSource.volume = 0.7f;
            audioSource.pitch = pitch;
            audioSource.clip = audioClip; 
            audioSource.Play();
        }
        else
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>(path);
            if (audioClip == null)
            {
                Debug.Log($"AudioCLup Missing ! {path}");
                return;
            }

            AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
            audioSource.volume = 0.3f;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }


    AudioClip GetOrAddAudioClip(string path)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(path, out audioClip) == false)
        {
            audioClip = Managers.Resource.Load<AudioClip>(path);
            _audioClips.Add(path, audioClip);
        }
        return audioClip;
    }

    public void ToggleBGMMute()
    {
        bgmOn = !bgmOn;
        PlayerPrefs.SetInt("BGM_MUTE", Convert.ToInt32(bgmOn));
        _audioSources[(int)Define.Sound.Bgm].mute = bgmOn;
    }
    
    public void ToggleSFXMute()
    {
        sfxOn = !sfxOn;
        PlayerPrefs.SetInt("SFX_MUTE", Convert.ToInt32(sfxOn));
        _audioSources[(int)Define.Sound.Effect].mute = sfxOn;
    }
}
