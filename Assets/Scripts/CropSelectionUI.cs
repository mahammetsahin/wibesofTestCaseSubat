using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CropSelectionUI : MonoBehaviour
{
    [System.Serializable]
    public class CropButton
    {
        public Button uiButton;
        public string cropType;
        public GameObject cropPrefab; // Reference to the crop prefab for drag-to-instantiate
    }
    
    [SerializeField] private List<CropButton> cropButtons = new List<CropButton>();
    [SerializeField] private Button cancelButton;
    
    private CropField targetCropField;
    private GameManager gameManager;
    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        Debug.Log($"CropSelectionUI Awake - Found GameManager: {gameManager != null}");
        
        // Set up click events for crop buttons
        for (int i = 0; i < cropButtons.Count; i++)
        {
            CropButton cropButton = cropButtons[i];
            if (cropButton.uiButton == null)
            {
                Debug.LogWarning("CropSelectionUI: Button reference is null!");
                continue;
            }
            
            // Clear existing listeners
            cropButton.uiButton.onClick.RemoveAllListeners();
            
            // Add simple click listener for direct planting
            string cropType = cropButton.cropType; // Create local variable for closure
            int buttonIndex = i; // Store the index for use in the lambda
            cropButton.uiButton.onClick.AddListener(() => { 
                Debug.Log($"Crop button clicked: {cropType} at index {buttonIndex}");
                PlantCrop(cropType, buttonIndex); 
            });
            
            Debug.Log($"Set up click listener for {cropButton.cropType} button at index {i}");
        }
        
        // Set up cancel button
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                Debug.Log("Cancel button clicked");
                CloseUI();
            });
            Debug.Log("Set up cancel button");
        }
    }
    
    public void SetTargetCropField(CropField cropField)
    {
        targetCropField = cropField;
        Debug.Log($"Target crop field set: {cropField.name}");
    }
    
    private void PlantCrop(string cropType, int cropIndex)
    {
        Debug.Log($"Planting {cropType} (index {cropIndex}) in target field");
        
        if (targetCropField != null && gameManager != null)
        {
            gameManager.PlantCrop(targetCropField, cropType, cropIndex);
            Debug.Log($"Successfully planted {cropType} with index {cropIndex}");
        }
        else
        {
            Debug.LogError($"Failed to plant crop: targetCropField={targetCropField}, gameManager={gameManager}");
        }
        
        CloseUI();
    }
    
    private void CloseUI()
    {
        Debug.Log("Closing crop selection UI");
        gameObject.SetActive(false);
    }
} 