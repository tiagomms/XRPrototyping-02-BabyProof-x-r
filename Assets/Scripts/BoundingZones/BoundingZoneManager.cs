using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoundingZoneManager : MonoBehaviour
{
    [SerializeField] private LabelOffsetConfig defaultOffsetConfig;

    [Header("Debug")]
    [SerializeField] private InputActionReference debugButton;
    [SerializeField] private Material defaultExternalMaterial;
    [SerializeField] private Material defaultInternalMaterial;

    public bool IsInitialized { get; private set; }

    private List<BoundingZoneChecker> allZones = new List<BoundingZoneChecker>();
    public List<BoundingZoneChecker> AllZones => allZones;
    private MRUK _mruk;

    public enum DebugState
    {
        None = 0,
        ShowInternal = 1,
        ShowExternal = 2,
        ShowBoth = 3
    }

    private DebugState _debugState = DebugState.None;

    public void Initialize()
    {
        _mruk = MRUK.Instance;

        if (_mruk == null)
        {
            XRDebugLogViewer.LogError($"No MRUK present - please set it up");
            return;
        }

        if (!_mruk.GetCurrentRoom())
        {
            XRDebugLogViewer.LogError($"No Room setup - please set a Room Environment in Meta Quest Settings to use this feature");
            _mruk = null;
            return;
        }

        SetupBoundingZones(_mruk.GetCurrentRoom().Anchors);
        IsInitialized = true;

    }

    private void Start()
    {
        debugButton.action.started += ToggleDebugMode;
    }

    private void OnDestroy()
    {
        debugButton.action.started -= ToggleDebugMode;
    }


    public void SetupBoundingZones(List<MRUKAnchor> anchors)
    {
        allZones.Clear();

        foreach (MRUKAnchor anchor in anchors)
        {
            Rect bounds = new();
            bool isBoundingZone = false;

            if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
            {
                bounds = anchor.PlaneRect.Value;
                // face transform is the anchor itself since it is a plane
                isBoundingZone = true;
            }
            // it is a volume - we will need to get the upper surface
            else if (anchor.VolumeBounds.HasValue)
            {
                // NOTE: according to <<AnchorPrefabSpawner>> the anchor z,y orientation is flipped. Hence we want the xy axis
                bounds = new()
                {
                    xMin = anchor.VolumeBounds.Value.min.x,
                    xMax = anchor.VolumeBounds.Value.max.x,
                    yMin = -anchor.VolumeBounds.Value.max.y,
                    yMax = -anchor.VolumeBounds.Value.min.y
                };
                // by multiplying the transform it moves and rotates accordingly to upper face
                isBoundingZone = true;
            }

            // if it is not a anchor to create bounding zones, then ignore
            if (!isBoundingZone) continue;
            CreateBoundingZone(anchor, bounds);
        }
    }

    private BoundingZoneChecker CreateBoundingZone(MRUKAnchor anchor, Rect boundsRect)
    {
        MRUKAnchor.SceneLabels labelID = anchor.Label;

        XRDebugLogViewer.Log($"{labelID} - AnchorPosition {anchor.transform.position} - Rect {boundsRect}");
        string objName = $"Zone_{labelID}_{allZones.Count}";
        GameObject zoneObj = new GameObject(objName);

        // FIXME: anchors tend to be mostly flat on the XZ axis, so I assume this. In the future, take a better look
        Quaternion planeAngle = Quaternion.Euler(0f, anchor.transform.eulerAngles.y, 0f);
        zoneObj.transform.SetPositionAndRotation(anchor.transform.position, planeAngle);
        zoneObj.transform.SetParent(transform);
        zoneObj.transform.localScale = Vector3.one;

        var checker = zoneObj.AddComponent<BoundingZoneChecker>();
        checker.Initialize(labelID, objName, boundsRect, defaultOffsetConfig, defaultExternalMaterial, defaultInternalMaterial);

        allZones.Add(checker);
        return checker;
    }

    /// <summary>
    /// Returns the first zone label where the point is in range.
    /// </summary>
    public bool TryGetZone(Vector3 point, out BoundingZoneChecker matchingZone)
    {
        if (IsInitialized)
        {
            foreach (var zone in allZones)
            {
                if (zone.IsPointInZone(point))
                {
                    matchingZone = zone;
                    return true;
                }
            }
        }
        matchingZone = null;
        return false;
    }

    /// <summary>
    /// Optionally returns just the labelID of the matched zone.
    /// </summary>
    public MRUKAnchor.SceneLabels? GetZoneLabel(Vector3 point)
    {
        return TryGetZone(point, out var zone) ? zone.labelID : null;
    }

    /// <summary>
    /// Optionally returns the unique ID of the matched zone.
    /// </summary>
    public string GetZoneID(Vector3 point)
    {
        return TryGetZone(point, out var zone) ? zone.id : null;
    }


    
    private void ToggleDebugMode(InputAction.CallbackContext context)
    {
        if (!_mruk) return;
        var newState = (DebugState)(((int)_debugState + 1) % 4);

        switch (newState)
        {
            case DebugState.None:
                foreach (var item in allZones)
                {
                    item.HideDebugCubes();
                }
                break;
            case DebugState.ShowInternal:
                foreach (var item in allZones)
                {
                    item.ShowOnlyInternalCube();
                }
                break;
            case DebugState.ShowExternal:
                foreach (var item in allZones)
                {
                    item.ShowOnlyExternalCube();
                }
                break;
            case DebugState.ShowBoth:
                foreach (var item in allZones)
                {
                    item.ShowBothDebugCubes();
                }
                break;
        }
        _debugState = newState;
    }
}
