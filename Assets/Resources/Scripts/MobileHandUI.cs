using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MobileHandUI : MonoBehaviour
{
    public Text hitCounterText;
    public Toggle resetToggle;

    public InputActionAsset XRInputActions;
    private InputAction leftHandJoystick;
    private InputAction leftHandAButton;

    public GameObject boxContents;
    public GameObject boxContentsPrefab;

    public GameObject stick;

    public Slider scaleSlider;
    public float minScale = 0.1f; 
    public float maxScale = 3.0f;
    private float currentScale = 1.0f;

    private int hitCount;

    void Start()
    {
        hitCount = 0;
        UpdateHitCounter();

        leftHandJoystick = XRInputActions.FindAction("LeftHand/Scaling", true);
        leftHandAButton = XRInputActions.FindAction("LeftHand/ResetToggle", true);
        leftHandJoystick.Enable();
        leftHandAButton.Enable();

        scaleSlider.minValue = minScale;
        scaleSlider.maxValue = maxScale;
        scaleSlider.value = currentScale;
    }

    private void Update()
    {
        if (leftHandAButton.WasPressedThisFrame())
        {
            // destroy billiardBox + make a new one + reset hitCount
            resetToggle.isOn = true;
            if (boxContents != null)
            {
                Destroy(boxContents);
            }

            boxContents = Instantiate(boxContentsPrefab, boxContentsPrefab.transform.position, Quaternion.identity);

            GameObject billiardBox = boxContents.transform.Find("BilliardBox").gameObject;
            Collider[] wallColliders = billiardBox.GetComponentsInChildren<Collider>();
            Collider stickCollider = stick.GetComponent<Collider>();
            if (stickCollider != null)
            {
                foreach (Collider wallCollider in wallColliders)
                {
                    Physics.IgnoreCollision(stickCollider, wallCollider);
                }
            }

            currentScale = 1.0f;
            scaleSlider.value = currentScale;
            hitCount = 0;
            UpdateHitCounter();
            resetToggle.isOn = false;
        }

        float joystickValue = leftHandJoystick.ReadValue<Vector2>().y;
        float scaleChange = joystickValue * Time.deltaTime;
        currentScale = Mathf.Clamp(currentScale + scaleChange, minScale, maxScale);
        boxContents.transform.localScale = Vector3.one * currentScale;
        scaleSlider.value = currentScale;
    }

    public void IncrementHitCount()
    {
        hitCount++;
        UpdateHitCounter();
    }

    private void UpdateHitCounter()
    {
        hitCounterText.text = "Hits: " + hitCount;
    }
}