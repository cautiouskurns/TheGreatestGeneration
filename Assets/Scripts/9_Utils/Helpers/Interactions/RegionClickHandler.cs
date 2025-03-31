// RegionClickHandler.cs - Aligned with EventBus
using UnityEngine;

public class RegionClickHandler : MonoBehaviour
{
    public string regionName;

    void OnMouseDown()
    {
        Debug.Log($"RegionClickHandler: Mouse down on {regionName}");
        
        // Trigger the RegionClicked event that MapController listens for
        EventBus.Trigger("RegionClicked", regionName);
    }
}