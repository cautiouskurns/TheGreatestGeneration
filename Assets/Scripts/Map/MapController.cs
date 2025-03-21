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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            string regionName = hit.collider.gameObject.name;
            Debug.Log($"🖱️ Region clicked: {regionName}");

            if (regionEntities != null && regionEntities.ContainsKey(regionName))
            {
                EventBus.Trigger("RegionSelected", regionEntities[regionName]);
            }
        }
    }
}

