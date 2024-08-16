using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAndBob : MonoBehaviour
{
    // Rotation speed around the Y-axis
    public float rotationSpeed = 30.0f;

    // Bobbing parameters
    public float bobbingAmplitude = 0.5f; // How high/low the object bobs
    public float bobbingSpeed = 1.0f;     // How fast the object bobs

    // Original position of the object
    private Vector3 startPosition;

    void Start()
    {
        // Record the original position of the object
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate the object around the Y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmplitude;

        // Apply the new Y position to the object
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
