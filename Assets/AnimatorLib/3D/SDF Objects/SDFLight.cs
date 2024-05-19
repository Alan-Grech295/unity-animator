using UnityEngine;

public struct SDFLightData
{
    public Vector3 Position;
    public Vector3 Direction;
    public Vector4 Color;
    public float Intensity;
    public int Type;
}

[ExecuteInEditMode]
public class SDFLight : MonoBehaviour
{
    public enum LightType { DIRECTIONAL = 0, POINT = 1 }

    public Color Color
    {
        get { return color; }
        set
        {
            color = value;
            dirty = true;
        }
    }
    public float Intensity
    {
        get { return intensity; }
        set
        {
            intensity = value;
            dirty = true;
        }
    }

    public LightType Type
    {
        get { return type; }
        set
        {
            type = value;
            dirty = true;
        }
    }

    [SerializeField]
    private Color color;
    [SerializeField]
    private float intensity = 1;
    [SerializeField]
    private LightType type;

    private Vector3 pastPos;
    private bool dirty = false;

    private SDFRef sdfRef;

    // Start is called before the first frame update
    void OnEnable()
    {
        sdfRef = SDFObjectManager.AddLight(GetLightData());

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
            sdfRef = SDFObjectManager.AddLight(GetLightData());
        }
#endif

        if (dirty)
        {
            SDFObjectManager.UpdateLight(GetLightData(), sdfRef);
            Clean();
        }
    }

    private void OnDisable()
    {
        SDFObjectManager.DestroyLight(sdfRef);
    }

    private SDFLightData GetLightData()
    {
        return new SDFLightData()
        {
            Position = transform.position,
            Direction = -transform.forward,
            Color = color,
            Intensity = intensity,
            Type = (int)type,
        };
    }

    private void Clean()
    {
        pastPos = transform.position;
    }
}
