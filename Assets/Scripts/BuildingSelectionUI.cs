using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class BuildingButton
    {
        public Button uiButton;
        public GameObject buildingPrefab;
        public string buildingName;
    }
    
    [SerializeField] private List<BuildingButton> buildingButtons = new List<BuildingButton>();
    [SerializeField] private GameManager gameManager;
    
    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("BuildingSelectionUI: GameManager reference not found!");
                return;
            }
        }
        
        // Set up event triggers for each button
        foreach (BuildingButton buildingButton in buildingButtons)
        {
            if (buildingButton.uiButton == null || buildingButton.buildingPrefab == null)
            {
                Debug.LogWarning("BuildingSelectionUI: Button or prefab reference is null!");
                continue;
            }
            
            // Add EventTrigger component if it doesn't exist
            EventTrigger trigger = buildingButton.uiButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = buildingButton.uiButton.gameObject.AddComponent<EventTrigger>();
            }
            
            // Clear existing entries
            trigger.triggers.Clear();
            
            // Add pointer down event
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            GameObject prefab = buildingButton.buildingPrefab; // Create local variable for closure
            pointerDownEntry.callback.AddListener((data) => { OnButtonDown(prefab); });
            trigger.triggers.Add(pointerDownEntry);
            
            // Add pointer up event
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnButtonUp(); });
            trigger.triggers.Add(pointerUpEntry);
            
            // Add pointer exit event (in case pointer leaves button while held)
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((data) => { OnButtonExit(prefab); });
            trigger.triggers.Add(pointerExitEntry);
            
            Debug.Log($"Set up EventTrigger for {buildingButton.buildingName} button");
        }
    }
    
    private void OnButtonDown(GameObject prefab)
    {
        // Call GameManager's OnBuildingButtonDown method
        gameManager.OnBuildingButtonDown(prefab);
    }
    
    private void OnButtonUp()
    {
        // Call GameManager's OnBuildingButtonUp method
        gameManager.OnBuildingButtonUp();
    }
    
    private void OnButtonExit(GameObject prefab)
    {
        // Optional: You can handle the case where the pointer exits the button while held
        // This could either cancel the operation or start the drag immediately
        // For now, we'll do nothing and let the drag threshold logic handle it
    }
} 