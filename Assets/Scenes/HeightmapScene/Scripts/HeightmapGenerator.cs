using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeightmapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct ChunkGroup
    {
        public Vector2Int position;
        public Vector2Int size;
    }
    public int size;
    public int chunkSize;
    public int textureDownsample = 1;
    public int dissolveSpeed = 5;

    public ChunkGroup[] visibleChunks;

    [Header("Noise options")]
    public float scale;
    public float height;
    public int octaves;
    public float gain;
    public float lacunarity;
    public Vector2 offset;

    public bool generateMesh = false;
    public bool hideChunks = false;

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
            generateMesh = false;
        }

        if (hideChunks)
        {
            hideChunks = false;
            HideChunks();
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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        CreateTexture();

        renderer.material.SetFloat("_ShowRaw", 0);

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }
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

    public void HideChunks()
    {
        StartCoroutine(HideChunksEnum());
    }

    IEnumerator HideChunksEnum()
    {
        Vector2Int[] chunks = ToChunks(visibleChunks);

        List<int> visibleTris = new List<int>(chunks.Length * chunkSize * chunkSize * 6);
        foreach (Vector2Int chunk in chunks)
        {
            if (!InBounds(chunk)) continue;

            AddChunkTris(ref visibleTris, chunk);
        }

        int[] visibleTrisArray = visibleTris.ToArray();

        int[] currentTris = mesh.triangles;

        List<(int, int, int, int, int, int)> quads = new List<(int, int, int, int, int, int)>(currentTris.Length / 6);
        HashSet<int> trisSet = new HashSet<int>(visibleTrisArray);

        for (int i = 0; i < currentTris.Length; i += 6)
        {
            if (trisSet.Contains(currentTris[i])) continue;

            quads.Add((currentTris[i], currentTris[i + 1], currentTris[i + 2],
                       currentTris[i + 3], currentTris[i + 4], currentTris[i + 5]));
        }

        quads.Shuffle();

        while (quads.Count > 0)
        {
            quads.RemoveRange(0, Mathf.Min(dissolveSpeed, quads.Count));

            mesh.triangles = FlattenAndCombine(quads, visibleTrisArray);

            yield return null;
        }
    }

    private int[] FlattenAndCombine(List<(int, int, int, int, int, int)> quads, int[] tris)
    {
        int[] newTris = new int[quads.Count * 6 + tris.Length];

        for (int i = 0, start = 0; i < quads.Count; i++, start += 6)
        {
            var (t0, t1, t2, t3, t4, t5) = quads[i];

            newTris[start] = t0;
            newTris[start + 1] = t1;
            newTris[start + 2] = t2;
            newTris[start + 3] = t3;
            newTris[start + 4] = t4;
            newTris[start + 5] = t5;
        }

        Array.Copy(tris, 0, newTris, quads.Count * 6, tris.Length);

        return newTris;
    }

    private void AddChunkTris(ref List<int> tris, Vector2Int chunk)
    {
        int start = chunk.x * chunkSize + chunk.y * chunkSize * (size + 1);
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int tri = start + x + y * (size + 1);
                tris.Add(tri);
                tris.Add(tri + size + 1);
                tris.Add(tri + 1);

                tris.Add(tri + 1);
                tris.Add(tri + size + 1);
                tris.Add(tri + size + 2);
            }
        }
    }

    private bool InBounds(Vector2Int chunk)
    {
        return chunk.x * chunkSize < size && chunk.y * chunkSize < size;
    }

    private Vector2Int[] ToChunks(ChunkGroup[] chunkGroups)
    {
        List<Vector2Int> chunks = new List<Vector2Int>(chunkGroups.Length * 2);

        foreach (ChunkGroup chunkGroup in chunkGroups)
        {
            for (int y = chunkGroup.position.y; y < chunkGroup.position.y + chunkGroup.size.y; y++)
            {
                for (int x = chunkGroup.position.x; x < chunkGroup.position.x + chunkGroup.size.x; x++)
                {
                    chunks.Add(new Vector2Int(x, y));
                }
            }
        }

        return chunks.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        Vector2Int[] chunks = ToChunks(visibleChunks);

        foreach (Vector2Int chunk in chunks)
        {
            Vector3 newPos = new Vector3(pos.x + chunk.x * chunkSize * scale.x, pos.y, pos.z + chunk.y * chunkSize * scale.z);
            Gizmos.DrawWireCube(newPos + chunkSize / 2f * scale, Vector3.Scale(new Vector3(chunkSize, 1, chunkSize), scale));
        }
    }
}

public static class Extensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
