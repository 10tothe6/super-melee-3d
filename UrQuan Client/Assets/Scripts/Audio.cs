using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    public AudioClip[] sounds;
    public AudioClip[] music;
    public AudioClip[] wins;

    private List<GameObject> audioObjects;
    public GameObject audioPrefab;

    public Slider volume;

    void Awake()
    {
        audioObjects = new List<GameObject>();
    }

    public void PlaySound(int soundType)
    {
        GameObject newAudio = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
        newAudio.transform.SetParent(transform);
        newAudio.GetComponent<AudioSource>().clip = sounds[soundType];
        newAudio.GetComponent<AudioSource>().Play();

        audioObjects.Add(newAudio);
    }

    public void PlayWin(int winType)
    {
        bool foundAudio = false;
        foreach (GameObject currentAudio in audioObjects)
        {
            if (currentAudio.GetComponent<AudioSource>().clip == wins[winType])
            {
                foundAudio = true;
                break;
            }
        }

        if (!foundAudio)
        {
            GameObject newAudio = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
            newAudio.transform.SetParent(transform);
            newAudio.GetComponent<AudioSource>().clip = wins[winType];
            newAudio.GetComponent<AudioSource>().Play();

            audioObjects.Add(newAudio);
        }
    }

    public void PlaySingleSound(int soundType)
    {
        bool foundAudio = false;
        foreach (GameObject currentAudio in audioObjects)
        {
            if (currentAudio.GetComponent<AudioSource>().clip == sounds[soundType])
            {
                foundAudio = true;
                break;
            }
        }

        if (!foundAudio)
        {
            GameObject newAudio = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
            newAudio.transform.SetParent(transform);
            newAudio.GetComponent<AudioSource>().clip = sounds[soundType];
            newAudio.GetComponent<AudioSource>().Play();

            audioObjects.Add(newAudio);
        }
    }

    public void PlayResetSound(int soundType)
    {
        bool foundAudio = false;
        foreach (GameObject currentAudio in audioObjects)
        {
            if (currentAudio.GetComponent<AudioSource>().clip == sounds[soundType])
            {
                currentAudio.GetComponent<AudioSource>().Play();
                foundAudio = true;
                break;
            }
        }

        if (!foundAudio)
        {
            GameObject newAudio = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
            newAudio.transform.SetParent(transform);
            newAudio.GetComponent<AudioSource>().clip = sounds[soundType];
            newAudio.GetComponent<AudioSource>().Play();

            audioObjects.Add(newAudio);
        }
    }

    public void PlayMusic(int musicType)
    {
        if (musicType != -1)
        {
            if (music[musicType] != GetComponent<AudioSource>().clip)
            {
                GetComponent<AudioSource>().clip = music[musicType];
                GetComponent<AudioSource>().loop = true;
                GetComponent<AudioSource>().Play();
            }
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }
    }

    private void Update()
    {
        GetComponent<AudioSource>().volume = volume.value;

        for (int i = audioObjects.Count; i > 0; i--)
        {
            if (!audioObjects[i - 1].GetComponent<AudioSource>().isPlaying)
            {
                Destroy(audioObjects[i - 1]);
                audioObjects.RemoveAt(i - 1);
            }
            else
            {
                audioObjects[i - 1].GetComponent<AudioSource>().volume = volume.value;
            }
        }
    }
}
