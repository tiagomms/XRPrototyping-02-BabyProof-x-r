using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages microphone recording state across all MicRecorder instances.
/// Prevents multiple simultaneous recordings and provides events for UI updates.
/// </summary>
public class MicRecorderManager : MonoBehaviour
{
    // Singleton instance
    private static MicRecorderManager instance;
    public static MicRecorderManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MicRecorderManager>();
            }
            return instance;
        }
    }

    [Header("Recording Settings")]
    [SerializeField] private int duration = 30; // seconds - centralized duration for all recorders

    [Header("Recording State")]
    [SerializeField] private bool isAnyRecording = false;
    [SerializeField] private MicRecorder currentRecordingRecorder = null;

    // Events for UI and other systems
    public UnityEvent<MicRecorder> onRecordingStarted;
    public UnityEvent<MicRecorder> onRecordingStopped;
    public UnityEvent<MicRecorder> onRecordingBlocked;
    public UnityEvent<int> onDurationChanged; // New event for duration changes

    // List of all registered recorders
    private HashSet<MicRecorder> registeredRecorders = new HashSet<MicRecorder>();

    /// <summary>
    /// Gets the current recording duration in seconds.
    /// </summary>
    public int Duration => duration;

    void Start()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError("[MicRecorderManager] Multiple MicRecorderManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[MicRecorderManager] Initialized successfully with duration: {duration}s");
    }

    /// <summary>
    /// Sets the recording duration for all registered recorders.
    /// </summary>
    /// <param name="newDuration">The new duration in seconds.</param>
    public void SetDuration(int newDuration)
    {
        if (newDuration <= 0)
        {
            Debug.LogWarning("[MicRecorderManager] Duration must be greater than 0 seconds");
            return;
        }

        if (newDuration == duration)
        {
            return; // No change needed
        }

        int oldDuration = duration;
        duration = newDuration;

        // Update all registered recorders
        UpdateAllRecorderDurations();

        // Invoke duration change event
        onDurationChanged?.Invoke(duration);

        Debug.Log($"[MicRecorderManager] Duration changed from {oldDuration}s to {duration}s");
    }

    /// <summary>
    /// Updates the duration of all registered recorders to match the manager's duration.
    /// </summary>
    private void UpdateAllRecorderDurations()
    {
        foreach (var recorder in registeredRecorders)
        {
            if (recorder != null)
            {
                recorder.SetDuration(duration);
            }
        }
    }

    /// <summary>
    /// Registers a MicRecorder with the manager.
    /// </summary>
    /// <param name="recorder">The MicRecorder to register.</param>
    public void RegisterRecorder(MicRecorder recorder)
    {
        if (recorder == null)
        {
            Debug.LogWarning("[MicRecorderManager] Attempted to register null recorder");
            return;
        }

        registeredRecorders.Add(recorder);
        
        // Set the recorder's duration to match the manager's duration
        recorder.SetDuration(duration);
        
        Debug.Log($"[MicRecorderManager] Registered recorder: {recorder.name}. Total recorders: {registeredRecorders.Count}");
    }

    /// <summary>
    /// Unregisters a MicRecorder from the manager.
    /// </summary>
    /// <param name="recorder">The MicRecorder to unregister.</param>
    public void UnregisterRecorder(MicRecorder recorder)
    {
        if (recorder == null) return;

        registeredRecorders.Remove(recorder);
        
        // If this was the current recording recorder, clear it
        if (currentRecordingRecorder == recorder)
        {
            currentRecordingRecorder = null;
            isAnyRecording = false;
        }

        Debug.Log($"[MicRecorderManager] Unregistered recorder: {recorder.name}. Total recorders: {registeredRecorders.Count}");
    }

    /// <summary>
    /// Requests to start recording with a specific recorder.
    /// </summary>
    /// <param name="requestingRecorder">The recorder requesting to start recording.</param>
    /// <returns>True if recording can start, false if blocked.</returns>
    public bool RequestStartRecording(MicRecorder requestingRecorder)
    {
        if (requestingRecorder == null)
        {
            Debug.LogWarning("[MicRecorderManager] Attempted to start recording with null recorder");
            return false;
        }

        if (isAnyRecording)
        {
            Debug.LogWarning($"[MicRecorderManager] Recording blocked - {currentRecordingRecorder?.name} is already recording");
            onRecordingBlocked?.Invoke(requestingRecorder);
            return false;
        }

        // Start recording
        isAnyRecording = true;
        currentRecordingRecorder = requestingRecorder;
        
        onRecordingStarted?.Invoke(requestingRecorder);
        Debug.Log($"[MicRecorderManager] Recording started with: {requestingRecorder.name}");
        
        return true;
    }

    /// <summary>
    /// Notifies the manager that recording has stopped.
    /// </summary>
    /// <param name="stoppingRecorder">The recorder that stopped recording.</param>
    public void NotifyRecordingStopped(MicRecorder stoppingRecorder)
    {
        if (stoppingRecorder == null) return;

        if (currentRecordingRecorder == stoppingRecorder)
        {
            isAnyRecording = false;
            currentRecordingRecorder = null;
            
            onRecordingStopped?.Invoke(stoppingRecorder);
            Debug.Log($"[MicRecorderManager] Recording stopped: {stoppingRecorder.name}");
        }
        else
        {
            Debug.LogWarning($"[MicRecorderManager] Received stop notification from non-current recorder: {stoppingRecorder.name}");
        }
    }

    /// <summary>
    /// Gets whether any recorder is currently recording.
    /// </summary>
    /// <returns>True if any recorder is recording, false otherwise.</returns>
    public bool IsAnyRecording()
    {
        return isAnyRecording;
    }

    /// <summary>
    /// Gets the currently recording recorder, if any.
    /// </summary>
    /// <returns>The currently recording recorder, or null if none.</returns>
    public MicRecorder GetCurrentRecordingRecorder()
    {
        return currentRecordingRecorder;
    }

    /// <summary>
    /// Gets all registered recorders.
    /// </summary>
    /// <returns>Collection of all registered recorders.</returns>
    public IReadOnlyCollection<MicRecorder> GetAllRecorders()
    {
        return registeredRecorders;
    }

    /// <summary>
    /// Gets the number of registered recorders.
    /// </summary>
    /// <returns>Number of registered recorders.</returns>
    public int GetRecorderCount()
    {
        return registeredRecorders.Count;
    }

    /// <summary>
    /// Checks if a specific recorder is currently recording.
    /// </summary>
    /// <param name="recorder">The recorder to check.</param>
    /// <returns>True if the recorder is currently recording, false otherwise.</returns>
    public bool IsRecorderRecording(MicRecorder recorder)
    {
        return currentRecordingRecorder == recorder && isAnyRecording;
    }

    /// <summary>
    /// Forces all recorders to stop recording.
    /// </summary>
    public void ForceStopAllRecordings()
    {
        if (!isAnyRecording) return;

        Debug.LogWarning("[MicRecorderManager] Force stopping all recordings");
        
        foreach (var recorder in registeredRecorders)
        {
            if (recorder != null && recorder.IsRecording())
            {
                recorder.StopAndSave(false);
            }
        }

        isAnyRecording = false;
        currentRecordingRecorder = null;
    }

    void OnDestroy()
    {
        // Clean up events
        onRecordingStarted?.RemoveAllListeners();
        onRecordingStopped?.RemoveAllListeners();
        onRecordingBlocked?.RemoveAllListeners();
        onDurationChanged?.RemoveAllListeners();

        // Clear the singleton instance when destroyed
        if (instance == this)
        {
            instance = null;
        }

    }
} 