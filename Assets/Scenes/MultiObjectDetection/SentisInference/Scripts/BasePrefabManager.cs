using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    /// <summary>
    /// Base class for prefab managers that handle object detection overlays
    /// Contains all common functionality between DetectionPrefabManager and HazardOverlayManager
    /// </summary>
    public class BasePrefabManager : MonoBehaviour
    {
        [System.Serializable]
        public class BaseOverlayData
        {
            public int ModelLabelId;
            public GameObject overlayObject;
            public Vector3 targetPosition;
            public int framesUnmatched;
            public int framesMatched;
        }

        [Header("Prefab Placement Settings")]
        public float matchThreshold = 0.3f; // meters
        public float minMovementThreshold = 0.05f; // don't update if below this
        public float yOffset = 0.15f; // placing prefab y meters above
        public float lerpSpeed = 5f;
        public int maxFramesUnmatched = 3; // minimum consecutive frames before destroying overlay (if already showing)
        public int minFramesMatched = 1; // minimum consecutive frames before showing overlay
        public float spawnScaleDuration = 0.3f; // spawn animation duration
        public float destroyScaleDuration = 0.2f;

        protected List<BaseOverlayData> activeOverlays = new();

        /// <summary>
        /// Main method to update prefabs based on detected objects
        /// </summary>
        /// <param name="detectedBoxes">List of detected bounding boxes</param>
        public virtual void UpdatePrefabs(List<BoundingBox> detectedBoxes)
        {
            // Step 1: Mark all overlays as unmatched
            foreach (var overlay in activeOverlays)
                overlay.framesUnmatched++;

            foreach (var box in detectedBoxes)
            {
                if (!box.WorldPos.HasValue)
                    continue;

                Vector3 worldPos = box.WorldPos.Value + Vector3.up * yOffset;

                // Step 2: Try to find closest matching overlay of same type
                BaseOverlayData match = FindMatchingOverlay(box, worldPos);

                if (match != null)
                {
                    UpdateExistingOverlay(match, worldPos, box);
                    continue;
                }

                // Step 3: No match found — create a new overlay
                CreateNewOverlay(box, worldPos);
            }

            // Step 4: Update and clean overlays
            UpdateAndCleanOverlays();
        }

        /// <summary>
        /// Find a matching overlay for the given box and world position
        /// </summary>
        protected virtual BaseOverlayData FindMatchingOverlay(BoundingBox box, Vector3 worldPos)
        {
            float bestDist = float.MaxValue;
            BaseOverlayData match = null;

            foreach (var overlay in activeOverlays)
            {
                // Check if this overlay matches the box type (to be overridden by child classes)
                if (!IsOverlayMatchingBox(overlay, box)) continue;

                float dist = Vector3.Distance(overlay.overlayObject.transform.position, worldPos);
                if (dist < matchThreshold && dist < bestDist)
                {
                    bestDist = dist;
                    match = overlay;
                }
            }

            return match;
        }

        /// <summary>
        /// Check if an overlay matches the given box (to be overridden by child classes)
        /// </summary>
        protected virtual bool IsOverlayMatchingBox(BaseOverlayData overlay, BoundingBox box)
        {
            return overlay.ModelLabelId == box.Id;
        }

        /// <summary>
        /// Update an existing overlay with new position and data
        /// </summary>
        protected virtual void UpdateExistingOverlay(BaseOverlayData overlay, Vector3 worldPos, BoundingBox box)
        {
            // Only update if distance is meaningfully different
            float delta = Vector3.Distance(overlay.targetPosition, worldPos);
            if (delta > minMovementThreshold)
            {
                overlay.targetPosition = worldPos;
            }
            overlay.framesUnmatched = 0;
            overlay.framesMatched++;

            GameObject overlayObj = overlay.overlayObject;
            // Only show overlay if minimum consecutive frames are reached
            if (overlay.framesMatched >= minFramesMatched && !overlay.overlayObject.activeSelf)
            {
                ShowPrefabAndDoScaleUpAnimation(overlayObj);
            }

            // Update label
            UpdateLabel(overlayObj, box);
        }

        /// <summary>
        /// Create a new overlay for the given box (to be overridden by child classes)
        /// </summary>
        protected virtual void CreateNewOverlay(BoundingBox box, Vector3 worldPos)
        {
            GameObject prefab = GetPrefabForBox(box);
            GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);
            obj.transform.localScale = Vector3.zero;
            obj.SetActive(false); // Start hidden

            // NOTE: this is very dumb - but in case multiple frame matching does not work 
            //  then we have this protection.
            if (minFramesMatched == 1)
            {
                ShowPrefabAndDoScaleUpAnimation(obj);
            }

            // Update label if any
            UpdateLabel(obj, box);

            var overlayData = CreateOverlayData(box, obj, worldPos);
            activeOverlays.Add(overlayData);
        }

        /// <summary>
        /// Get the appropriate prefab for the given box (to be overridden by child classes)
        /// </summary>
        protected virtual GameObject GetPrefabForBox(BoundingBox box)
        {
            Debug.LogWarning("GetPrefabForBox not implemented in base class");
            return null;
        }

        /// <summary>
        /// Create overlay data for the given box (to be overridden by child classes)
        /// </summary>
        protected virtual BaseOverlayData CreateOverlayData(BoundingBox box, GameObject obj, Vector3 worldPos)
        {
            return new BaseOverlayData
            {
                ModelLabelId = box.Id,
                overlayObject = obj,
                targetPosition = worldPos,
                framesUnmatched = 0,
                framesMatched = 1, // First frame detected
            };
        }

        /// <summary>
        /// Update and clean overlays
        /// </summary>
        protected virtual void UpdateAndCleanOverlays()
        {
            for (int i = activeOverlays.Count - 1; i >= 0; i--)
            {
                var overlay = activeOverlays[i];

                // Smooth movement toward target position
                overlay.overlayObject.transform.position = Vector3.Lerp(
                    overlay.overlayObject.transform.position,
                    overlay.targetPosition,
                    Time.deltaTime * lerpSpeed
                );

                // Cleanup if unmatched too long
                if (overlay.framesUnmatched > maxFramesUnmatched)
                {
                    GameObject toDestroy = overlay.overlayObject;
                    activeOverlays.RemoveAt(i);

                    toDestroy.transform.DOScale(Vector3.zero, destroyScaleDuration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(toDestroy));
                }
            }
        }

        /// <summary>
        /// Show prefab with scale up animation
        /// </summary>
        protected virtual void ShowPrefabAndDoScaleUpAnimation(GameObject overlayObj)
        {
            overlayObj.SetActive(true);
            overlayObj.transform.localScale = Vector3.zero;
            overlayObj.transform.DOScale(Vector3.one, spawnScaleDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Update label on the overlay object
        /// </summary>
        protected virtual void UpdateLabel(GameObject overlayObject, BoundingBox box)
        {
            // TODO: improve this implementation it is terrible x)
            Text text = overlayObject.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = $"{box.UILabel}";
                return;
            }

            TMP_Text tmpText = overlayObject.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"{box.UILabel}";
                return;
            }
        }
    }
} 