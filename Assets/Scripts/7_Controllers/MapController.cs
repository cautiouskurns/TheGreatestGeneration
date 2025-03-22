using UnityEngine;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    public Camera mainCamera;
    private Dictionary<string, RegionEntity> regionEntities;

    private void OnEnable()
    {
        EventBus.Subscribe("RegionEntitiesReady", OnRegionEntitiesReady);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionEntitiesReady);
    }

    private void OnRegionEntitiesReady(object data)
    {
        regionEntities = data as Dictionary<string, RegionEntity>;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectRegionClick();
        }
    }

    private void DetectRegionClick()
    {
        Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            string regionName = hit.collider.gameObject.name;
            Debug.Log($"🖱️ Region clicked: {regionName}");

            if (regionEntities != null && regionEntities.ContainsKey(regionName))
            {
                EventBus.Trigger("RegionSelected", regionEntities[regionName]);
            }
            else
            {
                Debug.LogWarning($"⚠️ Region {regionName} was clicked but not found in regionEntities dictionary");
            }
        }
        else
        {
            Debug.Log("🖱️ Click detected but no region was hit");
        }
    }
}

