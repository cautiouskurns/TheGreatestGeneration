using UnityEngine;
using UnityEngine.UI;

public class DialogueSystemTester : MonoBehaviour
{
    public Button testSimpleButton;
    public Button testMultiLineButton;
    public Button testChoicesButton;
    
    void Start()
    {
        if (testSimpleButton != null)
            testSimpleButton.onClick.AddListener(TestSimpleDialogue);
            
        if (testMultiLineButton != null)
            testMultiLineButton.onClick.AddListener(TestMultiLineDialogue);
            
        if (testChoicesButton != null)
            testChoicesButton.onClick.AddListener(TestChoicesDialogue);
    }
    
    void TestSimpleDialogue()
    {
        EventDialogueManager.ShowSimpleDialogue(
            "Economic Update", 
            "The economy is currently in the Expansion phase. Markets are optimistic."
        );
    }
    
    void TestMultiLineDialogue()
    {
        string[] lines = new string[]
        {
            "Greetings, Your Excellency. I am Lord Blackwood, your Economic Minister.",
            "Our nation's financial situation requires your immediate attention.",
            "The steel shortage is impacting our industrial capacity, and the merchants are demanding action.",
            "What would you like me to focus on in the next quarter?"
        };
        
        EventDialogueManager.ShowMultiLineDialogue("Cabinet Meeting", lines);
    }
    
    void TestChoicesDialogue()
    {
        string message = "A delegation from the neighboring country of Westoria has arrived. They propose a new trade agreement that would increase our iron imports but require us to export more grain.";
        
        string[] choices = new string[]
        {
            "Accept the trade agreement",
            "Decline politely and maintain current trade levels",
            "Counter-offer with a modified proposal",
            "Delay the decision and gather more information"
        };
        
        EventDialogueManager.ShowChoiceDialogue("Diplomatic Envoy", message, choices);
    }
}