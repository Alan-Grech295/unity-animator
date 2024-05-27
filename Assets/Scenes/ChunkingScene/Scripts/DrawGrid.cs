using System.Collections.Generic;
using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public LineSegment segmentPrefab;
    public Bounds bounds;
    public float increment = 5.0f;

    //public bool regenerate = true;

    private List<LineSegment> gridLineSegments = new List<LineSegment>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if (regenerate)
        //{
        //    regenerate = false;
        //    CreateGrid();
        //}
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        increment = Mathf.Max(1, increment);

        for (float x = bounds.min.x; x <= bounds.max.x; x += increment)
        {
            Gizmos.DrawLine(new Vector3(x, bounds.max.y, bounds.min.z), new Vector3(x, bounds.max.y, bounds.max.z));
        }

        for (float y = bounds.min.z; y <= bounds.max.z; y += increment)
        {
            Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.max.y, y), new Vector3(bounds.max.x, bounds.max.y, y));
        }
    }

    public void CreateGrid()
    {
        foreach (var seg in gridLineSegments)
        {
            Destroy(seg.gameObject);
        }

        gridLineSegments.Clear();

        float delay = 0;

        for (float x = bounds.min.x; x <= bounds.max.x; x += increment, delay += 0.1f)
        {
            GameObject go = Instantiate(segmentPrefab.gameObject);
            go.transform.SetParent(transform);

            LineSegment segment = go.GetComponent<LineSegment>();
            gridLineSegments.Add(segment);
            segment.Start = new Vector3(x, bounds.max.y, bounds.min.z);
            segment.End = new Vector3(x, bounds.max.y, bounds.min.z);
            AnimatorManager.AnimateValue(v => { segment.End = new Vector3(segment.End.x, bounds.max.y, v); },
                bounds.min.z, bounds.max.z, 0.1f, delay, AnimatorManager.FLOAT_LERP, AnimatorManager.EASE_IN_OUT_CUBIC);
        }

        for (float y = bounds.min.z; y <= bounds.max.z; y += increment, delay += 0.1f)
        {
            GameObject go = Instantiate(segmentPrefab.gameObject);
            go.transform.SetParent(transform);

            LineSegment segment = go.GetComponent<LineSegment>();
            gridLineSegments.Add(segment);
            segment.Start = new Vector3(bounds.min.x, bounds.max.y, y);
            segment.End = new Vector3(bounds.min.x, bounds.max.y, y);
            AnimatorManager.AnimateValue(v => { segment.End = new Vector3(v, bounds.max.y, segment.End.z); },
                bounds.min.x, bounds.max.x, 0.1f, delay, AnimatorManager.FLOAT_LERP, AnimatorManager.EASE_IN_OUT_CUBIC);
        }
    }
}
