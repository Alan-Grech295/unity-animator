using UnityEditor;
using UnityEngine;

public struct LineSegmentData : SDFData
{
    public Vector3 Start;
    public Vector3 End;
    public float Thickness;
    public int PerspectiveThickness;
    public int MaterialIndex;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.LINE_SEGMENT;
}

[ExecuteInEditMode]
public class LineSegment : MonoBehaviour
{

#if UNITY_EDITOR
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
#endif

    public SDFMaterial material;

    [SerializeField]
    private bool perspectiveThickness;

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
            if (startTransform == null)
                GetTransforms();
            startTransform.position = value;
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
            if (endTransform == null)
                GetTransforms();
            endTransform.position = value;
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

        GetTransforms();
        start = startTransform.position;
        end = endTransform.position;
    }

    private void GetTransforms()
    {
        startTransform = transform.GetChild(0);
        endTransform = transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (pastPos != transform.position)
        {
            dirty = true;

            start = startTransform.position;
            end = endTransform.position;
        }

        if (!Application.isPlaying)
        {
            dirty = true;

            if (sdfRef == null)
            {
                sdfRef = SDFObjectManager.Add(GetSegmentData(), material);
            }

            if (startTransform == null)
            {
                startTransform = transform.GetChild(0);
                startTransform.position = start;
            }

            start = startTransform.position;

            if (endTransform == null)
            {
                endTransform = transform.GetChild(1);
                endTransform.position = end;
            }

            end = endTransform.position;
        }

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
            Start = start,
            End = end,
            PerspectiveThickness = perspectiveThickness ? 1 : 0,
            Thickness = Mathf.Abs(thickness),
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
        dirty = false;
    }
}
