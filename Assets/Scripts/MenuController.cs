using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private PlayerController playerController; // Reference to the PlayerController script
    public GameObject menuObject; // Reference to the parent menu object in the Unity Editor
    public GameObject cubeToSpawn;

    public List<GameObject> savedObjects = new List<GameObject>(); // List to store saved objects
    public ScrollRect scrollView; // Reference to the scroll view
    public Transform scrollViewContent; // Reference to the content of the object scroll view
    public GameObject listButtonPrefab; // Reference to the button prefab

    public Text selectedObjectText; // Text for the top right

    public Text XText; // Text for displaying the X position
    public Text YText; // Text for displaying the Y position
    public Text ZText; // Text for displaying the Z position

    public InputField xInputField;
    public InputField yInputField;      // Input fields for Positions
    public InputField zInputField;


    // Start is called before the first frame update
    void Start()
    {
        // Ensure that the menuObject reference is set in the Unity Editor
        if (menuObject == null)
        {
            Debug.LogError("Menu Object reference is not set!");
        }
        else
        {
            // Initially, make sure the menu is inactive
            menuObject.SetActive(false);
        }

        // Find the PlayerController script on the player object
        playerController = FindObjectOfType<PlayerController>();

        // Call PopulateScrollView to populate it initially
        PopulateScrollView();

        UpdateSelectedObjectPosition(); // Start by setting selected Object text to N/A
    }

    // Update is called once per frame
    void Update()
    {
        // Check if a new item is saved, and update the scroll view if needed
        if (savedObjects.Count != scrollViewContent.childCount)
        {
            PopulateScrollView();
        }

        UpdateSelectedObjectPosition();
    }

    public void ActivateMenu()
    {
        // Activate the menu object
        menuObject.SetActive(true);
    }

    public void DeactivateMenu()
    {
        // Deactivate the menu object
        menuObject.SetActive(false);
    }

    public void SaveObject()
    {
        GameObject objectToSave = playerController.selectedObject;

        // Check if the objectToSave is not null and not already in the list
        if (objectToSave != null && !savedObjects.Contains(objectToSave))
        {
            // Add the object to the savedObjects list
            savedObjects.Add(objectToSave);
        }
    }

    public void SpawnSavedObject(GameObject savedObject)
    {
        // Raycast from the camera to determine the spawn position
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 spawnPosition = hit.point;

            Quaternion spawnRotation = Quaternion.identity; // No rotation

            // Instantiate the saved object at the spawn position
            GameObject spawnedObject = Instantiate(savedObject, spawnPosition, spawnRotation);

            // Enable the spawned object (in case the original has been 'deleted'
            spawnedObject.SetActive(true);
        }
        else
        {
            Debug.LogError("No valid spawn location found.");
        }
    }


    public void SpawnCube()
    {
        // Check if the prefab to spawn is set
        if (cubeToSpawn != null)
        {
            // Raycast from the camera to determine the spawn position
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 spawnPosition = hit.point;
                Quaternion spawnRotation = Quaternion.identity; // No rotation

                Instantiate(cubeToSpawn, spawnPosition, spawnRotation);
            }
            else
            {
                Debug.LogError("No valid spawn location found.");
            }
        }
        else
        {
            Debug.LogError("Prefab to spawn is not set!");
        }
    }

    private void PopulateScrollView()
    {
        // Clear the existing items in the scroll view
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // Loop through the savedObjects list and create a UI item for each saved object
        foreach (GameObject savedObject in savedObjects)
        {
            // Instantiate a new UI item from the prefab
            GameObject listItem = Instantiate(listButtonPrefab, scrollViewContent);

            // Get the Text component from the button to set its name
            Text buttonText = listItem.GetComponentInChildren<Text>();
            buttonText.text = savedObject.name; // Set the button text to the object's name

            // Add a button click event to spawn the saved object
            Button spawnButton = listItem.GetComponentInChildren<Button>();
            spawnButton.onClick.AddListener(() => SpawnSavedObject(savedObject));
        }
    }

    public void NewSelectedObject()
    {
        selectedObjectText.text = playerController.selectedObject.name; // Update name

        UpdateSelectedObjectPosition(); // Update position text
    }

    // Function to update the selected object's position
    public void UpdateSelectedObjectPosition()
    {
        if (playerController.selectedObject != null)
        {
            // Get the selectedObject's position
            Vector3 selectedObjectPosition = playerController.selectedObject.transform.position;

            // Display the X, Y, and Z positions
            XText.text = "X: " + selectedObjectPosition.x.ToString("F2"); // Format to two decimal places
            YText.text = "Y: " + selectedObjectPosition.y.ToString("F2");
            ZText.text = "Z: " + selectedObjectPosition.z.ToString("F2");
        }
        else
        {
            // Handle the case when no object is selected
            selectedObjectText.text = "N/A";
            XText.text = "X: N/A";
            YText.text = "Y: N/A";
            ZText.text = "Z: N/A";
        }
    }

    public void IncreaseXPosition()
    {
        if (playerController.selectedObject != null && xInputField != null)
        {
            float increment = float.Parse(xInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.x += increment;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void DecreaseXPosition()
    {
        if (playerController.selectedObject != null && xInputField != null)
        {
            float decrement = float.Parse(xInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.x -= decrement;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void IncreaseYPosition()
    {
        if (playerController.selectedObject != null && yInputField != null)
        {
            float increment = float.Parse(yInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.y += increment;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void DecreaseYPosition()
    {
        if (playerController.selectedObject != null && yInputField != null)
        {
            float decrement = float.Parse(yInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.y -= decrement;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void IncreaseZPosition()
    {
        if (playerController.selectedObject != null && zInputField != null)
        {
            float increment = float.Parse(zInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.z += increment;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void DecreaseZPosition()
    {
        if (playerController.selectedObject != null && zInputField != null)
        {
            float decrement = float.Parse(zInputField.text); // Parse the input field value as a float
            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.z -= decrement;
            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }
}
