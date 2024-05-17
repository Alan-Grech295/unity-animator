using UnityEngine;

public struct BoxData : SDFData
{
    public Vector3 position;
    public Vector3 scale;
    public Matrix4x4 rotationInverse;
    public int MaterialIndex;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.BOX;
}

[ExecuteInEditMode]
public class Box : MonoBehaviour
{
    public SDFMaterial material;
    SDFRef sdfRef;

    private Vector3 pastPos;
    private bool dirty = false;

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

#if UNITY_EDITOR
        dirty = true;

        if (sdfRef == null)
        {
            sdfRef = SDFObjectManager.Add(GetBoxData(), material);
        }
#endif

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
        return new BoxData()
        {
            position = transform.position,
            scale = transform.localScale,
            rotationInverse = Matrix4x4.Rotate(transform.rotation).inverse,
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
