using System;
using System.Threading.Tasks;
using UnityEngine;

namespace AI
{
    public enum VoiceIntent { Activate, Deactivate, Unknown }
    public abstract class BaseAIReasoning : MonoBehaviour
    {
        public virtual async Task<VoiceIntent> ParseIntent(string transcript)
        {
            await Task.Yield();
            throw new NotImplementedException();
        }
    }
}