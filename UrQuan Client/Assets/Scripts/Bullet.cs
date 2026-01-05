using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //CLIENT SIDE BULLET

    public Vector3 velocity;
    private float startTime;
    private Vector3 oldPos;

    private void Awake()
    {
        startTime = Time.time;
    }

    private void FixedUpdate()
    {
        transform.position += velocity;

        if (Time.time > startTime + 15)
        {
            Destroy(this.gameObject);
        }
    }
}
