using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    private PlayerController playerController; // Reference to the PlayerController script

    private Collider _collider;
    private Outline outline;
    private Rigidbody rb;

    public bool isSelected = false;
    public bool beingViewed = false;

    public bool gridSnapping = false; // By default, gridSnapping is off

    // Start is called before the first frame update
    void Start()
    {
        // Get the collider component attached to the GameObject
        _collider = GetComponent<Collider>();

        // Find the PlayerController script on the player object
        playerController = FindObjectOfType<PlayerController>();

        // Get the Rigidbody component attached to the GameObject
        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        // Get the Outline component attached to this object
        outline = GetComponent<Outline>();

        // Initially, turn off the outline
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DestroyOnFall();        // If an object falls below a certain Y, it is removed from the game

        if (isSelected)
        {
            // Turn on outline when selected
            if (outline != null)
            {
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.enabled = true;
                outline.OutlineColor = Color.green;
            }
        }

        else if (beingViewed && !isSelected)
        {
            // Turn on outline when selected
            if (outline != null)
            {
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.enabled = true;
                outline.OutlineColor = Color.magenta;
            }
        }

        else if (!beingViewed && !isSelected)
        {
            // Turn off the outline when deselected
            if (outline != null)
            {
                outline.OutlineMode = Outline.Mode.OutlineHidden;
                outline.enabled = false;
            }
        }

        // Check if gridSnapping is enabled
        if (gridSnapping)
        {
            // Snap the object to the closest integer x, y, and z positions
            if (rb != null && rb.velocity.magnitude < 0.5f)
            {
                SnapToGrid();
            }
        }
    }

    public void TurnOffCollider()
    {
        // Turn off the collider
        if (_collider != null)
        {
            _collider.enabled = false;
        }
    }

    public void TurnOnCollider()
    {
        // Turn on the collider
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    public void DestroyOnFall()
    {
        if (transform.position.y < -100)
        {
            gameObject.SetActive(false);        // Cant destroy it in case it has been saved

            if (playerController.selectedObject == gameObject)
            {
                playerController.selectedObject = null;     // If it's the current selectedObject, take it off the player
            }
        }
    }

    // Function to handle object selection
    public void SelectObject()
    {
        isSelected = true;
    }

    // Function to handle deselecting the object
    public void DeselectObject()
    {
        isSelected = false;
    }

    // Function to handle outline when being viewed
    public void LookingAtObject()
    {
        beingViewed = true;
    }

    // Function to handle outline when being viewed
    public void NotLookingAtObject()
    {
        beingViewed = false;
    }

    private void SnapToGrid()
    {
        Vector3 newPosition = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            Mathf.Round(transform.position.z)
        );

        // Snap the object's position
        transform.position = newPosition;
    }

    public void ToggleGridSnapping()        // To be referenced by the menucontroller
    {
        gridSnapping = !gridSnapping;
    }

    void MergeObjects(GameObject otherObject)
    {
        // Calculate the relative position and rotation of the other object
        Vector3 relativePosition = otherObject.transform.position - transform.position;
        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * otherObject.transform.rotation;

        // Parent the other object to this object to make them move together
        otherObject.transform.SetParent(transform);

        // Adjust the position and rotation to maintain the relative position and rotation
        otherObject.transform.localPosition = relativePosition;
        otherObject.transform.localRotation = relativeRotation;

        // Disable the Rigidbody of the other object to prevent physics interactions
        Rigidbody otherRigidbody = otherObject.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            otherRigidbody.isKinematic = true;
        }
    }

}
