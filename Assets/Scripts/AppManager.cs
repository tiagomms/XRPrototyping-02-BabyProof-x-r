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
        OnAppStarted?.Invoke();
        OnAppStateChanged?.Invoke(true);

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
        // Fade out any existing sounds and fade in new ones
        BabyProofxrAudioManager.Instance.FadeAllToSilence();
        BabyProofxrAudioManager.Instance.FadeSFXToFull();

        _deactivateSound.SafeStop();

        // Play ambient and activate sounds with delay
        _ambientSound.SafePlayDelayed(0.5f);
        _activateSound.SafePlayDelayed(0.5f);

        // Fade music to full volume
        BabyProofxrAudioManager.Instance.FadeMusicToFull(duration: 1.0f, delay: 2.0f);
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
