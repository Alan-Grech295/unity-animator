using UnityEngine;

public class Test : MonoBehaviour
{
    public RenderTexture texture;
    public GameObject cube;
    public int dataPoints = 255;
    Camera camera;
    float step = 0;

    float curDist;
    string output = "";
    int i = 0;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        step = (camera.farClipPlane - camera.nearClipPlane) / (float)dataPoints;
        output = "Distance,Depth\n";

        cube.transform.position = Vector3.forward * (camera.nearClipPlane + cube.transform.localScale.z / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (i >= dataPoints)
        {
            Debug.Log(output);
            return;
        }
        RenderTexture.active = texture;

        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

        texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture2D.Apply();

        Debug.Log($"{(curDist) / (camera.farClipPlane - camera.nearClipPlane)},{cube.transform.position.z},{texture2D.GetPixel(texture.width / 2, texture.height / 2).r}");

        output += $"{(curDist) / (camera.farClipPlane - camera.nearClipPlane)},{texture2D.GetPixel(texture.width / 2, texture.height / 2).r}\n";

        RenderTexture.active = null;

        curDist += step;
        cube.transform.position = Vector3.forward * (camera.nearClipPlane + cube.transform.localScale.z / 2f + curDist);
        i++;
    }
}
