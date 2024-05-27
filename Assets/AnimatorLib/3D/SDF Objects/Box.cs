using UnityEditor;
using UnityEngine;

public struct BoxData : SDFData
{
    public Vector3 position;
    public Vector3 scale;
    public Matrix4x4 transformationInverse;
    public int MaterialIndex;
    public Vector3 boundsMin;
    public Vector3 boundsMax;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.BOX;
}

[ExecuteInEditMode]
public class Box : MonoBehaviour
{
    public SDFMaterial material;
    SDFRef sdfRef;

    private Vector3 pastPos;
    private bool dirty = false;

#if UNITY_EDITOR
    [MenuItem("GameObject/SDF/Box")]
    static void CreateBox(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("SDF Box");
        go.AddComponent<Box>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
#endif

    // Start is called before the first frame update
    void OnEnable()
    {
        sdfRef = SDFObjectManager.Add(GetBoxData(), material);

        pastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (pastPos != transform.position)
            dirty = true;

        if (!Application.isPlaying)
        {
            dirty = true;

            if (sdfRef == null)
            {
                sdfRef = SDFObjectManager.Add(GetBoxData(), material);
            }
        }

        if (dirty)
        {
            SDFObjectManager.Update(GetBoxData(), sdfRef);
            SDFObjectManager.UpdateMaterial(material, SDFObjectManager.SDFType.BOX, sdfRef);
            Clean();
        }
    }

    private void OnDisable()
    {
        SDFObjectManager.Destroy(SDFObjectManager.SDFType.BOX, sdfRef);
    }

    private BoxData GetBoxData()
    {
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = Vector3.zero;
            corner.x = (i & 1) == 0 ? -0.5f : 0.5f;
            corner.y = (i & 2) == 0 ? -0.5f : 0.5f;
            corner.z = (i & 4) == 0 ? -0.5f : 0.5f;

            corner = transform.TransformPoint(corner);
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        return new BoxData()
        {
            position = transform.position,
            scale = transform.localScale,
            transformationInverse = (Matrix4x4.Rotate(transform.rotation) * Matrix4x4.Translate(transform.position)).inverse,
            boundsMin = min,
            boundsMax = max,
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
        dirty = false;
    }
}
