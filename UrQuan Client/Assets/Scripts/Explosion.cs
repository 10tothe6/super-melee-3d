using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    //CLIENT SIDE BOMB

    private float startTime;
    private Vector3 oldPos;

    public Sprite[] anim;
    private float speed = 0.2f;

    private void Awake()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        transform.forward = GameObject.Find("LocalPlayer(Clone)").GetComponent<PlayerController>().cam.position - transform.position;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < anim.Length; i++)
        {
            if (Time.time > startTime + (speed / anim.Length) * i)
            {
                GetComponent<SpriteRenderer>().sprite = anim[i];
            }
        }

        if (Time.time > startTime + speed)
        {
            Destroy(this.gameObject);
        }
    }
}
