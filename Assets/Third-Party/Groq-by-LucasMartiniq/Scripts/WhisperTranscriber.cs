using NaughtyAttributes;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

public class WhisperTranscriber : MonoBehaviour
{
    public string whisperEndpoint = "https://api.groq.com/openai/v1/audio/transcriptions";

    /*
    public MicRecorder recorder;

    public void Initialize(MicRecorder recorder)
    {
        this.recorder = recorder;
    }

    [Button]
    private void TranscribeLatestAudio()
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return;
        }

        StartCoroutine(Transcribe(recorder.GetLastFilePath(), OnTranscription));
    }
    */

    public IEnumerator Transcribe(string filePath, System.Action<string> onResult)
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            onResult?.Invoke(null);
            yield break;
        }

        byte[] audioData = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        form.AddField("model", "whisper-large-v3");
        form.AddField("language", "en");
        form.AddField("prompt", "Transcribe the following audio");
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post(whisperEndpoint, form);
        request.SetRequestHeader("Authorization", "Bearer " + APIKeyLoader.Config.groqKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Transcription failed: " + request.error);
            onResult?.Invoke(null);
        }
        else
        {
            string json = request.downloadHandler.text;
            string transcript = ParseTranscription(json);
            onResult?.Invoke(transcript);
        }
    }

    /// <summary>
    /// Async version of the Transcribe method using async/await pattern.
    /// </summary>
    /// <param name="filePath">Path to the audio file to transcribe.</param>
    /// <returns>Task containing the transcribed text.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when APIKeyLoader is not found.</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when audio file is not found.</exception>
    /// <exception cref="System.Exception">Thrown when transcription API call fails.</exception>
    public async Task<string> TranscribeAsync(string filePath)
    {
        if (APIKeyLoader.Instance == null)
        {
            throw new System.InvalidOperationException("APIKeyLoader instance not found!");
        }

        if (!File.Exists(filePath))
        {
            throw new System.IO.FileNotFoundException($"Audio file not found: {filePath}");
        }

        try
        {
            byte[] audioData = File.ReadAllBytes(filePath);

            WWWForm form = new WWWForm();
            form.AddField("model", "whisper-large-v3");
            form.AddField("language", "en");
            form.AddField("prompt", "Transcribe the following audio");
            form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");

            UnityWebRequest request = UnityWebRequest.Post(whisperEndpoint, form);
            request.SetRequestHeader("Authorization", "Bearer " + APIKeyLoader.Config.groqKey);

            // Send the request asynchronously
            var operation = request.SendWebRequest();

            // Wait for the operation to complete
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new System.Exception($"Transcription failed: {request.error}");
            }

            string json = request.downloadHandler.text;
            string transcript = ParseTranscription(json);
            
            Debug.Log($"Transcription completed: {transcript}");
            return transcript;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during transcription: {ex.Message}");
            throw; // Re-throw the exception to allow calling code to handle it
        }
    }

    string ParseTranscription(string json)
    {
        int start = json.IndexOf("\"text\":\"") + 8;
        int end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }

    void OnTranscription(string text)
    {
        Debug.Log("Transcription:" + text);
    }
}
