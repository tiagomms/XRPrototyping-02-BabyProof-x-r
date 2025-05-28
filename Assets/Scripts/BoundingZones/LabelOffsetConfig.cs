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
        public float Horizontal;
        public float Vertical;
    }
    
    [Serializable]
    public struct OffsetEntry
    {
        public MRUKAnchor.SceneLabels label;
        public OffsetSet ExternalOffset;
        public OffsetSet InternalOffset;
    }

    [SerializeField] private List<OffsetEntry> labelOffsets;
    [SerializeField] private OffsetSet defaultExternalOffset = new OffsetSet { Horizontal = 0.2f, Vertical = 0.2f };
    [SerializeField] private OffsetSet defaultInternalOffset = new OffsetSet { Horizontal = 0.2f, Vertical = 0.2f };

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
