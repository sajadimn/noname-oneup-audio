using UnityEngine;

public static class AudioVolumeMath
{
    private const float MinimumSliderVolume = 0.0001f;
    private const float DecibelsPerLogUnit = 20f;

    public static float ToDecibels(float sliderVolume, float categoryVolume, float muteVolume)
    {
        float volume = Mathf.Clamp(sliderVolume, MinimumSliderVolume, 1f);
        return Mathf.Max(categoryVolume + Mathf.Log10(volume) * DecibelsPerLogUnit, muteVolume);
    }
}
