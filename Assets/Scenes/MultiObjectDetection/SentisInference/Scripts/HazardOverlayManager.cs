using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class HazardOverlayManager : MonoBehaviour
    {
        public enum HazardType { Regular = 0, Dangerous = 1, Choking = 2 }

        [System.Serializable]
        public class HazardOverlay
        {
            public GameObject overlayObject;
            public Vector3 targetPosition;
            public int framesUnmatched;
            public HazardType type;
        }

        [Header("Hazard Type Prefabs")]
        public GameObject regularPrefab;
        public GameObject dangerousPrefab;
        public GameObject chokingPrefab;

        [Header("Settings")]
        public float matchThreshold = 0.3f; // meters
        public float minMovementThreshold = 0.05f; // don't update if below this
        public float yOffset = 0.15f;
        public float lerpSpeed = 5f;
        public int maxFramesUnmatched = 3;
        public float spawnScaleDuration = 0.3f;
        public float destroyScaleDuration = 0.2f;

        private List<HazardOverlay> activeOverlays = new();

        public void UpdateHazards(List<BabyProofxrInferenceUiManager.BabyProofBoundingBox> detectedBoxes)
        {
            // Step 1: Mark all overlays as unmatched
            foreach (var overlay in activeOverlays)
                overlay.framesUnmatched++;

            foreach (var box in detectedBoxes)
            {
                if (!box.BaseBox.WorldPos.HasValue)
                    continue;

                Vector3 worldPos = box.BaseBox.WorldPos.Value + Vector3.up * yOffset;
                HazardType type = DetermineHazardType(box);

                // Step 2: Try to find closest matching overlay of same type
                HazardOverlay match = null;
                float bestDist = float.MaxValue;

                foreach (var overlay in activeOverlays)
                {
                    if (overlay.type != type) continue;

                    float dist = Vector3.Distance(overlay.overlayObject.transform.position, worldPos);
                    if (dist < matchThreshold && dist < bestDist)
                    {
                        bestDist = dist;
                        match = overlay;
                    }
                }

                if (match != null)
                {
                    // Only update if distance is meaningfully different
                    float delta = Vector3.Distance(match.targetPosition, worldPos);
                    if (delta > minMovementThreshold)
                    {
                        match.targetPosition = worldPos;
                    }
                    match.framesUnmatched = 0;

                    // update label
                    UpdateLabel(match.overlayObject, box);
                    continue;
                }

                // Step 3: No match found — create a new overlay
                GameObject prefab = GetHazardPrefab(type);
                GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity);
                obj.transform.localScale = Vector3.zero;
                obj.transform.DOScale(Vector3.one, spawnScaleDuration).SetEase(Ease.OutBack);

                // update label
                UpdateLabel(obj, box);

                activeOverlays.Add(new HazardOverlay
                {
                    overlayObject = obj,
                    targetPosition = worldPos,
                    framesUnmatched = 0,
                    type = type
                });
            }

            // Step 4: Update and clean overlays
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

        public GameObject GetHazardPrefab(HazardType type)
        {
            if (type == HazardType.Dangerous)
            {
                return dangerousPrefab;
            }
            if (type == HazardType.Choking)
            {
                return chokingPrefab;
            }
            return regularPrefab;
        }
        private HazardType DetermineHazardType(BabyProofxrInferenceUiManager.BabyProofBoundingBox box)
        {
            if (box.IsDangerous)
            {
                return HazardType.Dangerous;
            }
            else if (box.IsChockingHazard)
            {
                return HazardType.Choking;
            }
            return HazardType.Regular;
        }

        private void UpdateLabel(GameObject overlayObject, BabyProofxrInferenceUiManager.BabyProofBoundingBox box)
        {
            Text text = overlayObject.GetComponentInChildren<Text>();
            if (text == null) return;
            text.text = $"{box.BaseBox.Label}";
        }
    }
}
