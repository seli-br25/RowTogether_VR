using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMechanic : MonoBehaviour
{
    private Collider ballCollider;
    public GameObject billiardBox;
    private Collider boxCollider;
    private Rigidbody rb;

    public GameObject walls;
    private Collider[] wallColliders;

    // Start is called before the first frame update
    void Start()
    {
        ballCollider = this.GetComponent<Collider>();
        wallColliders = walls.GetComponentsInChildren<Collider>();
        boxCollider = billiardBox.GetComponent<Collider>();
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hole") && IsBallInsideBox())
        {
            if (ballCollider != null && wallColliders != null)
            {
                foreach (Collider wallCollider in wallColliders)
                {
                    Physics.IgnoreCollision(ballCollider, wallCollider, true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            if (ballCollider != null && wallColliders != null)
            {
                foreach (Collider wallCollider in wallColliders)
                {
                    Physics.IgnoreCollision(ballCollider, wallCollider, false);
                }
            }
        }
        else if (other.CompareTag("BilliardBox"))
        {
            if (!IsBallInsideBox())
            {
                // activate gravity if ball is outside of billiard box
                rb.useGravity = true;
            }
        }
    }

    public bool IsBallInsideBox()
    {
        return boxCollider.bounds.Contains(this.transform.position);
    }
}
