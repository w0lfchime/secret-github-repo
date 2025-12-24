using UnityEngine;
using TMPro;

public class SelectableObject : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Text to display above this object when hovered")]
    [SerializeField] private TextMeshPro hoverText;
    
    [Tooltip("Optional: Direct reference to outline component")]
    [SerializeField] private Outline outlineComponent;
    
    [Header("Outline Settings")]
    [Tooltip("Color when hovering (not selected)")]
    [SerializeField] private Color hoverColor = Color.yellow;
    
    [Tooltip("Color when selected")]
    [SerializeField] private Color selectedColor = Color.green;
    
    [Tooltip("Outline width")]
    [SerializeField] private float outlineWidth = 5f;
    
    private bool isSelected = false;
    private bool isHovered = false;

    void Start()
    {
        // Try to get Outline component if not assigned
        if (outlineComponent == null)
        {
            outlineComponent = GetComponent<Outline>();
            
            // If no Outline component exists, add one
            if (outlineComponent == null)
            {
                outlineComponent = gameObject.AddComponent<Outline>();
            }
        }
        
        // Configure outline
        outlineComponent.OutlineWidth = outlineWidth;
        outlineComponent.enabled = false;
        
        // Hide text initially
        if (hoverText != null)
        {
            hoverText.gameObject.SetActive(false);
        }
    }

    public void OnHoverEnter()
    {
        isHovered = true;
        
        // Show text
        if (hoverText != null)
        {
            hoverText.gameObject.SetActive(true);
        }
        
        // Show outline if not already selected
        if (!isSelected && outlineComponent != null)
        {
            outlineComponent.OutlineColor = hoverColor;
            outlineComponent.enabled = true;
        }
    }

    public void OnHoverExit()
    {
        isHovered = false;
        
        // Hide text
        if (hoverText != null)
        {
            hoverText.gameObject.SetActive(false);
        }
        
        // Hide outline if not selected
        if (!isSelected && outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }

    public void Select()
    {
        isSelected = true;
        
        // Enable outline with selected color
        if (outlineComponent != null)
        {
            outlineComponent.OutlineColor = selectedColor;
            outlineComponent.enabled = true;
        }
    }

    public void Deselect()
    {
        isSelected = false;
        
        // If still hovering, keep hover outline, otherwise disable
        if (isHovered && outlineComponent != null)
        {
            outlineComponent.OutlineColor = hoverColor;
            outlineComponent.enabled = true;
        }
        else if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
