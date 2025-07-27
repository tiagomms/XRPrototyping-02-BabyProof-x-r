using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.IO;

public class BoundingZoneChecker : MonoBehaviour
{
    [Header("Identifiers")]
    [SerializeField] public MRUKAnchor.SceneLabels labelID { get; private set; }
    [SerializeField] public string id { get; private set; }

    [Header("Plane Setup")]
    [SerializeField] public Rect boundsRect { get; private set; } // In local XZ space: position = local center, size = width/height

    [Header("Config")]
    [SerializeField] public LabelOffsetConfig offsetConfig { get; private set; }

    [Header("Debug")]
    [SerializeField] public Material externalMaterial { get; private set; }
    [SerializeField] public Material internalMaterial { get; private set; }

    private Bounds externalBounds;
    private Bounds internalBounds;
    public Bounds ExternalBounds => externalBounds;

    public Bounds InternalBounds => internalBounds;

    private GameObject externalCube;
    private GameObject internalCube;

    private LabelOffsetConfig.ExternalOffset _defaultExternalOffset = new() { HorizontalRatio = 1.2f, VerticalMeters = 0.2f };
    private LabelOffsetConfig.InternalOffset _defaultInternalOffset = new() { HorizontalRatio = 0.8f, VerticalMeters = 0.2f };

    private float _defaultVerticalInternalBottomOffset = 0.05f;
    private float _defaultVerticalExternalBottomOffset = 0.03f;

    private float _visualVerticalInternalBottomOffset = 0.01f;
    private float _visualVerticalExternalBottomOffset = 0.005f;

    public void Initialize(MRUKAnchor.SceneLabels labelID, string id, Rect boundsRect, LabelOffsetConfig offsetConfig, Material externalMaterial, Material internalMaterial)
    {
        this.labelID = labelID;
        this.id = id;
        this.boundsRect = boundsRect;
        this.offsetConfig = offsetConfig;
        this.externalMaterial = externalMaterial;
        this.internalMaterial = internalMaterial;

        SetupBounds();
    }

    private void SetupBounds()
    {
        var (externalOffset, internalOffset) = offsetConfig != null
            ? offsetConfig.GetOffsets(labelID)
            : (_defaultExternalOffset, _defaultInternalOffset);

        // Calculate horizontal offsets based on ratios
        Vector3 externalExtents = new Vector3(
            boundsRect.width * externalOffset.HorizontalRatio,
            externalOffset.VerticalMeters,
            boundsRect.height * externalOffset.HorizontalRatio
        );

        Vector3 internalExtents = new Vector3(
            Mathf.Max(0, boundsRect.width * internalOffset.HorizontalRatio),
            Mathf.Max(0, internalOffset.VerticalMeters),
            Mathf.Max(0, boundsRect.height * internalOffset.HorizontalRatio)
        );

        // Calculate center positions to make bounds extend mostly upward
        // The center should be positioned so that the bottom is at -_defaultVerticalBottomOffset
        // and the top extends upward by the full vertical meters
        Vector3 externalCenter = new Vector3(0f, (externalExtents.y / 2f) - _defaultVerticalExternalBottomOffset, 0f);
        Vector3 internalCenter = new Vector3(0f, (internalExtents.y / 2f) - _defaultVerticalInternalBottomOffset, 0f);

        // Create bounds with adjusted centers
        externalBounds = new Bounds(externalCenter, externalExtents);
        internalBounds = new Bounds(internalCenter, internalExtents);
    }

    public bool IsPointInZone(Vector3 worldPoint)
    {
        // Convert point into local space of the face
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        if (
            labelID == MRUKAnchor.SceneLabels.FLOOR ||
            internalBounds.extents.x == 0f || internalBounds.extents.y == 0f || internalBounds.extents.z == 0f
        ) // if internal bounds is plane/line/dot then I just want to check external bounds
        {
            return externalBounds.Contains(localPoint);
        }
        return externalBounds.Contains(localPoint) && !internalBounds.Contains(localPoint);
    }


    #region DEBUG
    private void OnDrawGizmos()
    {
        SetupBounds();

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, externalBounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, internalBounds.size);

        Gizmos.matrix = oldMatrix;
    }

    private GameObject CreateBoundingCube(Vector3 scaleExtents, Vector3 center, Material cubeMaterial)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(transform, false);

        cube.transform.localPosition = center;
        cube.transform.localScale = scaleExtents;

        cube.transform.localRotation = Quaternion.identity;
        cube.GetComponent<Renderer>().material = cubeMaterial;
        Destroy(cube.GetComponent<Collider>());
        return cube;
    }

    // Calculate center positions to make bounds extend mostly upward
    // The center should be positioned so that the bottom is at -_defaultVerticalBottomOffset
    // and the top extends upward by the full vertical meters
    private void CreateDebugExternalCube()
    {
        externalCube = CreateBoundingCube(externalBounds.size, externalBounds.center, externalMaterial);
    }

    private void CreateDebugInternalCube()
    {
        internalCube = CreateBoundingCube(internalBounds.size, internalBounds.center, internalMaterial);
    }

    // visual cube is a cube that is the same size as the external cube but with the y-axis scaled to the default vertical bottom offset
    // for prettier visuals
    private void CreateVisualExternalCube()
    {
        Vector3 flatY_externalExtents = externalBounds.size;
        flatY_externalExtents.y = _visualVerticalExternalBottomOffset * 2f;
        externalCube = CreateBoundingCube(flatY_externalExtents, Vector3.zero, externalMaterial);
    }

    private void CreateVisualInternalCube()
    {
        Vector3 flatY_internalExtents = internalBounds.size;
        flatY_internalExtents.y = _visualVerticalInternalBottomOffset * 2f;
        internalCube = CreateBoundingCube(flatY_internalExtents, Vector3.zero, internalMaterial);
    }


    // Debug cubes
    public void ShowOnlyDebugInternalCube()
    {
        HideCubes();
        CreateDebugInternalCube();
    }

    public void ShowOnlyDebugExternalCube()
    {
        HideCubes();
        CreateDebugExternalCube();
    }

    public void ShowBothDebugCubes()
    {
        HideCubes(); // Ensure clean state

        CreateDebugExternalCube();
        CreateDebugInternalCube();
    }

    // Visual cubes
    public void ShowOnlyVisualInternalCube()
    {
        HideCubes();
        CreateVisualInternalCube();
    }

    public void ShowOnlyVisualExternalCube()
    {
        HideCubes();
        CreateVisualExternalCube();
    }

    public void ShowBothVisualCubes()
    {
        HideCubes(); // Ensure clean state

        CreateVisualExternalCube();
        CreateVisualInternalCube();
    }

    // generic
    public void HideCubes()
    {
        HideExternalCube();
        HideInternalCube();
    }

    private void HideInternalCube()
    {
        if (internalCube != null) Destroy(internalCube);
    }

    private void HideExternalCube()
    {
        if (externalCube != null) Destroy(externalCube);
    }


    #endregion
}
