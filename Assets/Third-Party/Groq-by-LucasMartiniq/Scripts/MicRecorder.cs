using UnityEngine;
using System.IO;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Events;
using System;

public class MicRecorder : MonoBehaviour
{
    protected AudioClip recordedClip;
    protected string micDevice;
    protected string filePath;

    [Header("Mic Settings")]
    private int duration = 60; // seconds
    public int sampleRate = 44100;

    // TODO: unsure if i should separate onRecordedAudio from TimePassed
    public UnityEvent<AudioClip> onRecordedAudio;
    public UnityEvent<AudioClip> onDurationPassed;

    // Recording state tracking
    private bool isRecording = false;
    private float recordingStartTime = 0f;
    private bool hasTriggeredOverRecord = false;

    private float _startTime;


    protected void Start()
    {
        micDevice = Microphone.devices[0];

        // Register with the MicRecorderManager
        try
        {
            MicRecorderManager.Instance.RegisterRecorder(this);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{nameof(MicRecorder)}] Failed to register with MicRecorderManager: {ex.Message}");
        }
    }

    [Button]
    public void StartRecording()
    {
        // Request permission from MicRecorderManager
        if (!MicRecorderManager.Instance.RequestStartRecording(this))
        {
            Debug.LogWarning($"[{nameof(MicRecorder)}] Recording blocked - another recorder is already active");
            return;
        }

        recordingStartTime = Time.time;
        recordedClip = Microphone.Start(micDevice, false, duration, sampleRate);
        recordedClip.name = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        isRecording = true;
        hasTriggeredOverRecord = false;
        Debug.Log("Recording...");
    }

    /// <summary>
    /// Method that stops recording, sets audioclip and tries to save it (setting filePath)
    /// </summary>
    /// <param name="invokeOnRecordedAudio">Triggers on Recorded Audio event if true</param>
    /// <param name="saveClipInRootPath">If != None, then it will save in persistent or temporaryPath. Note we should always save to run speech-to-text AI. Default: temporary</param>
    /// <param name="relativePath">Relative path (folder). Ignored if saveClipInRootPath = None.</param>
    /// <param name="filename">Filename base. Ignored if saveClipInRootPath = None.</param>
    /// <param name="appendDateTimeToFileName">If true, adds DataTime to filename for differentiation. Ignored if saveClipInRootPath = None.</param>
    [Button]
    public virtual void StopAndSave(bool invokeOnRecordedAudio = true, FileEnumPath saveClipInRootPath = FileEnumPath.Temporary, string relativePath = "Recordings", string filename = "recorded_audio", bool appendDateTimeToFileName = true)
    {
        if (!isRecording)
        {
            Debug.LogWarning($"[{nameof(MicRecorder)}] Attempted to stop recording when not recording");
            return;
        }

        Microphone.End(micDevice);
        isRecording = false;
        // Trim Clips when you stop the recording before time
        if (!hasTriggeredOverRecord)
        {
            recordedClip = TrimClip(recordedClip, GetCurrentRecordingTime());
        }

        hasTriggeredOverRecord = false;

        // Notify the manager that recording has stopped
        MicRecorderManager.Instance.NotifyRecordingStopped(this);

        Debug.Log("Recording stopped.");

        // storing filePath
        filePath = recordedClip.TrySaveWav(saveClipInRootPath, relativePath, filename, appendDateTimeToFileName, $"{nameof(MicRecorder)} {nameof(StopAndSave)}");

        if (invokeOnRecordedAudio)
            onRecordedAudio.Invoke(recordedClip);
    }

    void Update()
    {
        // Check if recording has exceeded the duration limit
        if (isRecording && !hasTriggeredOverRecord)
        {
            if (GetCurrentRecordingTime() >= duration)
            {
                hasTriggeredOverRecord = true;
                StopAndSave(false);
                onDurationPassed.Invoke(recordedClip);
                Debug.LogWarning($"[{nameof(MicRecorder)}] Recording exceeded duration limit of {duration} seconds!");
            }
        }
    }

    private AudioClip TrimClip(AudioClip clip, float length)
    {
        // Calculate the number of samples to keep
        int samples = Mathf.CeilToInt(clip.frequency * length);
        if (samples <= 0 || samples > clip.samples)
        {
            Debug.LogWarning($"Invalid sample count: {samples}. Using original clip.");
            return clip;
        }

        // Create arrays for each channel
        float[] data = new float[samples * clip.channels];

        // Get the audio data
        if (!clip.GetData(data, 0))
        {
            Debug.LogError("Failed to get audio data from clip");
            return clip;
        }

        // Create the new clip
        AudioClip trimmedClip = AudioClip.Create(
            $"{clip.name}_trimmed",
            samples,
            clip.channels,
            clip.frequency,
            false
        );

        // Set the data
        if (!trimmedClip.SetData(data, 0))
        {
            Debug.LogError("Failed to set audio data to trimmed clip");
            return clip;
        }

        return trimmedClip;
    }

    /// <summary>
    /// Gets whether the microphone is currently recording.
    /// </summary>
    /// <returns>True if recording, false otherwise.</returns>
    public bool IsRecording()
    {
        return isRecording;
    }

    /// <summary>
    /// Gets the current recording time in seconds.
    /// </summary>
    /// <returns>Current recording time, or 0 if not recording.</returns>
    public float GetCurrentRecordingTime()
    {
        if (!isRecording)
        {
            return 0f;
        }

        return Time.time - recordingStartTime;
    }

    /// <summary>
    /// Gets the remaining recording time in seconds.
    /// </summary>
    /// <returns>Remaining recording time, or 0 if not recording or exceeded limit.</returns>
    public float GetRemainingRecordingTime()
    {
        if (!isRecording)
        {
            return 0f;
        }

        float remaining = duration - GetCurrentRecordingTime();
        return Mathf.Max(0f, remaining);
    }

    /// <summary>
    /// Gets whether the recording has exceeded the duration limit.
    /// </summary>
    /// <returns>True if recording has exceeded duration, false otherwise.</returns>
    public bool HasExceededDuration()
    {
        return hasTriggeredOverRecord;
    }

    /// <summary>
    /// Sets the recording duration. This method is called by MicRecorderManager.
    /// </summary>
    /// <param name="newDuration">The new duration in seconds.</param>
    public void SetDuration(int newDuration)
    {
        if (newDuration <= 0)
        {
            Debug.LogWarning("[{nameof(MicRecorder)}] Duration must be greater than 0 seconds");
            return;
        }

        if (newDuration == duration)
        {
            return; // No change needed
        }

        int oldDuration = duration;
        duration = newDuration;

        Debug.Log($"[{nameof(MicRecorder)}] Duration updated from {oldDuration}s to {duration}s");
    }

    /// <summary>
    /// Checks if this recorder is the one currently recording.
    /// </summary>
    /// <returns>True if this recorder is the active one, false otherwise.</returns>
    public bool IsActiveRecorder()
    {
        return MicRecorderManager.Instance.IsRecorderRecording(this);
    }

#if UNITY_EDITOR
    // NOTE: Only works in UNITY_EDITOR - for test mode usage only
    public void OverrideAudioClipAndPath(AudioClip clip)
    {
        recordedClip = clip;
        filePath = AssetDatabase.GetAssetPath(clip);
        Debug.Log($"[{nameof(MicRecorder)}] - TEST MODE: OverrideAudioClipAndPath by {clip.name}");
    }
#endif

    public string GetLastFilePath() => filePath;

    public AudioClip GetLastAudioClip() => recordedClip;

    void OnDestroy()
    {
        // Unregister from the MicRecorderManager
        try
        {
            if (MicRecorderManager.Instance != null)
            {
                MicRecorderManager.Instance.UnregisterRecorder(this);
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[{nameof(MicRecorder)}] Failed to unregister from MicRecorderManager: {ex.Message}");
        }

        // Clean up events
        onRecordedAudio?.RemoveAllListeners();
        onDurationPassed?.RemoveAllListeners();
    }
}
