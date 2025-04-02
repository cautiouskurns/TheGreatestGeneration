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