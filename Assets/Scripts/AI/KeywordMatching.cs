using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AI
{
    public class KeywordMatching : BaseAIReasoning
    {
        List<string> activateKeywords = new() { "activate", "start", "go", "enable", "turn on", "launch", "begin", "opne" };
        List<string> deactivateKeywords = new() { "deactivate", "stop", "disable", "turn off", "cancel", "end", "shutdown", "close" };
        public override async Task<VoiceIntent> ParseIntent(string transcript)
        {
            transcript = transcript.ToLower();

            if (activateKeywords.Any(k => transcript.Contains(k)))
                return VoiceIntent.Activate;

            if (deactivateKeywords.Any(k => transcript.Contains(k)))
                return VoiceIntent.Deactivate;

            return VoiceIntent.Unknown;
        }
    }
}