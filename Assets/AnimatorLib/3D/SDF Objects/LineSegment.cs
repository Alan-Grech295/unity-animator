using UnityEditor;
using UnityEngine;

public struct LineSegmentData : SDFData
{
    public Vector3 Start;
    public Vector3 End;
    public float Thickness;
    public int MaterialIndex;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.LINE_SEGMENT;
}

[ExecuteInEditMode]
public class LineSegment : MonoBehaviour
{
    [MenuItem("GameObject/SDF/Line Segment")]
    static void CreateBox(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("SDF Line Segment");
        LineSegment segment = go.AddComponent<LineSegment>();

        GameObject start = new GameObject("Start");
        GameObject end = new GameObject("End");

        start.transform.SetParent(go.transform);
        end.transform.SetParent(go.transform);

        segment.startTransform = start.transform;
        segment.endTransform = end.transform;

        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    public SDFMaterial material;
    public float Thickness
    {
        get
        {
            return thickness;
        }

        set
        {
            thickness = value;
            dirty = true;
        }
    }

    public Vector3 Start
    {
        get
        {
            return start;
        }

        set
        {
            start = value;
            dirty = true;
        }
    }

    public Vector3 End
    {
        get
        {
            return end;
        }

        set
        {
            end = value;
            dirty = true;
        }
    }

    [SerializeField]
    private float thickness = 1;
    private Vector3 start;
    private Vector3 end;

    SDFRef sdfRef;

    private Vector3 pastPos;
    private bool dirty = false;

    private Transform startTransform;
    private Transform endTransform;

    // Start is called before the first frame update
    void OnEnable()
    {
        sdfRef = SDFObjectManager.Add(GetSegmentData(), material);

        pastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (pastPos != transform.position)
            dirty = true;

#if UNITY_EDITOR
        dirty = true;

        if (sdfRef == null)
        {
            sdfRef = SDFObjectManager.Add(GetSegmentData(), material);
        }
#endif

        if (dirty)
        {
            SDFObjectManager.Update(GetSegmentData(), sdfRef);
            SDFObjectManager.UpdateMaterial(material, SDFObjectManager.SDFType.LINE_SEGMENT, sdfRef);
            Clean();
        }
    }

    private void OnDisable()
    {
        SDFObjectManager.Destroy(SDFObjectManager.SDFType.LINE_SEGMENT, sdfRef);
    }

    private LineSegmentData GetSegmentData()
    {
        return new LineSegmentData()
        {
            Start = startTransform != null ? startTransform.position : Vector3.zero,
            End = endTransform != null ? endTransform.position : Vector3.zero,
            Thickness = Mathf.Abs(thickness),
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
