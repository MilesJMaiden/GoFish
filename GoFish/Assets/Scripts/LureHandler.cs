using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LureHandler : MonoBehaviour
{
    public AudioClip splashSound; // Assign this in the Unity Inspector
    private AudioSource audioSource;
    private Rigidbody lureRB;
    private bool inWater = false;
    public GameObject SplashEffect;

    // Bobbing parameters
    public float bobbingAmplitude = 0.1f; // How high/low the object bobs
    public float bobbingSpeed = 5.0f;     // How fast the object bobs
    private float offset = 0.1f;

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
            float newY = (startPosition.y + offset) + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;

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
            SplashEffect.SetActive(true);
            Debug.Log("Splash! Lure hit the water.");
        }
    }
}