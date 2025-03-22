using UnityEngine;

public class MapController : MonoBehaviour
{
    public Camera mainCamera;
    
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMapClick();
        }
    }

    private void HandleMapClick()
    {
        Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Region"))
        {
            string regionName = hit.collider.gameObject.name;
            Debug.Log($"Region clicked: {regionName}");

            gameManager.SelectRegion(regionName);
        }
    }
}
