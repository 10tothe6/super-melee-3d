using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    private Vector3 oldPos;

    void FixedUpdate()
    {
        if (oldPos != transform.position)
        {
            transform.forward = -(transform.position - oldPos);
        }

        oldPos = transform.position;
    }
}
