
using System;
using UnityEngine;

public static class AudioExtensions
{
    public static string TrySaveWav(this AudioClip clip, FileEnumPath rootPath, string relativePath, string filename, bool appendDateTimeToFileName, string logPrefix = null)
    {
        logPrefix ??= $"{nameof(AudioExtensions)} - {nameof(TrySaveWav)}";
        if (clip == null)
        {
            Debug.LogError($"[{logPrefix} - Not Saved] No Audio Clip to SAVE");
            return null;
        }
		if (rootPath == FileEnumPath.None)
		{
			Debug.Log($"[{logPrefix} - Not Saved] rootPath set to None so it is not recorded");
            return null;
		}

		if (filename.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
		{
			filename = filename[..^4]; // or filename.Substring(0, filename.Length - 4);
		}

		string storedFilePath = FileManagementExtensions.GenerateFilePath(appPath: rootPath, relativePath: relativePath, fileName: filename, extension: FileExtensions.WAV, appendDateTime: appendDateTimeToFileName);

        if (SavWav.Save(storedFilePath, clip))
        {
            Debug.Log($"[{logPrefix} - Saved!] to: {storedFilePath}");
        }
        return storedFilePath;
    }
}