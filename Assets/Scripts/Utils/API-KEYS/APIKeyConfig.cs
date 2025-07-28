using UnityEngine;

/// <summary>
/// Asset that holds API Keys to be used in project.
/// </summary>
[CreateAssetMenu(menuName = "Config/API Key Config")]
public class APIKeyConfig : ScriptableObject
{
    [Header("Groq")]
    public string groqKey;

    [Header("Runware - Text2Image")]
    public string runwareKey;

    [Header("StableFast - Image to 3D")]
    public string stableFastKey;
}
