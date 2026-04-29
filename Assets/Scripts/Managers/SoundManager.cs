using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager
{
    private AudioSource[] audioSources = new AudioSource[(int)Definitions.Sound.MaxCount];
    private Dictionary<string,AudioClip> _audioClips = new Dictionary<string,AudioClip>();
    private bool bgmOn;
    private bool sfxOn;

    public void Init()
    {
        GameObject root = GameObject.Find("@Sound");
        if(root == null)
        {
            root = new GameObject { name = "@Sound" };
            UnityEngine.Object.DontDestroyOnLoad(root);
             
            string[] soundNames = System.Enum.GetNames(typeof(Definitions.Sound));
            for(int i = 0; i < soundNames.Length-1; i++)
            {
                GameObject go = new GameObject { name = soundNames[i] };
                audioSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root.transform;
            }

            audioSources[(int)Definitions.Sound.Bgm].loop = true;
            
            bgmOn = PlayerPrefs.GetInt("BGM_MUTE", 0) == 1;
            audioSources[(int)Definitions.Sound.Bgm].mute = bgmOn;
            
            sfxOn = PlayerPrefs.GetInt("SFX_MUTE", 0) == 1;
            audioSources[(int)Definitions.Sound.Effect].mute = sfxOn;
        }
    }

    public void Play(string path, Definitions.Sound type=Definitions.Sound.Effect, float pitch = 1.0f)
    {
        if (!path.Contains("Sounds/"))
        {
            path = $"Sounds/{path}";
        }
        
        if (type == Definitions.Sound.Bgm)
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>(path);
            if(audioClip == null )
            {
                Debug.Log($"AudioClip Missing ! {path}");
                return;
            }

            AudioSource audioSource = audioSources[(int)Definitions.Sound.Bgm];
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
                Debug.Log($"AudioClip Missing ! {path}");
                return;
            }

            AudioSource audioSource = audioSources[(int)Definitions.Sound.Effect];
            audioSource.volume = 0.3f;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
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
