using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Audio_Manager : MonoBehaviour
{
    #region VARIABLE
    public static Audio_Manager Instance;
    public class AudioArg
    {
        public AudioSource targetSource;
        public int targetClip;
        public bool isOverlap = false;
    }
    public AudioSource MusicSource;     // source for playing music
    public AudioSource GlobalSource;    // source for global sfx
    public AudioSource InterfaceSource; // source for ui sfx
    public AudioSource InGameSource;    // source for specific object

    public Toggle MusicToggle;
    public Toggle SoundToggle;

    public List<AudioSource> Audios;
    public List<AudioClip> Clips;
    public List<AudioClip> Interface_Clips;

    [Header("Setting Option")]
    public bool MusicOn = true;
    public bool SoundOn = true;
    public float SoundVolume = 0.35f;
    public float MusicVolume = 0.85f;
    #endregion

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }


        if (MusicOn)
            MusicSource.Play();
    }

    public void ToggleMusic(bool Val)
    {
        MusicOn = MusicToggle.isOn;
        if (MusicOn && !MusicSource.isPlaying)
        {
            MusicSource.Play();
        }
        else if (!MusicOn)
        {
            MusicSource.Stop();
        }
    }
    public void ToggleSound(bool Val)
    {
        SoundOn = SoundToggle.isOn;
        if (!SoundOn)
        {
            GlobalSource.Stop();
            InGameSource.Stop();
            InterfaceSource.Stop();
        }
    }

    #region INTERNAL FUNCTION
    public void PauseSource(string type)
    {
        if (type == "internal")
        {
            GlobalSource.Pause();
            if (InGameSource != null)
            {
                InGameSource.Pause();
            }
        }
        else if (type == "external")
        {
            MusicSource.Pause();
            InterfaceSource.Pause();
        }
    }
    public void UnpauseSource(string type)
    {
        if (type == "internal")
        {
            GlobalSource.UnPause();
            if (InGameSource != null)
            {
                InGameSource.UnPause();
            }
        }
        else if (type == "external")
        {
            MusicSource.UnPause();
            InterfaceSource.UnPause();
        }
    }
    #endregion

    #region AUDIO FUNCTION
    public void PlayInterface(int Clip)
    {
        if (SoundOn)
            InterfaceSource.PlayOneShot(Interface_Clips[Clip]);
    }
    public void PlayGlobal(int Clip)
    {
        if (SoundOn)
            GlobalSource.PlayOneShot(Clips[Clip]);
    }
    public void PlayClip(AudioSource Source, int Clip)
    {
        if (SoundOn)
        {
            Source.clip = Clips[Clip];
            Source.Play(0);
        }
    }
    public void PlayClipOverlap(AudioSource Source, int Clip)
    {
        if (SoundOn)
            Source.PlayOneShot(Clips[Clip]);
    }

    public void PlayAt(int clip, Vector3 destination, float volume)
    {
        if(SoundOn)
            AudioSource.PlayClipAtPoint(Clips[clip], destination, volume);
    }

    public void SignalSFX(object sender, AudioArg e)
    {
        if (SoundOn)
        {
            if (e.isOverlap)
            {
                e.targetSource.PlayOneShot(Clips[e.targetClip]);
            }
            else
            {
                e.targetSource.clip = Clips[e.targetClip];
                e.targetSource.Play(0);
            }
        }
    }
    #endregion

    private void OnEnable()
    {
        MusicOn = Setting.musicsetting;
        SoundOn = Setting.soundsetting;
        MusicToggle.isOn = Setting.musicsetting;
        SoundToggle.isOn = Setting.soundsetting;
    }
    private void OnDisable()
    {
        Setting.musicsetting = MusicOn;
        Setting.soundsetting = SoundOn;
    }
}

