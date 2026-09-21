using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class GameAudioManager : MonoBehaviour
{
    private sealed class SfxVoice
    {
        public AudioSource Source { get; }
        public GameAudioPriority Priority { get; private set; }
        public float StartedAt { get; private set; }

        public bool IsAvailable => !Source.isPlaying;

        public SfxVoice(AudioSource source)
        {
            Source = source;
        }

        public void Play(AudioClip clip, float volume, GameAudioPriority priority, float startedAt)
        {
            Source.Stop();
            Source.clip = clip;
            Source.volume = volume;
            Priority = priority;
            StartedAt = startedAt;
            Source.Play();
        }

        public void Stop()
        {
            Source.Stop();
            Source.clip = null;
            Priority = GameAudioPriority.Low;
            StartedAt = 0f;
        }
    }

    private const string ConfigResourcePath = "Configs/GameAudioConfig";
    private const string SoundPreferenceKey = "Setting_Sound";

    public static GameAudioManager Instance { get; private set; }

    private GameAudioConfigSO config;
    private AudioSource bgmSource;
    private Transform sfxPoolRoot;
    private GameSettingsManager subscribedSettings;
    private Button pressedButton;
    private readonly List<RaycastResult> raycastResults = new();
    private readonly Dictionary<GameAudioCue, float> lastSfxPlayedAt = new();
    private readonly List<SfxVoice> sfxVoices = new();
    private bool bgmPausedBySetting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateRuntimeInstance()
    {
        if (Instance != null)
            return;

        GameObject root = new("GameAudioManager");
        root.AddComponent<GameAudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        config = Resources.Load<GameAudioConfigSO>(ConfigResourcePath);
        bgmSource = CreateSource("BGM", true);
        CreateSfxPool();
        SceneManager.sceneLoaded += HandleSceneLoaded;

        ApplySoundEnabled(ReadSoundPreference());
    }

    private void Start()
    {
        BindSettings();
        PlaySceneBgm(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (subscribedSettings == null)
            BindSettings();

        DetectButtonClick();
    }

    public void PlaySfx(GameAudioCue cue)
    {
        if (!IsSoundEnabled() || config == null)
            return;

        if (!config.TryGetSfx(
                cue,
                out AudioClip clip,
                out float volume,
                out float minInterval,
                out GameAudioPriority priority))
            return;

        float now = Time.unscaledTime;
        if (lastSfxPlayedAt.TryGetValue(cue, out float lastPlayedAt) && now < lastPlayedAt + minInterval)
            return;

        SfxVoice voice = AcquireSfxVoice(priority);
        if (voice == null)
            return;

        lastSfxPlayedAt[cue] = now;
        voice.Play(clip, volume, priority, now);
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = config != null ? config.BgmVolume : 1f;

        if (IsSoundEnabled())
            bgmSource.Play();

        bgmPausedBySetting = false;
    }

    public void StopBgm()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        GameObject sourceObject = new(sourceName);
        sourceObject.transform.SetParent(transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }

    private void CreateSfxPool()
    {
        GameObject poolObject = new("SFX Pool");
        poolObject.transform.SetParent(transform, false);
        sfxPoolRoot = poolObject.transform;

        int initialSize = config != null ? config.InitialSfxPoolSize : 8;
        for (int i = 0; i < initialSize; i++)
            CreateSfxVoice();
    }

    private SfxVoice CreateSfxVoice()
    {
        int voiceNumber = sfxVoices.Count + 1;
        GameObject voiceObject = new($"SFX Voice {voiceNumber:00}");
        voiceObject.transform.SetParent(sfxPoolRoot, false);

        AudioSource source = voiceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        SfxVoice voice = new(source);
        sfxVoices.Add(voice);
        return voice;
    }

    private SfxVoice AcquireSfxVoice(GameAudioPriority requestedPriority)
    {
        foreach (SfxVoice voice in sfxVoices)
        {
            if (voice.IsAvailable)
                return voice;
        }

        int maxPoolSize = config != null ? config.MaxSfxPoolSize : 16;
        if (sfxVoices.Count < maxPoolSize)
            return CreateSfxVoice();

        SfxVoice replacement = null;
        foreach (SfxVoice voice in sfxVoices)
        {
            if (voice.Priority > requestedPriority)
                continue;

            if (replacement == null ||
                voice.Priority < replacement.Priority ||
                (voice.Priority == replacement.Priority && voice.StartedAt < replacement.StartedAt))
            {
                replacement = voice;
            }
        }

        return replacement;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSettings();
        PlaySceneBgm(scene.name);
        pressedButton = null;
    }

    private void PlaySceneBgm(string sceneName)
    {
        if (config == null)
            return;

        AudioClip targetClip = config.GetBgm(sceneName);
        if (targetClip != null)
            PlayBgm(targetClip);
    }

    private void BindSettings()
    {
        GameSettingsManager settings = GameSettingsManager.Instance;
        if (settings == null || settings == subscribedSettings)
            return;

        UnbindSettings();
        subscribedSettings = settings;
        subscribedSettings.OnSoundChanged += ApplySoundEnabled;
        ApplySoundEnabled(settings.SoundEnabled);
    }

    private void UnbindSettings()
    {
        if (subscribedSettings != null)
            subscribedSettings.OnSoundChanged -= ApplySoundEnabled;

        subscribedSettings = null;
    }

    private void ApplySoundEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (bgmSource.isPlaying)
            {
                bgmSource.Pause();
                bgmPausedBySetting = true;
            }

            StopAllSfx();
            return;
        }

        if (bgmSource.clip == null || bgmSource.isPlaying)
            return;

        if (bgmPausedBySetting)
            bgmSource.UnPause();
        else
            bgmSource.Play();

        bgmPausedBySetting = false;
    }

    private void StopAllSfx()
    {
        foreach (SfxVoice voice in sfxVoices)
            voice.Stop();
    }

    private bool IsSoundEnabled()
    {
        return subscribedSettings != null
            ? subscribedSettings.SoundEnabled
            : ReadSoundPreference();
    }

    private static bool ReadSoundPreference()
    {
        return PlayerPrefs.GetInt(SoundPreferenceKey, 1) == 1;
    }

    private void DetectButtonClick()
    {
        if (Input.GetMouseButtonDown(0))
            pressedButton = FindButtonUnderPointer();

        if (!Input.GetMouseButtonUp(0))
            return;

        Button releasedButton = FindButtonUnderPointer();
        if (pressedButton != null && pressedButton == releasedButton && pressedButton.IsInteractable())
            PlaySfx(GameAudioCue.ButtonClick);

        pressedButton = null;
    }

    private Button FindButtonUnderPointer()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return null;

        PointerEventData pointerData = new(eventSystem) { position = Input.mousePosition };
        raycastResults.Clear();
        eventSystem.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();
            if (button != null)
                return button;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindSettings();
        Instance = null;
    }
}
