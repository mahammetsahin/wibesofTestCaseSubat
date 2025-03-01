using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Transform gridPlane;
    
    [Header("Placement Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float buildingHoverHeight = 0.5f; // Height buildings hover permanently
    [SerializeField] private float dragHoverHeight = 0.5f; // Height objects hover while being dragged
    
    [Header("UI References")]
    [SerializeField] private GameObject cropSelectionUI;
    [SerializeField] private GameObject cropTimerUI;
    [SerializeField] private TextMeshProUGUI cropTimerText;
    [SerializeField] private GameObject cropCollectedUI;
    [SerializeField] private TextMeshProUGUI cropCollectedText;
    
    // Dictionary to store grid cells
    private Dictionary<Vector2Int, GridCell> gridCells = new Dictionary<Vector2Int, GridCell>();
    
    // Object placement variables
    private PlaceableObject selectedObject;
    private Vector2Int startPosition;
    private bool hasStartPosition = false;
    private bool isDragging = false;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private Vector3 dragOffset;
    private Coroutine holdCoroutine;
    
    // Track UI button press state
    private GameObject pendingPrefab = null;
    private Vector2 buttonPressPosition;
    private float dragThreshold = 20f; // Pixels the pointer needs to move to trigger instantiation
    
    // Add a new variable to track crop field interactions
    private CropField selectedCropField = null;
    
    // Add a dictionary to track crop timer coroutines for each crop field
    private Dictionary<CropField, Coroutine> cropTimerCoroutines = new Dictionary<CropField, Coroutine>();
    
    private void Start()
    {
        InitializeGrid();
        
        // Make sure the grid has a collider
        if (gridPlane != null)
        {
            // Ensure we have a collider
            BoxCollider gridCollider = gridPlane.GetComponent<BoxCollider>();
            if (gridCollider == null)
            {
                gridCollider = gridPlane.gameObject.AddComponent<BoxCollider>();
                Debug.Log("Added BoxCollider to grid plane");
            }
            
            // The grid plane's local scale is set to match the grid dimensions in InitializeGrid
            // The collider should be sized to match the plane's actual dimensions in world space
            
            // For a default Unity plane (10x10 units), we need to adjust the collider size
            // based on the plane's scale and our grid dimensions
            float gridWorldWidth = gridWidth * cellSize;
            float gridWorldHeight = gridHeight * cellSize;
            
            // If using a default Unity plane (which is 10x10 units)
            Vector3 planeScale = gridPlane.localScale;
            Debug.Log($"Grid plane scale: {planeScale}");
            
            // Set collider size based on whether we're using a default plane or a custom mesh
            if (gridPlane.GetComponent<MeshFilter>()?.sharedMesh?.name == "Plane")
            {
                // Default Unity plane is 10x10 units, so scale accordingly
                gridCollider.size = new Vector3(10f, 0.1f, 10f);
                gridCollider.center = new Vector3(0, 0, 0);
                Debug.Log("Using default Unity plane (10x10) - adjusted collider size accordingly");
            }
            else
            {
                // Custom mesh - assume size matches local scale
                gridCollider.size = new Vector3(1f, 0.1f, 1f);
                gridCollider.center = new Vector3(0, 0, 0);
                Debug.Log("Using custom mesh for grid - set collider to match mesh size");
            }
            
            Debug.Log($"Set grid collider size to {gridCollider.size} and center to {gridCollider.center}");
            
            // Make sure the grid plane has the correct layer
            if (gridLayerMask.value > 0 && ((1 << gridPlane.gameObject.layer) & gridLayerMask.value) == 0)
            {
                Debug.LogError($"Grid plane layer ({LayerMask.LayerToName(gridPlane.gameObject.layer)}) is not included in the gridLayerMask! Raycasts won't hit it.");
                
                // Try to find a layer that is in the mask
                for (int i = 0; i < 32; i++)
                {
                    if (((1 << i) & gridLayerMask.value) != 0)
                    {
                        Debug.Log($"Setting grid plane layer to {LayerMask.LayerToName(i)} to match gridLayerMask");
                        gridPlane.gameObject.layer = i;
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Grid plane reference is missing! Please assign it in the inspector.");
        }
        
        // Log camera and layer mask
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log("Using Camera.main as mainCamera reference");
        }
        
        // If gridLayerMask is not set, default to everything
        if (gridLayerMask.value == 0)
        {
            gridLayerMask = Physics.DefaultRaycastLayers;
            Debug.LogWarning("gridLayerMask was not set! Defaulting to Physics.DefaultRaycastLayers");
        }
        
        Debug.Log($"Grid Layer Mask: {gridLayerMask.value} (Layer names: {LayerMaskToString(gridLayerMask)})");
        
    }
    
    private void Update()
    {
        // Handle input directly instead of using EventTrigger
        HandleDirectInput();
        
        // If we have a selected object from UI that's following the pointer,
        // update its position every frame
        if (isDragging && selectedObject != null && !hasStartPosition)
        {
            UpdateObjectPositionUnderPointer();
        }
        
        // Debug visualization in game view
        if (gridPlane != null)
        {
            // Draw a wireframe box around the grid
            float gridWorldWidth = gridWidth * cellSize;
            float gridWorldHeight = gridHeight * cellSize;
            Vector3 center = new Vector3(gridWorldWidth/2, 0, gridWorldHeight/2);
            Vector3 size = new Vector3(gridWorldWidth, 0.1f, gridWorldHeight);
            Debug.DrawLine(center + new Vector3(-size.x/2, 0, -size.z/2), center + new Vector3(size.x/2, 0, -size.z/2), Color.blue);
            Debug.DrawLine(center + new Vector3(size.x/2, 0, -size.z/2), center + new Vector3(size.x/2, 0, size.z/2), Color.blue);
            Debug.DrawLine(center + new Vector3(size.x/2, 0, size.z/2), center + new Vector3(-size.x/2, 0, size.z/2), Color.blue);
            Debug.DrawLine(center + new Vector3(-size.x/2, 0, size.z/2), center + new Vector3(-size.x/2, 0, -size.z/2), Color.blue);
        }
    }
    
    private void HandleDirectInput()
    {
        // Only process if we have a camera
        if (mainCamera == null)
        {
            Debug.LogError("Main camera reference is null!");
            return;
        }
        
        // Get pointer position (works for both mouse and touch)
        Vector3 pointerPosition = Input.mousePosition;
        bool isPointerDown = Input.GetMouseButtonDown(0);
        bool isPointerHeld = Input.GetMouseButton(0);
        bool isPointerUp = Input.GetMouseButtonUp(0);
        
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerPosition = touch.position;
            
            // Convert touch phase to pointer events
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isPointerDown = true;
                    isPointerHeld = true;
                    isPointerUp = false;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    isPointerDown = false;
                    isPointerHeld = true;
                    isPointerUp = false;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isPointerDown = false;
                    isPointerHeld = false;
                    isPointerUp = true;
                    break;
            }
        }
        
        // Check for pending prefab instantiation (drag from UI button)
        if (pendingPrefab != null && isPointerHeld)
        {
            // Calculate distance moved since button press
            float dragDistance = Vector2.Distance(buttonPressPosition, pointerPosition);
            
            // If dragged far enough, instantiate the object
            if (dragDistance > dragThreshold)
            {
                Debug.Log($"Drag threshold reached ({dragDistance:F1} > {dragThreshold}), instantiating {pendingPrefab.name}");
                SelectObjectFromUI(pendingPrefab);
                pendingPrefab = null; // Clear pending prefab to avoid multiple instantiations
            }
        }
        
        // Cast a ray from the pointer position
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        bool hitGrid = Physics.Raycast(ray, out RaycastHit hit, 100f, gridLayerMask);
        
        // Debug visualization of the ray
        Debug.DrawRay(ray.origin, ray.direction * 100f, hitGrid ? Color.green : Color.red);
        
        // If we're dragging an object, update its position
        if (isDragging && selectedObject != null)
        {
            // If we're not hitting the grid but we're dragging, use a plane at y=0
            if (!hitGrid)
            {
                Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);
                if (horizontalPlane.Raycast(ray, out float distance))
                {
                    hit.point = ray.GetPoint(distance);
                    hitGrid = true;
                }
            }
            
            if (hitGrid)
            {
                // Calculate the center offset based on the object's size
                Vector2Int size = selectedObject.Size;
                Vector3 centerOffset = new Vector3(size.x * cellSize / 2, 0, size.y * cellSize / 2);
                
                // Get the hit point for positioning
                Vector3 hitPoint = hit.point;
                
                // For grid cell validation, we need to get the cell position where the pivot (corner) would be
                Vector3 pivotPoint = hitPoint - centerOffset;
                Vector2Int cellPos = WorldToGridPosition(pivotPoint);
                
                // For visual positioning, center the object on the pointer
                Vector3 targetPosition = hitPoint - centerOffset;
                
                if (hasStartPosition)
                {
                    // If we're moving an existing object, we've already calculated the offset
                    // No need to add dragOffset since we want to center on the pointer
                }
                
                // Apply hover height while dragging for all objects
                targetPosition.y = dragHoverHeight;
                
                // Update object position to follow pointer
                selectedObject.transform.position = targetPosition;
                
                // Check placement validity based on the grid cell
                bool canPlace = CanPlaceObjectAt(cellPos, selectedObject);
                
                // Update visual feedback
                selectedObject.SetValidPlacement(canPlace);
            }
        }
        
        // POINTER DOWN - Initial selection
        if (isPointerDown)
        {
            Debug.Log($"Pointer DOWN at {pointerPosition}, hitGrid: {hitGrid}");
            
            if (hitGrid)
            {
                // Get the cell position
                Vector2Int cellPos = WorldToGridPosition(hit.point);
                gridCells.TryGetValue(cellPos, out GridCell cell);
                // Check if we hit an existing object
                if (cell != null && !cell.IsEmpty)
                {
                    GameObject hitObject = cell.OccupyingObject;
                    if (hitObject != null)
                    {
                        PlaceableObject placeableObj = hitObject.GetComponent<PlaceableObject>();
                        
                        if (placeableObj != null)
                        {
                            Debug.Log($"Selected object: {hitObject.name}, type: {placeableObj.GetType().Name}");
                            
                            // If it's a crop field, store reference but don't show UI yet
                            if (placeableObj is CropField cropField)
                            {
                                selectedCropField = cropField;
                                Debug.Log($"Selected crop field at {cellPos}");
                            }
                            else if (placeableObj is Building)
                            {
                                // Start tracking for potential drag
                                selectedObject = placeableObj;
                                startPosition = placeableObj.GridPosition;
                                hasStartPosition = true;
                                isHolding = true;
                                holdTimer = 0f;
                                
                                Debug.Log($"Started holding building at {startPosition}");
                            }
                        }
                    }
                }
                else if (selectedObject != null && isDragging)
                {
                    // Try to place the selected object from UI
                    TryPlaceObject(cellPos);
                }
            }
        }
        
        // POINTER HOLD - Track hold duration for buildings
        if (isPointerHeld && isHolding && !isDragging && selectedObject != null)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= holdDuration)
            {
                Debug.Log($"Hold duration reached after {holdTimer:F2} seconds - starting drag");
                
                // Start dragging
                isDragging = true;
                
                // Store the object's current position before removing from grid
                Vector3 objectPosition = selectedObject.transform.position;
                
                // Remove from grid temporarily
                RemoveObjectFromGrid(selectedObject);
                
                // Calculate drag offset (difference between object center and mouse hit point)
                if (hitGrid)
                {
                    dragOffset = objectPosition - hit.point;
                    Debug.Log($"Drag offset set to {dragOffset}");
                }
                else
                {
                    dragOffset = Vector3.zero;
                }
            }
        }
        
        // POINTER UP - Place the object or cancel drag
        if (isPointerUp)
        {
            Debug.Log($"Pointer UP, isDragging: {isDragging}, isHolding: {isHolding}");
            
            // Handle crop field interaction on pointer up
            if (selectedCropField != null)
            {
                Debug.Log("Click on crop field - showing crop UI");
                HandleCropFieldClick(selectedCropField);
                selectedCropField = null;
            }
            
            if (isDragging && selectedObject != null)
            {
                // Check if we're over the grid
                bool isOverGrid = false;
                Vector2Int cellPos = Vector2Int.zero;
                
                // Cast a ray to check if we're over the grid
                Ray placementRay = mainCamera.ScreenPointToRay(pointerPosition);
                if (Physics.Raycast(placementRay, out RaycastHit placementHit, 100f, gridLayerMask))
                {
                    isOverGrid = true;
                    cellPos = WorldToGridPosition(placementHit.point);
                }
                else
                {
                    // Try with a horizontal plane as fallback
                    Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);
                    if (horizontalPlane.Raycast(placementRay, out float distance))
                    {
                        Vector3 hitPoint = placementRay.GetPoint(distance);
                        cellPos = WorldToGridPosition(hitPoint);
                        
                        // Check if the point is within grid bounds
                        isOverGrid = IsCellValid(cellPos);
                    }
                }
                
                if (!isOverGrid && !hasStartPosition)
                {
                    // Destroy objects that are dragged completely off the grid
                    Debug.Log($"Destroying object {selectedObject.name} as it's off the grid");
                    Destroy(selectedObject.gameObject);
                }
                else
                {
                    // Try to place the dragged object
                    bool placed = TryPlaceObject(cellPos);
                    
                    if (!placed)
                    {
                        if (hasStartPosition)
                        {
                            // Return to original position if it's an existing object being moved
                            PlaceObjectOnGrid(selectedObject, startPosition);
                            Debug.Log("Returning object to original position");
                        }
                        else
                        {
                            // Destroy the object if it's a new object that can't be placed
                            Debug.Log($"Destroying object {selectedObject.name} as it can't be placed");
                            Destroy(selectedObject.gameObject);
                        }
                    }
                    
                    // Reset visual feedback if the object wasn't destroyed
                    if (selectedObject != null)
                    {
                        selectedObject.SetValidPlacement(true);
                    }
                }
                
                // Reset dragging state
                isDragging = false;
                selectedObject = null;
            }
            else if (isHolding && selectedObject != null)
            {
                // Just a click, not a drag - do nothing with the building
                Debug.Log("Released before drag started - not moving building");
            }
            
            // Reset all state variables
            isDragging = false;
            isHolding = false;
            hasStartPosition = false;
            holdTimer = 0f;
            selectedObject = null;
            selectedCropField = null;
        }
    }
    
    #region Grid System
    
    private void InitializeGrid()
    {
        // Resize grid plane
        if (gridPlane != null)
        {
            // Set the grid plane scale to match the grid dimensions
            gridPlane.localScale = new Vector3(gridWidth * cellSize, 1, gridHeight * cellSize);
            Debug.Log($"Set grid plane scale to {gridPlane.localScale}");
            
            // Ensure the grid plane is at the origin
            if (gridPlane.position != Vector3.zero)
            {
                Debug.LogWarning($"Grid plane is not at origin! Current position: {gridPlane.position}. This may cause issues with grid calculations.");
            }
        }
        else
        {
            Debug.LogError("Grid plane reference is missing!");
        }
        
        // Create grid cells
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector2Int cellPos = new Vector2Int(x, z);
                gridCells[cellPos] = new GridCell(cellPos);
            }
        }
        
        Debug.Log($"Grid initialized with {gridWidth}x{gridHeight} cells, cell size: {cellSize}");
    }
    
    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        // Convert world position to grid position using floor values
        // This ensures positions like 1.9f and 1.2f both map to 1
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int z = Mathf.FloorToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, z);
    }
    
    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        // Convert grid position to world position (using corner of cell, not center)
        float x = gridPosition.x * cellSize;
        float z = gridPosition.y * cellSize;
        return new Vector3(x, 0, z);
    }
    
    private bool IsCellValid(Vector2Int cellPos)
    {
        return cellPos.x >= 0 && cellPos.x < gridWidth && 
               cellPos.y >= 0 && cellPos.y < gridHeight;
    }
    
    private bool CanPlaceObjectAt(Vector2Int baseCell, PlaceableObject obj)
    {
        // Check if all required cells are valid and empty
        Vector2Int size = obj.Size;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = new Vector2Int(baseCell.x + x, baseCell.y + y);
                
                // Check if cell is within grid bounds
                if (!IsCellValid(checkPos))
                    return false;
                
                // Check if cell is empty or occupied by the same object
                if (gridCells.TryGetValue(checkPos, out GridCell cell) && 
                    !cell.IsEmpty && cell.OccupyingObject != obj.gameObject)
                    return false;
            }
        }
        
        return true;
    }
    
    private List<Vector2Int> GetOccupiedCells(Vector2Int baseCell, Vector2Int size)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells.Add(new Vector2Int(baseCell.x + x, baseCell.y + y));
            }
        }
        
        return cells;
    }
    
    private void PlaceObjectOnGrid(PlaceableObject obj, Vector2Int baseCell)
    {
        // Set object position to grid using corner-based positioning (pivot at corner)
        Vector3 worldPos = GridToWorldPosition(baseCell);
        
        // Apply hover height only for buildings when placed
        if (obj is Building)
        {
            worldPos.y = buildingHoverHeight;
        }
        else
        {
            // All other objects go to ground level when placed
            worldPos.y = 0f;
        }
        
        // Snap the object to the grid position (corner-based)
        obj.transform.position = worldPos;
        
        // Mark cells as occupied
        foreach (Vector2Int cell in GetOccupiedCells(baseCell, obj.Size))
        {
            if (gridCells.TryGetValue(cell, out GridCell gridCell))
            {
                gridCell.SetObject(obj.gameObject);
            }
        }
        
        // Set object's grid position
        obj.GridPosition = baseCell;
        
        // Call the OnPlaced event
        obj.OnPlaced();
        
        // Log the final position for debugging
        Debug.Log($"Object {obj.name} placed at grid position {baseCell}, world position {worldPos}, actual position {obj.transform.position}");
    }
    
    private void RemoveObjectFromGrid(PlaceableObject obj)
    {
        if (obj == null)
            return;
            
        // Clear cells
        foreach (Vector2Int cell in GetOccupiedCells(obj.GridPosition, obj.Size))
        {
            if (gridCells.TryGetValue(cell, out GridCell gridCell) && 
                gridCell.OccupyingObject == obj.gameObject)
            {
                gridCell.ClearObject();
            }
        }
        
        // Call the OnRemoved event
        obj.OnRemoved();
    }
    
    #endregion
    
    #region Object Placement
    
    private bool TryPlaceObject(Vector2Int cellPos)
    {
        if (selectedObject == null)
            return false;
            
        if (CanPlaceObjectAt(cellPos, selectedObject))
        {
            PlaceObjectOnGrid(selectedObject, cellPos);
            return true;
        }
        
        return false;
    }
    
    // Called by UI buttons on pointer down
    public void OnBuildingButtonDown(GameObject prefab)
    {
        pendingPrefab = prefab;
        buttonPressPosition = Input.mousePosition;
        if (Input.touchCount > 0)
        {
            buttonPressPosition = Input.GetTouch(0).position;
        }
        Debug.Log($"Button down for {prefab.name}, waiting for drag");
    }
    
    // Called by UI buttons on pointer up (to cancel if no drag occurred)
    public void OnBuildingButtonUp()
    {
        Debug.Log("Button up, canceling pending prefab");
        pendingPrefab = null;
    }
    
    // This method is now only used internally after drag detection
    private void SelectObjectFromUI(GameObject prefab)
    {
        // Instantiate the selected object
        GameObject obj = Instantiate(prefab);
        selectedObject = obj.GetComponent<PlaceableObject>();
        
        if (selectedObject == null)
        {
            Destroy(obj);
            return;
        }
        
        // Start dragging the new object
        isDragging = true;
        hasStartPosition = false;
        
        // Position the object at a valid position under the pointer immediately
        UpdateObjectPositionUnderPointer();
        
        Debug.Log($"Instantiated {prefab.name} from UI, now following pointer");
    }
    
    // New method to update object position under pointer
    private void UpdateObjectPositionUnderPointer()
    {
        if (selectedObject == null)
            return;
            
        // Get pointer position (works for both mouse and touch)
        Vector3 pointerPosition = Input.mousePosition;
        if (Input.touchCount > 0)
        {
            pointerPosition = Input.GetTouch(0).position;
        }
        
        // Cast a ray from the pointer position
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        bool hitGrid = false;
        Vector3 targetPosition = Vector3.zero;
        Vector2Int cellPos = Vector2Int.zero;
        
        // If we hit the grid, position the object under the pointer
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, gridLayerMask))
        {
            hitGrid = true;
            
            // Calculate the center offset based on the object's size
            Vector2Int size = selectedObject.Size;
            Vector3 centerOffset = new Vector3(size.x * cellSize / 2, 0, size.y * cellSize / 2);
            
            // Get the hit point for positioning
            Vector3 hitPoint = hit.point;
            
            // For grid cell validation, we need to get the cell position where the pivot (corner) would be
            // This is the hit point minus the center offset
            Vector3 pivotPoint = hitPoint - centerOffset;
            cellPos = WorldToGridPosition(pivotPoint);
            
            // For visual positioning, center the object on the pointer
            targetPosition = hitPoint - centerOffset;
            
            // Apply hover height while dragging
            targetPosition.y = dragHoverHeight;
            
            Debug.Log($"Hit grid at {hit.point}, pivot point: {pivotPoint}, cell: {cellPos}, target position: {targetPosition}");
        }
        else
        {
            // If we don't hit the grid, use a plane at y=0
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);
            if (horizontalPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                
                // Calculate the center offset based on the object's size
                Vector2Int size = selectedObject.Size;
                Vector3 centerOffset = new Vector3(size.x * cellSize / 2, 0, size.y * cellSize / 2);
                
                // For grid cell validation, we need to get the cell position where the pivot (corner) would be
                Vector3 pivotPoint = hitPoint - centerOffset;
                cellPos = WorldToGridPosition(pivotPoint);
                
                // For visual positioning, center the object on the pointer
                targetPosition = hitPoint - centerOffset;
                
                // Check if the point is within grid bounds
                if (IsCellValid(cellPos))
                {
                    hitGrid = true;
                    targetPosition.y = dragHoverHeight;
                    
                    Debug.Log($"Hit horizontal plane at {hitPoint}, pivot point: {pivotPoint}, cell: {cellPos}, target position: {targetPosition}");
                }
            }
        }
        
        if (hitGrid)
        {
            // Update object position to follow pointer
            selectedObject.transform.position = targetPosition;
            
            // Check placement validity based on the grid cell where the pivot (corner) would be
            bool canPlace = CanPlaceObjectAt(cellPos, selectedObject);
            
            // Update visual feedback
            selectedObject.SetValidPlacement(canPlace);
            
            // Optional: Add additional visual feedback for invalid placement
            if (!canPlace)
            {
                // You could add particle effects, color changes, or other visual cues here
                Debug.Log($"Cannot place at {cellPos} - invalid position");
            }
        }
        else
        {
            // If we're completely off the grid, show invalid placement
            selectedObject.SetValidPlacement(false);
            Debug.Log("Pointer is not over the grid");
        }
    }
    
    #endregion
    
    #region Crop System
    
    private void HandleCropFieldClick(CropField cropField)
    {
        Debug.Log($"HandleCropFieldClick called for {cropField.name}, HasCrop: {cropField.HasCrop}, IsGrown: {cropField.IsGrown}");
        
        if (cropField.HasCrop)
        {
            // Check if crop is grown
            if (cropField.IsGrown)
            {
                // Collect crop
                string cropType = cropField.PlantedCropType;
                ShowCropCollectedUI(cropType);
                cropField.HarvestCrop();
                
                // Clear any timer coroutines for this crop field
                if (cropTimerCoroutines.TryGetValue(cropField, out Coroutine timerCoroutine))
                {
                    StopCoroutine(timerCoroutine);
                    cropTimerCoroutines.Remove(cropField);
                    Debug.Log($"Cleared timer coroutine for harvested crop field {cropField.name}");
                }
                
                Debug.Log($"Harvested {cropType} from {cropField.name}");
            }
            else
            {
                // Show timer UI
                ShowCropTimerUI(cropField);
                Debug.Log($"Showing timer UI for {cropField.name}");
            }
        }
        else
        {
            // Show crop selection UI
            ShowCropSelectionUI(cropField);
            Debug.Log($"Showing crop selection UI for {cropField.name}");
        }
    }
    
    private void ShowCropSelectionUI(CropField cropField)
    {
        Debug.Log($"ShowCropSelectionUI called for {cropField.name}");
        
        // Show UI for selecting crop type
        cropSelectionUI.SetActive(true);
        
        // Store reference to the crop field for the UI buttons
        cropSelectionUI.GetComponent<CropSelectionUI>().SetTargetCropField(cropField);
        
        Debug.Log("Crop selection UI activated");
    }
    
    private void ShowCropTimerUI(CropField cropField)
    {
        StartCoroutine(ShowCropTimerUICoroutine(cropField));
    }
    
    private IEnumerator ShowCropTimerUICoroutine(CropField cropField)
    {
        // Ensure Hide UI with crop collected info
        cropCollectedUI.SetActive(false);
        // Show UI with timer
        cropTimerUI.SetActive(true);

        // Stop previous coroutine for this crop field if it exists
        if (cropTimerCoroutines.TryGetValue(cropField, out Coroutine existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
        }

        // Start new coroutine to update timer and store reference
        Coroutine newCoroutine = StartCoroutine(UpdateCropTimer(cropField));
        cropTimerCoroutines[cropField] = newCoroutine;

        yield return new WaitForSeconds((cropField.GetTimeUntilGrown() > 1f) ? 1f: cropField.GetTimeUntilGrown() - .1f);

        // Disable UI with timer
        cropTimerUI.SetActive(false);
    }

    private IEnumerator UpdateCropTimer(CropField cropField)
    {
        while (cropTimerUI.activeSelf && !cropField.IsGrown)
        {
            float timeLeft = cropField.GetTimeUntilGrown();
            cropTimerText.text = $"Time left: {timeLeft:F1} seconds";
            
            yield return new WaitForSeconds(0.1f);
            
            // If crop becomes grown during waiting
            if (cropField.IsGrown)
            {
                cropTimerUI.SetActive(false);
                ShowCropCollectedUI(cropField.PlantedCropType);
                break;
            }
        }
        
        // Remove reference from dictionary when done
        if (cropTimerCoroutines.ContainsKey(cropField))
        {
            cropTimerCoroutines.Remove(cropField);
        }
    }
    
    private void ShowCropCollectedUI(string cropType)
    {
        // Ensure Hide UI with timer
        cropTimerUI.SetActive(false);
        // Show UI with crop collected info
        cropCollectedUI.SetActive(true);
        cropCollectedText.text = $"1 {cropType} collected!";
        
        // Auto-hide after delay
        StartCoroutine(AutoHideUI(cropCollectedUI, 2f));
    }
    
    private IEnumerator AutoHideUI(GameObject uiElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiElement.SetActive(false);
    }
    
    public void PlantCrop(CropField cropField, string cropType, int cropIndex = -1)
    {
        Debug.Log($"PlantCrop called with cropField: {cropField.name}, cropType: {cropType}, cropIndex: {cropIndex}");
        cropField.PlantCrop(cropType, cropIndex);
        cropSelectionUI.SetActive(false);
        Debug.Log($"Planted {cropType} in {cropField.name} and closed UI");
    }

    #endregion

    #region Debug


    // Helper method to convert layer mask to readable string
    private string LayerMaskToString(LayerMask mask)
    {
        string result = "";
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                result += LayerMask.LayerToName(i) + ", ";
            }
        }
        return result.TrimEnd(',', ' ');
    }
    
    private IEnumerator TestGridAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("Running grid test...");
        
        // Test raycast against grid
        if (mainCamera != null && gridPlane != null)
        {
            // Cast a ray from the center of the screen
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Debug.Log($"Test ray origin: {ray.origin}, direction: {ray.direction}");
            
            // Try to hit the grid with the ray
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log($"Test ray hit something: {hit.collider.gameObject.name} at point {hit.point}");
                
                // Check if it hit our grid
                if (hit.collider.gameObject == gridPlane.gameObject)
                {
                    Debug.Log($"Successfully hit grid at point {hit.point}");
                    Vector2Int gridPos = WorldToGridPosition(hit.point);
                    Debug.Log($"Grid position: {gridPos}");
                    
                    // Visual debug - draw a sphere at the hit point
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.transform.position = hit.point;
                    marker.transform.localScale = Vector3.one * 0.2f;
                    marker.GetComponent<Renderer>().material.color = Color.red;
                    Destroy(marker, 5f);
                }
                else
                {
                    Debug.LogError($"Ray hit {hit.collider.gameObject.name} instead of the grid plane!");
                }
            }
            else
            {
                Debug.LogError("Test ray didn't hit anything! Check camera position and grid plane position/size.");
                
                // Try with all layers to see if we're hitting anything
                if (Physics.Raycast(ray, out hit, 100f, -1))
                {
                    Debug.Log($"Test ray hit {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}, but this layer is not in the gridLayerMask.");
                }
                else
                {
                    Debug.Log("Test ray didn't hit anything even with all layers included. Check if there are any colliders in the scene.");
                }
            }
        }
    }
    
    
    // Keep this method to avoid errors in case it's referenced elsewhere
    private IEnumerator HoldTimerCoroutine(Vector3 initialPosition)
    {
        // This method is no longer used - hold timing is handled in HandleDirectInput
        Debug.LogWarning("HoldTimerCoroutine called but is deprecated - using direct timing in Update instead");
        yield break;
    }
    
    private void OnDestroy()
    {
        // Clean up all crop timer coroutines
        foreach (var kvp in cropTimerCoroutines)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        cropTimerCoroutines.Clear();
        
        Debug.Log("GameManager destroyed - cleaned up all coroutines");
    }
    
    #endregion
}

// Grid cell class
[System.Serializable]
public class GridCell
{
    public Vector2Int Position { get; private set; }
    public GameObject OccupyingObject { get; private set; }
    public bool IsEmpty => OccupyingObject == null;
    
    public GridCell(Vector2Int position)
    {
        Position = position;
        OccupyingObject = null;
    }
    
    public void SetObject(GameObject obj)
    {
        OccupyingObject = obj;
    }
    
    public void ClearObject()
    {
        OccupyingObject = null;
    }
} 