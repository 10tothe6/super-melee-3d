using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : MonoBehaviour
{
    private Transform target;
    public ushort number;
    public ushort type;

    private bool init;

    public float health;

    public int shipType;

    private void Awake()
    {
        init = false;
        if (shipType == 1)
        {
            health = 1;
        }
        if (shipType == 2)
        {
            health = 2;
        }
    }

    public void Initialize()
    {
        target = null;
        foreach (Player currentPlayer in Player.list.Values)
        {
            if (target == null || Vector3.Distance(currentPlayer.transform.position, transform.position) < Vector3.Distance(target.position, transform.position))
            {
                if (currentPlayer.Id != GetComponent<Bullet>().id)
                {
                    target = currentPlayer.transform;
                }
            }
        }

        init = true;
    }

    private void FixedUpdate()
    {
        if (health < 1)
        {
            GetComponent<Bullet>().Kill();
        }

        if (init)
        {
            if (target != null)
            {
                if (shipType == 1)
                {
                    transform.forward = Vector3.Lerp(transform.forward, target.position - transform.position, 1);
                }
                else
                {
                    transform.forward = Vector3.Lerp(transform.forward, target.position - transform.position, 0.75f);
                }
            }

            GetComponent<Bullet>().playerSource.SendMissile(transform.position, type, number);
        }
    }
}
