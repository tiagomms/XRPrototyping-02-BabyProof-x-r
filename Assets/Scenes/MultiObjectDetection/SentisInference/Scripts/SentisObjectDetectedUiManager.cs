// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Assertions;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class SentisObjectDetectedUiManager : MonoBehaviour
    {
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        private PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;
        private Vector2Int CameraResolution => m_webCamTextureManager.RequestedResolution;
        [SerializeField] private GameObject m_detectionCanvas;
        [SerializeField] private float m_canvasDistance = 1f;

        [Header("Test in play mode")]
        [SerializeField] protected TestImageManager m_testImageManager;
        [SerializeField] protected Camera m_debugCamera;

        private Pose m_captureCameraPose;
        private Vector3 m_capturePosition;
        private Quaternion m_captureRotation;

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            // In editor, we don't need to wait for camera permissions
            yield break;
#endif

            if (m_webCamTextureManager == null)
            {
                Debug.LogError($"PCA: {nameof(m_webCamTextureManager)} field is required "
                            + $"for the component {nameof(SentisObjectDetectedUiManager)} to operate properly");
                enabled = false;
                yield break;
            }

            // Make sure the manager is disabled in scene and enable it only when the required permissions have been granted
            Assert.IsFalse(m_webCamTextureManager.enabled);
            while (PassthroughCameraPermissions.HasCameraPermission != true)
            {
                yield return null;
            }

            // Set the 'requestedResolution' and enable the manager
            m_webCamTextureManager.RequestedResolution = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye).Resolution;
            m_webCamTextureManager.enabled = true;

            var cameraCanvasRectTransform = m_detectionCanvas.GetComponentInChildren<RectTransform>();
            var leftSidePointInCamera = PassthroughCameraUtils.ScreenPointToRayInCamera(CameraEye, new Vector2Int(0, CameraResolution.y / 2));
            var rightSidePointInCamera = PassthroughCameraUtils.ScreenPointToRayInCamera(CameraEye, new Vector2Int(CameraResolution.x, CameraResolution.y / 2));
            var horizontalFoVDegrees = Vector3.Angle(leftSidePointInCamera.direction, rightSidePointInCamera.direction);
            var horizontalFoVRadians = horizontalFoVDegrees / 180 * Math.PI;
            var newCanvasWidthInMeters = 2 * m_canvasDistance * Math.Tan(horizontalFoVRadians / 2);
            var localScale = (float)(newCanvasWidthInMeters / cameraCanvasRectTransform.sizeDelta.x);
            cameraCanvasRectTransform.localScale = new Vector3(localScale, localScale, localScale);
        }

        public void UpdatePosition()
        {
            // Position the canvas in front of the camera
            m_detectionCanvas.transform.position = m_capturePosition;
            m_detectionCanvas.transform.rotation = m_captureRotation;
        }

        public void CapturePosition()
        {
#if UNITY_EDITOR
            // In editor, use a default position
            m_capturePosition = m_testImageManager.transform.position - m_debugCamera.transform.forward * 0.02f;
            m_captureRotation = m_testImageManager.transform.rotation;
            //Vector3 direction = m_testImageManager.transform.position - m_debugCamera.transform.position;
            //m_captureRotation = Quaternion.LookRotation(direction);//Quaternion.Euler(0, m_testImageManager.transform.rotation.y, 0);
            return;
#endif

            // Capture the camera pose and position the canvas in front of the camera
            m_captureCameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(CameraEye);
            m_capturePosition = m_captureCameraPose.position + m_captureCameraPose.rotation * Vector3.forward * m_canvasDistance;
            m_captureRotation = Quaternion.Euler(0, m_captureCameraPose.rotation.eulerAngles.y, 0);
        }

        public Vector3 GetCapturedCameraPosition()
        {
#if UNITY_EDITOR
            return m_testImageManager.transform.position;
#endif
            return m_captureCameraPose.position;
        }
    }
}
