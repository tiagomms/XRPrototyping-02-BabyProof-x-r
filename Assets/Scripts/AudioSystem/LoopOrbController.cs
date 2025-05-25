using System.Collections.Generic;
using UnityEngine;

namespace XRLoopPedal.AudioSystem
{
    // Core controller for each orb
    public class LoopOrbController : MonoBehaviour, ILoopOrb
    {
        [SerializeField] private GameObject stepsParent;
        [SerializeField] private List<LoopSteps3D> stepVisuals;
        [SerializeField] private MeshRenderer orbMeshRenderer;

        [Header("Loop Settings")]
        [SerializeField] private float loopLength = 4f; // In seconds
        [SerializeField] private int totalSteps = 4;
        [SerializeField] private LoopOrbState currentState = LoopOrbState.Disabled;

        private int currentStep = 0;
        private float loopTimer = 0f;

        public LoopOrbState CurrentState => currentState;

        public void SetState(LoopOrbState newState)
        {
            currentState = newState;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            bool showSteps = currentState != LoopOrbState.Disabled;
            stepsParent.SetActive(showSteps);
        }

        private void Update()
        {
            if (currentState != LoopOrbState.Playing && currentState != LoopOrbState.Recording)
                return;

            loopTimer += Time.deltaTime;

            float stepDuration = loopLength / totalSteps;
            int step = Mathf.FloorToInt(loopTimer / stepDuration);

            if (step != currentStep)
            {
                currentStep = step % totalSteps;
                PlayStep(currentStep);

                if (currentStep == 0 && currentState == LoopOrbState.Recording)
                {
                    // Loop restart reached
                    Debug.Log("Loop Restart");
                }
            }

            if (loopTimer >= loopLength)
                loopTimer = 0;
        }

        public void PlayStep(int index)
        {
            for (int i = 0; i < stepVisuals.Count; i++)
                stepVisuals[i].SetActive(i == index);
        }

        public void ResetLoop()
        {
            currentStep = 0;
            loopTimer = 0f;
        }
    }
}