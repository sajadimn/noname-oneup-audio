Audio Unity Package
===

## Description
This package contains the audio manager with categories, mixer control and pooled audio sources. Sounds stay in your project, never in this package.

## Dependencies
- [noname-oneup-singleton](https://github.com/sajadimn/noname-oneup-singleton). Add it to `Packages/manifest.json` before this package, or it cannot compile.
- [noname-oneup-dotween](https://github.com/sajadimn/noname-oneup-dotween). Without it, mixer changes are instant instead of a 1 second fade.

## How to use
1. Add this package. It creates `AudioMixer`, `Music`, `SFX` and `Resources/AudioConfig` in `Assets/Settings/Audio`.
2. Add your sounds to `Music` and `SFX`.
3. Call `AudioManager.Instance.Play(AudioCategoryType.SFX, "YourSoundName");`. No prefab or scene object is needed.
