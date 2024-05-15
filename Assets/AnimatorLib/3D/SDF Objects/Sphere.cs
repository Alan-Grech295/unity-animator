using UnityEngine;

public struct SphereData : SDFData
{
    public Vector3 position;
    public float radius;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.SPHERE;
}

[ExecuteInEditMode]
public class Sphere : MonoBehaviour
{
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
        sdfRef = SDFObjectManager.Add(new SphereData()
        {
            position = transform.position,
            radius = radius
        });

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
            sdfRef = SDFObjectManager.Add(new SphereData()
            {
                position = transform.position,
                radius = radius
            });
        }
#endif

        if (dirty)
        {
            SDFObjectManager.Update(new SphereData()
            {
                position = transform.position,
                radius = radius
            }, sdfRef);
            Clean();
        }
    }

    private void OnDisable()
    {
        SDFObjectManager.Destroy(SDFObjectManager.SDFType.SPHERE, sdfRef);
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
