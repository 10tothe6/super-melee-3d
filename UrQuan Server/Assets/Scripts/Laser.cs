using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private float startTime;

    void Awake()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time > startTime + 0.3f)
        {
            Destroy(this.gameObject);
        }
    }
}
