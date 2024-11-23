using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StickMechanic : MonoBehaviour
{
    public GameObject billiardBox;

    public XRBaseController leftController; 
    public XRBaseController rightController; 
    public float hitVibrationIntensity = 0.5f;
    public float hitVibrationDuration = 0.1f;

    private MobileHandUI handUI;

    // Start is called before the first frame update
    void Start()
    {
        // deactivate collision detection for Stick and BilliardBox
        Collider stickCollider = this.GetComponent<Collider>();
        Collider[] wallColliders = billiardBox.GetComponentsInChildren<Collider>();

        if (stickCollider != null)
        {
            foreach (Collider wallCollider in wallColliders)
            {
                Physics.IgnoreCollision(stickCollider, wallCollider);
            }
        }

        handUI = FindObjectOfType<MobileHandUI>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        // send haptic feedback if stick collides with a ball 
        if (collision.gameObject.CompareTag("Ball"))
        {
            SendHapticFeedback(hitVibrationIntensity, hitVibrationDuration);
            if (handUI != null)
            {
                handUI.IncrementHitCount();
            }
        }
    }


    private void SendHapticFeedback(float intensity, float duration)
    {
        if (leftController != null)
        {
            leftController.SendHapticImpulse(intensity, duration);
        }

        if (rightController != null)
        {
            rightController.SendHapticImpulse(intensity, duration);
        }
    }
}
