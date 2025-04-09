using UnityEngine;
using TMPro;
using V1.Core;
using V1.UI;
using V1.Data;
using V1.Managers;

namespace V1.Controllers
{  
    /// CLASS PURPOSE:
    /// MapController acts as a bridge between the map view and the broader game logic,
    /// handling region selection events, updating the visual highlights, and displaying
    /// region information on the UI.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Listen to region click events and handle region selection
    /// - Delegate model updates to the GameManager
    /// - Control visual highlights on the map view
    /// - Update region-specific information text panel
    ///
    /// KEY COLLABORATORS:
    /// - MapView: Applies visual highlights to selected regions
    /// - GameManager: Handles model-level selection and updates
    /// - EventBus: Listens for region click events dispatched by the user
    /// - TextMeshProUGUI: Displays region-specific info (e.g., wealth, production)
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Utilizes EventBus for decoupled communication
    /// - Retrieves static region data from MapDataSO via MapView
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Decouple UI update logic into a separate display handler or panel controller
    /// - Store a cached region lookup dictionary for faster access
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add tooltip, contextual info, or region action menu
    /// - Allow deselection or multi-region selection support
    /// - Trigger additional updates (e.g., camera focus or audio feedback) on selection

    public class MapController : MonoBehaviour
    {
        public MapView mapView;
        public TextMeshProUGUI infoText;
        private GameManager gameManager;

        private string selectedRegion = "";

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
            }
        }

        private void OnEnable()
        {
            // Listen for RegionClicked events from RegionClickHandler
            EventBus.Subscribe("RegionClicked", OnRegionClicked);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionClicked", OnRegionClicked);
        }

        private void OnRegionClicked(object regionNameObj)
        {
            string regionName = (string)regionNameObj;
            Debug.Log($"MapController received RegionClicked for: {regionName}");
            
            // Pass to GameManager which handles the model update
            if (gameManager != null)
            {
                gameManager.SelectRegion(regionName);
            }
        }

        void SelectRegion(string regionName)
        {
            // Reset previous selection
            if (!string.IsNullOrEmpty(selectedRegion))
            {
                Color originalColor = GetRegionNationColor(selectedRegion);
                mapView.ResetHighlight(selectedRegion, originalColor);
            }

            // Set new selection
            selectedRegion = regionName;
            mapView.HighlightRegion(regionName);

            // Update info panel
            MapDataSO.RegionData regionData = GetRegionData(regionName);
            if (regionData != null)
            {
                infoText.text = $"Region: {regionName}\nWealth: {regionData.initialWealth}\nProduction: {regionData.initialProduction}";
            }
        }

        MapDataSO.RegionData GetRegionData(string regionName)
        {
            foreach (var nation in mapView.mapData.nations)
            {
                foreach (var region in nation.regions)
                {
                    if (region.regionName == regionName)
                        return region;
                }
            }
            return null;
        }

        Color GetRegionNationColor(string regionName)
        {
            foreach (var nation in mapView.mapData.nations)
            {
                foreach (var region in nation.regions)
                {
                    if (region.regionName == regionName)
                        return nation.nationColor;
                }
            }
            return Color.white;
        }
    }
}