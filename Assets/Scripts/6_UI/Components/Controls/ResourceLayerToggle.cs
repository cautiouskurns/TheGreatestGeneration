/// CLASS PURPOSE:
/// ResourceLayerToggle controls the UI button responsible for toggling the visibility
/// of resource icons on the map. It provides visual feedback through icon color and text updates.
///
/// CORE RESPONSIBILITIES:
/// - Toggle the visibility of resource overlays via ScreenSpaceResourceVisualization
/// - Update icon color and label text based on current toggle state
/// - Initialize toggle state on startup and bind button interactions
///
/// KEY COLLABORATORS:
/// - ScreenSpaceResourceVisualization: Displays resource icons across the map
/// - Unity UI (Button, Image, TextMeshPro): Provides feedback and interaction
///
/// CURRENT ARCHITECTURE NOTES:
/// - Automatically finds the visualization system if not assigned in the Inspector
/// - Uses default Unity UI components with public color and label presets
///
/// REFACTORING SUGGESTIONS:
/// - Add toggle state persistence between sessions
/// - Encapsulate color/text switching logic in a separate helper method or struct
///
/// EXTENSION OPPORTUNITIES:
/// - Add tooltip or animation on toggle
/// - Integrate keyboard shortcut or gamepad toggle support
/// - Allow switching between multiple resource display modes

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the visibility of the resource layer with UI feedback
/// </summary>
public class ResourceLayerToggle : MonoBehaviour
{
    [Header("References")]
    public ScreenSpaceResourceVisualization visualizationSystem;
    public GameObject toggleIcon;
    public TextMeshProUGUI toggleText;
    
    [Header("Toggle States")]
    public Color enabledColor = new Color(0.2f, 0.8f, 0.2f);
    public Color disabledColor = new Color(0.8f, 0.2f, 0.2f);
    public string enabledText = "Resources: ON";
    public string disabledText = "Resources: OFF";
    
    private Button toggleButton;
    
    private void Awake()
    {
        toggleButton = GetComponent<Button>();
        
        if (toggleButton == null)
        {
            toggleButton = gameObject.AddComponent<Button>();
        }
        
        // Add toggle listener
        toggleButton.onClick.AddListener(OnToggleClicked);
    }
    
    private void Start()
    {
        // Find the visualization system if not assigned
        if (visualizationSystem == null)
        {
            visualizationSystem = FindFirstObjectByType<ScreenSpaceResourceVisualization>();
        }
        
        // Initialize toggle state
        UpdateToggleState();
    }
    
    private void OnToggleClicked()
    {
        if (visualizationSystem != null)
        {
            visualizationSystem.ToggleResourceIcons();
            UpdateToggleState();
        }
    }
    
    private void UpdateToggleState()
    {
        if (visualizationSystem == null) return;
        
        bool isEnabled = visualizationSystem.showResourceIcons;
        
        // Update icon color
        if (toggleIcon != null)
        {
            Image iconImage = toggleIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = isEnabled ? enabledColor : disabledColor;
            }
        }
        
        // Update text
        if (toggleText != null)
        {
            toggleText.text = isEnabled ? enabledText : disabledText;
            toggleText.color = isEnabled ? enabledColor : disabledColor;
        }
    }
}