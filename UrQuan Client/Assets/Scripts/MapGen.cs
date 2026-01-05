using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGen : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject metalPrefab;
    public Transform mapParent;

    //CLIENT SIDE

    public void Generate(ushort seed, ushort count, ushort radius)
    {
        System.Random rnd = new System.Random(seed);

        for (int i = mapParent.childCount; i > 0; i--)
        {
            Destroy(mapParent.GetChild(i - 1).gameObject);
        }

        for (int i = 0; i < count; i++)
        {
            int metalChance = rnd.Next(1, 10);

            if (metalChance > 8)
            {
                GameObject newMetal = Instantiate(metalPrefab, new Vector3(rnd.Next(-radius, radius), rnd.Next(-radius, radius), rnd.Next(-radius, radius)), Quaternion.identity);
                newMetal.transform.SetParent(mapParent);
            }
            else
            {
                GameObject newAsteroid = Instantiate(asteroidPrefab, new Vector3(rnd.Next(-radius, radius), rnd.Next(-radius, radius), rnd.Next(-radius, radius)), Quaternion.identity);
                int scale = rnd.Next(1, 10);
                newAsteroid.transform.localScale = new Vector3(scale, scale, scale);
                newAsteroid.transform.SetParent(mapParent);
                newAsteroid.GetComponent<Asteroid>().velocity = new Vector3((float)rnd.Next(10, 75) / 100, (float)rnd.Next(10, 75) / 100, (float)rnd.Next(10, 75) / 100);
            }
        }
    }
}
