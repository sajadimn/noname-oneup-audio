using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public enum AudioCategoryType
{
    Music = 0,
    SFX = 1,
    Dialogue = 2,
    Reporter = 3
}

[Serializable]
public class AudioVolume
{
    public AudioCategoryType type;
    public int baseVolume;
}

[Serializable]
public class AudioCategory
{
    public AudioCategoryType type;
    [Range(-80, 20)] public float categoryVolume;
    public List<AudioCollection> audioCollections = new List<AudioCollection>();
}

[Serializable]
public class ImpactMixer
{
    public string impactedMixer;
    [Range(-80, 20)] public float impactedMixerVolume;

    [HideInInspector] public float impactedMixerBeforeVolume;
    [HideInInspector] public bool impactedState = false;
    [HideInInspector] public int impactedCount = 0;
}

[Serializable]
public class AudioCollection
{
    public string tag;
    public bool runInBaseAudio;
    public AudioData audioData;
    public AudioMixerGroup audioMixer;
    public bool impactOnMixers;
    [AudioShowIf(nameof(impactOnMixers))]
    public List<ImpactMixer> impactMixers = new List<ImpactMixer>();
}

[Serializable]
public class Audio
{
    public string audioName;
    public AudioCount audioCount = AudioCount.Single;

    [AudioShowIf(nameof(audioCount), AudioCount.Single)]
    public AudioClip audioClip;

    [AudioShowIf(nameof(audioCount), AudioCount.Multiple)]
    public List<AudioClip> audioClips;

    [Range(0, 1)] public float volume = 1.0f;
    [Range(0.1f, 3.0f)] public float pitch = 1.0f;

    public AudioClip GetClip()
    {
        if (audioCount == AudioCount.Single)
            return audioClip;

        return audioClips == null || audioClips.Count == 0
            ? null
            : audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
    }
}

public enum AudioCount
{
    Single = 0,
    Multiple = 1
}

[Serializable]
public class RuntimeAudio
{
    public string audioName;
    public AudioSource audioSource;
}