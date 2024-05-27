using UnityEngine;

public class CameraLineSetter : MonoBehaviour
{
    public LineSegment[] lineSegments;
    public Box nearPlane;
    public float planeThickness = 0.1f;
    public float distance = 1;

    // Start is called before the first frame update
    void Start()
    {
        SetLines();

        SetLineVisibility(false);
    }

    public void SetLines()
    {
        Camera camera = GetComponent<Camera>();
        foreach (LineSegment segment in lineSegments)
        {
            segment.Start = transform.position - transform.forward * distance;
        }

        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        for (int i = 0; i < 4; i++)
        {
            var worldSpaceCorner = camera.transform.TransformVector(frustumCorners[i]);
            lineSegments[i].End = worldSpaceCorner + ((worldSpaceCorner - transform.position).normalized * distance);
        }

        Vector2 nearSize = GetFrustumSize(camera, camera.nearClipPlane);
        Vector2 farSize = GetFrustumSize(camera, camera.farClipPlane);

        nearPlane.transform.localScale = new Vector3(nearSize.x, nearSize.y, planeThickness);
        nearPlane.transform.localPosition = Vector3.forward * camera.nearClipPlane;
    }

    Vector2 GetFrustumSize(Camera camera, float distance)
    {
        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), distance, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        return new Vector2(frustumCorners[2].x - frustumCorners[0].x, frustumCorners[1].y - frustumCorners[0].y);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLineVisibility(bool visible)
    {
        foreach (LineSegment segment in lineSegments)
        {
            segment.gameObject.SetActive(visible);
        }

        nearPlane.gameObject.SetActive(visible);
    }
}
