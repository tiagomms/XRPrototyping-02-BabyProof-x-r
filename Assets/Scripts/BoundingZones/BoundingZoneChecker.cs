using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BoundingZoneChecker : MonoBehaviour
{
    [Header("Identifiers")]
    [SerializeField] public MRUKAnchor.SceneLabels labelID { get; private set; }
    [SerializeField] public string id { get; private set; }

    [Header("Plane Setup")]
    [SerializeField] public Rect boundsRect { get; private set; } // In local XZ space: position = local center, size = width/height
    [SerializeField] public Matrix4x4 planeMatrix { get; private set; } // World-space center + rotation

    [Header("Config")]
    [SerializeField] public LabelOffsetConfig offsetConfig { get; private set; }

    [Header("Debug")]
    [SerializeField] public Material externalMaterial { get; private set; }
    [SerializeField] public Material internalMaterial { get; private set; }

    private Bounds externalBounds;
    private Bounds internalBounds;
    private Matrix4x4 inverseMatrix;

    private GameObject externalCube;
    private GameObject internalCube;

    private LabelOffsetConfig.OffsetSet _defaultOffsetConfig = new() { Horizontal = 0.2f, Vertical = 0.2f };

    public void Initialize(MRUKAnchor.SceneLabels labelID, string id, Rect boundsRect, Matrix4x4 faceTransform, LabelOffsetConfig offsetConfig, Material externalMaterial, Material internalMaterial)
    {
        this.labelID = labelID;
        this.id = id;
        this.boundsRect = boundsRect;
        this.planeMatrix = faceTransform;
        this.offsetConfig = offsetConfig;
        this.externalMaterial = externalMaterial;
        this.internalMaterial = internalMaterial;

        inverseMatrix = faceTransform.inverse;

        SetupBounds();
    }
    private void SetupBounds()
    {
        var (externalOffset, internalOffset) = offsetConfig != null
            ? offsetConfig.GetOffsets(labelID)
            : (_defaultOffsetConfig, _defaultOffsetConfig);


        Vector3 extents = new Vector3(
            boundsRect.width * 0.5f + externalOffset.Horizontal,
            externalOffset.Vertical,
            boundsRect.height * 0.5f + externalOffset.Horizontal
        );

        Vector3 internalExtents = new Vector3(
            Mathf.Max(0, boundsRect.width * 0.5f - internalOffset.Horizontal),
            Mathf.Max(0, internalOffset.Vertical),
            Mathf.Max(0, boundsRect.height * 0.5f - internalOffset.Horizontal)
        );

        // Start with local-aligned bounds
        externalBounds = new Bounds(Vector3.zero, extents * 2f);
        internalBounds = new Bounds(Vector3.zero, internalExtents * 2f);
    }

    public bool IsPointInZone(Vector3 worldPoint)
    {
        // Convert point into local space of the face
        Vector3 localPoint = inverseMatrix.MultiplyPoint3x4(worldPoint);
        if (labelID == MRUKAnchor.SceneLabels.FLOOR)
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
        Gizmos.matrix = planeMatrix;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boundsRect.center, externalBounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boundsRect.center, internalBounds.size);

        Gizmos.matrix = oldMatrix;
    }

    public void ShowDebugCubes()
    {
        HideDebugCubes(); // Ensure clean state

        externalCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        externalCube.transform.SetParent(transform, false);
        externalCube.transform.localPosition = boundsRect.center;
        externalCube.transform.localRotation = Quaternion.identity;
        externalCube.transform.localScale = externalBounds.size;
        externalCube.GetComponent<Renderer>().material = externalMaterial;

        internalCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        internalCube.transform.SetParent(transform, false);
        internalCube.transform.localPosition = boundsRect.center;
        internalCube.transform.localRotation = Quaternion.identity;
        internalCube.transform.localScale = internalBounds.size * 1.01f; // just to be sure it is seen
        internalCube.GetComponent<Renderer>().material = internalMaterial;

        Destroy(externalCube.GetComponent<Collider>());
        Destroy(internalCube.GetComponent<Collider>());
    }

    public void HideDebugCubes()
    {
        if (externalCube != null) Destroy(externalCube);
        if (internalCube != null) Destroy(internalCube);
    }

    #endregion
}
