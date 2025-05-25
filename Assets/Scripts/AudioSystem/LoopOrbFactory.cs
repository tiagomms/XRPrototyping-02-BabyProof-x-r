using UnityEngine;

namespace XRLoopPedal.AudioSystem
{
    // Factory for spawning orbs
    public class LoopOrbFactory : MonoBehaviour
    {
        [SerializeField] private GameObject orbPrefab;

        public GameObject CreateOrb(Vector3 position, Transform parent = null)
        {
            return Instantiate(orbPrefab, position, Quaternion.identity, parent);
        }
    }
}