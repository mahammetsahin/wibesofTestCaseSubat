using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : PlaceableObject
{
    // Buildings can be more than 1x1 cell
    // Size is already defined in the base class
    
    // Additional building-specific properties can be added here
    [Header("Building Properties")]
    [SerializeField] private string buildingName;
    [SerializeField] private string buildingDescription;
    
    public override void OnPlaced()
    {
        base.OnPlaced();
        // Additional building-specific placement logic
    }
}
