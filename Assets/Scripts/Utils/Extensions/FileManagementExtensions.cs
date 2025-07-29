using UnityEngine;
using System;
using System.IO;

public enum FileEnumPath
{
    None = 0,
    Temporary = 1,
    Persistent = 2
}

/// <summary>
/// Utility methods for file operations in Unity persistent and temporary cache paths.
/// </summary>
public static class FileManagementExtensions
{
    
    #region PATH
    /// <summary>
    /// Generates a file path using the specified app path, relative directory, and file name, with optional datetime appended.
    /// </summary>
    /// <param name="appPath">The base application path (Temporary, Persistent, etc.).</param>
    /// <param name="relativePath">Relative directory path (can be null or empty).</param>
    /// <param name="fileName">The file name (with extension).</param>
    /// <param name="appendDateTime">Whether to append the current datetime to the file name.</param>
    /// <returns>The combined file path, or null if arguments are invalid.</returns>
    public static string GenerateFilePath(FileEnumPath appPath, string relativePath, string fileName, string extension, bool appendDateTime = false)
    {
        string basePath = appPath.GetFolderPathFromEnum();
        if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("FileManagementExtensions.GenerateFilePath ERROR - basePath or fileName is null or empty");
            return null;
        }
        string name = $"{fileName}.{extension.ToLower()}";
        if (appendDateTime)
        {
            string timeOfCreation = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            name = $"{nameWithoutExt}_{timeOfCreation}.{extension.ToLower()}";
        }
        string fullPath = string.IsNullOrEmpty(relativePath)
            ? Path.Combine(basePath, name)
            : Path.Combine(basePath, relativePath, name);
        return fullPath;
    }
    public static string GetFolderPathFromEnum(this FileEnumPath fileStorage)
    {
        switch (fileStorage)
        {
            case FileEnumPath.None:
                return null;
            case FileEnumPath.Temporary:
                return Application.temporaryCachePath;
            case FileEnumPath.Persistent:
                return Application.persistentDataPath;
            default:
                return Application.dataPath;
        }
    }
    #endregion

    #region SAVE
    /// <summary>
    /// Saves a byte array to Application.temporaryCachePath at the given relative path, creating directories as needed.
    /// </summary>
    /// <param name="relativePath">Relative file path (e.g. "subdir/file.wav").</param>
    /// <param name="data">File data to save.</param>
    /// <returns>The full file path, or null if failed.</returns>
    public static string SaveInTemporaryDataPath(string relativePath, byte[] data)
    {
        return SaveInRootedDataPath(Application.temporaryCachePath, relativePath, data);
    }

    /// <summary>
    /// Saves a byte array to Application.persistentDataPath at the given relative path, creating directories as needed.
    /// </summary>
    /// <param name="relativePath">Relative file path (e.g. "subdir/file.wav").</param>
    /// <param name="data">File data to save.</param>
    /// <returns>The full file path, or null if failed.</returns>
    public static string SaveInPersistentDataPath(string relativePath, byte[] data)
    {
        return SaveInRootedDataPath(Application.persistentDataPath, relativePath, data);
    }

    /// <summary>
    /// Internal helper to save a byte array to a rooted directory + relative path, creating directories as needed.
    /// </summary>
    /// <param name="rootPath">Root directory (e.g. Application.temporaryCachePath).</param>
    /// <param name="relativePath">Relative file path.</param>
    /// <param name="data">File data to save.</param>
    /// <returns>The full file path, or null if failed.</returns>
    public static string SaveInRootedDataPath(string rootPath, string relativePath, byte[] data)
    {
        if (string.IsNullOrEmpty(rootPath) || data == null)
        {
            Debug.LogError("FileManagementExtensions.SaveInRootedDataPath ERROR - invalid arguments");
            return null;
        }
        try
        {
            string fullPath = string.IsNullOrEmpty(relativePath) ? rootPath : Path.Combine(rootPath, relativePath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(fullPath, data);
            Debug.Log($"Saved file: {fullPath}");
            return fullPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"FileManagementExtensions.SaveInRootedDataPath ERROR - {ex.Message}");
            return null;
        }
    }
    #endregion

    #region MOVE
    /// <summary>
    /// Moves a file to persistentDataPath, mimicking any subdirectories from the original path if present.
    /// </summary>
    /// <param name="filePath">The full path of the file to move.</param>
    /// <param name="persistentSubDir">Optional subdirectory under persistentDataPath to move the file into.</param>
    /// <returns>The new file path in persistentDataPath, or null if failed.</returns>
    public static string MoveFileToPersistentDataPath(string filePath, string persistentSubDir = null)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError($"FileManagementExtensions.MoveFileToPersistentDataPath ERROR - file does not exist: {filePath}");
            return null;
        }
        try
        {
            string relativePath = filePath.Replace(Application.temporaryCachePath, string.Empty).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string persistentDir = Application.persistentDataPath;
            if (!string.IsNullOrEmpty(persistentSubDir))
            {
                persistentDir = Path.Combine(persistentDir, persistentSubDir);
            }
            string newFilePath = Path.Combine(persistentDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
            File.Move(filePath, newFilePath);
            Debug.Log($"Moved file to persistentDataPath: {newFilePath}");
            return newFilePath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"FileManagementExtensions.MoveFileToPersistentDataPath ERROR - {ex.Message}");
            return null;
        }
    }
    #endregion

    #region CLEAN
    /// <summary>
    /// Deletes all files and directories in Application.temporaryCachePath.
    /// </summary>
    public static void CleanTemporaryCachePath()
    {
        CleanDirectoryInternal(Application.temporaryCachePath);
    }

    /// <summary>
    /// Deletes all files and directories in Application.persistentDataPath.
    /// </summary>
    public static void CleanPersistentDataPath()
    {
        CleanDirectoryInternal(Application.persistentDataPath);
    }

    /// <summary>
    /// Deletes all files with the given extension in temp and/or persistent paths.
    /// </summary>
    /// <param name="extension">File extension (e.g. ".wav").</param>
    /// <param name="includeTemp">Whether to clean in temporaryCachePath.</param>
    /// <param name="includePersistent">Whether to clean in persistentDataPath.</param>
    public static void CleanFilesOfType(string extension, bool includeTemp = true, bool includePersistent = true)
    {
        if (string.IsNullOrEmpty(extension))
        {
            Debug.LogError("FileManagementExtensions.CleanFilesOfType ERROR - extension is null or empty");
            return;
        }
        if (includeTemp)
        {
            CleanFilesOfTypeInDirectory(Application.temporaryCachePath, extension);
        }
        if (includePersistent)
        {
            CleanFilesOfTypeInDirectory(Application.persistentDataPath, extension);
        }
    }

    /// <summary>
    /// Deletes the specified directory (relative to the root of temp/persistent) in both locations as specified.
    /// </summary>
    /// <param name="relativeDir">Relative directory path to clean.</param>
    /// <param name="includeTemp">Whether to clean in temporaryCachePath.</param>
    /// <param name="includePersistent">Whether to clean in persistentDataPath.</param>
    public static void CleanDirectory(string relativeDir, bool includeTemp = true, bool includePersistent = true)
    {
        if (string.IsNullOrEmpty(relativeDir))
        {
            Debug.LogError("FileManagementExtensions.CleanDirectory ERROR - relativeDir is null or empty");
            return;
        }
        if (includeTemp)
        {
            string tempPath = Path.Combine(Application.temporaryCachePath, relativeDir);
            CleanDirectoryInternal(tempPath);
        }
        if (includePersistent)
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, relativeDir);
            CleanDirectoryInternal(persistentPath);
        }
    }

    /// <summary>
    /// Internal helper to delete all files and subdirectories in a directory.
    /// </summary>
    /// <param name="dirPath">Directory to clean.</param>
    private static void CleanDirectoryInternal(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogWarning($"FileManagementExtensions.CleanDirectoryInternal - Directory does not exist: {dirPath}");
            return;
        }
        try
        {
            foreach (string file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
            foreach (string dir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
            {
                Directory.Delete(dir, true);
            }
            Debug.Log($"Cleaned directory: {dirPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"FileManagementExtensions.CleanDirectoryInternal ERROR - {ex.Message}");
        }
    }

    /// <summary>
    /// Internal helper to delete all files of a given type in a directory.
    /// </summary>
    /// <param name="dirPath">Directory to search.</param>
    /// <param name="extension">File extension to delete.</param>
    private static void CleanFilesOfTypeInDirectory(string dirPath, string extension)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogWarning($"FileManagementExtensions.CleanFilesOfTypeInDirectory - Directory does not exist: {dirPath}");
            return;
        }
        try
        {
            foreach (string file in Directory.GetFiles(dirPath, "*" + extension, SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
            Debug.Log($"Cleaned files of type {extension} in: {dirPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"FileManagementExtensions.CleanFilesOfTypeInDirectory ERROR - {ex.Message}");
        }
    }
    #endregion
} 