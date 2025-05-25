using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// A singleton log viewer for displaying debug logs in XR using a world-space canvas.
/// Automatically scrolls to the bottom and supports color-coded messages with optional timestamps or index labels.
/// </summary>
public class XRDebugLogViewer : MonoBehaviour
{
    private enum LogType
    {
        Default = 0,
        Warning = 1,
        Error = 2
    }
    // === Singleton Setup ===

    /// <summary>
    /// Global access point to the log viewer.
    /// </summary>
    public static XRDebugLogViewer Instance { get; private set; }

    private void Awake()
    {
        // Enforce singleton pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // === UI References ===

    [Header("UI References")]
    [Tooltip("Reference to the ScrollRect controlling vertical scrolling.")]
    [SerializeField] private ScrollRect scrollRect;

    [Tooltip("Reference to the TextMeshProUGUI element displaying the logs.")]
    [SerializeField] private TextMeshProUGUI logText;

    // === Settings ===

    [Header("Settings")]
    [Tooltip("Maximum number of log entries to retain. Older entries will be discarded.")]
    [SerializeField] private int maxLogCount = 1000;

    [Tooltip("If true, prepend each log with a timestamp. If false, prepend with incrementing index.")]
    [SerializeField] private bool useTimestamps = true;

    // === Colors ===

    [Header("Log Colors")]
    [Tooltip("Text color for standard informational logs.")]
    [SerializeField] private Color infoColor = Color.white;

    [Tooltip("Text color for warnings.")]
    [SerializeField] private Color warningColor = Color.yellow;

    [Tooltip("Text color for errors.")]
    [SerializeField] private Color errorColor = Color.red;

    [Space]
    [SerializeField] private bool linkToDebugLog = true;

    // === Internal State ===

    private readonly List<string> logLines = new(); // Stores formatted log lines
    private int logCounter = 0;                     // Index used if timestamps are disabled

    private void Start()
    {
        ClearLogs(); // Initialize log view on start
    }

    // === Public Logging API ===

    /// <summary>
    /// Logs a standard info message to the viewer.
    /// </summary>
    public static void Log(string message)
    {
        Instance?.AddLog(message, Instance.infoColor);
        Instance?.WriteToDebug(message, LogType.Default);
    }

    private void WriteToDebug(string message, LogType type)
    {
        if (!linkToDebugLog) return;
        switch (type)
        {
            case LogType.Default:
                Debug.Log(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Error:
                Debug.LogError(message);
                break;
        }
    }

    /// <summary>
    /// Logs a warning message to the viewer.
    /// </summary>
    public static void LogWarning(string message)
    {
        Instance?.AddLog(message, Instance.warningColor);
        Instance?.WriteToDebug(message, LogType.Warning);

    }

    /// <summary>
    /// Logs an error message to the viewer.
    /// </summary>
    public static void LogError(string message)
    {
        Instance?.AddLog(message, Instance.errorColor);
        Instance?.WriteToDebug(message, LogType.Error);

    }

    /// <summary>
    /// Clears all logs and resets the viewer.
    /// </summary>
    public static void Clear()
    {
        Instance?.ClearLogs();
    }

    // === Internal Logging Logic ===

    private void AddLog(string message, Color color)
    {
        //string logCounterFormat = String.Format("{0:0000}", logCounter);
        string prefix = useTimestamps
            ? $"[{DateTime.Now:HH:mm:ss}]"
            : $"[{logCounter:0000}]";

        string coloredMessage = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{prefix} {message}</color>";

        logLines.Add(coloredMessage);
        logCounter++;

        if (logLines.Count > maxLogCount)
            logLines.RemoveAt(0);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        logText.text = string.Join("\n", logLines);

        // Force Unity to update layout and scroll to bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void ClearLogs()
    {
        logLines.Clear();
        logText.text = string.Empty;
        logCounter = 0;
    }
}
