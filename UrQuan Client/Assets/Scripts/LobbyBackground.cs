using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyBackground : MonoBehaviour
{
    private Vector3 startPos;
    private float x;
    private float y;

    public float moveSpeedX;
    public float moveSpeedY;

    public float xFactor;
    public float yFactor;

    private void Awake()
    {
        startPos = transform.position;
        x = 0;
        y = 0.5f;
    }

    private void FixedUpdate()
    {
        transform.position = startPos + new Vector3(Mathf.Sin(x) * xFactor, Mathf.Sin(y) * yFactor, 0);

        x += moveSpeedX;
        y += moveSpeedY;

        if (x > 9999)
        {
            x = 0;
        }

        if (y > 9999)
        {
            y = 0;
        }
    }
}
