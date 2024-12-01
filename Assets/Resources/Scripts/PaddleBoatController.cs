using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PaddleBoatController : MonoBehaviour
{
    public float forceMultiplier = 2f;
    public float torqueMultiplier = 10f;

    public GameObject boat;
    private Collider boatCollider;
    private Rigidbody boatRigidBody;
    private Rigidbody paddleRigidBody;
    public Collider paddleCollider;
    public Collider handleCollider;

    private bool inWater = false;
    [SerializeField]
    public bool isLeftPaddle;

    private Vector3 lastPosition;
    private Vector3 calculatedVelocity;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    // Start is called before the first frame update
    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        lastPosition = Vector3.zero;

        boatCollider = boat.GetComponent<Collider>();
        boatRigidBody = boat.GetComponent<Rigidbody>();
        paddleRigidBody = this.GetComponent<Rigidbody>();

        Physics.IgnoreCollision(paddleCollider, boatCollider);
        Physics.IgnoreCollision(handleCollider, boatCollider);
    }

    void FixedUpdate()
    {
        if (inWater)
        {
            // check if paddle was outside of the water before
            if (lastPosition == Vector3.zero)
            {
                lastPosition = paddleCollider.bounds.center;
                return;
            }

            // calculate change of paddle
            Vector3 currentPosition = paddleCollider.bounds.center;
            calculatedVelocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            lastPosition = currentPosition;

            float zMovement = Vector3.Dot(calculatedVelocity, boat.transform.forward);

            if (Mathf.Abs(zMovement) > 0.1f)
            {
                float speed = Mathf.Abs(zMovement);
                Vector3 forceDirection;
                float appliedTorque;

                if (zMovement < 0)
                {
                    // forward movement
                    forceDirection = boat.transform.forward * speed * forceMultiplier;
                    appliedTorque = isLeftPaddle ? torqueMultiplier * speed : -torqueMultiplier * speed;
                } else
                {
                    // backward movement
                    forceDirection = -boat.transform.forward * speed * forceMultiplier;
                    appliedTorque = isLeftPaddle ? -torqueMultiplier * speed : torqueMultiplier * speed;
                }

                // Apply force and torque
                boatRigidBody.AddForce(forceDirection);
                boatRigidBody.AddTorque(Vector3.up * appliedTorque);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            inWater = true;
            //paddleRigidBody.isKinematic = false;
            Debug.Log("Paddle is in water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            inWater = false;
            paddleRigidBody.isKinematic = true;
            Debug.Log("Paddle outside of water");
            lastPosition = Vector3.zero;
        }
    }

    public void OnPaddleRelease()
    {
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        inWater = false;
        lastPosition = Vector3.zero;
    }
}
