using System;
using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;


[CreateAssetMenu(menuName = "BabyProof/Label Offset Config", fileName = "LabelOffsetConfig")]
public class LabelOffsetConfig : ScriptableObject
{
    [Serializable]
    public struct OffsetSet
    {
        [Tooltip("Horizontal offset as a ratio of the surface width/height. For external bounds: 1.0-1.5 (100%-150% of surface size). For internal bounds: 0.0-1.0 (0%-100% of surface size).")]
        [Range(0f, 1.5f)]
        public float HorizontalRatio;

        [Tooltip("Vertical offset in meters. Must be positive.")]
        [Min(0f)]
        public float VerticalMeters;
    }
    
    [Serializable]
    public struct OffsetEntry
    {
        public MRUKAnchor.SceneLabels label;
        
        [Tooltip("External offset configuration. HorizontalRatio should be between 1.0 and 1.5 to create a safety margin around the surface.")]
        public OffsetSet ExternalOffset;
        
        [Tooltip("Internal offset configuration. HorizontalRatio should be between 0.0 and 1.0 to create a hole inside the surface.")]
        public OffsetSet InternalOffset;
    }

    [SerializeField] private List<OffsetEntry> labelOffsets;
    
    [Tooltip("Default external offset configuration. HorizontalRatio should be between 1.0 and 1.5 to create a safety margin around the surface.")]
    [SerializeField] private OffsetSet defaultExternalOffset = new OffsetSet { HorizontalRatio = 1.2f, VerticalMeters = 0.2f };
    
    [Tooltip("Default internal offset configuration. HorizontalRatio should be between 0.0 and 1.0 to create a hole inside the surface.")]
    [SerializeField] private OffsetSet defaultInternalOffset = new OffsetSet { HorizontalRatio = 0.8f, VerticalMeters = 0.2f };

    public (OffsetSet external, OffsetSet internalSet) GetOffsets(MRUKAnchor.SceneLabels label)
    {
        foreach (OffsetEntry entry in labelOffsets)
        {
            if (entry.label == label)
                return (entry.ExternalOffset, entry.InternalOffset);
        }

        return (defaultExternalOffset, defaultInternalOffset);
    }
}
