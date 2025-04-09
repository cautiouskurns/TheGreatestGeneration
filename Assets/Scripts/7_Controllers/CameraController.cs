/// CLASS PURPOSE:
/// CameraController enables player-controlled panning and zooming of the camera
/// using keyboard and mouse input. It provides smooth and clamped movement within
/// an orthographic camera setup.
///
/// CORE RESPONSIBILITIES:
/// - Handle keyboard input for horizontal and vertical camera movement
/// - Process mouse scroll wheel input for orthographic zoom
/// - Clamp zoom values to defined minimum and maximum bounds
///
/// KEY COLLABORATORS:
/// - Unity Input System: Supplies raw input values for movement and zoom
/// - Camera.main: Modified directly to change zoom level via orthographic size
///
/// CURRENT ARCHITECTURE NOTES:
/// - Operates under MonoBehaviour’s Update loop for real-time responsiveness
/// - Assumes the presence of a Main Camera with orthographic projection
///
/// REFACTORING SUGGESTIONS:
/// - Replace hardcoded Input calls with Unity’s new Input System for flexibility
/// - Separate panning and zooming into helper methods for modularity
/// - Add boundary constraints for camera movement if needed
///
/// EXTENSION OPPORTUNITIES:
/// - Support drag-based panning or edge scrolling
/// - Add smooth zoom interpolation for better user experience
/// - Integrate camera shake or follow modes for dynamic gameplay

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    void Update()
    {
        // Panning
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        transform.position += new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;

        // Zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }
}
