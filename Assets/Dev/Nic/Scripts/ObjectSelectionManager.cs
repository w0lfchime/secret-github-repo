using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectSelectionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Camera to use for raycasting (leave empty to use main camera)")]
    [SerializeField] private Camera raycastCamera;
    
    [Tooltip("Layer mask for selectable objects")]
    [SerializeField] private LayerMask selectableLayer;
    
    [Tooltip("Maximum raycast distance")]
    [SerializeField] private float maxRaycastDistance = 100f;
    
    private SelectableObject currentlyHovered;
    private SelectableObject currentlySelected;
    private GameData gameData;

    public GameObject startButton;

    void Start()
    {
        gameData = GameObject.Find("GameData").GetComponent<GameData>();
        // Use main camera if not assigned
        if (raycastCamera == null)
        {
            raycastCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleHoverDetection();
        HandleSelection();
    }

    private void HandleHoverDetection()
    {
        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Check if we hit a selectable object
        if (Physics.Raycast(ray, out hit, maxRaycastDistance, selectableLayer))
        {
            SelectableObject hoveredObject = hit.collider.GetComponent<SelectableObject>();
            
            if (hoveredObject != null)
            {
                // If we're hovering over a new object
                if (hoveredObject != currentlyHovered)
                {
                    // Exit previous hover
                    if (currentlyHovered != null)
                    {
                        currentlyHovered.OnHoverExit();
                    }
                    
                    // Enter new hover
                    currentlyHovered = hoveredObject;
                    currentlyHovered.OnHoverEnter();
                }
            }
        }
        else
        {
            // Not hovering over anything, exit current hover
            if (currentlyHovered != null)
            {
                currentlyHovered.OnHoverExit();
                currentlyHovered = null;
            }
        }
    }

    private void HandleSelection()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // If we're hovering over an object, select it
            if (currentlyHovered != null)
            {
                // Deselect previously selected object
                if (currentlySelected != null && currentlySelected != currentlyHovered)
                {
                    currentlySelected.Deselect();
                }
                
                // Select the new object
                currentlySelected = currentlyHovered;
                currentlySelected.Select();

                gameData.charNum = currentlySelected.playerNUm;

                startButton.SetActive(true);
            }
        }
    }

    //Method to get currently selected object
    public SelectableObject GetSelectedObject()
    {
        return currentlySelected;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Cutscene");
    }

    
}
