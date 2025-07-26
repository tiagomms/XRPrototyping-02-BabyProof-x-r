using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    /// <summary>
    /// Slightly different detection manager - it's responsibility is to place prefabs automatically
    /// </summary>
    public class DetectionPrefabManager : BasePrefabManager
    {

        public GameObject spawnPrefab;

        /// <summary>
        /// Get the spawn prefab for any detected box
        /// </summary>
        protected override GameObject GetPrefabForBox(BoundingBox box)
        {
            return spawnPrefab;
        }
    }
}
