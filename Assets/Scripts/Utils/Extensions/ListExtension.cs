using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension methods for List<T>.
/// </summary>
public static class ListExtension
{
    private static readonly System.Random rng = new System.Random();

    /// <summary>
    /// Returns a random entry from the list. Returns default(T) if the list is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to select from.</param>
    /// <returns>A random entry from the list, or default(T) if the list is null or empty.</returns>
    public static T GetRandomEntry<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("List with no entry - return null");
            return default(T);
        }
        int index = rng.Next(list.Count);
        return list[index];
    }

    public static T NextEntry<T>(this List<T> list, T entry)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("List with no entry - return null");
            return default(T);
        }

        int firstEntryIndex = list.IndexOf(entry);
        if (firstEntryIndex == -1)
        {
            Debug.LogError($"Entry {entry} of type {typeof(T)} not in list - return null");
            return default(T);
        }
        // NOTE: goes back full circle if last entry of list
        return list[(firstEntryIndex + 1) % list.Count];
    }
}