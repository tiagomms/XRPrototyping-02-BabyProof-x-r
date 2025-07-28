using UnityEngine;
using DG.Tweening;

namespace Utils
{
    /// <summary>
    /// Extension methods for safe AudioSource operations without null propagation warnings.
    /// Provides convenient methods for common AudioSource operations with built-in null checking.
    /// All methods are prefixed with "Safe" to indicate they perform null checks.
    /// </summary>
    public static class AudioSourceUtils
    {
        #region Play Operations

        /// <summary>
        /// Safely plays an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play</param>
        public static void SafePlay(this AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }

            /// <summary>
        /// Safely plays an AudioSource with a specific clip if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play</param>
        /// <param name="clip">The AudioClip to play</param>
        public static void SafePlay(this AudioSource audioSource, AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Safely plays an AudioSource at a specific position if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play</param>
        /// <param name="position">The position to play the sound at</param>
        public static void SafePlayAtPoint(this AudioSource audioSource, Vector3 position)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                AudioSource.PlayClipAtPoint(audioSource.clip, position, audioSource.volume);
            }
        }

        /// <summary>
        /// Safely plays an AudioSource with a delay if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play</param>
        /// <param name="delay">Delay in seconds before playing</param>
        public static void SafePlayDelayed(this AudioSource audioSource, float delay)
        {
            if (audioSource != null)
            {
                audioSource.PlayDelayed(delay);
            }
        }

        /// <summary>
        /// Safely plays an AudioSource with a scheduled time if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to play</param>
        /// <param name="time">The time to schedule the play</param>
        public static void SafePlayScheduled(this AudioSource audioSource, double time)
        {
            if (audioSource != null)
            {
                audioSource.PlayScheduled(time);
            }
        }

    #endregion

            #region Stop Operations

        /// <summary>
        /// Safely stops an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to stop</param>
        public static void SafeStop(this AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        /// <summary>
        /// Safely pauses an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to pause</param>
        public static void SafePause(this AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// Safely unpauses an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to unpause</param>
        public static void SafeUnPause(this AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }

        /// <summary>
        /// Safely stops an AudioSource with a delay if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to stop</param>
        /// <param name="delay">Delay in seconds before stopping</param>
        public static void SafeStopDelayed(this AudioSource audioSource, float delay)
        {
            if (audioSource != null)
            {
                // Use DOTween to delay the stop operation
                DOTween.Sequence()
                    .AppendInterval(delay)
                    .OnComplete(() => audioSource.Stop());
            }
        }

        /// <summary>
        /// Safely pauses an AudioSource with a delay if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to pause</param>
        /// <param name="delay">Delay in seconds before pausing</param>
        public static void SafePauseDelayed(this AudioSource audioSource, float delay)
        {
            if (audioSource != null)
            {
                // Use DOTween to delay the pause operation
                DOTween.Sequence()
                    .AppendInterval(delay)
                    .OnComplete(() => audioSource.Pause());
            }
        }

        /// <summary>
        /// Safely unpauses an AudioSource with a delay if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to unpause</param>
        /// <param name="delay">Delay in seconds before unpausing</param>
        public static void SafeUnPauseDelayed(this AudioSource audioSource, float delay)
        {
            if (audioSource != null)
            {
                // Use DOTween to delay the unpause operation
                DOTween.Sequence()
                    .AppendInterval(delay)
                    .OnComplete(() => audioSource.UnPause());
            }
        }

    #endregion

            #region Volume Operations

        /// <summary>
        /// Safely sets the volume of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="volume">The volume to set (0.0 to 1.0)</param>
        public static void SafeSetVolume(this AudioSource audioSource, float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
            }
        }

        /// <summary>
        /// Safely gets the volume of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get volume from</param>
        /// <returns>The current volume, or 0 if the AudioSource is null</returns>
        public static float SafeGetVolume(this AudioSource audioSource)
        {
            return audioSource != null ? audioSource.volume : 0f;
        }

    #endregion

            #region Pitch Operations

        /// <summary>
        /// Safely sets the pitch of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="pitch">The pitch to set</param>
        public static void SafeSetPitch(this AudioSource audioSource, float pitch)
        {
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }
        }

        /// <summary>
        /// Safely gets the pitch of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get pitch from</param>
        /// <returns>The current pitch, or 1 if the AudioSource is null</returns>
        public static float SafeGetPitch(this AudioSource audioSource)
        {
            return audioSource != null ? audioSource.pitch : 1f;
        }

        #endregion

        #region State Operations

        /// <summary>
        /// Safely checks if an AudioSource is playing if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to check</param>
        /// <returns>True if playing, false if null or not playing</returns>
        public static bool SafeIsPlaying(this AudioSource audioSource)
        {
            return audioSource != null && audioSource.isPlaying;
        }

        /// <summary>
        /// Safely checks if an AudioSource is paused if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to check</param>
        /// <returns>True if paused, false if null or not paused</returns>
        public static bool SafeIsPaused(this AudioSource audioSource)
        {
            return audioSource != null && audioSource.isPlaying == false && audioSource.time > 0f;
        }

        /// <summary>
        /// Safely gets the current time of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get time from</param>
        /// <returns>The current time in seconds, or 0 if the AudioSource is null</returns>
        public static float SafeGetTime(this AudioSource audioSource)
        {
            return audioSource != null ? audioSource.time : 0f;
        }

        /// <summary>
        /// Safely sets the time of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="time">The time to set in seconds</param>
        public static void SafeSetTime(this AudioSource audioSource, float time)
        {
            if (audioSource != null)
            {
                audioSource.time = Mathf.Max(0f, time);
            }
        }

        #endregion

        #region Clip Operations

        /// <summary>
        /// Safely sets the clip of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="clip">The AudioClip to set</param>
        public static void SafeSetClip(this AudioSource audioSource, AudioClip clip)
        {
            if (audioSource != null)
            {
                audioSource.clip = clip;
            }
        }

        /// <summary>
        /// Safely gets the clip of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get clip from</param>
        /// <returns>The current AudioClip, or null if the AudioSource is null</returns>
        public static AudioClip SafeGetClip(this AudioSource audioSource)
        {
            return audioSource != null ? audioSource.clip : null;
        }

        #endregion

        #region Loop Operations

        /// <summary>
        /// Safely sets the loop property of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="loop">Whether the audio should loop</param>
        public static void SafeSetLoop(this AudioSource audioSource, bool loop)
        {
            if (audioSource != null)
            {
                audioSource.loop = loop;
            }
        }

        /// <summary>
        /// Safely gets the loop property of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get loop property from</param>
        /// <returns>The current loop setting, or false if the AudioSource is null</returns>
        public static bool SafeGetLoop(this AudioSource audioSource)
        {
            return audioSource != null && audioSource.loop;
        }

        #endregion

        #region Mute Operations

        /// <summary>
        /// Safely sets the mute property of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to modify</param>
        /// <param name="mute">Whether the audio should be muted</param>
        public static void SafeSetMute(this AudioSource audioSource, bool mute)
        {
            if (audioSource != null)
            {
                audioSource.mute = mute;
            }
        }

        /// <summary>
        /// Safely gets the mute property of an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to get mute property from</param>
        /// <returns>The current mute setting, or false if the AudioSource is null</returns>
        public static bool SafeGetMute(this AudioSource audioSource)
        {
            return audioSource != null && audioSource.mute;
        }

        #endregion

        #region Utility Operations

        /// <summary>
        /// Safely restarts an AudioSource if it's not null.
        /// </summary>
        /// <param name="audioSource">The AudioSource to restart</param>
        public static void SafeRestart(this AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.time = 0f;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Safely fades an AudioSource to a target volume over time using DOTween.
        /// </summary>
        /// <param name="audioSource">The AudioSource to fade</param>
        /// <param name="targetVolume">The target volume (0.0 to 1.0)</param>
        /// <param name="duration">The duration of the fade in seconds</param>
        /// <param name="onComplete">Optional callback when fade completes</param>
        public static void SafeFadeTo(this AudioSource audioSource, float targetVolume, float duration, System.Action onComplete = null)
        {
            if (audioSource != null)
            {
                targetVolume = Mathf.Clamp01(targetVolume);
                
                // Use DOTween for smooth volume transitions
                DOTween.To(
                    () => audioSource.volume,
                    (value) => audioSource.volume = value,
                    targetVolume,
                    duration
                ).OnComplete(() => onComplete?.Invoke());
            }
            else
            {
                onComplete?.Invoke();
            }
        }

    #endregion
}
}