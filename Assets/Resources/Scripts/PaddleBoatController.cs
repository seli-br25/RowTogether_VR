using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleBoatController : MonoBehaviour
{
    public Collider boatCollider;
    public Collider playerCollider;
    // Start is called before the first frame update
    void Start()
    {
        Collider paddleCollider = this.GetComponent<Collider>();
        Physics.IgnoreCollision(paddleCollider, boatCollider);
        Physics.IgnoreCollision(paddleCollider, playerCollider);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
