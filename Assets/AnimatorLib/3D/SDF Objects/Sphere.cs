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
    private float radius;

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
