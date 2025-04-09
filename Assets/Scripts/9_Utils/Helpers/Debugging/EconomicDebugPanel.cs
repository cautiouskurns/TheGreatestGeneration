/// CLASS PURPOSE:
/// EconomicDebugPanel provides a simple UI overlay for displaying internal debug output
/// from the EnhancedEconomicSystem during simulation. It includes controls for toggling
/// visibility and triggering manual state verification.
///
/// CORE RESPONSIBILITIES:
/// - Display debug output text tied to EnhancedEconomicSystemDebug
/// - Enable manual triggering of verification checks
/// - Allow toggling visibility of the debug panel
///
/// KEY COLLABORATORS:
/// - EnhancedEconomicSystemDebug: Supplies and updates debug text
/// - EnhancedEconomicSystem: Source system being debugged
/// - Unity UI (TextMeshProUGUI, Button): Provides user interaction and output display
///
/// CURRENT ARCHITECTURE NOTES:
/// - Listeners are added and removed at runtime via UnityEvents
/// - Debug text field is set by direct reference assignment
///
/// REFACTORING SUGGESTIONS:
/// - Support runtime assignment or cycling of multiple debug sources
/// - Add collapsible sections or category filters for output
///
/// EXTENSION OPPORTUNITIES:
/// - Integrate log history, export, or search features
/// - Enable remote or networked debug syncing for multiplayer
/// - Add color-coding or formatting to enhance readability

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple UI panel to display economic debug information
/// </summary>
public class EconomicDebugPanel : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject debugPanel;
    public TextMeshProUGUI debugText;
    public Button toggleButton;
    public Button verifyNowButton;
    
    [Header("Debug Target")]
    public EnhancedEconomicSystemDebug economicDebug;
    
    private void Start()
    {
        // Find the debug component if not assigned
        if (economicDebug == null)
        {
            EnhancedEconomicSystem system = FindFirstObjectByType<EnhancedEconomicSystem>();
            if (system != null)
            {
                economicDebug = system.GetComponent<EnhancedEconomicSystemDebug>();
            }
        }
        
        // Set up toggle button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleDebugPanel);
        }
        
        // Set up verify now button
        if (verifyNowButton != null && economicDebug != null)
        {
            verifyNowButton.onClick.AddListener(economicDebug.ManualVerifyState);
        }
        
        // Initially hide panel
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
        
        // Set up debug text reference
        if (debugText != null && economicDebug != null)
        {
            economicDebug.debugOutputText = debugText;
        }
    }
    
    private void ToggleDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
        }
        
        if (verifyNowButton != null)
        {
            verifyNowButton.onClick.RemoveAllListeners();
        }
    }
}