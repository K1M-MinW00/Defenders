using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioConfig", menuName = "Game/Audio/Game Audio Config")]
public class GameAudioConfigSO : ScriptableObject
{
    [Serializable]
    private struct SfxEntry
    {
        public GameAudioCue cue;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
        [Min(0f)] public float minInterval;
    }

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string battleSceneName = "GameScene";

    [Header("BGM")]
    [SerializeField] private AudioClip startAndLobbyBgm;
    [SerializeField] private AudioClip battleBgm;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.7f;

    [Header("SFX")]
    [SerializeField] private SfxEntry[] sfxEntries = Array.Empty<SfxEntry>();

    public float BgmVolume => bgmVolume;

    public AudioClip GetBgm(string sceneName)
    {
        if (sceneName == startSceneName || sceneName == lobbySceneName)
            return startAndLobbyBgm;

        return sceneName == battleSceneName ? battleBgm : null;
    }

    public bool TryGetSfx(GameAudioCue cue, out AudioClip clip, out float volume, out float minInterval)
    {
        foreach (SfxEntry entry in sfxEntries)
        {
            if (entry.cue != cue || entry.clip == null)
                continue;

            clip = entry.clip;
            volume = entry.volume;
            minInterval = entry.minInterval;
            return true;
        }

        clip = null;
        volume = 0f;
        minInterval = 0f;
        return false;
    }
}
