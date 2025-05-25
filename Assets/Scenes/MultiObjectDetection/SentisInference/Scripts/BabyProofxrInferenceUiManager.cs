// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Data.Common;
using DG.Tweening.Plugins.Options;
using Meta.XR.Samples;
using Unity.Burst.Intrinsics;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //[MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class BabyProofxrInferenceUiManager : MonoBehaviour
    {
        [Header("Placement configureation")]
        [SerializeField] private EnvironmentRayCastSampleManager m_environmentRaycast;
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        private PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("UI display references")]
        [SerializeField] private SentisObjectDetectedUiManager m_detectionCanvas;
        [SerializeField] private RawImage m_displayImage;
        [SerializeField] private Sprite m_boxTexture;
        [SerializeField] private Font m_font;
        [SerializeField] private int m_fontSize = 80;

        [Header("Dangerous display references")]
        [SerializeField] private Color m_dangerousBoxColor;
        [SerializeField] private Color m_dangerousFontColor;

        [Header("Chocking hazard display referneces")]
        [SerializeField] private float chockingHazardMaxSize = 0.032f; // according to studies
        [SerializeField] private Color m_chockingBoxColor;
        [SerializeField] private Color m_chockingFontColor;
        
        [Space(10)]
        public UnityEvent<int> OnObjectsDetected;

        public List<BoundingBox> BoxDrawn = new();

        private string[] m_labels;
        private string[] m_dangerousLabels;
        private List<GameObject> m_boxPool = new();
        private Transform m_displayLocation;

        private Dictionary<int, string> m_dangerousLabelAssetDict;

        private int m_nbrDangerousObjects;
        private int m_nbrChockingObjects;

        //bounding box data
        public struct BoundingBox
        {
            public int Id;
            public bool IsDangerous;
            public bool IsChockingHazard;
            public float CenterX;
            public float CenterY;
            public float Width;
            public float Height;
            public string Label;
            public Vector3? WorldPos;
            public string ClassName;
        }

        #region Unity Functions
        private void Start()
        {
            m_displayLocation = m_displayImage.transform;
        }
        #endregion

        #region Detection Functions
        public void OnObjectDetectionError()
        {
            // Clear current boxes
            ClearAnnotations();

            m_nbrDangerousObjects = 0;
            m_nbrChockingObjects = 0;
            // Set obejct found to 0
            OnObjectsDetected?.Invoke(0);
        }
        #endregion

        #region BoundingBoxes functions
        public void SetLabels(TextAsset labelsAsset, TextAsset dangerousLabels)
        {
            //Parse neural net m_labels
            m_labels = labelsAsset.text.Split('\n');

            // Register the labels of considered dangerous objects for babies
            var dangerousLabelsSplit = dangerousLabels.text.Split('\n');

            // Create dictionary            
            m_dangerousLabelAssetDict = new Dictionary<int, string>();
            foreach (string dangerousLabel in dangerousLabelsSplit)
            {
                int mlClassificationIndex = Array.IndexOf(m_labels, dangerousLabel);
                if (mlClassificationIndex >= 0)
                {
                    m_dangerousLabelAssetDict.Add(mlClassificationIndex, dangerousLabel);
                }
            }
        }

        public void SetDangerousLabels(TextAsset labelsAsset)
        {
            //Parse neural net m_labels
            m_dangerousLabels = labelsAsset.text.Split('\n');
        }

        public void SetDetectionCapture(Texture image)
        {
            m_displayImage.texture = image;
            m_detectionCanvas.CapturePosition();
        }

        public void FilterResults(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
        {

        }

        public void DrawUIBoxes(Tensor<float> output, Tensor<int> labelIDs, float imageWidth, float imageHeight)
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

            //Get the camera intrinsics
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var camRes = intrinsics.Resolution;

            int boxesDetected = 0;
            List<BoundingBox> tempBoundingBoxes = new();

            //Draw the bounding boxes
            for (var n = 0; n < boxesFound; n++)
            {
                // Get bounding box center coordinates
                var centerX = output[n, 0] * scaleX - halfWidth;
                var centerY = output[n, 1] * scaleY - halfHeight;
                var boxWidth = output[n, 2] * scaleX;
                var boxHeight = output[n, 3] * scaleY;

                var centerPerX = (centerX + halfWidth) / displayWidth;
                var centerPerY = (centerY + halfHeight) / displayHeight;
                Vector3? centerWorldPos = CalculateWorldPosition(ref camRes, centerPerX, centerPerY);

                // Calculate surrounding box size in the real world (to understand if it is a chocking hazard)
                Vector2[] vector2s = {
                    new Vector2(-boxWidth / 2, 0),
                    new Vector2(boxWidth / 2, 0),
                    new Vector2(0, -boxHeight / 2),
                    new Vector2(0, boxHeight / 2)
                };
                float[] surroundBoxWorldDistance = new float[4];
                for (int i = 0; i < vector2s.Length; i++)
                {
                    Vector2 v2 = vector2s[i];

                    float perX = (centerX + halfWidth + v2.x) / displayWidth;
                    float perY = (centerY + halfHeight + v2.y) / displayHeight;
                    Vector3? worldPos = CalculateWorldPosition(ref camRes, perX, perY);

                    surroundBoxWorldDistance[i] = worldPos != null ? Vector3.Distance((Vector3)centerWorldPos, (Vector3)worldPos) : Mathf.Infinity;
                }

                // If any of the sizes is a chocking hazard (object small on both sides)
                bool isChockingHazard = surroundBoxWorldDistance[0] + surroundBoxWorldDistance[1] < chockingHazardMaxSize
                    && surroundBoxWorldDistance[2] + surroundBoxWorldDistance[3] < chockingHazardMaxSize;
                
                // check if it is on the list of dangerous objects
                bool isDangerousObject = m_dangerousLabelAssetDict.ContainsKey(labelIDs[n]);

                XRDebugLogViewer.Log($"n: {n} - {labelIDs[n]}: IsDangerous: {isDangerousObject} ; isChockingHazard {isChockingHazard}; surroundingDistance {surroundBoxWorldDistance}");
                // remove if object is not dangerous nor chockingHazard
                if (!isDangerousObject && !isChockingHazard)
                {
                    continue;
                }

                // filtered - let's do this
                // Get object class name
                var classname = m_labels[labelIDs[n]].Replace(" ", "_");
                // Create a new bounding box
                var box = new BoundingBox
                {
                    Id = n,
                    IsDangerous = isDangerousObject,
                    IsChockingHazard = isChockingHazard,
                    CenterX = centerX,
                    CenterY = centerY,
                    ClassName = classname,
                    Width = boxWidth,
                    Height = boxHeight,
                    Label = $"Id: {n} Class: {classname} Center (px): {(int)centerX},{(int)centerY} Center (%): {centerPerX:0.00},{centerPerY:0.00}",
                    WorldPos = centerWorldPos,
                };

                boxesDetected++;
                tempBoundingBoxes.Add(box);

            }
            // limit results
            boxesDetected = Mathf.Min(boxesDetected, 200);

            OnObjectsDetected?.Invoke(boxesDetected);

            for (int i = 0; i < boxesDetected; i++)
            {
                BoundingBox box = tempBoundingBoxes[i];
                // Add to the list of boxes
                BoxDrawn.Add(box);

                // Draw 2D box
                DrawBox(box);
            }

        }

        private Vector3? CalculateWorldPosition(ref Vector2Int camRes, float perX, float perY)
        {
            // Get the 3D marker world position using Depth Raycast
            var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
            var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
            var worldPos = m_environmentRaycast.PlaceGameObjectByScreenPos(ray);
            return worldPos;
        }

        private void ClearAnnotations()
        {
            foreach (var box in m_boxPool)
            {
                box?.SetActive(false);
            }
            BoxDrawn.Clear();
        }

        private void DrawBox(BoundingBox box)
        {
            int id = box.Id;
            Color drawingColor = box.IsDangerous ? m_dangerousBoxColor : m_chockingBoxColor;
            //Create the bounding box graphic or get from pool
            GameObject panel;
            if (id < m_boxPool.Count)
            {
                panel = m_boxPool[id];
                if (panel == null)
                {
                    panel = CreateNewBox(drawingColor);
                }
                else
                {
                    panel.SetActive(true);
                }
            }
            else
            {
                panel = CreateNewBox(drawingColor);
            }
            //Set box position
            panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY, box.WorldPos.HasValue ? box.WorldPos.Value.z : 0.0f);
            //Set box rotation
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - m_detectionCanvas.GetCapturedCameraPosition());
            //Set box size
            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(box.Width, box.Height);
            //Set label text
            var label = panel.GetComponentInChildren<Text>();
            label.text = box.Label;
            label.fontSize = 12;
        }

        private GameObject CreateNewBox(Color color)
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
            txt.color = color;
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
    }
}
