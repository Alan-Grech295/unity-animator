using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface SDFData
{
    public SDFObjectManager.SDFType Type { get; }
}


public class SDFObjectManager
{
    public enum SDFType { SPHERE, BOX, LINE_SEGMENT }
    public class SDFList<T> where T : struct
    {
        private List<T> sdfs;
        private List<SDFRef> refs;
        private ComputeBuffer buffer;

        private T[] blankSDF;

        public ComputeBuffer Buffer
        {
            get
            {
                return buffer;
            }
        }

        public SDFList(T blankSDF)
        {
            sdfs = new List<T>();
            refs = new List<SDFRef>();

            this.blankSDF = new T[] { blankSDF };

            ComputeHelper.CreateStructuredBuffer(ref buffer, this.blankSDF);
        }

        ~SDFList()
        {
            if (buffer != null)
            {
                buffer.Dispose();
            }
        }

        public T this[int index]
        {
            get
            {
                return sdfs[index];
            }

            set
            {
                sdfs[index] = value;
                UpdateBuffer();
            }
        }

        public int Count
        {
            get
            {
                return sdfs.Count;
            }
        }

        private void UpdateBuffer()
        {
            buffer.SetData(sdfs.ToArray());
        }

        public SDFRef Add(T value)
        {
            sdfs.Add(value);
            SDFRef sdfRef = new SDFRef(sdfs.Count - 1);
            refs.Add(sdfRef);

            ComputeHelper.CreateStructuredBuffer<T>(ref buffer, sdfs.ToArray());

            return sdfRef;
        }

        public void Remove(T value)
        {
            int index = sdfs.IndexOf(value);
            Remove(index);
        }

        public void Remove(int index)
        {
            sdfs.RemoveAt(index);
            refs.RemoveAt(index);

            for (int i = index; i < refs.Count; i++)
            {
                refs[i].Index--;
            }

            ComputeHelper.CreateStructuredBuffer<T>(ref buffer, sdfs.Count > 0 ? sdfs.ToArray() : blankSDF);
        }
    }
    public static SDFObjectManager Instance
    {
        get
        {
            if (instance == null)
                instance = new SDFObjectManager();

            return instance;
        }
    }

    private static SDFObjectManager instance = null;

    private SDFList<SphereData> sphereSDFs = new SDFList<SphereData>(new SphereData() { position = Vector3.zero, radius = 0 });
    private SDFList<BoxData> boxSDFs = new SDFList<BoxData>(new BoxData() { scale = Vector3.zero });
    private SDFList<LineSegmentData> lineSegmentSDFs = new SDFList<LineSegmentData>(new LineSegmentData());

    private SDFList<SDFLightData> lights = new SDFList<SDFLightData>(new SDFLightData());

    private Dictionary<SDFMaterial, int> materialToIndex = new Dictionary<SDFMaterial, int>()
    {
        { defaultMaterial, 0 },
    };

    private List<SDFMaterial> sDFMaterials = new List<SDFMaterial>()
    {
        defaultMaterial
    };

    private static SDFMaterial defaultMaterial = new SDFMaterial()
    {
        Albedo = new Color(1, 0, 1, 1),
        Smoothness = 0.0f,
        SpecularPower = 1.0f,
        Ambient = new Color(),
        AmbientStrength = 0.0f,
        Opacity = 1.0f,
        Lit = false,
    };

    private ComputeBuffer materialBuffer;

    public static ComputeBuffer SphereBuffer
    {
        get
        {
            return Instance.sphereSDFs.Buffer;
        }
    }

    public static ComputeBuffer BoxBuffer
    {
        get
        {
            return Instance.boxSDFs.Buffer;
        }
    }

    public static ComputeBuffer SegmentBuffer
    {
        get
        {
            return Instance.lineSegmentSDFs.Buffer;
        }
    }

    public static ComputeBuffer LightBuffer
    {
        get
        {
            return Instance.lights.Buffer;
        }
    }

    public static SDFList<SphereData> Spheres
    {
        get
        {
            return Instance.sphereSDFs;
        }
    }

    public static SDFList<BoxData> Boxes
    {
        get
        {
            return Instance.boxSDFs;
        }
    }

    public static SDFList<LineSegmentData> Segments
    {
        get
        {
            return Instance.lineSegmentSDFs;
        }
    }

    public static SDFList<SDFLightData> Lights
    {
        get
        {
            return Instance.lights;
        }
    }

    public static ComputeBuffer MaterialBuffer
    {
        get
        {
            Instance.materialBuffer.SetData(Instance.sDFMaterials.Select(m => m.ToMaterialData()).ToArray());
            return Instance.materialBuffer;
        }
    }

    // Start is called before the first frame update
    public SDFObjectManager()
    {
        ComputeHelper.CreateStructuredBuffer<SDFMaterialData>(ref materialBuffer, 1);
    }

    public static SDFRef Add<T>(T data, SDFMaterial material) where T : struct, SDFData
    {
        if (material == null)
            material = defaultMaterial;

        switch (data.Type)
        {
            case SDFType.SPHERE:
                {
                    SphereData sphereData = (SphereData)Convert.ChangeType(data, typeof(SphereData));
                    sphereData.MaterialIndex = GetMaterialIndex(material);
                    return Instance.sphereSDFs.Add(sphereData);
                }
            case SDFType.BOX:
                {
                    BoxData boxData = (BoxData)Convert.ChangeType(data, typeof(BoxData));
                    boxData.MaterialIndex = GetMaterialIndex(material);
                    return Instance.boxSDFs.Add(boxData);
                }
            case SDFType.LINE_SEGMENT:
                {
                    LineSegmentData segmentData = (LineSegmentData)Convert.ChangeType(data, typeof(LineSegmentData));
                    segmentData.MaterialIndex = GetMaterialIndex(material);
                    return Instance.lineSegmentSDFs.Add(segmentData);
                }
        }

        return null;
    }

    private static int GetMaterialIndex(SDFMaterial material)
    {
        if (!Instance.materialToIndex.ContainsKey(material))
        {
            Instance.materialToIndex[material] = Instance.sDFMaterials.Count;
            Instance.sDFMaterials.Add(material);
            ComputeHelper.CreateStructuredBuffer<SDFMaterialData>(ref Instance.materialBuffer, Instance.sDFMaterials.Count);
        }
        return Instance.materialToIndex[material];
    }

    public static void Update<T>(T data, SDFRef sdfRef) where T : struct, SDFData
    {
        switch (data.Type)
        {
            case SDFType.SPHERE:
                {
                    SphereData sphereData = (SphereData)Convert.ChangeType(data, typeof(SphereData));
                    sphereData.MaterialIndex = Instance.sphereSDFs[sdfRef.Index].MaterialIndex;
                    Instance.sphereSDFs[sdfRef.Index] = sphereData;
                    break;
                }
            case SDFType.BOX:
                {
                    BoxData boxData = (BoxData)Convert.ChangeType(data, typeof(BoxData));
                    boxData.MaterialIndex = Instance.boxSDFs[sdfRef.Index].MaterialIndex;
                    Instance.boxSDFs[sdfRef.Index] = boxData;
                    break;
                }
            case SDFType.LINE_SEGMENT:
                {
                    LineSegmentData segmentData = (LineSegmentData)Convert.ChangeType(data, typeof(LineSegmentData));
                    segmentData.MaterialIndex = Instance.lineSegmentSDFs[sdfRef.Index].MaterialIndex;
                    Instance.lineSegmentSDFs[sdfRef.Index] = segmentData;
                    break;
                }
        }
    }

    public static void UpdateMaterial(SDFMaterial material, SDFType type, SDFRef sdfRef)
    {
        if (material == null)
            material = defaultMaterial;

        int materialIndex = GetMaterialIndex(material);

        switch (type)
        {
            case SDFType.SPHERE:
                {
                    SphereData sphereData = Instance.sphereSDFs[sdfRef.Index];
                    sphereData.MaterialIndex = materialIndex;
                    Instance.sphereSDFs[sdfRef.Index] = sphereData;
                    break;
                }
            case SDFType.BOX:
                {
                    BoxData boxData = Instance.boxSDFs[sdfRef.Index];
                    boxData.MaterialIndex = materialIndex;
                    Instance.boxSDFs[sdfRef.Index] = boxData;
                    break;
                }
            case SDFType.LINE_SEGMENT:
                {
                    LineSegmentData segmentData = Instance.lineSegmentSDFs[sdfRef.Index];
                    segmentData.MaterialIndex = materialIndex;
                    Instance.lineSegmentSDFs[sdfRef.Index] = segmentData;
                    break;
                }
        }
    }

    public static SDFRef AddLight(SDFLightData data)
    {
        return Instance.lights.Add(data);
    }

    public static void UpdateLight(SDFLightData data, SDFRef sdfRef)
    {
        Instance.lights[sdfRef.Index] = data;
    }

    public static void DestroyLight(SDFRef sdfRef)
    {
        Instance.lights.Remove(sdfRef.Index);
    }

    public static void Destroy(SDFType type, SDFRef sdfRef)
    {
        switch (type)
        {
            case SDFType.SPHERE:
                Instance.sphereSDFs.Remove(sdfRef.Index);
                break;
            case SDFType.BOX:
                Instance.boxSDFs.Remove(sdfRef.Index);
                break;
            case SDFType.LINE_SEGMENT:
                Instance.lineSegmentSDFs.Remove(sdfRef.Index);
                break;
        }
    }
}

public class SDFRef
{
    public int Index { get; set; }

    public SDFRef(int index)
    {
        Index = index;
    }
}
