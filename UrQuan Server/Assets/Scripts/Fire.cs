using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    //SERVER SIDE FIRE

    public Vector3 velocity;
    private Vector3 oldPos;
    private float startTime;
    public float id;

    public float damage;

    public PlayerMovement playerSource;

    private void Awake()
    {
        startTime = Time.time;
        oldPos = transform.position;
    }

    private void FixedUpdate()
    {
        transform.position += velocity;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2.5f);
        foreach (Collider hit in hitColliders)
        {
            if (hit.transform.gameObject.GetComponent<Fighter>() != null)
            {
                hit.transform.gameObject.GetComponent<Fighter>().health -= damage;
            }
            else if (hit.transform.gameObject.GetComponent<Homing>() != null)
            {
                hit.transform.gameObject.GetComponent<Homing>().health -= damage;
            }
            else if (hit.transform.parent.gameObject.GetComponent<Player>() != null)
            {
                if (hit.transform.parent.gameObject.GetComponent<Player>().Id != id && hit.transform.parent.gameObject.layer == 6)
                {
                    playerSource.SendHit(hit.transform.parent.gameObject.GetComponent<Player>().Id, damage, 0);
                    hit.transform.parent.gameObject.GetComponent<PlayerMovement>().health -= damage;
                    Destroy(this.gameObject);
                }
            }
        }

        if (Time.time > startTime + 6)
        {
            Destroy(this.gameObject);
        }

        oldPos = transform.position;
    }
}
