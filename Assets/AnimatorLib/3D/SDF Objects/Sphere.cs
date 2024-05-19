using UnityEditor;
using UnityEngine;

public struct SphereData : SDFData
{
    public Vector3 position;
    public float radius;
    public int MaterialIndex;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.SPHERE;
}

[ExecuteInEditMode]
public class Sphere : MonoBehaviour
{
    [MenuItem("GameObject/SDF/Sphere")]
    static void CreateBox(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("SDF Sphere");
        go.AddComponent<Sphere>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    public SDFMaterial material;
    public float Radius
    {
        get
        {
            return radius;
        }

        set
        {
            radius = value;
            dirty = true;
        }
    }

    [SerializeField]
    private float radius = 1;

    SDFRef sdfRef;

    private Vector3 pastPos;
    private bool dirty = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        sdfRef = SDFObjectManager.Add(GetSphereData(), material);

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
            sdfRef = SDFObjectManager.Add(GetSphereData(), material);
        }
#endif

        if (dirty)
        {
            SDFObjectManager.Update(GetSphereData(), sdfRef);
            SDFObjectManager.UpdateMaterial(material, SDFObjectManager.SDFType.SPHERE, sdfRef);
            Clean();
        }
    }

    private void OnDisable()
    {
        SDFObjectManager.Destroy(SDFObjectManager.SDFType.SPHERE, sdfRef);
    }

    private SphereData GetSphereData()
    {
        return new SphereData()
        {
            position = transform.position,
            radius = radius
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
