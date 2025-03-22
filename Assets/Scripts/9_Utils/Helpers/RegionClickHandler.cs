// RegionClickHandler.cs - New script
using UnityEngine;
using UnityEngine.Events;

public class RegionClickHandler : MonoBehaviour
{
    public string regionName;
    
    // Create an event that will be triggered when this region is clicked
    public static event System.Action<string> OnRegionClicked;

    void OnMouseDown()
    {
        // Trigger the event with this region's name
        OnRegionClicked?.Invoke(regionName);
    }
}