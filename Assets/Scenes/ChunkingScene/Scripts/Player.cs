using UnityEngine;

public class Player : MonoBehaviour
{
    public bool drop = false;
    MeshRenderer renderer;
    Rigidbody rb;

    bool falling = false;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        renderer.enabled = false;
    }

    public void Show()
    {
        renderer.enabled = true;
    }

    public void Drop()
    {
        rb.isKinematic = false;
        falling = true;
        renderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (drop)
        {
            Drop();
            drop = false;
        }

        if (falling)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
    }
}
