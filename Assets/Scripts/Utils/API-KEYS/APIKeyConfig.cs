using UnityEngine;

/// <summary>
/// Asset that holds API Keys to be used in project.
/// </summary>
[CreateAssetMenu(menuName = "Config/API Key Config")]
public class APIKeyConfig : ScriptableObject
{
    [Header("Groq")]
    public string groqKey;

}
