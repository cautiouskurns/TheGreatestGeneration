using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float panSpeed = 5f;

    void Update()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            Camera.main.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 20f);
        }

        if (Input.GetMouseButton(1)) // Right-click to pan
        {
            float moveX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float moveY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;
            Camera.main.transform.Translate(moveX, moveY, 0);
        }
    }
}
