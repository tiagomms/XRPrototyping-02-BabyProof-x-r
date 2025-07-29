using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Example script demonstrating how to use MicRecorderManager for UI updates.
/// This shows how to handle recording state changes and blocked recording attempts.
/// </summary>
public class MicRecorderUIExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private Image recordingIndicator;

    [Header("Colors")]
    [SerializeField] private Color recordingColor = Color.red;
    [SerializeField] private Color blockedColor = Color.yellow;
    [SerializeField] private Color idleColor = Color.green;

    private MicRecorder localRecorder;

    void Start()
    {
        // Get the local recorder component
        localRecorder = GetComponent<MicRecorder>();
        
        if (localRecorder == null)
        {
            Debug.LogError("[MicRecorderUIExample] No MicRecorder component found!");
            return;
        }

        // Subscribe to MicRecorderManager events
        MicRecorderManager.Instance.onRecordingStarted.AddListener(OnRecordingStarted);
        MicRecorderManager.Instance.onRecordingStopped.AddListener(OnRecordingStopped);
        MicRecorderManager.Instance.onRecordingBlocked.AddListener(OnRecordingBlocked);

        // Subscribe to local recorder events
        localRecorder.onRecordedAudio.AddListener(OnAudioRecorded);
        localRecorder.onDurationPassed.AddListener(OnDurationPassed);

        // Setup button listeners
        if (recordButton != null)
            recordButton.onClick.AddListener(OnRecordButtonClicked);
        
        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopButtonClicked);

        // Initialize UI
        UpdateUI();
    }

    void Update()
    {
        UpdateTimeDisplay();
    }

    /// <summary>
    /// Called when any recorder starts recording.
    /// </summary>
    private void OnRecordingStarted(MicRecorder recorder)
    {
        Debug.Log($"[MicRecorderUIExample] Recording started by: {recorder.name}");
        UpdateUI();
    }

    /// <summary>
    /// Called when any recorder stops recording.
    /// </summary>
    private void OnRecordingStopped(MicRecorder recorder)
    {
        Debug.Log($"[MicRecorderUIExample] Recording stopped by: {recorder.name}");
        UpdateUI();
    }

    /// <summary>
    /// Called when a recording attempt is blocked.
    /// </summary>
    private void OnRecordingBlocked(MicRecorder recorder)
    {
        Debug.Log($"[MicRecorderUIExample] Recording blocked for: {recorder.name}");
        
        // Show blocked message
        if (statusText != null)
        {
            statusText.text = "Recording blocked - another recorder is active";
            statusText.color = blockedColor;
        }

        // Reset after a short delay
        Invoke(nameof(UpdateUI), 2f);
    }

    /// <summary>
    /// Called when this recorder finishes recording audio.
    /// </summary>
    private void OnAudioRecorded(AudioClip clip)
    {
        Debug.Log($"[MicRecorderUIExample] Audio recorded: {clip.name} ({clip.length:F1}s)");
        
        if (statusText != null)
        {
            statusText.text = $"Audio recorded: {clip.length:F1}s";
        }
    }

    /// <summary>
    /// Called when this recorder exceeds duration limit.
    /// </summary>
    private void OnDurationPassed(AudioClip clip)
    {
        Debug.Log($"[MicRecorderUIExample] Duration limit reached: {clip.name}");
        
        if (statusText != null)
        {
            statusText.text = "Duration limit reached!";
            statusText.color = blockedColor;
        }
    }

    /// <summary>
    /// Called when record button is clicked.
    /// </summary>
    private void OnRecordButtonClicked()
    {
        if (localRecorder != null)
        {
            localRecorder.StartRecording();
        }
    }

    /// <summary>
    /// Called when stop button is clicked.
    /// </summary>
    private void OnStopButtonClicked()
    {
        if (localRecorder != null && localRecorder.IsRecording())
        {
            localRecorder.StopAndSave();
        }
    }

    /// <summary>
    /// Updates the UI based on current recording state.
    /// </summary>
    private void UpdateUI()
    {
        bool isAnyRecording = MicRecorderManager.Instance.IsAnyRecording();
        bool isThisRecording = localRecorder != null && localRecorder.IsRecording();
        bool isThisActive = localRecorder != null && localRecorder.IsActiveRecorder();

        // Update buttons
        if (recordButton != null)
        {
            recordButton.interactable = !isAnyRecording;
        }

        if (stopButton != null)
        {
            stopButton.interactable = isThisRecording;
        }

        // Update status text
        if (statusText != null)
        {
            if (isThisRecording)
            {
                statusText.text = "Recording...";
                statusText.color = recordingColor;
            }
            else if (isAnyRecording)
            {
                statusText.text = "Another recorder is active";
                statusText.color = blockedColor;
            }
            else
            {
                statusText.text = "Ready to record";
                statusText.color = idleColor;
            }
        }

        // Update recording indicator
        if (recordingIndicator != null)
        {
            recordingIndicator.color = isThisActive ? recordingColor : idleColor;
        }

        // Update time slider
        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(isThisRecording);
        }
    }

    /// <summary>
    /// Updates the time display during recording.
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (localRecorder == null || !localRecorder.IsRecording()) return;

        float currentTime = localRecorder.GetCurrentRecordingTime();
        float remainingTime = localRecorder.GetRemainingRecordingTime();

        // Update time text
        if (timeText != null)
        {
            timeText.text = $"Time: {currentTime:F1}s / {MicRecorderManager.Instance.Duration}s";
        }

        // Update time slider
        if (timeSlider != null)
        {
            timeSlider.value = currentTime / MicRecorderManager.Instance.Duration;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (MicRecorderManager.Instance != null)
        {
            MicRecorderManager.Instance.onRecordingStarted.RemoveListener(OnRecordingStarted);
            MicRecorderManager.Instance.onRecordingStopped.RemoveListener(OnRecordingStopped);
            MicRecorderManager.Instance.onRecordingBlocked.RemoveListener(OnRecordingBlocked);
        }

        if (localRecorder != null)
        {
            localRecorder.onRecordedAudio.RemoveListener(OnAudioRecorded);
            localRecorder.onDurationPassed.RemoveListener(OnDurationPassed);
        }

        // Clean up button listeners
        if (recordButton != null)
            recordButton.onClick.RemoveListener(OnRecordButtonClicked);
        
        if (stopButton != null)
            stopButton.onClick.RemoveListener(OnStopButtonClicked);
    }
} 