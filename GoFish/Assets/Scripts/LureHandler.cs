using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LureHandler : MonoBehaviour
{
    public AudioClip splashSound; // Assign this in the Unity Inspector
    private AudioSource audioSource;
    private Rigidbody lureRB;
    void Start()
    {
        // Initialize the audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = splashSound;
        lureRB = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        
        // Check if the lure has collided with the water object
        if (other.gameObject.CompareTag("Water"))
        {
            // Play the splash sound
            audioSource.Play();
            lureRB.isKinematic = true;
            lureRB.gameObject.transform.position += new Vector3(0, 10.0f * Time.deltaTime, 0); //Bob
            Debug.Log("Splash! Lure hit the water.");
        }
    }
}