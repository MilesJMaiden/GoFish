using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // Speed of rotation in degrees per second
    [SerializeField] private float rotationSpeed = 45f;

    void Update()
    {
        // Rotate the object around its Y-axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}