using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckHandSwing : MonoBehaviour
{
    private bool leftHandInFront;
    private bool rightHandInFront;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Right Hand Model")
        {
            rightHandInFront = true;
        }
        if (other.gameObject.tag == "Left Hand Model")
        {
            leftHandInFront = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Right Hand Model")
        {
            rightHandInFront = false;
        }
        if (other.gameObject.tag == "Left Hand Model")
        {
            leftHandInFront = false;
        }

    }

    public bool HandsInFront()
    {
        return rightHandInFront && leftHandInFront;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
