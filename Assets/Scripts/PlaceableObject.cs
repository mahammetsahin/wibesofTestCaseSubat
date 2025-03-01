using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableObject : MonoBehaviour
{
    [Header("Grid Properties")]
    [SerializeField] private Vector2Int size = Vector2Int.one;
    
    [Header("Visual Properties")]
    [SerializeField] private GameObject visualObject;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    
    private Renderer objectRenderer;
    
    public Vector2Int Size => size;
    public Vector2Int GridPosition { get; set; }
    
    protected virtual void Awake()
    {
        if (visualObject != null)
        {
            objectRenderer = visualObject.GetComponent<Renderer>();
        }
    }
    
    public void SetValidPlacement(bool isValid)
    {
        if (objectRenderer != null && defaultMaterial != null && invalidPlacementMaterial != null)
        {
            objectRenderer.material = isValid ? defaultMaterial : invalidPlacementMaterial;
        }
    }
    
    // Method to be called when object is placed on the grid
    public virtual void OnPlaced()
    {
        // Base implementation does nothing
    }
    
    // Method to be called when object is removed from the grid
    public virtual void OnRemoved()
    {
        // Base implementation does nothing
    }
} 