using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class HazardOverlayManager : BasePrefabManager
    {
        public enum HazardType { Regular = 0, Dangerous = 1, Choking = 2 }

        [System.Serializable]
        public class HazardOverlay : BaseOverlayData
        {
            public HazardType type;
        }

        [Header("Hazard Type Prefabs")]
        public GameObject regularPrefab;
        public GameObject dangerousPrefab;
        public GameObject chokingPrefab;

        /// <summary>
        /// Get the appropriate prefab based on hazard type
        /// </summary>
        protected override GameObject GetPrefabForBox(BoundingBox box)
        {
            HazardType type = DetermineHazardType(box);
            return GetHazardPrefab(type);
        }

        /// <summary>
        /// Override to use HazardOverlay instead of BaseOverlayData
        /// </summary>
        protected override BaseOverlayData CreateOverlayData(BoundingBox box, GameObject obj, Vector3 worldPos)
        {
            HazardType type = DetermineHazardType(box);
            return new HazardOverlay
            {
                ModelLabelId = box.Id,
                overlayObject = obj,
                targetPosition = worldPos,
                framesUnmatched = 0,
                framesMatched = 1,
                type = type
            };
        }

        /// <summary>
        /// Override to check both ModelLabelId and HazardType for matching
        /// </summary>
        protected override bool IsOverlayMatchingBox(BaseOverlayData overlay, BoundingBox box)
        {
            if (overlay is HazardOverlay hazardOverlay)
            {
                HazardType type = DetermineHazardType(box);
                return hazardOverlay.ModelLabelId == box.Id && hazardOverlay.type == type;
            }
            return false;
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
        private HazardType DetermineHazardType(BoundingBox box)
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

        private void ShowPrefabAndDoScaleUpAnimation(GameObject overlayObj)
        {
            overlayObj.SetActive(true);
            overlayObj.transform.localScale = Vector3.zero;
            overlayObj.transform.DOScale(Vector3.one, spawnScaleDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Unsure you want this, at least for logging might be good
        /// </summary>
        /// <param name="overlayObject"></param>
        /// <param name="box"></param>
        private void UpdateLabel(GameObject overlayObject, BoundingBox box)
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
