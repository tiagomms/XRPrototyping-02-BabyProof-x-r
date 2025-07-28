using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Supported image formats for encoding/decoding.
/// </summary>
public enum ImageExtensions
{
    JPG = 0,
    PNG = 1,
    WEBP = 2
}

public static class FileExtensions
{
    public static readonly string WAV = "wav";
    public static readonly string TXT = "txt";
    public static readonly string GLB = "glb";

}