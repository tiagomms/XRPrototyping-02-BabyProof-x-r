using UnityEngine;

namespace Utils
{
    public class FollowAxesWithDistanceThreshold : MonoBehaviour
    {
        [Tooltip("The transform to follow.")]
        public Transform target;

        [Tooltip("How fast this object follows the target.")]
        public float followSpeed = 5f;

        [Tooltip("Only move if the total distance exceeds this threshold.")]
        public float distanceThreshold = 0.05f;

        [Header("Axis Toggles")]
        public bool followX = false;
        public bool followY = false;
        public bool followZ = false;

        [Header("Offsets")]
        public float xOffset = 0f;
        public float yOffset = 0f;
        public float zOffset = 0f;

        private void OnValidate()
        {
            // Find center eye transform - prioritize "CenterEyeAnchor" GameObject, fallback to first Camera
            if (target == null)
            {
                GameObject centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
                if (centerEyeAnchor != null)
                {
                    target = centerEyeAnchor.transform;
                }
                else
                {
                    GameObject firstCamera = GameObject.FindWithTag("MainCamera");
                    if (firstCamera != null)
                    {
                        target = firstCamera.transform;
                    }
                }
            }
        } 

        private void Update()
        {
            if (target == null) return;

            Vector3 current = transform.position;
            Vector3 targetPos = target.position;

            // Build desired position using toggles and offsets
            float desiredX = followX ? targetPos.x + xOffset : current.x;
            float desiredY = followY ? targetPos.y + yOffset : current.y;
            float desiredZ = followZ ? targetPos.z + zOffset : current.z;

            Vector3 desiredPosition = new Vector3(desiredX, desiredY, desiredZ);

            // Calculate distance
            float distance = Vector3.Distance(current, desiredPosition);

            if (distance > distanceThreshold)
            {
                Vector3 smoothed = Vector3.Lerp(current, desiredPosition, followSpeed * Time.deltaTime);
                transform.position = smoothed;
            }
        }
    }
}
