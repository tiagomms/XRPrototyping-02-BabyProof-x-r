using UnityEngine;

/// <summary>
/// Asset that holds configuration for the app manager.
/// </summary>
[CreateAssetMenu(menuName = "Config/App Manager Config")]
public class AppManagerConfig : ScriptableObject
{
    public enum Environment
    {
        Development = 0, // In Dev/Prod/... mode, on start only the AI is shown.
        Debug = 99, // In Debug mode, all UI buttons in the Palm Menu are constantly shown.
    }

    public enum AIState 
    {
        Enabled = 0, // AI is enabled and can be used. To use a Config/API Key Config must be created and the Groq API Key must be set.
        Disabled = 1, // AI is faked (no API calls are made) and the user can see how the AI would work (with a disclaimer).
        // in this specific case, the mic is disabled and the system toggles between Activated and Deactivated states.
    }
    
    [Tooltip("Development: Only AI is shown on start. Debug: All UI buttons in the Palm Menu are constantly shown.")]
    public Environment environment;
    
    [Tooltip("Enabled: AI is active with API calls. Disabled: AI is faked (no API calls) and toggles between Activated/Deactivated states.")]
    public AIState aiState;
}
