using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public Vector3 velocity;

    void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + velocity.x, transform.eulerAngles.y + velocity.y, transform.eulerAngles.z + velocity.z);
    }
}
