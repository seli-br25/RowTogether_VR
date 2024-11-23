using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
public class ResetOrientation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    InputActionProperty primaryButton;
    [SerializeField]
    InputActionProperty secondaryButton;
    private XROrigin xrOrigin;
    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        primaryButton.action.performed += OnPrimaryButtonPressed;
        secondaryButton.action.performed += OnSecondaryButtonPressed;
    }


    private void OnDestroy()
    {
        // Unregister the callback to avoid memory leaks
        primaryButton.action.performed -= OnPrimaryButtonPressed;
        secondaryButton.action.performed -= OnSecondaryButtonPressed;
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        ResetCallback();
    }

    private void OnSecondaryButtonPressed(InputAction.CallbackContext context)
    {
        
        TurnAroundCallback();
    }
    public void TurnAroundCallback()
    {
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, -xrOrigin.Camera.transform.forward);
    }
    public void ResetCallback()
    {
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
    }
}
