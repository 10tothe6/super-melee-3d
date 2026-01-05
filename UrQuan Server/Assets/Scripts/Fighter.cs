using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    public Vector3 velocity;

    private Transform target;
    public float damage;

    public ushort number;
    public ushort type;
    public float id;
    public PlayerMovement playerSource;

    private float fireTimer;

    private bool init;

    public float health;

    private void Awake()
    {
        fireTimer = Time.time;
        init = false;

        health = 2;
    }

    public void Initialize()
    {
        target = null;
        foreach (Player currentPlayer in Player.list.Values)
        {
            if (target == null || Vector3.Distance(currentPlayer.transform.position, transform.position) < Vector3.Distance(target.position, transform.position))
            {
                if (currentPlayer.Id != id)
                {
                    target = currentPlayer.transform;
                }
            }
        }

        init = true;
    }

    private void FixedUpdate()
    {
        velocity = Vector3.Lerp(velocity, -transform.forward * 1.5f, 0.01f);

        if (target != null)
        {
            if (Vector3.Distance(target.position, transform.position) > 30)
            {
                transform.position += velocity;
            }
            else
            {
                if (Time.time > fireTimer + 3.5f)
                {
                    float closest = 0;
                    Transform reference = null;
                    ushort foundPlayer = 0;
                    foreach (Player currentPlayer in Player.list.Values)
                    {
                        if (Vector3.Distance(transform.position, currentPlayer.transform.position) < 40 && currentPlayer.Id != playerSource.player.Id)
                        {
                            if (closest == 0 || Vector3.Distance(transform.position, currentPlayer.transform.position) < closest)
                            {
                                foundPlayer = currentPlayer.Id;
                                closest = Vector3.Distance(transform.position, currentPlayer.transform.position);
                                reference = currentPlayer.transform;
                            }
                        }
                    }

                    if (foundPlayer != 0)
                    {
                        int laserDistance = Mathf.RoundToInt(closest);

                        for (int i = 0; i < laserDistance; i++)
                        {
                            GameObject laserSegment = Instantiate(GameLogic.Singleton.bulletPrefabs[11], transform.position + ((reference.position - transform.position).normalized * i), Quaternion.identity);
                            laserSegment.transform.forward = (reference.position - transform.position).normalized;

                            playerSource.SendBullet(transform.position + ((reference.position - transform.position).normalized * i), (reference.position - transform.position).normalized, 11);
                        }

                        reference.gameObject.GetComponent<PlayerMovement>().SendHit(reference.gameObject.GetComponent<Player>().Id, 1, 0);
                        reference.gameObject.GetComponent<PlayerMovement>().health -= 1;
                    }

                    fireTimer = Time.time;
                }
            }
        } 

        if (init)
        {
            if (target != null)
            {
                transform.forward = -Vector3.Lerp(transform.forward, target.position - transform.position, 0.9f);
            }

            playerSource.SendMissile(transform.position, type, number);
        }

        if (GameLogic.Singleton.gameState == 0 || health < 1)
        {
            playerSource.KillMissile(number, type);
            Destroy(this.gameObject);
        }
    }
}
