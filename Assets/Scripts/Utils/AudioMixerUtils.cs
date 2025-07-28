using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

namespace Utils
{
    /// <summary>
    /// Extension methods for safe AudioMixer operations without null propagation warnings.
    /// Provides convenient methods for common AudioMixer group operations with built-in null checking.
    /// All methods are prefixed with "Safe" to indicate they perform null checks.
    /// </summary>
    public static class AudioMixerUtils
    {
        #region Volume Control

        /// <summary>
        /// Safely sets the volume of an AudioMixer group if the mixer and group exist.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <param name="volume">Volume in dB (typically -80 to 0)</param>
        /// <returns>True if successful, false if mixer or group not found</returns>
        public static bool SafeSetVolume(this AudioMixer audioMixer, string groupName, float volume)
        {
            if (audioMixer == null || string.IsNullOrEmpty(groupName))
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot set volume - AudioMixer is null or group name is empty");
                return false;
            }

            bool success = audioMixer.SetFloat(groupName, volume);
            if (!success)
            {
                Debug.LogWarning($"AudioMixerUtils: Failed to set volume for group '{groupName}'");
            }
            return success;
        }

        /// <summary>
        /// Safely gets the volume of an AudioMixer group if the mixer and group exist.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to query</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <returns>Volume in dB, or -80 if mixer or group not found</returns>
        public static float SafeGetVolume(this AudioMixer audioMixer, string groupName)
        {
            if (audioMixer == null || string.IsNullOrEmpty(groupName))
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot get volume - AudioMixer is null or group name is empty");
                return -80f;
            }

            float volume;
            if (audioMixer.GetFloat(groupName, out volume))
            {
                return volume;
            }

            Debug.LogWarning($"AudioMixerUtils: Failed to get volume for group '{groupName}'");
            return -80f;
        }

        #endregion

        #region Volume Transitions

        /// <summary>
        /// Safely transitions the volume of an AudioMixer group over time using DOTween.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <param name="targetVolume">Target volume in dB</param>
        /// <param name="duration">Transition duration in seconds</param>
        /// <param name="delay">Delay before starting the transition</param>
        /// <param name="easeType">Easing type for the transition</param>
        /// <param name="onComplete">Optional callback when transition completes</param>
        /// <returns>The created Tween, or null if operation failed</returns>
        public static Tween SafeTransitionVolume(this AudioMixer audioMixer, string groupName, float targetVolume, float duration, float delay = 0f, Ease easeType = Ease.InOutQuad, System.Action onComplete = null)
        {
            if (audioMixer == null || string.IsNullOrEmpty(groupName))
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot transition volume - AudioMixer is null or group name is empty");
                onComplete?.Invoke();
                return null;
            }

            // Get current volume
            float currentVolume;
            if (!audioMixer.GetFloat(groupName, out currentVolume))
            {
                Debug.LogWarning($"AudioMixerUtils: Failed to get current volume for group '{groupName}'");
                onComplete?.Invoke();
                return null;
            }

            // Create tween
            Tween tween = DOTween.To(
                () => currentVolume,
                (value) => {
                    audioMixer.SetFloat(groupName, value);
                },
                targetVolume,
                duration
            )
            .SetDelay(delay)
            .SetEase(easeType)
            .OnComplete(() => onComplete?.Invoke());

            Debug.Log($"AudioMixerUtils: Volume transition for '{groupName}': {currentVolume}dB → {targetVolume}dB over {duration}s (delay: {delay}s)");
            return tween;
        }

        /// <summary>
        /// Safely fades an AudioMixer group to silence (mute).
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="delay">Delay before starting the fade</param>
        /// <param name="onComplete">Optional callback when fade completes</param>
        /// <returns>The created Tween, or null if operation failed</returns>
        public static Tween SafeFadeToSilence(this AudioMixer audioMixer, string groupName, float duration, float delay = 0f, System.Action onComplete = null)
        {
            return audioMixer.SafeTransitionVolume(groupName, -80f, duration, delay, Ease.InOutQuad, onComplete);
        }

        /// <summary>
        /// Safely fades an AudioMixer group to full volume (0dB).
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="delay">Delay before starting the fade</param>
        /// <param name="onComplete">Optional callback when fade completes</param>
        /// <returns>The created Tween, or null if operation failed</returns>
        public static Tween SafeFadeToFull(this AudioMixer audioMixer, string groupName, float duration, float delay = 0f, System.Action onComplete = null)
        {
            return audioMixer.SafeTransitionVolume(groupName, 0f, duration, delay, Ease.InOutQuad, onComplete);
        }

        #endregion

        #region Multiple Group Operations

        /// <summary>
        /// Safely transitions multiple AudioMixer groups simultaneously.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupVolumes">Dictionary of group names and target volumes</param>
        /// <param name="duration">Transition duration in seconds</param>
        /// <param name="delay">Delay before starting the transitions</param>
        /// <param name="easeType">Easing type for the transitions</param>
        /// <param name="onComplete">Optional callback when all transitions complete</param>
        /// <returns>Array of created Tweens</returns>
        public static Tween[] SafeTransitionMultipleVolumes(this AudioMixer audioMixer, System.Collections.Generic.Dictionary<string, float> groupVolumes, float duration, float delay = 0f, Ease easeType = Ease.InOutQuad, System.Action onComplete = null)
        {
            if (audioMixer == null || groupVolumes == null || groupVolumes.Count == 0)
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot transition multiple volumes - AudioMixer is null or no groups specified");
                onComplete?.Invoke();
                return new Tween[0];
            }

            Tween[] tweens = new Tween[groupVolumes.Count];
            int completedCount = 0;
            int totalCount = groupVolumes.Count;

            System.Action checkCompletion = () => {
                completedCount++;
                if (completedCount >= totalCount)
                {
                    onComplete?.Invoke();
                }
            };

            int index = 0;
            foreach (var kvp in groupVolumes)
            {
                tweens[index] = audioMixer.SafeTransitionVolume(
                    kvp.Key, 
                    kvp.Value, 
                    duration, 
                    delay, 
                    easeType, 
                    checkCompletion
                );
                index++;
            }

            return tweens;
        }

        /// <summary>
        /// Safely fades all specified AudioMixer groups to silence.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupNames">Array of group names to fade</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="delay">Delay before starting the fades</param>
        /// <param name="onComplete">Optional callback when all fades complete</param>
        /// <returns>Array of created Tweens</returns>
        public static Tween[] SafeFadeAllToSilence(this AudioMixer audioMixer, string[] groupNames, float duration, float delay = 0f, System.Action onComplete = null)
        {
            if (audioMixer == null || groupNames == null || groupNames.Length == 0)
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot fade to silence - AudioMixer is null or no groups specified");
                onComplete?.Invoke();
                return new Tween[0];
            }

            var groupVolumes = new System.Collections.Generic.Dictionary<string, float>();
            foreach (string groupName in groupNames)
            {
                groupVolumes[groupName] = -80f;
            }

            return audioMixer.SafeTransitionMultipleVolumes(groupVolumes, duration, delay, Ease.InOutQuad, onComplete);
        }

        /// <summary>
        /// Safely fades all specified AudioMixer groups to full volume.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to modify</param>
        /// <param name="groupNames">Array of group names to fade</param>
        /// <param name="duration">Fade duration in seconds</param>
        /// <param name="delay">Delay before starting the fades</param>
        /// <param name="onComplete">Optional callback when all fades complete</param>
        /// <returns>Array of created Tweens</returns>
        public static Tween[] SafeFadeAllToFull(this AudioMixer audioMixer, string[] groupNames, float duration, float delay = 0f, System.Action onComplete = null)
        {
            if (audioMixer == null || groupNames == null || groupNames.Length == 0)
            {
                Debug.LogWarning($"AudioMixerUtils: Cannot fade to full - AudioMixer is null or no groups specified");
                onComplete?.Invoke();
                return new Tween[0];
            }

            var groupVolumes = new System.Collections.Generic.Dictionary<string, float>();
            foreach (string groupName in groupNames)
            {
                groupVolumes[groupName] = 0f;
            }

            return audioMixer.SafeTransitionMultipleVolumes(groupVolumes, duration, delay, Ease.InOutQuad, onComplete);
        }

        #endregion

        #region Group Validation

        /// <summary>
        /// Safely checks if an AudioMixer group exists.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to check</param>
        /// <param name="groupName">Name of the audio group</param>
        /// <returns>True if the group exists, false otherwise</returns>
        public static bool SafeGroupExists(this AudioMixer audioMixer, string groupName)
        {
            if (audioMixer == null || string.IsNullOrEmpty(groupName))
            {
                return false;
            }

            float volume;
            return audioMixer.GetFloat(groupName, out volume);
        }

        /// <summary>
        /// Safely gets all available group names from an AudioMixer.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to query</param>
        /// <returns>Array of group names, or empty array if mixer is null</returns>
        public static string[] SafeGetGroupNames(this AudioMixer audioMixer)
        {
            if (audioMixer == null)
            {
                return new string[0];
            }

            // Note: Unity doesn't provide a direct way to get all group names
            // This is a limitation of the AudioMixer API
            // You would need to maintain your own list of group names
            Debug.LogWarning($"AudioMixerUtils: Unity AudioMixer API doesn't provide direct access to group names. Consider maintaining your own list.");
            return new string[0];
        }

        #endregion

        #region Utility Operations

        /// <summary>
        /// Safely stops all active volume transitions for an AudioMixer.
        /// </summary>
        /// <param name="audioMixer">The AudioMixer to stop transitions for</param>
        /// <param name="tweens">Array of active tweens to kill</param>
        public static void SafeStopAllTransitions(this AudioMixer audioMixer, Tween[] tweens)
        {
            if (tweens == null) return;

            foreach (Tween tween in tweens)
            {
                tween?.Kill();
            }

            Debug.Log("AudioMixerUtils: All volume transitions stopped");
        }

        /// <summary>
        /// Safely converts linear volume (0-1) to dB volume.
        /// </summary>
        /// <param name="linearVolume">Linear volume (0.0 to 1.0)</param>
        /// <returns>Volume in dB</returns>
        public static float SafeLinearToDecibels(float linearVolume)
        {
            linearVolume = Mathf.Clamp01(linearVolume);
            if (linearVolume <= 0f)
            {
                return -80f;
            }
            return 20f * Mathf.Log10(linearVolume);
        }

        /// <summary>
        /// Safely converts dB volume to linear volume (0-1).
        /// </summary>
        /// <param name="decibels">Volume in dB</param>
        /// <returns>Linear volume (0.0 to 1.0)</returns>
        public static float SafeDecibelsToLinear(float decibels)
        {
            if (decibels <= -80f)
            {
                return 0f;
            }
            return Mathf.Pow(10f, decibels / 20f);
        }

        #endregion
    }
}