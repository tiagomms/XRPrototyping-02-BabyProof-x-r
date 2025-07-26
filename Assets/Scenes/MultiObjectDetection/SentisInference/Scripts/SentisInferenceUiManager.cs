// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using Meta.XR.Samples;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public partial class SentisInferenceUiManager : MonoBehaviour
    {

        [Header("Placement configureation")]
        [SerializeField] protected EnvironmentRayCastSampleManager m_environmentRaycast;
        [SerializeField] protected WebCamTextureManager m_webCamTextureManager;
        protected PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("Prefab display references")]
        [SerializeField] protected BasePrefabManager m_detectionPrefabManager;
        
        [Header("UI display references")]
        [SerializeField] protected SentisObjectDetectedUiManager m_detectionCanvas;
        [SerializeField] protected RawImage m_displayImage;
        [SerializeField] protected Sprite m_boxTexture;
        [SerializeField] protected Font m_font;
        [SerializeField] protected int m_fontSize = 80;

        [SerializeField] private Color m_boxColor;
        [SerializeField] private Color m_fontColor;
        [Space(10)]
        public UnityEvent<int> OnObjectsDetected;

        [Space(10)]
        [Header("Debug purposes")]
        [SerializeField] protected Vector2Int debugResolution = new(1280, 960);
        [SerializeField] protected TestImageManager testImageManager;
        [SerializeField] protected Camera debugCamera;

        public List<BoundingBox> CurrentBoundingBoxList = new();

        protected string[] m_labels;
        protected List<GameObject> m_boxPool = new();
        protected Transform m_displayLocation;

        #region Unity Functions
        protected virtual void Start()
        {
            m_displayLocation = m_displayImage.transform;
        }
        #endregion

        #region Detection Functions
        public virtual void OnObjectDetectionError()
        {
            // Clear current boxes
            ClearAnnotations();

            // Set obejct found to 0
            OnObjectsDetected?.Invoke(0);
        }
        #endregion

        #region BoundingBoxes functions
        public virtual void SetLabels(TextAsset labelsAsset)
        {
            //Parse neural net m_labels
            m_labels = labelsAsset.text.Split('\n');
        }

        public void SetDetectionCapture(Texture image)
        {
            m_displayImage.texture = image;
            m_detectionCanvas.CapturePosition();
        }

        public virtual void BuildBoundingBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {
            // Updte canvas position
            m_detectionCanvas.UpdatePosition();

            // Clear current boxes
            ClearAnnotations();

            var displayWidth = m_displayImage.rectTransform.rect.width;
            var displayHeight = m_displayImage.rectTransform.rect.height;

            var scaleX = displayWidth / imageWidth;
            var scaleY = displayHeight / imageHeight;

            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            var boxesFound = output.shape[0];
            if (boxesFound <= 0)
            {
                OnObjectsDetected?.Invoke(0);
                return;
            }
            var maxBoxes = Mathf.Min(boxesFound, 200);

            OnObjectsDetected?.Invoke(maxBoxes);

            // Get camera resolution
            Vector2Int camRes;
#if !UNITY_EDITOR
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            camRes = intrinsics.Resolution;
#else
            camRes = debugResolution;
#endif
            //Draw the bounding boxes
            for (var n = 0; n < maxBoxes; n++)
            {
                // Get bounding box center coordinates
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var perX = (centerX + halfWidth) / displayWidth;
                var perY = (centerY + halfHeight) / displayHeight;
                var boxWidth = output[n, 2] * scaleX;
                var boxHeight = output[n, 3] * scaleY;

                // Get object class name
                var classname = m_labels[labelIDs[n]].Replace(" ", "_");

                var worldPos = CalculateWorldPosition(perX, perY, camRes, m_environmentRaycast);
                if (worldPos == null)
                {
                    continue;
                }

                // Get the 3D marker world position using Depth Raycast

                //var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
                //var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
                //var worldPos = m_environmentRaycast.PlaceGameObjectByScreenPos(ray);

                // Create a new bounding box
                var box = new BoundingBox
                {
                    Id = n, // added Id (n) for label identification
                    CenterX = centerX,
                    CenterY = centerY,
                    ClassName = classname,
                    Width = boxWidth,
                    Height = boxHeight,
                    UILabel = $"{classname}", // TODO: adjust here accordingly
                    LogLabel = $"Id: {n} Class: {classname} Center (px): {(int)centerX},{(int)centerY} Center (%): {perX:0.00},{perY:0.00}",
                    WorldPos = worldPos,
                };

                // Add to the list of boxes
                CurrentBoundingBoxList.Add(box);

                // NOTE: Draw 2D box () moved this logic to DrawBoundingBoxes for separation of concerns
                //DrawBox(box, n, m_boxColor, m_fontColor);
            }
        }

        public virtual void DrawBoundingBoxes()
        {
            foreach (var box in CurrentBoundingBoxList)
            {
                DrawBox(box, box.Id, m_boxColor, m_fontColor);
            }
        }

        public virtual void DrawPrefabs()
        {
            if (m_detectionPrefabManager != null)
            {
                m_detectionPrefabManager.UpdatePrefabs(CurrentBoundingBoxList);
            }
        }

        protected virtual void ClearAnnotations()
        {
            foreach (var box in m_boxPool)
            {
                box?.SetActive(false);
            }
            CurrentBoundingBoxList.Clear();
        }

        // NOTE: since there is a clear every redraw (there is no need to remake panels)
        protected virtual void DrawBox(BoundingBox box, int index, Color color, Color fontColor)
        {
            //Create the bounding box graphic or get from pool
            GameObject panel;
            if (index < m_boxPool.Count)
            {
                panel = m_boxPool[index];
                if (panel == null)
                {
                    panel = CreateNewBox(color, fontColor);
                }
                else
                {
                    panel.SetActive(true);
                }
            }
            else
            {
                panel = CreateNewBox(color, fontColor);
            }
            //Set box position
            panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY, box.WorldPos.HasValue ? box.WorldPos.Value.z : 0.0f);
            //Set box rotation
#if UNITY_EDITOR

#else
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - m_detectionCanvas.GetCapturedCameraPosition());
#endif
            //Set box size
            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(box.Width, box.Height);
            //Set label text
            var label = panel.GetComponentInChildren<Text>();
            label.text = box.LogLabel;
            label.fontSize = 12;
        }

        protected virtual GameObject CreateNewBox(Color color, Color fontColor)
        {
            //Create the box and set image
            var panel = new GameObject("ObjectBox");
            _ = panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = m_boxTexture;
            img.type = Image.Type.Sliced;
            img.fillCenter = false;
            panel.transform.SetParent(m_displayLocation, false);

            //Create the label
            var text = new GameObject("ObjectLabel");
            _ = text.AddComponent<CanvasRenderer>();
            text.transform.SetParent(panel.transform, false);
            var txt = text.AddComponent<Text>();
            txt.font = m_font;
            txt.color = fontColor;
            txt.fontSize = m_fontSize;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rt2 = text.GetComponent<RectTransform>();
            rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
            rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
            rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
            rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
            rt2.anchorMin = new Vector2(0, 0);
            rt2.anchorMax = new Vector2(1, 1);

            m_boxPool.Add(panel);
            return panel;
        }
        #endregion

        #region World-Position Calculations
        protected Vector3? CalculateWorldPosition(float perX, float perY, Vector2Int camRes, EnvironmentRayCastSampleManager environmentRaycast)
        {
            // Get the 3D marker world position using Depth Raycast
            var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
            Vector3? worldPos = null;
#if !UNITY_EDITOR
            var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
#else
            if (testImageManager == null)
            {
                Debug.LogWarning("TestImageManager reference is missing. Cannot calculate world position in Editor mode.");
                return null;
            }

            // Get the raw image's transform
            var rawImageTransform = testImageManager.transform;
            var rawImagePosition = rawImageTransform.position;
            var rawImageRotation = rawImageTransform.rotation;

            // Get the raw image's dimensions in world space
            var rawImageRect = testImageManager.RawImageToDisplay.GetComponent<RectTransform>();
            if (rawImageRect == null)
            {
                Debug.LogWarning("Raw image RectTransform is missing. Cannot calculate world position in Editor mode.");
                return null;
            }

            // Calculate the world space dimensions of the raw image
            var imageWidth = rawImageRect.rect.width * rawImageRect.lossyScale.x;
            var imageHeight = rawImageRect.rect.height * rawImageRect.lossyScale.y;

            // Calculate the offset from the center of the image based on percentages
            // perX: 0 = left edge, 1 = right edge
            // perY: 0 = top edge, 1 = bottom edge
            var xOffset = (perX - 0.5f) * imageWidth;
            var yOffset = (perY - 0.5f) * imageHeight;

            // Calculate the world position by offsetting from the raw image's center
            worldPos = rawImagePosition +
                              rawImageRotation * new Vector3(xOffset, yOffset, 0);


            Debug.Log($"[CalculateWorldPosition] UNITY_EDITOR {(worldPos - debugCamera.transform.position)}; perX: {perX}; perY: {perY}; width {imageWidth}; height: {imageHeight}; Offsets x {xOffset}; y {yOffset}");
            // Create a ray from the camera to this point
            if (debugCamera == null)
            {
                Debug.LogWarning("Main camera not found. Cannot calculate world position in Editor mode.");
                return null;
            }

            var ray = new Ray(debugCamera.transform.position, ((Vector3)worldPos - debugCamera.transform.position).normalized);
#endif
            
            // NOTE: way of avoiding Oculus altogether if you just want to test on UNITY EDITOR the Unity Sentis
            if (OVRManager.instance != null)
            {
                worldPos = environmentRaycast.PlaceGameObjectByScreenPos(ray);
            }

            return worldPos;
        }

        #endregion
    }
}
