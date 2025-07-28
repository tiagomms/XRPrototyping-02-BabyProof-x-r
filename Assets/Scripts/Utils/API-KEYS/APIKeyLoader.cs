using UnityEngine;
using System.Net.Http;
using GroqApiLibrary;

public class APIKeyLoader : MonoBehaviour
{
    public static APIKeyLoader Instance { get; private set; }
    public static APIKeyConfig Config { get; private set; }

    public HttpClient GroqHttpClient { get; private set; }
    public GroqApiClient GroqApi { get; private set; }

    private static bool _initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_initialized)
            return;

        Config = Resources.Load<APIKeyConfig>("APIKeys");
        if (Config == null)
        {
            Debug.LogError("Missing APIKeys.asset in Resources folder.");
            return;
        }

        GroqHttpClient = new HttpClient();
        GroqHttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.groqKey);
        GroqApi = new GroqApiClient(Config.groqKey, GroqHttpClient);

        _initialized = true;
    }

    /// <summary>
    /// Clears all headers from the GroqApiClient's HttpClient while preserving the authorization header.
    /// </summary>
    public void ClearGroqHeadersPreserveAuth()
    {
        if (GroqHttpClient == null) return;

        var authHeader = GroqHttpClient.DefaultRequestHeaders.Authorization;
        GroqHttpClient.DefaultRequestHeaders.Clear();
        if (authHeader != null)
        {
            GroqHttpClient.DefaultRequestHeaders.Authorization = authHeader;
        }
    }
}
