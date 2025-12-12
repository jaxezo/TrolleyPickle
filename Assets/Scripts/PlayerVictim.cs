using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Animation))]
[RequireComponent(typeof (Collider))]
[RequireComponent(typeof (Rigidbody))]
public class PlayerVictim : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag ("Player"))
        {
            GetComponent<Animation>().Play();
        }
    }
}
