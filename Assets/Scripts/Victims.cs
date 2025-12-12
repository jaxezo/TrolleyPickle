using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Animation))]
[RequireComponent(typeof (Collider))]
[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (AudioSource))]
public class Victims : MonoBehaviour
{
    public AudioClip audio_DeathSound;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Player"))
        {
            GetComponent<Animation>().Play();
            GetComponent<AudioSource>().clip = audio_DeathSound;
            GetComponent<AudioSource>().Play();
        }
    }
}
