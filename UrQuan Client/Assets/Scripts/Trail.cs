using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    private float startTime;

    void Awake()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (GameObject.Find("LocalPlayer(Clone)") != null)
        {
            transform.forward = GameObject.Find("LocalPlayer(Clone)").transform.position - transform.position;
        }

        if (Time.time > startTime + 0.5f)
        {
            Destroy(this.gameObject);
        }
    }
}
