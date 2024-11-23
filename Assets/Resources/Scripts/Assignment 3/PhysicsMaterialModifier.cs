using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsMaterialModifier : MonoBehaviour
{
    [SerializeField]
    private Collider floorCollider;
    [SerializeField]
    private PhysicMaterial newMaterial;
    [SerializeField]
    private float dragModifier = 5;
    private float initialDrag = 0;
    private PhysicMaterial initialMaterial;
    // Start is called before the first frame update
    void Start()
    {
        PhysicMaterial mat = floorCollider.material;
        if (mat != null)
        {
            initialMaterial = mat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            initialDrag = other.gameObject.GetComponent<Rigidbody>().drag;
            other.gameObject.GetComponent<Rigidbody>().drag = dragModifier;
            floorCollider.material = newMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<Rigidbody>().drag = initialDrag;
            floorCollider.material = initialMaterial;
        }
    }
}
