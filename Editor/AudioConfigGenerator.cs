using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[InitializeOnLoad]
public static class AudioConfigGenerator
{
    public const string MixerTemplatePath = "Packages/noname-oneup-audio/Template/AudioMixer.mixer";

    private const string Folder = "Assets/Settings/Audio";
    private const string MixerPath = Folder + "/AudioMixer.mixer";
    private const string MusicPath = Folder + "/Music.asset";
    private const string SfxPath = Folder + "/SFX.asset";
    private const string ConfigPath = Folder + "/Resources/" + AudioConfig.FileName + ".asset";
    private const float MusicVolume = 0f;
    private const float SfxVolume = 0f;

    static AudioConfigGenerator()
    {
        EditorApplication.delayCall += Generate;
    }

    [MenuItem("OneUp/Audio/Create Missing Configs")]
    public static void Generate()
    {
        CreateFolder("Assets", "Settings");
        CreateFolder("Assets/Settings", "Audio");
        CreateFolder(Folder, "Resources");

        if (!Exists(MixerPath))
            AssetDatabase.CopyAsset(MixerTemplatePath, MixerPath);

        AudioData music = CreateData(MusicPath);
        AudioData sfx = CreateData(SfxPath);

        if (Exists(ConfigPath))
            return;

        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        AssetDatabase.CreateAsset(CreateDefaultConfig(mixer, music, sfx), ConfigPath);
    }

    public static AudioConfig CreateDefaultConfig(AudioMixer mixer, AudioData music, AudioData sfx)
    {
        AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
        AudioConfig config = ScriptableObject.CreateInstance<AudioConfig>();
        config.masterMixer = mixer;
        config.audioCategories = new List<AudioCategory>
        {
            CreateCategory(AudioCategoryType.Music, MusicVolume, "Music", true, music, groups),
            CreateCategory(AudioCategoryType.SFX, SfxVolume, "SFX", false, sfx, groups),
        };
        return config;
    }

    private static AudioCategory CreateCategory(AudioCategoryType type, float volume, string tag, bool runInBaseAudio,
        AudioData data, AudioMixerGroup[] groups)
    {
        return new AudioCategory
        {
            type = type,
            categoryVolume = volume,
            audioCollections = new List<AudioCollection>
            {
                new AudioCollection
                {
                    tag = tag,
                    runInBaseAudio = runInBaseAudio,
                    audioData = data,
                    audioMixer = groups.FirstOrDefault(x => x.name == tag),
                },
            },
        };
    }

    private static AudioData CreateData(string path)
    {
        if (!Exists(path))
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AudioData>(), path);

        return AssetDatabase.LoadAssetAtPath<AudioData>(path);
    }

    private static bool Exists(string path)
    {
        return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            AssetDatabase.CreateFolder(parent, name);
    }
}
