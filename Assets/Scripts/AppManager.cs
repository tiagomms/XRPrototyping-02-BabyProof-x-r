using UnityEngine;
using UnityEngine.Events;
using Utils;


/// <summary>
/// Singleton manager responsible for app lifecycle, startup/shutdown sequences, and audio management.
/// </summary>
public class AppManager : MonoBehaviour
{
    public static AppManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _ambientSound;
    [SerializeField] private AudioSource _activateSound;
    [SerializeField] private AudioSource _deactivateSound;

    [Header("Events")]
    public UnityEvent OnAppStarted;
    public UnityEvent OnAppShutdown;
    public UnityEvent<bool> OnAppStateChanged;

    private bool _isAppRunning = false;

    /// <summary>
    /// Gets whether the app is currently running.
    /// </summary>
    public bool IsAppRunning => _isAppRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Starts the app and invokes the startup sequence.
    /// </summary>
    public void StartApp()
    {
        if (_isAppRunning) return;

        _isAppRunning = true;

        Debug.Log("App started - executing startup sequence");
        ExecuteStartupSequence();
    }

    /// <summary>
    /// Shuts down the app and invokes the shutdown sequence.
    /// </summary>
    public void ShutdownApp()
    {
        if (!_isAppRunning) return;

        _isAppRunning = false;
        OnAppShutdown?.Invoke();
        OnAppStateChanged?.Invoke(false);

        Debug.Log("App shutdown - executing shutdown sequence");
        ExecuteShutdownSequence();
    }

    /// <summary>
    /// Toggles the app state between running and shutdown.
    /// </summary>
    public void ToggleAppState()
    {
        if (_isAppRunning)
        {
            ShutdownApp();
        }
        else
        {
            StartApp();
        }
    }

    /// <summary>
    /// Executes the app startup sequence including audio management.
    /// </summary>
    private void ExecuteStartupSequence()
    {
        // Set music volume to null, and sfx volume to full
        BabyProofxrAudioManager.Instance.SetMusicVolumeImmediate(-80f);
        BabyProofxrAudioManager.Instance.SetSFXVolumeImmediate(0f);

        _deactivateSound.SafeStop();

        // Play ambient and activate sounds with delay
        _ambientSound.SafePlayDelayed(1f);
        _activateSound.SafePlayDelayed(1.5f);

        // Fade music to full volume and invoke events when complete
        BabyProofxrAudioManager.Instance.FadeMusicToFull(
            duration: 1.0f, 
            delay: 2.0f,
            onComplete: () => {
                // Invoke events after startup sequence completes (total ~2.5 seconds)
                OnAppStarted?.Invoke();
                OnAppStateChanged?.Invoke(true);
                Debug.Log("App startup sequence completed - events invoked");
            }
        );
    }

    /// <summary>
    /// Executes the app shutdown sequence including audio management.
    /// </summary>
    private void ExecuteShutdownSequence()
    {
        _activateSound.SafeStop();
        _deactivateSound.SafePlayDelayed(0.5f);

        // Fade all sounds to silence and stop ambient sound when complete
        BabyProofxrAudioManager.Instance.FadeAllToSilence(
            duration: 1.0f,
            delay: 0.5f,
            onComplete: () => { _ambientSound.SafeStop(); }
        );
    }
}
