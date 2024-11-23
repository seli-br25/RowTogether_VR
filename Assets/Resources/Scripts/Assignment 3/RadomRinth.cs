using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadomRinth : MonoBehaviour
{
    public List<GameObject> DoorOfLuck;
    // Start is called before the first frame update
    void Start()
    {
        if (DoorOfLuck != null && DoorOfLuck.Count > 0)
        {
            int door = Random.Range(0, DoorOfLuck.Count);
            DoorOfLuck[door].SetActive(true);
        }
        
    }
}
