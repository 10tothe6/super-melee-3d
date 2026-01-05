using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private float startTime;
    public GameObject source;

    void Awake()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time > startTime + 0.1f)
        {
            Destroy(this.gameObject);
        }

        if (source != null)
        {
            transform.position = source.transform.position;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
