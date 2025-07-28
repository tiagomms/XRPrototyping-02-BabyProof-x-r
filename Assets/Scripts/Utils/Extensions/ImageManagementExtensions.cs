using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

/// <summary>
/// Utility methods for image management, including base64 conversion, validation, and file operations.
/// </summary>
public static class ImageManagementExtensions
{
    private const int MAX_BASE64_SIZE_MB = 4;

    /// <summary>
    /// Validates that a base64 string does not exceed the maximum allowed size.
    /// </summary>
    /// <param name="base64String">The base64 string to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the base64 string exceeds the maximum size.</exception>
    public static void ValidateBase64Size(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            throw new ArgumentException("Base64 string cannot be null or empty");
        }

        double sizeInMB = (base64String.Length * 3.0 / 4.0) / (1024 * 1024);
        if (sizeInMB > MAX_BASE64_SIZE_MB)
        {
            throw new ArgumentException($"Base64 encoded image exceeds maximum size of {MAX_BASE64_SIZE_MB}MB");
        }
    }

    /// <summary>
    /// Converts an image file to a base64 string.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <returns>The base64 encoded string of the image.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the image file is not found.</exception>
    public static async Task<string> ConvertImageToBase64(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"Image file not found: {imagePath}");
        }

        byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
        return Convert.ToBase64String(imageBytes);
    }

    /// <summary>
    /// Writes a Texture2D to disk as a PNG file in the persistent data path.
    /// </summary>
    /// <param name="texture">The texture to save.</param>
    /// <param name="fileName">The file name (with or without extension).</param>
    /// <returns>The full path where the file was saved, or null if failed.</returns>
    public static string WriteImageOnDisk(Texture2D texture, string fileName, ImageExtensions extension, FileEnumPath appPath = FileEnumPath.Temporary, string relativePath = null, bool appendDateTime = false)
    {
        if (appPath == FileEnumPath.None)
        {
            Debug.LogWarning($"[{nameof(ImageManagementExtensions)}].WriteImageOnDisk WARNING - file enum path set to NONE - not saving image");
        }
        if (texture == null)
        {
            Debug.LogError($"[{nameof(ImageManagementExtensions)}].WriteImageOnDisk ERROR - texture is null");
            return null;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError($"[{nameof(ImageManagementExtensions)}].WriteImageOnDisk ERROR - fileName is null or empty");
            return null;
        }

        try
        {
            byte[] textureBytes = texture.EncodeToPNG();

            string filePath = FileManagementExtensions.GenerateFilePath(appPath, relativePath, fileName, extension.ToString().ToLower(), appendDateTime);
            FileManagementExtensions.SaveInRootedDataPath(filePath, null, textureBytes);

            Debug.Log($"Image written to disk: {filePath}");
            return filePath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{nameof(ImageManagementExtensions)}].WriteImageOnDisk ERROR - {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts a base64 string to a Texture2D.
    /// </summary>
    /// <param name="base64String">The base64 encoded image string.</param>
    /// <returns>A Texture2D created from the base64 data, or null if failed.</returns>
    public static Texture2D ConvertBase64ToTexture(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            Debug.LogError($"[{nameof(ImageManagementExtensions)}].ConvertBase64ToTexture ERROR - base64String is null or empty");
            return null;
        }

        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(imageBytes))
            {
                Debug.Log("Successfully converted base64 to Texture2D");
                return texture;
            }
            else
            {
                Debug.LogError($"[{nameof(ImageManagementExtensions)}].ConvertBase64ToTexture ERROR - failed to load image data");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{nameof(ImageManagementExtensions)}].ConvertBase64ToTexture ERROR - {ex.Message}");
            return null;
        }
    }

    public static async Task<Texture2D> GetRemoteTexture(string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            var asyncOp = www.SendWebRequest();

            while (asyncOp.isDone == false)
                await Task.Delay(1000 / 30);//30 hertz

            // read results:
            if (www.isNetworkError || www.isHttpError)
            {
                return null;
            }
            else
            {
                return DownloadHandlerTexture.GetContent(www);
            }
        }
    }
}