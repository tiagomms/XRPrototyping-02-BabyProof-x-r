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


    private List<BoundingZoneChecker> allZones = new List<BoundingZoneChecker>();
    private MRUK _mruk;
    private List<MRUKAnchor> _mrukVolumeAnchors;

    private bool isDebugModeOn = false;


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

    }

    private void Start()
    {
        debugButton.action.started += ToggleDebugMode;
    }

    private void OnDestroy()
    {
        debugButton.action.started -= ToggleDebugMode;
    }

    private void ToggleDebugMode(InputAction.CallbackContext context)
    {
        if (!_mruk) return;

        if (!isDebugModeOn)
        {
            ShowDebugBoundingZones();
        }
        else
        {
            HideDebugBoundingZones();
        }
    }

    public void SetupBoundingZones(List<MRUKAnchor> anchors)
    {
        allZones.Clear();
        _mrukVolumeAnchors = anchors;

        foreach (MRUKAnchor anchor in anchors)
        {
            Rect bounds = new();
            Matrix4x4 faceTransform = anchor.transform.localToWorldMatrix;
            bool isBoundingZone = false;

            if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
            {
                bounds = anchor.PlaneRect.Value;
                faceTransform *= Matrix4x4.TRS(
                    new Vector3(0f, 0f, 0f),
                    Quaternion.Euler(-90f, 0f, 0f),
                    Vector3.one
                );
                // face transform is the anchor itself since it is a plane
                isBoundingZone = true;
            }
            // it is a volume - we will need to get the upper surface
            else if (anchor.VolumeBounds != null)
            {
                bounds = new()
                {
                    xMin = anchor.VolumeBounds.Value.min.x,
                    xMax = anchor.VolumeBounds.Value.max.x,
                    yMin = -anchor.VolumeBounds.Value.max.z,
                    yMax = -anchor.VolumeBounds.Value.min.z
                };
                // by multiplying the transform it moves and rotates accordingly to upper face
                faceTransform *= Matrix4x4.TRS(
                    new Vector3(0f, anchor.VolumeBounds.Value.max.y, 0f),
                    Quaternion.Euler(-90f, 0f, 0f),
                    Vector3.one
                );
                isBoundingZone = true;
            }

            // if it is not a anchor to create bounding zones, then ignore
            if (!isBoundingZone) continue;
            CreateBoundingZone(anchor.Label, bounds, faceTransform);
        }
    }

    private BoundingZoneChecker CreateBoundingZone(MRUKAnchor.SceneLabels labelID, Rect boundsRect, Matrix4x4 faceTransform)
    {
        XRDebugLogViewer.Log($"{labelID} - Rect {boundsRect}, Transform {faceTransform}");
        string objName = $"Zone_{labelID}_{allZones.Count}";
        GameObject zoneObj = new GameObject(objName);
        zoneObj.transform.SetParent(transform);

        // Apply faceTransform (position + rotation only)
        zoneObj.transform.position = faceTransform.GetPosition();//.GetColumn(3); // position from matrix
        //zoneObj.transform.rotation = faceTransform.rotation;
        zoneObj.transform.rotation = Quaternion.LookRotation(
            faceTransform.GetColumn(2), // forward
            faceTransform.GetColumn(1)  // up
        );

        var checker = zoneObj.AddComponent<BoundingZoneChecker>();
        checker.Initialize(labelID, objName, boundsRect, faceTransform, defaultOffsetConfig, defaultExternalMaterial, defaultInternalMaterial);

        allZones.Add(checker);
        return checker;
    }

    public void ShowDebugBoundingZones()
    {
        foreach (var item in allZones)
        {
            item.ShowDebugCubes();
        }
        isDebugModeOn = true;
    }

    public void HideDebugBoundingZones()
    {
        foreach (var item in allZones)
        {
            item.HideDebugCubes();
        }
        isDebugModeOn = false;
    }

    /// <summary>
    /// Returns the first zone label where the point is in range.
    /// </summary>
    public bool TryGetZone(Vector3 point, out BoundingZoneChecker matchingZone)
    {
        foreach (var zone in allZones)
        {
            if (zone.IsPointInZone(point))
            {
                matchingZone = zone;
                return true;
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
}
