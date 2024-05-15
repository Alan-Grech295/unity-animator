using System;
using System.Collections.Generic;
using UnityEngine;

public interface SDFData
{
    public SDFObjectManager.SDFType Type { get; }
}


public class SDFObjectManager
{
    public enum SDFType { SPHERE, BOX }
    private class SDFList<T> where T : struct
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
    private SDFList<BoxData> boxSDFs = new SDFList<BoxData>(new BoxData() { transformationInverse = Matrix4x4.zero });
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

    // Start is called before the first frame update
    public SDFObjectManager()
    {

    }

    public static SDFRef Add<T>(T data) where T : SDFData
    {
        switch (data.Type)
        {
            case SDFType.SPHERE:
                return Instance.sphereSDFs.Add((SphereData)Convert.ChangeType(data, typeof(SphereData)));
            case SDFType.BOX:
                return Instance.boxSDFs.Add((BoxData)Convert.ChangeType(data, typeof(BoxData)));
        }

        return null;
    }

    public static void Update<T>(T data, SDFRef sdfRef) where T : SDFData
    {
        switch (data.Type)
        {
            case SDFType.SPHERE:
                Instance.sphereSDFs[sdfRef.Index] = (SphereData)Convert.ChangeType(data, typeof(SphereData));
                break;
            case SDFType.BOX:
                Instance.boxSDFs[sdfRef.Index] = (BoxData)Convert.ChangeType(data, typeof(BoxData));
                break;
        }
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
