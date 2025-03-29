using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CabinetDialogueUI : MonoBehaviour
{
    [Header("Cabinet UI References")]
    public GameObject cabinetPanel;
    public TextMeshProUGUI meetingTitleText;
    public TextMeshProUGUI agendaText;
    
    [Header("Advisor References")]
    public Transform advisorsContainer;
    public GameObject advisorSlotPrefab;
    
    [Header("Discussion References")]
    public TextMeshProUGUI discussionText;
    public Transform recommendationsContainer;
    public GameObject recommendationPrefab;
    
    [Header("Decision References")]
    public GameObject decisionPanel;
    public Transform optionsContainer;
    public GameObject decisionOptionPrefab;
    
    private List<AdvisorData> activeAdvisors = new List<AdvisorData>();
    private List<RecommendationData> activeRecommendations = new List<RecommendationData>();
    private Dictionary<string, GameObject> advisorSlots = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Hide panels by default
        if (cabinetPanel != null)
            cabinetPanel.SetActive(false);
            
        if (decisionPanel != null)
            decisionPanel.SetActive(false);
    }
    
    public void StartCabinetMeeting(string title, List<AdvisorData> advisors, List<string> agendaItems)
    {
        // Set up cabinet meeting
        cabinetPanel.SetActive(true);
        meetingTitleText.text = title;
        
        // Set agenda text
        string agendaString = "Meeting Agenda:\n";
        foreach (var item in agendaItems)
        {
            agendaString += $"• {item}\n";
        }
        agendaText.text = agendaString;
        
        // Clear and set up advisor slots
        ClearAdvisorSlots();
        SetupAdvisors(advisors);
        
        // Clear discussion text
        discussionText.text = "";
        
        // Start the discussion
        StartDiscussion();
    }
    
    private void ClearAdvisorSlots()
    {
        foreach (Transform child in advisorsContainer)
        {
            Destroy(child.gameObject);
        }
        
        advisorSlots.Clear();
        activeAdvisors.Clear();
    }
    
    private void SetupAdvisors(List<AdvisorData> advisors)
    {
        activeAdvisors = advisors;
        
        foreach (var advisor in advisors)
        {
            GameObject slotObj = Instantiate(advisorSlotPrefab, advisorsContainer);
            
            // Set up advisor slot with name, portrait, etc.
            AdvisorSlotUI slotUI = slotObj.GetComponent<AdvisorSlotUI>();
            if (slotUI != null)
            {
                slotUI.SetAdvisor(advisor);
                slotUI.OnAdvisorSelected += ShowAdvisorInput;
            }
            
            advisorSlots[advisor.id] = slotObj;
        }
    }
    
    private void StartDiscussion()
    {
        // Clear existing recommendations
        foreach (Transform child in recommendationsContainer)
        {
            Destroy(child.gameObject);
        }
        
        activeRecommendations.Clear();
        
        // Generate recommendations from each advisor
        foreach (var advisor in activeAdvisors)
        {
            List<RecommendationData> recommendations = GenerateRecommendations(advisor);
            
            foreach (var recommendation in recommendations)
            {
                // Create recommendation UI
                GameObject recObj = Instantiate(recommendationPrefab, recommendationsContainer);
                RecommendationUI recUI = recObj.GetComponent<RecommendationUI>();
                
                if (recUI != null)
                {
                    recUI.SetRecommendation(recommendation);
                    recUI.OnRecommendationSelected += SelectRecommendation;
                }
                
                activeRecommendations.Add(recommendation);
            }
        }
    }
    
    private List<RecommendationData> GenerateRecommendations(AdvisorData advisor)
    {
        // This would be implemented based on your gameplay logic
        // For now, returning a placeholder recommendation
        List<RecommendationData> recommendations = new List<RecommendationData>();
        
        recommendations.Add(new RecommendationData
        {
            advisorId = advisor.id,
            text = $"{advisor.title} recommends focusing on {advisor.specialty}.",
            options = new List<OptionData>
            {
                new OptionData
                {
                    text = $"Invest in {advisor.specialty}",
                    outcomes = new List<EventOutcomeData>()
                }
            }
        });
        
        return recommendations;
    }
    
    private void ShowAdvisorInput(string advisorId)
    {
        // Find the advisor
        AdvisorData advisor = activeAdvisors.Find(a => a.id == advisorId);
        
        if (advisor != null)
        {
            // Add advisor's input to the discussion
            discussionText.text += $"\n\n<color=#{ColorUtility.ToHtmlStringRGB(advisor.textColor)}>{advisor.name}:</color> \"{advisor.defaultAdvice}\"";
        }
    }
    
    private void SelectRecommendation(RecommendationData recommendation)
    {
        // Show decision panel with options from this recommendation
        decisionPanel.SetActive(true);
        
        // Clear existing options
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create option buttons
        foreach (var option in recommendation.options)
        {
            GameObject optionObj = Instantiate(decisionOptionPrefab, optionsContainer);
            DecisionOptionUI optionUI = optionObj.GetComponent<DecisionOptionUI>();
            
            if (optionUI != null)
            {
                optionUI.SetOption(option);
                optionUI.OnOptionSelected += ImplementDecision;
            }
        }
    }
    
    private void ImplementDecision(OptionData option)
    {
        // Apply the outcomes of this option
        foreach (var outcome in option.outcomes)
        {
            // You would implement the actual outcome effects here
            Debug.Log($"Implementing outcome: {outcome.description}");
        }
        
        // Close the cabinet meeting
        decisionPanel.SetActive(false);
        cabinetPanel.SetActive(false);
        
        // You might want to trigger an event to inform other systems the meeting has ended
        // For example:
        // EventBus.Trigger("CabinetMeetingCompleted", option);
    }
}

// Additional classes needed for the Cabinet UI

[System.Serializable]
public class AdvisorData
{
    public string id;
    public string name;
    public string title;
    public string specialty;
    public Sprite portrait;
    public Color textColor = Color.white;
    [TextArea(2, 4)]
    public string defaultAdvice;
}

[System.Serializable]
public class RecommendationData
{
    public string advisorId;
    [TextArea(2, 4)]
    public string text;
    public List<OptionData> options = new List<OptionData>();
}

[System.Serializable]
public class OptionData
{
    [TextArea(1, 3)]
    public string text;
    public List<EventOutcomeData> outcomes = new List<EventOutcomeData>();
}

// Additional UI components needed

public class AdvisorSlotUI : MonoBehaviour
{
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI titleText;
    public event System.Action<string> OnAdvisorSelected;
    
    private AdvisorData advisor;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(HandleClick);
    }
    
    public void SetAdvisor(AdvisorData data)
    {
        advisor = data;
        
        if (portraitImage != null && advisor.portrait != null)
            portraitImage.sprite = advisor.portrait;
            
        if (nameText != null)
            nameText.text = advisor.name;
            
        if (titleText != null)
            titleText.text = advisor.title;
    }
    
    private void HandleClick()
    {
        if (advisor != null)
            OnAdvisorSelected?.Invoke(advisor.id);
    }
}

public class RecommendationUI : MonoBehaviour
{
    public TextMeshProUGUI recommendationText;
    public event System.Action<RecommendationData> OnRecommendationSelected;
    
    private RecommendationData recommendation;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(HandleClick);
    }
    
    public void SetRecommendation(RecommendationData data)
    {
        recommendation = data;
        
        if (recommendationText != null)
            recommendationText.text = data.text;
    }
    
    private void HandleClick()
    {
        if (recommendation != null)
            OnRecommendationSelected?.Invoke(recommendation);
    }
}

public class DecisionOptionUI : MonoBehaviour
{
    public TextMeshProUGUI optionText;
    public event System.Action<OptionData> OnOptionSelected;
    
    private OptionData option;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(HandleClick);
    }
    
    public void SetOption(OptionData data)
    {
        option = data;
        
        if (optionText != null)
            optionText.text = data.text;
    }
    
    private void HandleClick()
    {
        if (option != null)
            OnOptionSelected?.Invoke(option);
    }
}