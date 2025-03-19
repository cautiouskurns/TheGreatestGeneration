using UnityEngine;

public class MapController : MonoBehaviour
{
    public Camera mainCamera;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectRegionClick();
        }
    }

    private void DetectRegionClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            string regionName = hit.collider.gameObject.name;
            Debug.Log("Region Selected: " + regionName);
            EventBus.Trigger("RegionSelected", regionName);
        }
    }
}

