/// <summary>
/// Loop Pedal XR Prototype - Production-Ready Codebase
/// Architecture using SOLID principles and modular design for extensibility.
/// Dependencies: DOTween (for animations), Unity XR SDKs
/// </summary>

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XRLoopPedal.AudioSystem
{
    // Manages loop orb coordination
    public class LoopOrbManager : MonoBehaviour
    {
        private List<LoopOrbController> orbs = new();

        public void RegisterOrb(LoopOrbController orb)
        {
            orbs.Add(orb);
        }

        public bool IsAnyOrbPlaying => orbs.Any(o => o.CurrentState == LoopOrbState.Playing);

        public void HandleRecording(LoopOrbController newOrb)
        {
            if (!IsAnyOrbPlaying)
            {
                newOrb.SetState(LoopOrbState.Recording);
                Debug.Log("First Orb - defines tempo");
            }
            else
            {
                newOrb.SetState(LoopOrbState.OnHold);
            }
        }
    }
}
