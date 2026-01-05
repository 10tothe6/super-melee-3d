using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    //SERVER SIDE BOMB

    private float startTime;
    public float id;

    public float damage;

    public PlayerMovement playerSource;

    public void Explode()
    {
        startTime = Time.time;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 7.5f);
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
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (Time.time > startTime + 2)
        {
            Destroy(this.gameObject);
        }
    }
}
