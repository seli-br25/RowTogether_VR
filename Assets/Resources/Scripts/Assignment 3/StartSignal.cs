using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSignal : MonoBehaviour
{
    public GameObject barrier;


    public void RemoveBarrier()
    {
        barrier.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (barrier == null)
        {  
            Debug.LogError("Null pointer on barrier");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
