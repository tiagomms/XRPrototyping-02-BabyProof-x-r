using UnityEngine;
using System.Net.Http;
using GroqApiLibrary;

public class APIKeyLoader : MonoBehaviour
{
    public static APIKeyLoader Instance { get; private set; }
    public static APIKeyConfig Config { get; private set; }

    [Header("API Configuration")]
    [SerializeField] private APIKeyConfig apiKeyConfig;

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

        InitializeAPI();
    }

    /// <summary>
    /// Initializes the API configuration and clients.
    /// </summary>
    private void InitializeAPI()
    {
        // Check if APIKeyConfig is assigned in inspector
        if (apiKeyConfig == null)
        {
            Debug.LogError("APIKeyConfig is not assigned in the inspector! Please follow these steps:");
            Debug.LogError("1. Create an APIKeyConfig asset: Right-click in Project window → Create → Config → API Key Config. Ideally in the Secrets folder so that is not published on github");
            Debug.LogError("2. Add your API keys to the config asset");
            Debug.LogError("3. Assign the config asset to the APIKeyLoader component in the inspector");
            return;
        }

        // Validate that the Groq API key is provided
        if (string.IsNullOrEmpty(apiKeyConfig.groqKey))
        {
            Debug.LogError("Groq API key is missing in the APIKeyConfig! Please add your Groq API key to the config asset.");
            return;
        }

        Config = apiKeyConfig;

        try
        {
            GroqHttpClient = new HttpClient();
            GroqHttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.groqKey);
            GroqApi = new GroqApiClient(Config.groqKey, GroqHttpClient);

            Debug.Log("APIKeyLoader initialized successfully with Groq API client.");
            _initialized = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize API clients: {ex.Message}");
        }
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

    /// <summary>
    /// Validates that the API configuration is properly set up.
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise.</returns>
    public bool IsConfigurationValid()
    {
        return apiKeyConfig != null && !string.IsNullOrEmpty(apiKeyConfig.groqKey);
    }
}
