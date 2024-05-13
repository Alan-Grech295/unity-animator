using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SDFObjectManager
{
    private class SDFList<T> where T : struct
    {
        private List<T> sdfs;
        private List<SDFRef> refs;
        private ComputeBuffer buffer;

        public ComputeBuffer Buffer
        {
            get
            {
                return buffer;
            }
        }

        public SDFList()
        {
            sdfs = new List<T>();
            refs = new List<SDFRef>();
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

            for(int i = index; i < refs.Count; i++)
            {
                refs[i].Index--;
            }

            ComputeHelper.CreateStructuredBuffer<T>(ref buffer, sdfs.ToArray());
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

    private SDFList<SphereData> sphereSDFs;
    public static ComputeBuffer SphereBuffer
    { 
        get
        {
            return Instance.sphereSDFs.Buffer;
        }
    }

    // Start is called before the first frame update
    public SDFObjectManager()
    {
        sphereSDFs = new SDFList<SphereData>();
    }

    public static SDFRef AddSphere(SphereData sphereData)
    {
        return Instance.sphereSDFs.Add(sphereData);
    }

    public static void UpdateSphere(SphereData sphereData, SDFRef sdfRef)
    {
        Instance.sphereSDFs[sdfRef.Index] = sphereData;
    }

    public static void RemoveSphere(SDFRef sdfRef)
    {
        Instance.sphereSDFs.Remove(sdfRef.Index);
    }

    public static void RemoveSphere(SphereData sphereData)
    {
        Instance.sphereSDFs.Remove(sphereData);
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
