using UnityEngine;

public struct BoxData : SDFData
{
    public Matrix4x4 transformationInverse;

    public SDFObjectManager.SDFType Type => SDFObjectManager.SDFType.BOX;
}

[ExecuteInEditMode]
public class Box : MonoBehaviour
{
    SDFRef sdfRef;

    private Vector3 pastPos;
    private bool dirty = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        sdfRef = SDFObjectManager.Add(GetBoxData());

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
            sdfRef = SDFObjectManager.Add(GetBoxData());
        }
#endif

        if (dirty)
        {
            SDFObjectManager.Update(GetBoxData(), sdfRef);
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
            transformationInverse = transform.localToWorldMatrix.inverse,
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
