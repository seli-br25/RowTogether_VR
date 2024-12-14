using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Realtime;

public class PaddleBoatController : MonoBehaviourPun, IPunOwnershipCallbacks
{

    private float forceMultiplier = 7f;
    private float torqueMultiplier = 12f;

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

    public bool enableLog;
    private int grabCount = 0;

    public PaddleAudioManager soundEffectManager;
    private Vector3 preWaterVelocitySoundDecider;

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

        if ((transform.localPosition != initialLocalPosition && transform.localRotation != initialLocalRotation) && !inWater)
        {
            // Update the paddle's velocity
            if (lastPosition == Vector3.zero)
            {
                lastPosition = paddleCollider.bounds.center;
                preWaterVelocitySoundDecider = Vector3.zero; // Reset velocity if no previous position exists
                return;
            }

            Vector3 currentPosition = paddleCollider.bounds.center;
            preWaterVelocitySoundDecider = (currentPosition - lastPosition) / Time.fixedDeltaTime;
            lastPosition = currentPosition;
        }

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
            if (transform.localPosition != initialLocalPosition || transform.localRotation != initialLocalRotation)
            {

                float paddleSpeed = preWaterVelocitySoundDecider.magnitude;

                if (paddleSpeed > 2)
                {
                    soundEffectManager.PlayRandomAudio(0);
                }
                else
                {
                    soundEffectManager.PlayRandomAudio(1);
                }
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Water"))
        {
            inWater = false;
            paddleRigidBody.isKinematic = true;
            lastPosition = Vector3.zero;
        }
    }

    public void Update()
    {
    }

    public void OnPaddleGrab()
    {
        grabCount++;
    }

    public void OnPaddleRelease()
    {
        grabCount--;

        if (grabCount <= 0)
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
            inWater = false;
            lastPosition = Vector3.zero;
        }
    }

    public void RequestOwnership()
    {
        photonView.RequestOwnership();
    }

    public void SetForceMultiplier(float newMultiplier)
    {
        forceMultiplier = newMultiplier;
    }

    public void SetTorqueMultiplier(float newMultiplier)
    {
        torqueMultiplier = newMultiplier;
    }

    public float GetForceMultiplier()
    {
        return forceMultiplier;
    }

    public float GetTorqueMultiplier()
    {
        return torqueMultiplier;
    }


    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log($"Ownership requested by: {requestingPlayer.NickName} for PhotonView: {targetView.ViewID}");

        // Ensure this is the correct PhotonView
        if (targetView == photonView && photonView.IsMine)
        {
            
            photonView.TransferOwnership(requestingPlayer);
            Debug.Log($"Ownership granted to: {requestingPlayer.NickName}");
        }
        else
        {
            Debug.LogWarning("Ownership request ignored; either not the owner or wrong PhotonView.");
        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {

        if (targetView == photonView)
        {
            //Debug.Log($"Ownership transferred from: {previousOwner.NickName} to: {photonView.Owner.NickName}");

            // Additional logic, if required, such as resetting UI or state
            if (!photonView.IsMine)
            {
                XRGrabInteractableNetworked interactable = GetComponent<XRGrabInteractableNetworked>();
                // Force release the object if it's currently grabbed
                //IXRSelectInteractor firstInteractor = GetComponent<XRGrabInteractableNetworked>().firstInteractorSelecting;
                List<IXRSelectInteractor> allInteractors = new List<IXRSelectInteractor> (interactable.interactorsSelecting);
                foreach (IXRSelectInteractor interactor in allInteractors)
                {
                    if (interactor != null)
                    {
                        interactable.interactionManager.SelectExit(interactor, interactable);
                    }
                }
                
                grabCount = 0;

            }
        }

    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        if (targetView == photonView)
        {
            Debug.LogError($"Ownership transfer failed for PhotonView: {targetView.ViewID}. Requested by: {senderOfFailedRequest.NickName}");

            // Handle failure, such as retrying or showing a message to the user
        }
    }
}
