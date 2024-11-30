using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleBoatController : MonoBehaviour
{
    public float forceMultiplier = 0.00000001f;
    public float torqueMultiplier = 4f;

    public GameObject boat;
    private Collider boatCollider;
    public Collider playerCollider;
    private Rigidbody boatRigidBody;
    private Rigidbody paddleRigidBody;
    private Collider paddleCollider;

    private bool inWater = false;
    [SerializeField]
    public bool isLeftPaddle;

    private Vector3 lastPosition;
    private Vector3 calculatedVelocity;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;

        boatCollider = boat.GetComponent<Collider>();
        boatRigidBody = boat.GetComponent<Rigidbody>();
        paddleRigidBody = this.GetComponent<Rigidbody>();
        paddleCollider = this.GetComponent<Collider>();

        Physics.IgnoreCollision(paddleCollider, boatCollider);
        Physics.IgnoreCollision(paddleCollider, playerCollider);
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            calculatedVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = transform.position;
            float speed = calculatedVelocity.magnitude;

            if (speed > 0.1f)
            {
                Vector3 forceDirection = -boat.transform.forward * speed * forceMultiplier;
                float appliedTorque = isLeftPaddle ? -torqueMultiplier * speed : torqueMultiplier * speed;

                // Apply force and torque
                boatRigidBody.AddForce(forceDirection);
                boatRigidBody.AddTorque(Vector3.up * appliedTorque);
                boatRigidBody.velocity = Vector3.ClampMagnitude(boatRigidBody.velocity, 0.5f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            inWater = true;
            paddleRigidBody.isKinematic = false;
            Debug.Log("Paddle is in water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Water") && this.transform.position.y > 0.7)
        {
            inWater = false;
            paddleRigidBody.isKinematic = true;
            Debug.Log("Paddle is in not water");
        }
    }

}
