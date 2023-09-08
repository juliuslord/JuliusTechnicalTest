using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Walking,
        Flying,
        InMenu
    }

    public PlayerState currentState = PlayerState.Walking;  // Initially set to Walking

    [Header("Movement")]
    public float walkSpeed = 5.0f;
    public float flySpeed = 10.0f;
    public float rotationSpeed = 2.0f;
    public float maxVerticalRotation = 80.0f;
    private Vector3 flyingVelocity = Vector3.zero;
    private float verticalRotation = 0;

    private CharacterController characterController;
    private PlayerState previousState = PlayerState.Walking;

    public MenuController menuController;

    public GameObject selectedObject;

    private Vector3 offset; // Offset between the mouse cursor and the selected object
    public float maxDragDistance = 10.0f; // Maximum distance the object can be dragged from the player
    private float minDragDistance = 1.0f; // Minimum allowed drag distance
    private bool isDragging = false; // Flag to track if dragging is in progress

    private bool isRotating = false;
    private Vector3 initialMousePosition;
    
    public float objRotationSpeed = 1.0f; // Adjust the rotation speed for larger rotations


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        selectedObject = null;

        // Hide the cursor when in the "Walking" or "Flying" state
        Cursor.visible = false;
    }

    void Update()
    {
        HandleStateChangeInput();
        SightHandler();

        // Handle input based on the current state
        switch (currentState)
        {
            case PlayerState.Walking:
                // Lock the cursor when in "Walking" state
                Cursor.lockState = CursorLockMode.Locked;
                HandleMouseInput();
                HandleWalkingMovement();
                menuController.DeactivateMenu();
                break;
            case PlayerState.Flying:
                // Lock the cursor when in "Flying" state
                Cursor.lockState = CursorLockMode.Locked;
                HandleMouseInput();
                HandleFlyingMovement();
                menuController.DeactivateMenu();
                break;
            case PlayerState.InMenu:
                // Unlock the cursor when in "InMenu" state
                Cursor.lockState = CursorLockMode.None;
                HandleMouseInputInMenu();
                menuController.ActivateMenu();
                break;
        }

        HandleObjectRotation();
        HandleObjectDrag();
        HandleScrollWheelInput();

        // Disable the selected object when the Delete key is pressed
        if (Input.GetKeyDown(KeyCode.Delete) && selectedObject != null)
        {
            HandleDeletingObjects();
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Cast a ray from the camera through the mouse cursor position and return the intersection point with the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    void HandleMouseInput()
    {
        // Mouse input for looking around
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Make the cursor invisible
        Cursor.visible = false;

        // Rotate the player and camera together
        transform.Rotate(Vector3.up * mouseX * rotationSpeed);
        verticalRotation -= mouseY * rotationSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRotation, maxVerticalRotation);
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleWalkingMovement()
    {
        // Calculate movement direction based on player input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Check if Left Shift key is held down and adjust the speed accordingly
        float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;

        Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;

        // Apply gravity
        moveDirection.y = Physics.gravity.y;

        // Move the character controller
        characterController.Move(moveDirection * walkSpeed * speedMultiplier * Time.deltaTime);
    }

    void HandleFlyingMovement()
    {
        // Calculate the movement direction based on player input relative to camera direction
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Check if Left Shift key is held down and adjust the speed accordingly
        float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? 2.0f : 1.0f;

        Vector3 moveDirection = Camera.main.transform.forward.normalized * moveZ + Camera.main.transform.right.normalized * moveX;

        // Check if there's no input for movement and stop instantly
        if (Mathf.Approximately(moveDirection.magnitude, 0f))
        {
            flyingVelocity = Vector3.zero;
        }
        else
        {
            // Move the character controller
            characterController.Move(flyingVelocity * Time.deltaTime);

            // Set flyingVelocity directly to the moveDirection
            flyingVelocity = moveDirection.normalized * flySpeed * speedMultiplier;

            // Debug the flyingVelocity
            // Debug.Log("Flying Velocity: " + flyingVelocity);
        }
    }

    void HandleStateChangeInput()
    {
        // Toggle between walking and flying states with the Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == PlayerState.Walking)
            {
                currentState = PlayerState.Flying;
            }
            else if (currentState == PlayerState.Flying)
            {
                currentState = PlayerState.Walking;
            }
        }

        // Change to InMenu state and save the current state as previousState
        if (currentState == PlayerState.Walking || currentState == PlayerState.Flying)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                previousState = currentState;
                currentState = PlayerState.InMenu;
            }
        }
        // Change back to the previous state from InMenu state
        else if (currentState == PlayerState.InMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentState = previousState;
            }
        }
    }

    // Sight handling function
    void SightHandler()
    {
        // Raycast from the camera
        Ray ray;

        // Cast a ray from the camera to the mouse cursor position in the "InMenu" state
        if (currentState == PlayerState.InMenu)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        else
        {
            // Cast a ray from the camera to the center of the screen in "Walking" and "Flying" states
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        RaycastHit hit;

        // Keep track of the object that the player is currently looking at
        GameObject objectInView = null;

        if (Physics.Raycast(ray, out hit))
        {
            // Draw a debug line (laser) from the camera to the hit point
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            // Check if the object has the ObjectHandler script
            ObjectHandler objectHandler = hit.collider.GetComponent<ObjectHandler>();
            if (objectHandler != null)
            {
                // Set the objectInView to the object the player is looking at
                objectInView = objectHandler.gameObject;

                // Handle E input to select the object
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectedObject != null)
                    {
                        selectedObject.GetComponent<ObjectHandler>().DeselectObject();
                    }

                    // Set the selected object to the one the player is looking at
                    selectedObject = objectHandler.gameObject;
                    isDragging = false; // Reset dragging

                    objectHandler.SelectObject();

                    menuController.NewSelectedObject(); // Set the UI selected object text
                }
            }
        }
        else
        {
            // Draw a debug line (laser) from the camera into the distance
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
        }

        // Turn off the outline for objects that are not in view and are not the selected object
        foreach (var outlineObject in FindObjectsOfType<Outline>())
        {
            if (outlineObject.gameObject != objectInView && outlineObject.gameObject != selectedObject)
            {
                outlineObject.GetComponent<ObjectHandler>().NotLookingAtObject();
            }

            // Check if the objectInView is not the selected object and has an Outline component
            if (objectInView != null && objectInView != selectedObject)
            {
                objectInView.GetComponent<ObjectHandler>().LookingAtObject();
            }
        }
    }

    void HandleObjectDrag()
    {
        // Handle object dragging
        if (selectedObject != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Cast a ray from the camera to the selected object
                Ray rayToSelectedObject = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Check if the ray intersects with the selected object
                if (Physics.Raycast(rayToSelectedObject, out hit) && hit.collider.gameObject == selectedObject)
                {
                    // Calculate the maximum drag distance based on the distance to the selected object
                    maxDragDistance = Vector3.Distance(transform.position, selectedObject.transform.position);
                    maxDragDistance = Mathf.Max(maxDragDistance, minDragDistance);

                    // Start dragging only if the object was not already being dragged
                    if (!isDragging)
                    {
                        offset = selectedObject.transform.position - GetMouseWorldPosition();
                        isDragging = true;

                        // Send a message to the ObjectHandler to turn off the collider
                        selectedObject.GetComponent<ObjectHandler>().TurnOffCollider();
                    }
                }
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                // Calculate the position to move the object to
                Vector3 newPosition = GetMouseWorldPosition() + offset;

                // Calculate the vector from the player to the new position
                Vector3 playerToNewPosition = newPosition - transform.position;

                // Limit the object to the maximum distance from the player
                if (playerToNewPosition.magnitude > maxDragDistance)
                {
                    newPosition = transform.position + playerToNewPosition.normalized * maxDragDistance;
                }

                /*
                // Check if there's an object in view (not looking at infinity)
                if (selectedObject != null)
                {
                    // Adjust the Y position to prevent falling through the floor
                    newPosition.y += 1.0f; // Increase the Y position by 1
                }
                */

                // Update the object's position
                selectedObject.transform.position = newPosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                // Stop dragging when the left mouse button is released
                isDragging = false;

                // Send a message to the ObjectHandler to turn on the collider
                selectedObject.GetComponent<ObjectHandler>().TurnOnCollider();
            }
        }
    }


        void HandleScrollWheelInput()
        {
            // Adjust the maximum drag distance using the scroll wheel
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            maxDragDistance += scrollDelta;

            // Ensure the maximum drag distance stays within the defined limits
            maxDragDistance = Mathf.Clamp(maxDragDistance, minDragDistance, float.MaxValue);
        }

    public void HandleDeletingObjects()            // This used to just destroy the object, but once I introduced saving, objects needed to be kept alive
    {
        selectedObject.SetActive(false);
        selectedObject = null; // Clear the selected object
        
    }

    void HandleObjectRotation()
        {
            if (selectedObject != null)
            {
                // Cast a ray from the camera to the selected object
                Ray rayToSelectedObject = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Check if the ray intersects with the selected object
                if (Physics.Raycast(rayToSelectedObject, out hit) && hit.collider.gameObject == selectedObject)
                {
                    // Start rotating the object when right mouse button is pressed
                    if (Input.GetMouseButtonDown(1))
                    {
                        isRotating = true;
                        initialMousePosition = Input.mousePosition;
                    }
                }

                // Rotate the selected object based on mouse movement
                if (isRotating)
                {
                    float XaxisRotation = Input.GetAxis("Mouse X") * objRotationSpeed;
                    float YaxisRotation = Input.GetAxis("Mouse Y") * objRotationSpeed;

                    selectedObject.transform.Rotate(Vector3.down, XaxisRotation);
                    selectedObject.transform.Rotate(Vector3.right, YaxisRotation);
                }

                // Stop rotating when right mouse button is released
                if (Input.GetMouseButtonUp(1))
                {
                    isRotating = false;
                }
            }
        }

    void HandleMouseInputInMenu()
    {
        // Mouse input is processed in the "InMenu" state
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Make the cursor visible to press buttons
        Cursor.visible = true;
    }

    /*                                                          // Left this in as a reminder, having keybindings is incompatible with naming objects
    void MenuButtonInput()
    {
        if (Input.GetKeyDown(KeyCode.C) && selectedObject != null)
        {
            // Save the current selected object to the Spawn list
            menuController.SaveObject();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Call the public function in MenuController to lock the selectedObject's rotation
            menuController.ToggleRotationLock();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            menuController.ToggleTime();
        }
    }
    */
}

