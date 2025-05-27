using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    /// <summary>
    /// Manages test images for development in the Unity Editor.
    /// This component will cycle through test images and provide them to the inference system.
    /// </summary>
    public class TestImageManager : MonoBehaviour
    {
        [Header("Test Images")]
        [SerializeField] private List<Texture2D> m_testImages = new();
        [SerializeField] private float m_imageChangeInterval = 3f;

        [SerializeField] private RawImage m_rawImageToDisplay;

        public RawImage RawImageToDisplay => m_rawImageToDisplay;

        [Header("Debug")]
        [SerializeField] private bool m_showDebugInfo = true;

        private int m_currentImageIndex = 0;
        private float m_nextImageChangeTime;
        private bool m_isInitialized = false;

        /// <summary>
        /// Gets the current test image texture.
        /// </summary>
        public Texture CurrentTexture { get; private set; }

        private void Start()
        {
            if (m_testImages.Count == 0)
            {
                Debug.LogWarning("No test images assigned to TestImageManager. Please add some test images in the inspector.");
                return;
            }

            m_isInitialized = true;
            CurrentTexture = m_testImages[0];
            m_nextImageChangeTime = Time.time + m_imageChangeInterval;

            if (m_showDebugInfo)
            {
                Debug.Log($"TestImageManager initialized with {m_testImages.Count} images. First image: {m_testImages[0].name}");
            }
        }

        private void Update()
        {
            if (!m_isInitialized || m_testImages.Count == 0) return;

            if (Time.time >= m_nextImageChangeTime)
            {
                CycleToNextImage();
            }
        }

        private void CycleToNextImage()
        {
            m_currentImageIndex = (m_currentImageIndex + 1) % m_testImages.Count;
            CurrentTexture = m_testImages[m_currentImageIndex];
            m_nextImageChangeTime = Time.time + m_imageChangeInterval;

            if (m_showDebugInfo)
            {
                Debug.Log($"TestImageManager: Switched to image {m_currentImageIndex + 1}/{m_testImages.Count}: {m_testImages[m_currentImageIndex].name}");
            }

            if (m_rawImageToDisplay)
            {
                m_rawImageToDisplay.texture = CurrentTexture;
            }
        }

        /// <summary>
        /// Adds a test image to the manager.
        /// </summary>
        public void AddTestImage(Texture2D image)
        {
            if (image == null) return;
            
            m_testImages.Add(image);
            if (!m_isInitialized)
            {
                m_isInitialized = true;
                CurrentTexture = image;
                m_nextImageChangeTime = Time.time + m_imageChangeInterval;
            }
        }

        /// <summary>
        /// Clears all test images.
        /// </summary>
        public void ClearTestImages()
        {
            m_testImages.Clear();
            m_isInitialized = false;
            CurrentTexture = null;
        }
    }
} 