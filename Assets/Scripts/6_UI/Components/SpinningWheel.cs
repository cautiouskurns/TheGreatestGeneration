using UnityEngine;

public class SpinningWheel : MonoBehaviour
{
    public float rotationSpeed = 300f; // degrees per second
    
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}