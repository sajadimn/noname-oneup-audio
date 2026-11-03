using UnityEditor;

public static class AudioConfigMenu
{
    public static readonly string[] Paths =
    {
        "Assets/Settings/Audio/Resources/AudioConfig.asset",
        "Assets/Settings/Audio/Music.asset",
        "Assets/Settings/Audio/SFX.asset",
    };

    [MenuItem("OneUp/Audio/Configs/Audio Config")]
    private static void OpenAudioConfig()
    {
        Open("Assets/Settings/Audio/Resources/AudioConfig.asset");
    }

    [MenuItem("OneUp/Audio/Configs/Music")]
    private static void OpenMusic()
    {
        Open("Assets/Settings/Audio/Music.asset");
    }

    [MenuItem("OneUp/Audio/Configs/SFX")]
    private static void OpenSFX()
    {
        Open("Assets/Settings/Audio/SFX.asset");
    }

    private static void Open(string path)
    {
        AudioConfigGenerator.Generate();
        UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
