using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioConfig : ScriptableObject
{
    public const string FileName = "AudioConfig";

    public AudioMixer masterMixer;
    public List<AudioCategory> audioCategories = new List<AudioCategory>();
    public List<string> onePlayPerMomentAudioNameList = new List<string>();
}
