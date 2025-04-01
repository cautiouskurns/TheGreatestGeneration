using UnityEngine;

using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages a group of tabs for UI navigation
/// </summary>
public class TabGroup : MonoBehaviour
{
    [Header("Tab References")]
    public List<UITabButton> tabButtons = new List<UITabButton>();
    public List<GameObject> tabPages = new List<GameObject>();
    
    [Header("Tab Styling")]
    public Color tabIdle = new Color(0.5f, 0.5f, 0.5f);
    public Color tabHover = new Color(0.7f, 0.7f, 0.7f);
    public Color tabActive = Color.white;
    
    private UITabButton selectedTab;
    
    private void Start()
    {
        // Initialize tabs
        for (int i = 0; i < tabButtons.Count && i < tabPages.Count; i++) 
        {
            // Ensure tab buttons are set up
            if (tabButtons[i] != null)
            {
                tabButtons[i].SetTabGroup(this);
                // Use i for closure in lambda
                int tabIndex = i;
                tabButtons[i].GetComponent<Button>().onClick.AddListener(() => OnTabSelected(tabButtons[tabIndex]));
            }
        }
        
        // Initialize first tab as selected if available
        if (tabButtons.Count > 0 && tabButtons[0] != null)
        {
            OnTabSelected(tabButtons[0]);
        }
    }
    
    public void OnTabSelected(UITabButton button)
    {
        selectedTab = button;
        
        // Update tab visuals
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] != null)
            {
                tabButtons[i].SetState(tabButtons[i] == selectedTab);
                
                // Show/hide corresponding page
                if (i < tabPages.Count && tabPages[i] != null)
                {
                    tabPages[i].SetActive(tabButtons[i] == selectedTab);
                }
            }
        }
    }
    
    public void OnTabEnter(UITabButton button)
    {
        // Only highlight if not the selected tab
        if (button != selectedTab)
        {
            button.SetColor(tabHover);
        }
    }
    
    public void OnTabExit(UITabButton button)
    {
        // Restore idle color if not the selected tab
        if (button != selectedTab)
        {
            button.SetColor(tabIdle);
        }
    }
}