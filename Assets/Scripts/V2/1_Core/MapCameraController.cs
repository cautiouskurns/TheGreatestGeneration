using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private bool boundCamera = true;
    [SerializeField] private Vector2 boundarySize = new Vector2(10f, 10f);
    
    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }
    
    private void Update()
    {
        HandlePanning();
        HandleZooming();
        
        if (boundCamera)
        {
            EnforceBoundaries();
        }
    }
    
    private void HandlePanning()
    {
        // Start drag on mouse down
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
        
        // End drag on mouse up
        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }
        
        // Process drag
        if (isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dragDelta = dragOrigin - currentPos;
            transform.position += dragDelta;
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        
        // Keyboard controls
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 movement = new Vector3(
                horizontal * panSpeed * Time.deltaTime,
                vertical * panSpeed * Time.deltaTime,
                0
            );
            transform.position += movement;
        }
    }
    
    private void HandleZooming()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            float newSize = cam.orthographicSize - scrollInput * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
    
    private void EnforceBoundaries()
    {
        // Calculate allowed position range based on camera size and boundaries
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;
        
        // Calculate boundaries
        float minX = -boundarySize.x/2 + horzExtent;
        float maxX = boundarySize.x/2 - horzExtent;
        float minY = -boundarySize.y/2 + vertExtent;
        float maxY = boundarySize.y/2 - vertExtent;
        
        // Clamp position
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        transform.position = position;
    }
}