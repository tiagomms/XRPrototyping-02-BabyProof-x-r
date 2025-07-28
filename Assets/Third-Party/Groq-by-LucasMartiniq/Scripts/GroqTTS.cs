using NaughtyAttributes;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public enum PlayAIVoice
{
    Aaliyah_PlayAI,
    Adelaide_PlayAI,
    Angelo_PlayAI,
    Arista_PlayAI,
    Atlas_PlayAI,
    Basil_PlayAI,
    Briggs_PlayAI,
    Calum_PlayAI,
    Celeste_PlayAI,
    Cheyenne_PlayAI,
    Chip_PlayAI,
    Cillian_PlayAI,
    Deedee_PlayAI,
    Eleanor_PlayAI,
    Fritz_PlayAI,
    Gail_PlayAI,
    Indigo_PlayAI,
    Jennifer_PlayAI,
    Judy_PlayAI,
    Mamaw_PlayAI,
    Mason_PlayAI,
    Mikail_PlayAI,
    Mitch_PlayAI,
    Nia_PlayAI,
    Quinn_PlayAI,
    Ruby_PlayAI,
    Thunder_PlayAI
}

public class GroqTTS : MonoBehaviour
{
    private const string apiUrl = "https://api.groq.com/openai/v1/audio/speech";

    private const string model = "playai-tts";

    [HideInInspector]
    [SerializeField] private PlayAIVoice selectedVoice = PlayAIVoice.Fritz_PlayAI;
    // Add a virtual property
    public virtual PlayAIVoice SelectedVoice
    {
        get => selectedVoice;
        set  { selectedVoice = value; onVoiceChange.Invoke(value); }
    }
    private const string responseFormat = "wav";

    public AudioClip GeneratedClip { get; private set; }

    [TextArea(5, 20)]
    [SerializeField] private string prompt = "I love building and shipping new features for our students!";
    public string Prompt => prompt;

    public UnityEvent<PlayAIVoice> onVoiceChange;
    public UnityEvent<AudioClip> onCompletedTTS;
    public UnityEvent onErrorTTS;

    public bool HasFinishedGeneratingClip { get; private set; }

    /// <summary>
    /// Generates TTS audio from the given text, but does not play it. Sets IsGenerated and audioSource.clip. 
    /// </summary>
    /// <param name="text">The text to synthesize and play</param>
    /// <param name="saveClipInRootPath">If != None, then it will save in persistent or temporaryPath</param>
    /// <param name="relativePath">Relative path (folder). Ignored if saveClipInRootPath = None.</param>
    /// <param name="filename">Filename base. Ignored if saveClipInRootPath = None.</param>
    /// <param name="appendDateTimeToFileName">If true, adds DataTime to filename for differentiation. Ignored if saveClipInRootPath = None.</param>
    /// <param name="voiceOverride">If not null, changes Selected Voice for TTS.</param>
    /// <returns>Task</returns>
    public async Task GenerateTTS(string text, FileEnumPath saveClipInRootPath, string relativePath = "TtS", string filename = "tts", bool appendDateTimeToFileName = true, PlayAIVoice? voiceOverride = null)
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] APIKeyLoader instance not found!");
            return;
        }

        if (voiceOverride is PlayAIVoice playAIVoice)
        {
            SelectedVoice = playAIVoice;
        }

        Debug.Log($"[{nameof(GroqTTS)}] Received text: {text}");
        var json = $"{{\"model\":\"{model}\",\"voice\":\"{GetVoiceName(SelectedVoice)}\",\"input\":\"{text}\",\"response_format\":\"{responseFormat}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HasFinishedGeneratingClip = false;

            var response = await APIKeyLoader.Instance.GroqHttpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"[{nameof(GroqTTS)}] TTS API call failed: {response.StatusCode}\nFor text: {text}");
                Debug.LogError($"[{nameof(GroqTTS)}] Error content: {errorContent}");
                return;
            }

            byte[] audioData = await response.Content.ReadAsByteArrayAsync();
            Debug.Log($"[{nameof(GroqTTS)}] Received audio data: {audioData?.Length ?? 0} bytes");

            if (audioData == null || audioData.Length == 0)
            {
                Debug.LogError($"[{nameof(GroqTTS)}] Audio data is null or empty!");
                return;
            }

            GeneratedClip = CreateClipFromWav(audioData);

            if (GeneratedClip == null)
            {
                Debug.LogError($"[{nameof(GroqTTS)}] GeneratedClip is null after CreateClipFromWav!\nFor text: {text}");
                return;
            }

            Debug.Log($"[{nameof(GroqTTS)}] Audio clip created - Length: {GeneratedClip.length}, Channels: {GeneratedClip.channels}, Frequency: {GeneratedClip.frequency}");

            if (GeneratedClip.length <= 0)
            {
                Debug.LogError($"[{nameof(GroqTTS)}] Generated clip length is {GeneratedClip.length} - this will cause the 'Length of created clip must be larger than 0' error!\nFor text: {text}");
            }

            string wavFilepath = GeneratedClip.TrySaveWav(saveClipInRootPath, relativePath, filename, appendDateTimeToFileName, $"{nameof(GroqTTS)} - {nameof(GenerateTTS)}");

            prompt = text;
            HasFinishedGeneratingClip = true;

            onCompletedTTS.Invoke(GeneratedClip);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] Error generating TTS: {ex.Message}");
            onErrorTTS.Invoke();
        }
    }


    /// <summary>
    /// Generates TTS audio and plays it if successful. 
    /// </summary>
    /// <param name="text">The text to synthesize and play</param>
    /// <param name="saveClipInRootPath">If != None, then it will save in persistent or temporaryPath</param>
    /// <param name="relativePath">Relative path (folder). Ignored if saveClipInRootPath = None.</param>
    /// <param name="filename">Filename base. Ignored if saveClipInRootPath = None.</param>
    /// <param name="appendDateTimeToFileName">If true, adds DataTime to filename for differentiation. Ignored if saveClipInRootPath = None.</param>
    /// <returns>Task - itself</returns>
    public async Task<GroqTTS> GenerateAndPlaySpeech(string text, AudioSource audioSource, FileEnumPath saveClipInRootPath, string relativePath = "TtS", string filename = "tts", bool appendDateTimeToFileName = true)
    {
        if (audioSource == null)
        {
            Debug.LogError($"[{nameof(GenerateAndPlaySpeech)}] - audio source required to Play immediately");
            return null;
        }

        await GenerateTTS(text, saveClipInRootPath, relativePath, filename, appendDateTimeToFileName);
        if (HasFinishedGeneratingClip)
        {
            audioSource.Stop();
            audioSource.clip = GeneratedClip;
            audioSource.Play();
            return this;
        }
        return null;
    }

    private string GetVoiceName(PlayAIVoice voice)
    {
        return voice.ToString().Replace('_', '-');
    }

    private AudioClip CreateClipFromWav(byte[] wav)
    {
        if (wav == null || wav.Length == 0)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] WAV data is null or empty!");
            return null;
        }
        if (wav.Length < 44) // 44 is minimum valid WAV header size
        {
            Debug.LogError($"[{nameof(GroqTTS)}] WAV too short to contain valid header: {wav.Length} bytes");
            return null;
        }

        int channels = BitConverter.ToInt16(wav, 22);
        int sampleRate = BitConverter.ToInt32(wav, 24);
        int bitsPerSample = BitConverter.ToInt16(wav, 34);

        int dataStart = FindDataChunk(wav) + 8;
        int sampleCount = (wav.Length - dataStart) / (bitsPerSample / 8);

        if (sampleCount <= 0)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] Sample count is {sampleCount} - this will result in a zero-length clip!\nSample count calculation: ({wav.Length} - {dataStart}) / ({bitsPerSample} / 8) = {sampleCount}");
            Debug.LogError($"[{nameof(GroqTTS)}] WAV header values - Channels: {channels}, SampleRate: {sampleRate}, BitsPerSample: {bitsPerSample}");
            return null;
        }

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            int sampleIndex = dataStart + i * 2; // assuming 16-bit PCM
            if (sampleIndex + 1 >= wav.Length)
            {
                Debug.LogError($"[{nameof(GroqTTS)}] Sample index calculation: {dataStart} + {i} * 2 = {sampleIndex}");
                break;
            }
            short sample = BitConverter.ToInt16(wav, sampleIndex);
            samples[i] = sample / 32768f;
        }

        int samplesPerChannel = sampleCount / channels;

        if (samplesPerChannel <= 0)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] Samples per channel calculation: {sampleCount} / {channels} = {samplesPerChannel}");
            return null;
        }

        AudioClip clip = AudioClip.Create($"GroqTTS_Audio-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}", samplesPerChannel, channels, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private int FindDataChunk(byte[] wav)
    {
        if (wav == null || wav.Length < 4)
        {
            Debug.LogError($"[{nameof(GroqTTS)}] WAV data is null or too small for DATA chunk search");
            throw new Exception("WAV data is null or too small");
        }

        for (int i = 12; i < wav.Length - 4; i++)
        {
            if (wav[i] == 'd' && wav[i + 1] == 'a' && wav[i + 2] == 't' && wav[i + 3] == 'a')
            {
                return i;
            }
        }

        Debug.LogError($"[{nameof(GroqTTS)}] DATA chunk not found in WAV data\nWAV data length: {wav.Length} bytes\nFirst 20 bytes as ASCII: {System.Text.Encoding.ASCII.GetString(wav, 0, Math.Min(20, wav.Length))}");
        throw new Exception("DATA chunk not found in WAV");
    }

    public void SetVoice(PlayAIVoice newVoice)
    {
        SelectedVoice = newVoice;
    }

    public void SetPrompt(string newPrompt)
    {
        prompt = newPrompt;
    }

#if UNITY_EDITOR
    public void ForceIsGenerated()
    {
        HasFinishedGeneratingClip = true;
    }
#endif
}
