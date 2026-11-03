using System.Collections;
using System.Linq;
using System.Collections.Generic;
#if ONEUP_DOTWEEN
using DG.Tweening;
#endif
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoSingleton<AudioManager>
{
    public AudioMixer masterMixer;
    public List<AudioCategory> audioCategories = new List<AudioCategory>();

    private AudioSource baseAudio;
    [HideInInspector] public int audioMixerMuteValue = -80;
    private List<RuntimeAudio> playingAudio = new List<RuntimeAudio>();
    private AudioSourcePool sourcePool;
    private const float MixerFadeDuration = 1.0f;

    public Dictionary<string, AudioSource> audioSourceDictionary = new Dictionary<string, AudioSource>();
    public List<string> onePlayPerMomentAudioNameList = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        LoadConfig();
        sourcePool = new AudioSourcePool(transform);
        SetBaseAudio();
    }

    private void Start()
    {
        ApplyCategoryVolumes();
    }

    public void ApplyCategoryVolumes()
    {
        foreach (AudioCategory category in audioCategories)
        {
            masterMixer.SetFloat(category.type.ToString(), category.categoryVolume);
        }
    }

    private void LoadConfig()
    {
        if (masterMixer != null && audioCategories.Count > 0)
            return;

        AudioConfig config = Resources.Load<AudioConfig>(AudioConfig.FileName);
        if (config == null)
            return;

        masterMixer = config.masterMixer;
        audioCategories = config.audioCategories;
        onePlayPerMomentAudioNameList = config.onePlayPerMomentAudioNameList;
    }

    private AudioCategory FindCategory(AudioCategoryType _audioType)
    {
        return audioCategories.Find(category => category.type == _audioType);
    }

    private AudioCollection FindCollection(AudioCategory _audioCategory, string _audioName)
    {
        return _audioCategory?.audioCollections.Find(collection =>
            collection.audioData.audios.Exists(audio => audio.audioName == _audioName));
    }

    private AudioCategory FindCategory(string _audioName)
    {
        return audioCategories.Find(category => FindCollection(category, _audioName) != null);
    }

    private Audio FindAudio(AudioCollection _audioCollection, string _audioName)
    {
        return _audioCollection?.audioData.audios.Find(audio => audio.audioName == _audioName);
    }

    private void SetBaseAudio()
    {
        GameObject newAudio = new GameObject();
        newAudio.name = "BaseAudio";
        newAudio.transform.SetParent(transform);
        baseAudio = newAudio.AddComponent<AudioSource>();
    }

    public void Play(AudioCategoryType _audioType, string _audioName, bool _loop = false, float? _volume = null,
        float? _pitch = null)
    {
        RemoveAudioSourceDictionaryNullParams();
        if (onePlayPerMomentAudioNameList.Exists(x => x == _audioName) == true &&
            audioSourceDictionary.ContainsKey(_audioName) == true)
            return;

        AudioCategory audioCategory = FindCategory(_audioType);
        if (audioCategory != null)
        {
            foreach (var audioCollection in audioCategory.audioCollections)
            {
                Audio currentAudio = FindAudio(audioCollection, _audioName);
                AudioClip clip = currentAudio?.GetClip();
                if (clip == null)
                    continue;

                PlayAudio(audioCollection, currentAudio, clip, _audioName, _loop, _volume, _pitch);
                return;
            }
        }

        Debug.LogWarning($"AudioManager - '{_audioName}' not found in {_audioType}.");
    }

    public void Play(AudioCategoryType _audioType, string _collectionTag, string _audioName, AudioClip _audio,
        bool _loop = false, float? _volume = null, float? _pitch = null)
    {
        RemoveAudioSourceDictionaryNullParams();
        if (onePlayPerMomentAudioNameList.Exists(x => x == _audioName) == true &&
            audioSourceDictionary.ContainsKey(_audioName) == true)
            return;

        AudioCategory audioCategory = FindCategory(_audioType);
        AudioCollection audioCollection =
            audioCategory.audioCollections.Find(collection => collection.tag == _collectionTag);
        Audio currentAudio = new Audio();
        currentAudio.audioName = _audioName;
        currentAudio.audioClip = _audio;
        PlayAudio(audioCollection, currentAudio, _audio, _audioName, _loop, _volume, _pitch);
    }

    public void Stop(AudioCategoryType _audioType, string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioType);
        if (audioCategory == null)
            return;
        AudioCollection audioCollection = FindCollection(audioCategory, _audioName);
        if (audioCollection == null)
            return;
        if (audioCollection.runInBaseAudio)
        {
            baseAudio.Stop();
        }
        else
        {
            List<RuntimeAudio> runtimeAudio = playingAudio.FindAll(x => x.audioName == _audioName);
            if (runtimeAudio != null && runtimeAudio.Count > 0)
            {
                foreach (var audio in runtimeAudio)
                {
                    DestroyAudio(audioCollection, audio);
                }
            }
        }
    }

    public bool IsAudioPlaying(AudioCategoryType _audioType, string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioType);
        if (audioCategory == null)
            return false;
        AudioCollection audioCollection = FindCollection(audioCategory, _audioName);
        if (audioCollection == null)
            return false;
        if (audioCollection.runInBaseAudio)
        {
            return baseAudio.isPlaying;
        }

        List<RuntimeAudio> runtimeAudio = playingAudio.FindAll(x => x.audioName == _audioName);
        return runtimeAudio.Count > 0;
    }

    public void Stop(string _audioName)
    {
        if (audioSourceDictionary.ContainsKey(_audioName) == false)
            return;

        AudioCollection audioCollection = FindCollection(FindCategory(_audioName), _audioName);
        if (audioCollection != null)
            SetImpactMixers(audioCollection, true);

        ReleaseSource(audioSourceDictionary[_audioName]);
    }

    public void StopAudioSourceLoop(string _audioName)
    {
        if (audioSourceDictionary.ContainsKey(_audioName) == false)
            return;
        AudioSource audioSource = audioSourceDictionary[_audioName];
        audioSource.loop = false;
        StartCoroutine(ReleaseAfter(audioSource, audioSource.clip.length));
    }

    public void RemoveAudioSourceDictionaryNullParams()
    {
        for (int i = 0; i < audioSourceDictionary.Count; i++)
        {
            var audioSource = audioSourceDictionary.ElementAt(i);
            if (audioSource.Value == null)
            {
                audioSourceDictionary.Remove(audioSource.Key);
            }
        }
    }

    public void StopBaseMusic()
    {
        baseAudio.Stop();
    }

    public void PlayOneShotAudio(string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioName);
        if (audioCategory == null)
        {
            Debug.LogWarning($"AudioManager - '{_audioName}' not found.");
            return;
        }

        Play(audioCategory.type, _audioName);
    }

    public void PlayAudioClip(AudioClip _audioClip, AudioMixerGroup _audioMixerGroup, bool _loop = false,
        float _volume = 1,
        float _pitch = 1)
    {
        AudioSource _newAudioSource = sourcePool.Get(_audioClip.name);
        _newAudioSource.outputAudioMixerGroup = _audioMixerGroup;
        _newAudioSource.clip = _audioClip;
        _newAudioSource.pitch = _pitch;
        _newAudioSource.volume = _volume;
        _newAudioSource.loop = _loop;

        _newAudioSource.Play();
        if (!_loop)
            StartCoroutine(ReleaseAfter(_newAudioSource, _audioClip.length));
    }

    private IEnumerator ReleaseAfter(AudioSource _audioSource, float _delay)
    {
        AudioClip clip = _audioSource.clip;
        yield return new WaitForSecondsRealtime(_delay);
        if (_audioSource != null && _audioSource.clip == clip)
            ReleaseSource(_audioSource);
    }

    private void ReleaseSource(AudioSource _audioSource)
    {
        playingAudio.RemoveAll(x => x.audioSource == _audioSource);
        foreach (string audioName in audioSourceDictionary.Where(x => x.Value == _audioSource).Select(x => x.Key).ToList())
        {
            audioSourceDictionary.Remove(audioName);
        }

        sourcePool.Release(_audioSource);
    }

    private void PlayAudio(AudioCollection _audioCcollection, Audio _audio, AudioClip _clip,
        string _audioName, bool _loop, float? _volume, float? _pitch)
    {
        SetImpactMixers(_audioCcollection, false);
        AudioSource newAudioSource = GetAudioSource(_audioCcollection, _audio);
        audioSourceDictionary[_audioName] = newAudioSource;
        SetAudioSource(_audioCcollection, _audio, _clip, _loop, _volume, _pitch, newAudioSource);
        SetToDestroyableAudio(_audioCcollection, _audioName, _loop, newAudioSource);
    }

    private void SetImpactMixers(AudioCollection _audioCcollection, bool _revert)
    {
        if (_audioCcollection.impactOnMixers)
        {
            foreach (var mixer in _audioCcollection.impactMixers)
            {
                if (_revert == false)
                {
                    mixer.impactedCount++;
                    if (mixer.impactedState == false)
                    {
                        mixer.impactedState = true;
                        masterMixer.GetFloat(mixer.impactedMixer, out mixer.impactedMixerBeforeVolume);
                        if (mixer.impactedMixerBeforeVolume != audioMixerMuteValue)
                        {
                            SetMixerChannelGain(mixer.impactedMixer, mixer.impactedMixerVolume);
                        }
                    }
                }
                else if (_revert == true)
                {
                    mixer.impactedCount--;
                    if (mixer.impactedCount == 0)
                    {
                        mixer.impactedState = false;
                        SetMixerChannelGain(mixer.impactedMixer, mixer.impactedMixerBeforeVolume);
                    }
                }
            }
        }
    }

    public void SetMixerChannelGain(string chanelName, float value)
    {
#if ONEUP_DOTWEEN
        masterMixer.GetFloat(chanelName, out float startValue);
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            masterMixer.SetFloat(chanelName, x);
        }, value, MixerFadeDuration);
#else
        masterMixer.SetFloat(chanelName, value);
#endif
    }

    private AudioSource GetAudioSource(AudioCollection _audioCcollection, Audio _audio)
    {
        AudioSource newAudioSource;
        if (_audioCcollection.runInBaseAudio)
        {
            newAudioSource = baseAudio;
        }
        else
        {
            newAudioSource = sourcePool.Get(_audio.audioName);
        }

        return newAudioSource;
    }

    private void SetAudioSource(AudioCollection _audioCollection, Audio _audio, AudioClip _clip, bool _loop,
        float? _volume, float? _pitch,
        AudioSource _newAudioSource)
    {
        _newAudioSource.outputAudioMixerGroup = _audioCollection.audioMixer;
        _newAudioSource.clip = _clip;
        _newAudioSource.pitch = _pitch ?? _audio.pitch;
        _newAudioSource.volume = _volume.HasValue ? _volume.Value * _audio.volume : _audio.volume;
        _newAudioSource.loop = _loop;
        _newAudioSource.Play();
    }

    private void SetToDestroyableAudio(AudioCollection _audioCollection, string _audioName, bool _loop,
        AudioSource _newAudioSource)
    {
        if (!_audioCollection.runInBaseAudio)
        {
            RuntimeAudio newRuntimeAudio = new RuntimeAudio();
            newRuntimeAudio.audioName = _audioName;
            newRuntimeAudio.audioSource = _newAudioSource;
            playingAudio.Add(newRuntimeAudio);
            if (_loop == false)
            {
                StartCoroutine(SetToDestroyable(_audioCollection, newRuntimeAudio));
            }
        }
    }

    private IEnumerator SetToDestroyable(AudioCollection _audioCollection, RuntimeAudio _newRuntimeAudio)
    {
        yield return new WaitForSecondsRealtime(_newRuntimeAudio.audioSource.clip.length);
        if (playingAudio.Contains(_newRuntimeAudio))
            DestroyAudio(_audioCollection, _newRuntimeAudio);
    }

    private void DestroyAudio(AudioCollection _audioCollection, RuntimeAudio _newRuntimeAudio)
    {
        SetImpactMixers(_audioCollection, true);
        if (playingAudio != null && _newRuntimeAudio != null)
        {
            ReleaseSource(_newRuntimeAudio.audioSource);
        }
    }


    public Audio GetAudioClip(string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioName);

        if (audioCategory == null)
            return null;
        AudioCollection audioCollection = FindCollection(audioCategory, _audioName);

        if (audioCollection == null)
            return null;

        return FindAudio(audioCollection, _audioName);
    }

    public Audio GetAudioClip(AudioCategoryType _audioType, string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioType);
        if (audioCategory == null)
            return null;

        AudioCollection audioCollection = FindCollection(audioCategory, _audioName);
        if (audioCollection == null)
            return null;

        return FindAudio(audioCollection, _audioName);
    }

    public AudioMixerGroup GetOutputAudioMixer(string _audioName)
    {
        AudioCategory audioCategory = FindCategory(_audioName);

        if (audioCategory == null)
            return null;
        AudioCollection audioCollection = FindCollection(audioCategory, _audioName);

        if (audioCollection == null)
            return null;

        return audioCollection.audioMixer;
    }

    public AudioMixerGroup GetOutputAudioMixer(AudioCategoryType _audioType)
    {
        AudioCategory audioCategory = FindCategory(_audioType);
        if (audioCategory == null)
            return masterMixer.outputAudioMixerGroup;

        AudioCollection audioCollection = audioCategory.audioCollections.Find(x => x.audioMixer != null);
        if (audioCollection == null)
            return masterMixer.outputAudioMixerGroup;

        return audioCollection.audioMixer;
    }

    public void SetAudioMixerState(AudioCategoryType audioType, float volumeBetweenZeroToOne)
    {
        AudioCategory audioCategory = FindCategory(audioType);
        if (audioCategory == null)
            return;

        float newVolume = AudioVolumeMath.ToDecibels(volumeBetweenZeroToOne, audioCategory.categoryVolume, audioMixerMuteValue);
        masterMixer.SetFloat(audioType.ToString(), newVolume);
    }
}