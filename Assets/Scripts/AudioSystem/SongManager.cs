using UnityEngine;

namespace XRLoopPedal.AudioSystem
{
    // Controls global state and master tempo
    public class SongManager : MonoBehaviour
    {
        private static SongManager _instance;
        public static SongManager Instance => _instance;

        [SerializeField] private float bpm = 90f;

        public float LoopDuration => 60f / bpm * 4; // Assuming 4 beats per bar

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);
            else
                _instance = this;
        }
    }
}