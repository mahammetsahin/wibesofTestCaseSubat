using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropField : PlaceableObject
{
    [Header("Crop Field Properties")]
    [SerializeField] private Texture2D emptyCropFieldTexture;
    [SerializeField] private Texture2D[] plantedCropTextures; // Index 0 for first crop type, 1 for second
    [SerializeField] private Texture2D[] grownCropTextures;   // Index 0 for first crop type, 1 for second
    
    [Header("Crop Growth Settings")]
    [SerializeField] private float[] growthTimes = { 5f, 10f }; // Growth times in seconds for each crop type
    
    private Renderer fieldRenderer;
    private string plantedCropType;
    private int plantedCropIndex = -1;
    private DateTime plantTime;
    private bool hasPlantedCrop = false;
    private bool isCropGrown = false;
    
    // For drag-to-instantiate functionality
    private string pendingCropType;
    
    public bool HasCrop => hasPlantedCrop;
    public bool IsGrown => isCropGrown;
    public string PlantedCropType => plantedCropType;
    
    protected override void Awake()
    {
        base.Awake();
        fieldRenderer = GetComponentInChildren<Renderer>();
        
        // Set initial texture to empty field
        if (fieldRenderer != null && emptyCropFieldTexture != null)
        {
            fieldRenderer.material.mainTexture = emptyCropFieldTexture;
        }
    }
    
    private void Update()
    {
        // Check if crop has grown
        if (hasPlantedCrop && !isCropGrown && plantedCropIndex >= 0)
        {
            TimeSpan elapsed = DateTime.Now - plantTime;
            if (elapsed.TotalSeconds >= growthTimes[plantedCropIndex])
            {
                SetCropGrown();
            }
        }
    }
    
    // Methods for pending crop type (used by drag-to-instantiate)
    public void SetPendingCropType(string cropType)
    {
        pendingCropType = cropType;
    }
    
    public bool HasPendingCropType()
    {
        return !string.IsNullOrEmpty(pendingCropType);
    }
    
    public string GetPendingCropType()
    {
        return pendingCropType;
    }
    
    public void ClearPendingCropType()
    {
        pendingCropType = null;
    }
    
    public void PlantCrop(string cropType, int cropIndex = -1)
    {
        // Determine crop index based on type if not provided
        if (cropIndex < 0)
        {
            // Fallback to the old method for backward compatibility
            cropIndex = cropType == "FastCrop" ? 0 : 1;
            Debug.Log($"Using fallback crop index determination: {cropType} -> index {cropIndex}");
        }
        else
        {
            Debug.Log($"Using provided crop index: {cropIndex} for {cropType}");
        }
        
        // Make sure the index is valid
        if (cropIndex >= growthTimes.Length)
        {
            Debug.LogWarning($"Crop index {cropIndex} is out of range! Using index 0 instead.");
            cropIndex = 0;
        }
        
        // Store planting information
        plantedCropType = cropType;
        plantedCropIndex = cropIndex;
        plantTime = DateTime.Now;
        hasPlantedCrop = true;
        isCropGrown = false;
        
        // Update visual
        if (fieldRenderer != null && plantedCropTextures.Length > cropIndex)
        {
            fieldRenderer.material.mainTexture = plantedCropTextures[cropIndex];
        }
    }
    
    private void SetCropGrown()
    {
        isCropGrown = true;
        
        // Update visual to grown crop
        if (fieldRenderer != null && grownCropTextures.Length > plantedCropIndex)
        {
            fieldRenderer.material.mainTexture = grownCropTextures[plantedCropIndex];
        }
    }
    
    public void HarvestCrop()
    {
        // Reset crop state
        hasPlantedCrop = false;
        isCropGrown = false;
        plantedCropIndex = -1;
        plantedCropType = null;
        
        // Reset visual to empty field
        if (fieldRenderer != null && emptyCropFieldTexture != null)
        {
            fieldRenderer.material.mainTexture = emptyCropFieldTexture;
        }
    }
    
    public float GetTimeUntilGrown()
    {
        if (!hasPlantedCrop || isCropGrown || plantedCropIndex < 0)
            return 0f;
            
        TimeSpan elapsed = DateTime.Now - plantTime;
        float totalGrowthTime = growthTimes[plantedCropIndex];
        float timeLeft = Mathf.Max(0, totalGrowthTime - (float)elapsed.TotalSeconds);
        
        return timeLeft;
    }
}
