using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : MonoBehaviour
{
    private Vector3 oldPos;
    private float startTime;
    private float trailTime;

    public GameObject trailPrefab;

    void Awake()
    {
        oldPos = Vector3.zero;

        startTime = Time.time;
        trailTime = Time.time;
    }

    void FixedUpdate()
    {
        if (Time.time > trailTime + 0.05f)
        {
            if (trailPrefab != null)
            {
                Instantiate(trailPrefab, transform.position, Quaternion.identity);
            }
            trailTime = Time.time;
        }

        if (oldPos != transform.position)
        {
            transform.forward = transform.position - oldPos;
        }

        oldPos = transform.position;

        if (Time.time > startTime + 15)
        {
            GameLogic logic = GameObject.Find("NetworkManager").GetComponent<GameLogic>();
            Missile remove = null;

            foreach (Missile currentMissile in logic.missiles)
            {
                if (currentMissile.reference == this.gameObject)
                {
                    remove = currentMissile;
                }
            }
            logic.missiles.Remove(remove);

            Destroy(this.gameObject);
        }
    }
}
