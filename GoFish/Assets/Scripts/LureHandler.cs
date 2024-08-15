using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LureHandler : MonoBehaviour
{
    public AudioClip splashSound; // Assign this in the Unity Inspector
    private AudioSource audioSource;
    private Rigidbody lureRB;
    private bool inWater = false;

    // Bobbing parameters
    public float bobbingAmplitude = 0.5f; // How high/low the object bobs
    public float bobbingSpeed = 1.0f;     // How fast the object bobs

    // Original position of the object
    private Vector3 startPosition;

    void Start()
    {
        // Initialize the audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = splashSound;
        lureRB = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        if (inWater)
        {
            // Calculate the new Y position using a sine wave
            float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;

            // Apply the new Y position to the object
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
        // Check if the lure has collided with the water object
        if (other.gameObject.CompareTag("Water"))
        {
            // Play the splash sound
            inWater = true;
            audioSource.Play();
            lureRB.isKinematic = true;
            lureRB.gameObject.transform.position += new Vector3(0, 10.0f * Time.deltaTime, 0); //Bob
            Debug.Log("Splash! Lure hit the water.");
        }
    }
}