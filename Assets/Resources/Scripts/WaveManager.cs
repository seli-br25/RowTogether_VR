using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    public float amplitude = 1f;
    public float lenth = 1f;
    public float speed = 1f;
    public float offset = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        offset += Time.deltaTime * speed; 
    }

    public float GetWaveHeight(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        float adjustedX = localPos.x / transform.localScale.x;
        return amplitude * Mathf.Sin(adjustedX / lenth + offset);
    }

    public float GetWaveHeightForBoat(Vector3 worldPos)
    {
        return GetWaveHeight(worldPos) * transform.localScale.y;
    }



}
