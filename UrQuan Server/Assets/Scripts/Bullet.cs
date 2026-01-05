using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //SERVER SIDE BULLET

    public Vector3 velocity;
    private Vector3 oldPos;
    private float startTime;
    public float id;

    public float damage;

    public LayerMask whatIsShip;
    public LayerMask whatIsFighter;
    public LayerMask whatIsMissile;
    public PlayerMovement playerSource;
    public int shipType;

    private float hitRadius;

    private void Awake()
    {
        if (GetComponent<Homing>() != null)
        {
            hitRadius = 7f;
        }
        else
        {
            hitRadius = 5.5f;
        }

        startTime = Time.time;
        oldPos = transform.position;
    }

    public void Kill()
    {
        playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
        Destroy(this.gameObject);
    }

    private void FixedUpdate()
    {
        if (GameLogic.Singleton.gameState == 0)
        {
            if (GetComponent<Homing>() != null)
            {
                playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
                Destroy(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        if (GetComponent<Homing>() != null)
        {
            if (shipType == 1)
            {
                velocity = Vector3.Lerp(velocity, transform.forward * 1.75f, 0.0125f);
                transform.position += velocity;
            }
            else
            {
                velocity = Vector3.Lerp(velocity, transform.forward * 2.5f, 0.0075f);
                transform.position += velocity;
            }
        }
        else
        {
            transform.position += velocity;
        }

        RaycastHit hit;
        if (Physics.SphereCast(oldPos, hitRadius, transform.position - oldPos, out hit, Vector3.Distance(oldPos, transform.position), whatIsShip))
        {
            if (hit.transform.parent.gameObject.GetComponent<Player>().Id != id)
            {
                playerSource.SendHit(hit.transform.parent.gameObject.GetComponent<Player>().Id, damage, 0);
                hit.transform.parent.gameObject.GetComponent<PlayerMovement>().health -= damage;
                if (GetComponent<Homing>() != null)
                {
                    playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
                }
                Destroy(this.gameObject);
            }
        }

        if (Physics.SphereCast(oldPos, hitRadius, transform.position - oldPos, out hit, Vector3.Distance(oldPos, transform.position), whatIsFighter))
        {
            hit.transform.gameObject.GetComponent<Fighter>().health -= damage;
            if (GetComponent<Homing>() != null)
            {
                playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
            }
            Destroy(this.gameObject);
        }

        if (Physics.SphereCast(oldPos, hitRadius, transform.position - oldPos, out hit, Vector3.Distance(oldPos, transform.position), whatIsMissile))
        {
            hit.transform.gameObject.GetComponent<Homing>().health -= damage;
            if (GetComponent<Homing>() != null)
            {
                playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
            }
            Destroy(this.gameObject);
        }

        if (Time.time > startTime + 15)
        {
            Destroy(this.gameObject);
            if (GetComponent<Homing>() != null)
            {
                playerSource.KillMissile(GetComponent<Homing>().number, GetComponent<Homing>().type);
            }
        }

        oldPos = transform.position;
    }
}
