using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    private PlayerController playerController; // Reference to the PlayerController script

    private Collider _collider;

    // Start is called before the first frame update
    void Start()
    {
        // Get the collider component attached to the GameObject
        _collider = GetComponent<Collider>();

        // Turn off outline to start
        GetComponent<Outline>().enabled = false;

        // Find the PlayerController script on the player object
        playerController = FindObjectOfType<PlayerController>();

    }

    // Update is called once per frame
    void Update()
    {
        // Check if this object is selected by the player
        if (playerController.selectedObject == gameObject)
        {
            // Print a debug message
            Debug.Log(gameObject.name + " is selected");

            // Turn on outline when selected
            GetComponent<Outline>().enabled = true;
            GetComponent<Outline>().OutlineColor = Color.green;
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

   
}
