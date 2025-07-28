using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Singleton AudioManager that handles AudioMixer groups (Music and SFX) with DOTween transitions.
/// Provides smooth volume transitions with duration, target value, and delay parameters.
/// </summary>
public class BabyProofxrAudioManager : MonoBehaviour
{
    [Header("Audio Mixer Configuration")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicGroupName = "Music";
    [SerializeField] private string sfxGroupName = "SFX";

    [Header("Default Transition Settings")]
    [SerializeField] private float defaultTransitionDuration = 1f;
    [SerializeField] private float defaultDelay = 0f;

    // Singleton instance
    private static BabyProofxrAudioManager _instance;
    public static BabyProofxrAudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BabyProofxrAudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BabyProofxrAudioManager");
                    _instance = go.AddComponent<BabyProofxrAudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // Current volume levels (in dB)
    private float _currentMusicVolume = 0f;
    private float _currentSFXVolume = 0f;

    // Active tweens for volume transitions
    private Tween _musicVolumeTween;
    private Tween _sfxVolumeTween;

    // Properties for current volume levels
    public float CurrentMusicVolume => _currentMusicVolume;
    public float CurrentSFXVolume => _currentSFXVolume;

    private void Awake()
    {
        // Singleton pattern implementation
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager()
    {
        // Validate AudioMixer reference
        if (audioMixer == null)
        {
            Debug.LogError("BabyProofxrAudioManager: AudioMixer reference is missing!");
            return;
        }

        // Get initial volume levels from the mixer
        audioMixer.GetFloat(musicGroupName, out _currentMusicVolume);
        audioMixer.GetFloat(sfxGroupName, out _currentSFXVolume);

        Debug.Log($"BabyProofxrAudioManager initialized - Music: {_currentMusicVolume}dB, SFX: {_currentSFXVolume}dB");
    }

    #region Music Volume Control

    /// <summary>
    /// Transitions the Music group volume to the target value over the specified duration.
    /// </summary>
    /// <param name="targetVolume">Target volume in dB (typically -80 to 0)</param>
    /// <param name="duration">Transition duration in seconds</param>
    /// <param name="delay">Delay before starting the transition</param>
    /// <param name="easeType">Easing type for the transition</param>
    /// <param name="onComplete">Optional callback to execute when the transition completes</param>
    public void TransitionMusicVolume(float targetVolume, float duration = -1f, float delay = -1f, Ease easeType = Ease.InOutQuad, System.Action onComplete = null)
    {
        if (audioMixer == null) return;

        duration = duration < 0 ? defaultTransitionDuration : duration;
        delay = delay < 0 ? defaultDelay : delay;

        // Kill existing tween if running
        _musicVolumeTween?.Kill();

        // Create new tween
        _musicVolumeTween = DOTween.To(
            () => _currentMusicVolume,
            (value) => {
                _currentMusicVolume = value;
                audioMixer.SetFloat(musicGroupName, value);
            },
            targetVolume,
            duration
        )
        .SetDelay(delay)
        .SetEase(easeType)
        .OnComplete(() => {
            _musicVolumeTween = null;
            onComplete?.Invoke();
        });

        Debug.Log($"Music volume transition: {_currentMusicVolume}dB → {targetVolume}dB over {duration}s (delay: {delay}s)");
    }

    /// <summary>
    /// Sets the Music group volume immediately without transition.
    /// </summary>
    /// <param name="volume">Target volume in dB</param>
    public void SetMusicVolumeImmediate(float volume)
    {
        if (audioMixer == null) return;

        _musicVolumeTween?.Kill();
        _musicVolumeTween = null;

        _currentMusicVolume = volume;
        audioMixer.SetFloat(musicGroupName, volume);

        Debug.Log($"Music volume set immediately to {volume}dB");
    }

    /// <summary>
    /// Fades the Music group volume to silence (mute).
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeMusicToSilence(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionMusicVolume(-80f, duration, delay, Ease.InOutQuad, onComplete);
    }

    /// <summary>
    /// Fades the Music group volume to full volume (0dB).
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeMusicToFull(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionMusicVolume(0f, duration, delay, Ease.InOutQuad, onComplete);
    }

    #endregion

    #region SFX Volume Control

    /// <summary>
    /// Transitions the SFX group volume to the target value over the specified duration.
    /// </summary>
    /// <param name="targetVolume">Target volume in dB (typically -80 to 0)</param>
    /// <param name="duration">Transition duration in seconds</param>
    /// <param name="delay">Delay before starting the transition</param>
    /// <param name="easeType">Easing type for the transition</param>
    /// <param name="onComplete">Optional callback to execute when the transition completes</param>
    public void TransitionSFXVolume(float targetVolume, float duration = -1f, float delay = -1f, Ease easeType = Ease.InOutQuad, System.Action onComplete = null)
    {
        if (audioMixer == null) return;

        duration = duration < 0 ? defaultTransitionDuration : duration;
        delay = delay < 0 ? defaultDelay : delay;

        // Kill existing tween if running
        _sfxVolumeTween?.Kill();

        // Create new tween
        _sfxVolumeTween = DOTween.To(
            () => _currentSFXVolume,
            (value) => {
                _currentSFXVolume = value;
                audioMixer.SetFloat(sfxGroupName, value);
            },
            targetVolume,
            duration
        )
        .SetDelay(delay)
        .SetEase(easeType)
        .OnComplete(() => {
            _sfxVolumeTween = null;
            onComplete?.Invoke();
        });

        Debug.Log($"SFX volume transition: {_currentSFXVolume}dB → {targetVolume}dB over {duration}s (delay: {delay}s)");
    }

    /// <summary>
    /// Sets the SFX group volume immediately without transition.
    /// </summary>
    /// <param name="volume">Target volume in dB</param>
    public void SetSFXVolumeImmediate(float volume)
    {
        if (audioMixer == null) return;

        _sfxVolumeTween?.Kill();
        _sfxVolumeTween = null;

        _currentSFXVolume = volume;
        audioMixer.SetFloat(sfxGroupName, volume);

        Debug.Log($"SFX volume set immediately to {volume}dB");
    }

    /// <summary>
    /// Fades the SFX group volume to silence (mute).
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeSFXToSilence(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionSFXVolume(-80f, duration, delay, Ease.InOutQuad, onComplete);
    }

    /// <summary>
    /// Fades the SFX group volume to full volume (0dB).
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeSFXToFull(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionSFXVolume(0f, duration, delay, Ease.InOutQuad, onComplete);
    }

    #endregion

    #region Global Volume Control

    /// <summary>
    /// Transitions both Music and SFX volumes simultaneously.
    /// </summary>
    /// <param name="musicVolume">Target music volume in dB</param>
    /// <param name="sfxVolume">Target SFX volume in dB</param>
    /// <param name="duration">Transition duration in seconds</param>
    /// <param name="delay">Delay before starting the transitions</param>
    /// <param name="easeType">Easing type for the transitions</param>
    /// <param name="onComplete">Optional callback to execute when both transitions complete</param>
    public void TransitionAllVolumes(float musicVolume, float sfxVolume, float duration = -1f, float delay = -1f, Ease easeType = Ease.InOutQuad, System.Action onComplete = null)
    {
        // Track completion of both transitions
        bool musicCompleted = false;
        bool sfxCompleted = false;

        System.Action checkCompletion = () => {
            if (musicCompleted && sfxCompleted)
            {
                onComplete?.Invoke();
            }
        };

        // Start music transition
        TransitionMusicVolume(musicVolume, duration, delay, easeType, () => {
            musicCompleted = true;
            checkCompletion();
        });

        // Start SFX transition
        TransitionSFXVolume(sfxVolume, duration, delay, easeType, () => {
            sfxCompleted = true;
            checkCompletion();
        });
    }

    /// <summary>
    /// Fades all audio to silence.
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeAllToSilence(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionAllVolumes(-80f, -80f, duration, delay, Ease.InOutQuad, onComplete);
    }

    /// <summary>
    /// Fades all audio to full volume.
    /// </summary>
    /// <param name="duration">Fade duration in seconds</param>
    /// <param name="delay">Delay before starting the fade</param>
    /// <param name="onComplete">Optional callback to execute when the fade completes</param>
    public void FadeAllToFull(float duration = -1f, float delay = -1f, System.Action onComplete = null)
    {
        TransitionAllVolumes(0f, 0f, duration, delay, Ease.InOutQuad, onComplete);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Stops all active volume transitions.
    /// </summary>
    public void StopAllTransitions()
    {
        _musicVolumeTween?.Kill();
        _sfxVolumeTween?.Kill();
        _musicVolumeTween = null;
        _sfxVolumeTween = null;

        Debug.Log("All volume transitions stopped");
    }

    /// <summary>
    /// Gets the current volume level for the specified group.
    /// </summary>
    /// <param name="groupName">Name of the audio group</param>
    /// <returns>Current volume in dB, or -80 if group not found</returns>
    public float GetGroupVolume(string groupName)
    {
        if (audioMixer == null) return -80f;

        float volume;
        if (audioMixer.GetFloat(groupName, out volume))
        {
            return volume;
        }

        Debug.LogWarning($"Audio group '{groupName}' not found in AudioMixer");
        return -80f;
    }

    /// <summary>
    /// Checks if a volume transition is currently active for the specified group.
    /// </summary>
    /// <param name="groupName">Name of the audio group ("Music" or "SFX")</param>
    /// <returns>True if a transition is active</returns>
    public bool IsTransitionActive(string groupName)
    {
        switch (groupName.ToLower())
        {
            case "music":
                return _musicVolumeTween != null && _musicVolumeTween.IsActive();
            case "sfx":
                return _sfxVolumeTween != null && _sfxVolumeTween.IsActive();
            default:
                Debug.LogWarning($"Unknown audio group: {groupName}");
                return false;
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up tweens when the object is destroyed
        _musicVolumeTween?.Kill();
        _sfxVolumeTween?.Kill();
    }
}
