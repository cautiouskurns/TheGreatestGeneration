using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a selectable tab in a tab group
/// </summary>
public class UITabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TabGroup tabGroup;
    private Image background;
    
    private void Awake()
    {
        background = GetComponent<Image>();
    }
    
    public void SetTabGroup(TabGroup newTabGroup)
    {
        tabGroup = newTabGroup;
    }
    
    public void SetColor(Color color)
    {
        if (background != null)
        {
            background.color = color;
        }
    }
    
    public void SetState(bool isActive)
    {
        if (background != null)
        {
            background.color = isActive ? tabGroup.tabActive : tabGroup.tabIdle;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tabGroup != null)
        {
            tabGroup.OnTabEnter(this);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tabGroup != null)
        {
            tabGroup.OnTabExit(this);
        }
    }
}
