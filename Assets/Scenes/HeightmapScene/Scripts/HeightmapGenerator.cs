using System.Collections;
using System.Linq;
using UnityEngine;

public class HeightmapGenerator : MonoBehaviour
{
    public int size;
    public int textureDownsample = 1;

    [Header("Noise options")]
    public float scale;
    public float height;
    public int octaves;
    public float gain;
    public float lacunarity;
    public Vector2 offset;

    public bool generateMesh = false;

    MeshFilter filter;
    MeshRenderer renderer;
    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generateMesh)
        {
            GenerateMesh();
        }
    }

    public void GenerateMesh()
    {
        var (verts, uvs, tris) = GeneratePlaneVertices(size);

        float[,] heightMap = GenerateHeightmap(size + 1, 1, scale, octaves, gain, lacunarity, offset);

        for (int y = 0, i = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++, i++)
            {
                verts[i].y = heightMap[x, y] * height;
            }
        }

        mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        CreateTexture();

        renderer.material.SetFloat("_ShowRaw", 0);
    }

    (Vector3[], Vector2[], int[]) GeneratePlaneVertices(int size)
    {
        var mesh = new Mesh();
        Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] tris = new int[size * size * 6];

        for (int y = 0, i = 0, j = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, y);
                uvs[i] = new Vector2((float)x / size, (float)y / size);

                if (x >= size || y >= size) continue;

                int vI = x + y * (size + 1);

                tris[j] = vI;
                tris[j + 1] = vI + size + 1;
                tris[j + 2] = vI + 1;

                tris[j + 3] = vI + 1;
                tris[j + 4] = vI + size + 1;
                tris[j + 5] = vI + size + 2;

                j += 6;
            }
        }

        return (vertices, uvs, tris);
    }

    public void CreatePlane()
    {
        StartCoroutine(CreatePlaneEnum());
    }

    IEnumerator CreatePlaneEnum()
    {
        var (verts, uvs, tris) = GeneratePlaneVertices(size);

        renderer.material.SetFloat("_ShowRaw", 1);
        renderer.material.SetTexture("_Texture", null);

        mesh = new Mesh();
        mesh.vertices = verts;
        mesh.uv = uvs;

        filter.sharedMesh = mesh;

        for (int i = 6; i < tris.Length; i += 6)
        {
            mesh.triangles = tris.Take(i).ToArray();
            mesh.RecalculateNormals();
            yield return new WaitForSeconds(0.001f);
        }

        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    public void CreateTexture()
    {
        float[,] heightMap = GenerateHeightmap(size + 1, 1, scale, octaves, gain, lacunarity, offset);

        int texSize = size / textureDownsample;

        Texture2D texture = new Texture2D(texSize, texSize);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float h = heightMap[Mathf.FloorToInt((float)x / texSize * (size + 1)),
                    Mathf.FloorToInt((float)y / texSize * (size + 1))];
                texture.SetPixel(x, y, new Color(h, h, h));
            }
        }

        texture.Apply();
        renderer.material.SetTexture("_Texture", texture);
    }

    public void ShowColorTexture()
    {
        AnimatorManager.AnimateValue((v) => renderer.material.SetFloat("_ShowRaw", v), 1, 0, 0.5f, 0,
                                     AnimatorManager.FLOAT_LERP, AnimatorManager.EASE_OUT_EXPO);

    }

    public void HeightmapMesh()
    {
        float[,] heightMap = GenerateHeightmap(size + 1, 1, scale, octaves, gain, lacunarity, offset);

        AnimatorManager.AnimateValue((h) =>
        {
            Vector3[] verts = mesh.vertices;
            for (int y = 0, i = 0; y < size + 1; y++)
            {
                for (int x = 0; x < size + 1; x++, i++)
                {
                    verts[i].y = heightMap[x, y] * h;
                }
            }

            mesh.vertices = verts;
            mesh.RecalculateNormals();
        },
        0, height, 1, 0, AnimatorManager.FLOAT_LERP,
        AnimatorManager.EASE_IN_OUT_CUBIC);
    }

    void CreateMesh(int size, float height, float scale, int octaves, float gain, float lacunarity)
    {
        var (verts, uvs, tris) = GeneratePlaneVertices(size);

        float[,] heightMap = GenerateHeightmap(size + 1, 1, scale, octaves, gain, lacunarity, offset);

        int texSize = size / textureDownsample;

        Texture2D texture = new Texture2D(texSize, texSize);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float h = heightMap[Mathf.FloorToInt((float)x / texSize * (size + 1)),
                    Mathf.FloorToInt((float)y / texSize * (size + 1))];
                texture.SetPixel(x, y, new Color(h, h, h));
            }
        }

        texture.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Texture", texture);

        mesh = new Mesh();

        AnimatorManager.AnimateValue((h) =>
        {
            for (int y = 0, i = 0; y < size + 1; y++)
            {
                for (int x = 0; x < size + 1; x++, i++)
                {
                    verts[i].y = heightMap[x, y] * h;
                }
            }

            mesh.vertices = verts;
            mesh.RecalculateNormals();
        },
        0, height, 1, 5, (start, end, t) =>
        {
            return Mathf.Lerp(start, end, t);
        },
        AnimatorManager.EASE_IN_OUT_CUBIC);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        filter.mesh = mesh;
    }

    float[,] GenerateHeightmap(int size, float height, float scale, int octaves, float gain, float lacunarity, Vector2 offset)
    {
        float[,] heightMap = new float[size, size];
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float frequency = 1;
                float amplitude = 1;
                for (int i = 0; i < octaves; i++)
                {
                    heightMap[x, y] += Mathf.PerlinNoise((x + offset.x) * scale * frequency, (y + offset.y) * scale * frequency) * amplitude;
                    frequency *= lacunarity;
                    amplitude *= gain;
                }

                min = Mathf.Min(min, heightMap[x, y]);
                max = Mathf.Max(max, heightMap[x, y]);
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                heightMap[x, y] = (heightMap[x, y] - min) / (max - min) * height;
            }
        }

        return heightMap;
    }
}
