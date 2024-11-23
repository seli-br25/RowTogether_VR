using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueballRandomSpawn : MonoBehaviour
{
    public float pyramidRadius = 0.1f;
    public Transform billiardboxCenter;
    public Transform wallFront;
    public Transform wallLeft;
    public Transform wallRight;
    public Transform wallBack;
    public Transform wallTop;
    public Transform wallBottom;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = GetRandomPositionInsideCube();

        while (Vector3.Distance(randomPosition, billiardboxCenter.position) < pyramidRadius)
        {
            randomPosition = GetRandomPositionInsideCube();
        }

        this.transform.position = randomPosition;
    }

    Vector3 GetRandomPositionInsideCube()
    {
        float x = Random.Range(wallLeft.position.x, wallRight.position.x);
        float y = Random.Range(wallBottom.position.y, wallTop.position.y);
        float z = Random.Range(wallBack.position.z, wallFront.position.z);

        return new Vector3(x, y, z);
    }
}
