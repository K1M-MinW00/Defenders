using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioConfig", menuName = "Game/Audio/Game Audio Config")]
public class GameAudioConfigSO : ScriptableObject
{
    [Serializable]
    private struct SfxEntry
    {
        public GameAudioCue cue;
        public AudioClipSettings audio;
        [Min(0f)] public float minInterval;
        public GameAudioPriority priority;
    }

    [Header("Scene Names")]
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string battleSceneName = "GameScene";

    [Header("BGM")]
    [SerializeField] private AudioClipSettings startAndLobbyBgm = new();
    [SerializeField] private AudioClipSettings battleBgm = new();

    [Header("SFX")]
    [SerializeField, Min(1)] private int initialSfxPoolSize = 8;
    [SerializeField, Min(1)] private int maxSfxPoolSize = 16;
    [SerializeField] private SfxEntry[] sfxEntries = Array.Empty<SfxEntry>();

    public int InitialSfxPoolSize => Mathf.Max(1, initialSfxPoolSize);
    public int MaxSfxPoolSize => Mathf.Max(InitialSfxPoolSize, maxSfxPoolSize);

    public AudioClipSettings GetBgm(string sceneName)
    {
        if (sceneName == startSceneName || sceneName == lobbySceneName)
            return startAndLobbyBgm;

        return sceneName == battleSceneName ? battleBgm : null;
    }

    public bool TryGetSfx(
        GameAudioCue cue,
        out AudioClipSettings audio,
        out float minInterval,
        out GameAudioPriority priority)
    {
        foreach (SfxEntry entry in sfxEntries)
        {
            if (entry.cue != cue || entry.audio == null || !entry.audio.IsValid)
                continue;

            audio = entry.audio;
            minInterval = entry.minInterval;
            priority = entry.priority;
            return true;
        }

        audio = null;
        minInterval = 0f;
        priority = GameAudioPriority.Low;
        return false;
    }
}
