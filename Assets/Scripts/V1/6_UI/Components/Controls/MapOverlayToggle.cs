using UnityEngine;
using UnityEngine.UI;

namespace V1.UI
{ 
    /// CLASS PURPOSE:
    /// MapOverlayToggle manages the UI buttons that allow the player to switch between
    /// different map overlay modes (e.g., wealth, population, production) and reflects
    /// the current selection visually.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Handle button clicks to change the map overlay mode
    /// - Update button colors to reflect the active overlay
    /// - Initialize with correct map reference and UI setup
    /// 
    /// KEY COLLABORATORS:
    /// - MapView: Applies the selected overlay mode to the map
    /// - Unity UI Buttons: Trigger overlay changes and reflect current state
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses enum from MapView to control available overlay modes
    /// - Automatically finds MapView reference if not assigned in Inspector
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Abstract button setup logic for easier addition/removal of overlay types
    /// - Support dynamic overlays or configuration-driven mode registration
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Add keyboard shortcut integration for fast overlay switching
    /// - Animate transitions or highlight changes between overlays
    /// - Show tooltip or legend for each overlay type
    /// 
    public class MapOverlayToggle : MonoBehaviour
    {
        [Header("MapView Reference")]
        public MapView mapView;

        [Header("UI Buttons")]
        public Button defaultModeButton;
        public Button wealthModeButton;
        public Button populationModeButton;
        public Button productionModeButton;

        [Header("Button Colors")]
        public Color activeButtonColor = Color.green;
        public Color inactiveButtonColor = Color.white;

        private void Start()
        {
            // Ensure MapView is assigned
            if (mapView == null)
            {
                mapView = FindFirstObjectByType<MapView>();
            }

            // Set up button listeners
            SetupButtonListeners();

            // Initially highlight default mode
            UpdateButtonColors(MapView.MapOverlayMode.Default);
        }

        private void SetupButtonListeners()
        {
            if (defaultModeButton != null)
                defaultModeButton.onClick.AddListener(() => ChangeOverlayMode(MapView.MapOverlayMode.Default));

            if (wealthModeButton != null)
                wealthModeButton.onClick.AddListener(() => ChangeOverlayMode(MapView.MapOverlayMode.Wealth));

            if (populationModeButton != null)
                populationModeButton.onClick.AddListener(() => ChangeOverlayMode(MapView.MapOverlayMode.Population));

            if (productionModeButton != null)
                productionModeButton.onClick.AddListener(() => ChangeOverlayMode(MapView.MapOverlayMode.Production));
        }

        private void ChangeOverlayMode(MapView.MapOverlayMode newMode)
        {
            if (mapView != null)
            {
                mapView.SetOverlayMode(newMode);
                UpdateButtonColors(newMode);
            }
        }

        private void UpdateButtonColors(MapView.MapOverlayMode activeMode)
        {
            SetButtonColor(defaultModeButton, activeMode == MapView.MapOverlayMode.Default);
            SetButtonColor(wealthModeButton, activeMode == MapView.MapOverlayMode.Wealth);
            SetButtonColor(populationModeButton, activeMode == MapView.MapOverlayMode.Population);
            SetButtonColor(productionModeButton, activeMode == MapView.MapOverlayMode.Production);
        }

        private void SetButtonColor(Button button, bool isActive)
        {
            if (button != null)
            {
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isActive ? activeButtonColor : inactiveButtonColor;
                }
            }
        }
    }
}