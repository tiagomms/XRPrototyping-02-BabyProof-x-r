// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    //base bounding box implementation
    public struct BoundingBox
    {
        public int Id; // added Id (n) for label identification
        public float CenterX;
        public float CenterY;
        public float Width;
        public float Height;
        public string LogLabel; // Renamed from Label - separated from UILabel which is cleaner
        public string UILabel;
        public Vector3? WorldPos;
        public string ClassName;

        // BabyProofxr
        public bool IsDangerous;
        public bool IsChockingHazard;
    }
}
