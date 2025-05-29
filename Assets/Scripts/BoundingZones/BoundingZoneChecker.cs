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
        Vector3 extents = new Vector3(
            boundsRect.width * externalOffset.HorizontalRatio,
            externalOffset.VerticalMeters,
            boundsRect.height * externalOffset.HorizontalRatio
        );

        Vector3 internalExtents = new Vector3(
            Mathf.Max(0, boundsRect.width * internalOffset.HorizontalRatio),
            Mathf.Max(0, internalOffset.VerticalMeters),
            Mathf.Max(0, boundsRect.height * internalOffset.HorizontalRatio)
        );

        // Start with local-aligned bounds
        externalBounds = new Bounds(Vector3.zero, extents);
        internalBounds = new Bounds(Vector3.zero, internalExtents);
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

    private void CreateExternalDebugCube()
    {

        externalCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        externalCube.transform.SetParent(transform, false);
        externalCube.transform.localPosition = Vector3.zero;
        externalCube.transform.localRotation = Quaternion.identity;
        externalCube.transform.localScale = externalBounds.size;
        externalCube.GetComponent<Renderer>().material = externalMaterial;
        Destroy(externalCube.GetComponent<Collider>());
    }

    private void CreateInternalDebugCube()
    {
        internalCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        internalCube.transform.SetParent(transform, false);
        internalCube.transform.localPosition = Vector3.zero;
        internalCube.transform.localRotation = Quaternion.identity;
        internalCube.transform.localScale = internalBounds.size;
        internalCube.GetComponent<Renderer>().material = internalMaterial;
        Destroy(internalCube.GetComponent<Collider>());
    }
    
    private void HideInternalCube()
    {
        if (internalCube != null) Destroy(internalCube);
    }

    private void HideExternalCube()
    {
        if (externalCube != null) Destroy(externalCube);
    }

    public void ShowOnlyInternalCube()
    {
        HideDebugCubes();
        CreateInternalDebugCube();
    }

    public void ShowOnlyExternalCube()
    {
        HideDebugCubes();
        CreateExternalDebugCube();
    }

    public void ShowBothDebugCubes()
    {
        HideDebugCubes(); // Ensure clean state

        CreateExternalDebugCube();
        CreateInternalDebugCube();

    }

    public void HideDebugCubes()
    {
        HideExternalCube();
        HideInternalCube();
    }


    #endregion
}
