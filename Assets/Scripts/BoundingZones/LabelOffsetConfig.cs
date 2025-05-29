using System;
using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;


[CreateAssetMenu(menuName = "BabyProof/Label Offset Config", fileName = "LabelOffsetConfig")]
public class LabelOffsetConfig : ScriptableObject
{
    [Serializable]
    public struct ExternalOffset
    {
        [Tooltip("Horizontal offset as a ratio of the surface width/height. Must be between 1.0 and 1.5 (100%-150% of surface size) to create a safety margin around the surface.")]
        [Range(1f, 1.5f)]
        public float HorizontalRatio;

        [Tooltip("Vertical offset in meters. Must be positive.")]
        [Min(0f)]
        public float VerticalMeters;
    }

    [Serializable]
    public struct InternalOffset
    {
        [Tooltip("Horizontal offset as a ratio of the surface width/height. Must be between 0.0 and 1.0 (0%-100% of surface size) to create a hole inside the surface.")]
        [Range(0f, 1f)]
        public float HorizontalRatio;

        [Tooltip("Vertical offset in meters. Must be positive.")]
        [Min(0f)]
        public float VerticalMeters;
    }
    
    [Serializable]
    public struct OffsetEntry
    {
        public MRUKAnchor.SceneLabels label;
        
        [Tooltip("External offset configuration for creating a safety margin around the surface.")]
        public ExternalOffset ExternalOffset;
        
        [Tooltip("Internal offset configuration for creating a hole inside the surface.")]
        public InternalOffset InternalOffset;
    }

    [SerializeField] private List<OffsetEntry> labelOffsets;
    
    [Tooltip("Default external offset configuration for creating a safety margin around the surface.")]
    [SerializeField] private ExternalOffset defaultExternalOffset = new ExternalOffset { HorizontalRatio = 1.2f, VerticalMeters = 0.2f };
    
    [Tooltip("Default internal offset configuration for creating a hole inside the surface.")]
    [SerializeField] private InternalOffset defaultInternalOffset = new InternalOffset { HorizontalRatio = 0.8f, VerticalMeters = 0.2f };

    public (ExternalOffset external, InternalOffset internalSet) GetOffsets(MRUKAnchor.SceneLabels label)
    {
        foreach (OffsetEntry entry in labelOffsets)
        {
            if (entry.label == label)
                return (entry.ExternalOffset, entry.InternalOffset);
        }

        return (defaultExternalOffset, defaultInternalOffset);
    }
}
