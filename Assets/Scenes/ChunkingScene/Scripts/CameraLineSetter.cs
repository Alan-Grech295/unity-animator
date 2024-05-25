using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLineSetter : MonoBehaviour
{
    public LineSegment[] lineSegments;
    public Box[] planes;
    public float planeThickness = 0.1f;
    public float distance = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void SetLines()
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

        planes[0].transform.localScale = new Vector3(nearSize.x, nearSize.y, planeThickness);
        planes[0].transform.localPosition = Vector3.forward * camera.nearClipPlane;
        planes[1].transform.localScale = new Vector3(farSize.x, farSize.y, planeThickness);
        planes[1].transform.localPosition = Vector3.forward * camera.farClipPlane;
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
        SetLines();
    }
}
