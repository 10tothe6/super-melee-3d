using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    //CLIENT SIDE FIRE

    public Vector3 velocity;
    private float startTime;
    private Vector3 oldPos;

    public Sprite[] anim;
    private float speed = 6;

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
        transform.position += velocity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider hit in hitColliders)
        {
            if (hit.transform.parent.gameObject.layer == 6)
            {
                Destroy(this.gameObject);
            }
        }

        for (int i = 0; i < anim.Length; i++)
        {
            if (Time.time > startTime + ((speed / 2) / anim.Length) * i)
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
