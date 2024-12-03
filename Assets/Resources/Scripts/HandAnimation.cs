using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimation : MonoBehaviour
{
    private Animator animator;
    public InputActionAsset inputActions;
    private InputAction gripAction; 
    private InputAction pinchAction;
    public bool photonUse = false;
    void Start()
    {
        if (gameObject.name.Contains("Left"))
        {
            gripAction = inputActions.FindAction("LeftHand/Grip", true);
            pinchAction = inputActions.FindAction("LeftHand/Pinch", true);
        }
        else if (gameObject.name.Contains("Right"))
        {
            gripAction = inputActions.FindAction("RightHand/Grip", true);
            pinchAction = inputActions.FindAction("RightHand/Pinch", true);
        }
        else
        {
            Debug.LogError("Error: HandAnimation Skript is not in an Controller-Gameobject!");
        }


        gripAction.Enable();
        pinchAction.Enable();

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // only perform update if HandAnimation script used by XROrigin
        if (!photonUse)
        {
            float gripValue = gripAction.ReadValue<float>();
            animator.SetBool("isFist", gripValue > 0.1f);

            float pinchValue = pinchAction.ReadValue<float>();
            animator.SetBool("isPinching", pinchValue > 0.1f);
        } else
        // do not perform update loop as photon view player handanimation will be triggered by Character.cs, that polls for photonView.isMine
        {
            return;
        }
    }

    // Triggered by Update loop of Character.cs under condition that this photonview.isMine
    public void photonHandAnimation()
    {
        if (photonUse)
        {
            float gripValue = gripAction.ReadValue<float>();
            animator.SetBool("isFist", gripValue > 0.1f);

            float pinchValue = pinchAction.ReadValue<float>();
            animator.SetBool("isPinching", pinchValue > 0.1f);
        }
    }

}
