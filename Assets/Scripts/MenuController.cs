using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private PlayerController playerController; // Reference to the PlayerController script
    public GameObject menuObject; // Reference to the parent menu object

    public List<GameObject> savedObjects = new List<GameObject>(); // List to store saved objects
    public ScrollRect scrollView; // Reference to the scroll view
    public Transform scrollViewContent; // Reference to the content of the object scroll view
    public GameObject listButtonPrefab; // Reference to the button prefab

    public InputField selectedObjectInput; // Reference to the name above the view of the selected object

    public VirtualViewer virtualViewer;

    [Header("Position")]
    public Text XText; // Text for displaying the X position
    public Text YText; // Text for displaying the Y position
    public Text ZText; // Text for displaying the Z position

    public InputField xInputField;
    public InputField yInputField;      // Input fields for Positions
    public InputField zInputField;

    [Header("Scale")]
    public Text XScaleText; // Text for displaying the X Scale
    public Text YScaleText; // Text for displaying the Y Scale
    public Text ZScaleText; // Text for displaying the Z Scale

    public InputField xScaleInputField;
    public InputField yScaleInputField;      // Input fields for Scale
    public InputField zScaleInputField;

    [Header("Material Replacement")]
    public Material redMaterial;
    public Material orangeMaterial;
    public Material yellowMaterial;         // Materials for the Primary and Secondary colours
    public Material greenMaterial;
    public Material blueMaterial;
    public Material purpleMaterial;
    public Material blackMaterial;
    public Material whiteMaterial;

    [Header("Physics Materials")]
    public PhysicMaterial[] availableMaterials; // Array of available PhysicMaterials
    public Dropdown materialDropdown;
    public Text physMaterialText;

    // Boolean to track whether time is paused or not
    public bool timePaused = false;

    // Reference to the UI Text element you want to modify
    public Text infoText;

    // The text in the bottom left with main controls
    public Text controlText;

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

        // Ensure that the materialDropdown reference is set in the Unity Editor
        if (materialDropdown == null)
        {
            Debug.LogError("Material Dropdown reference is not set!");
        }
        else
        {
            // Attach the MaterialSelectionChanged function to the Dropdown's OnValueChanged event
            materialDropdown.onValueChanged.AddListener(MaterialSelectionChanged);
        }

        // An event listener to the selectedObjectInput's "End Edit" event
        if (selectedObjectInput != null)
        {
            selectedObjectInput.onEndEdit.AddListener(RenameSelectedObject);
        }

        // Set the Dropdown options and selection based on the selectedObject
        UpdateDropdownOptionsAndSelection();

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

        TimeControl();

        UpdateSelectedObjectPosition();
    }

    public void ActivateMenu()
    {
        // Activate the menu object
        menuObject.SetActive(true);
        controlText.gameObject.SetActive(false);
    }

    public void DeactivateMenu()
    {
        // Deactivate the menu object
        menuObject.SetActive(false);
        controlText.gameObject.SetActive(true);
    }

    public void SaveObject()
    {
        GameObject objectToSave = playerController.selectedObject;

        // Check if the objectToSave is not null and not already in the list
        if (objectToSave != null && !savedObjects.Contains(objectToSave))
        {
            // Add the object to the savedObjects list
            savedObjects.Add(objectToSave);

            ShowInfoText("Saved " + objectToSave.name);
        }
    }

    public void SpawnSavedObject(GameObject savedObject)
    {
        // Raycast from the camera to determine the spawn position
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 spawnPosition = hit.point + new Vector3(0, 1, 0);

            Quaternion spawnRotation = Quaternion.identity; // No rotation

            // Instantiate the saved object at the spawn position
            GameObject spawnedObject = Instantiate(savedObject, spawnPosition, spawnRotation);

            // Enable the spawned object (in case the original has been 'deleted')
            spawnedObject.SetActive(true);
        }
        else
        {
            Debug.LogError("No valid spawn location found.");
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

    // Function to rename the selected object based on the input field's text
    public void RenameSelectedObject(string newName)
    {
        if (playerController.selectedObject != null)
        {
            // Set the new name for the selected object
            playerController.selectedObject.name = newName;
        }
    }

    public void NewSelectedObject()
    {
        // Set the default value of the InputField to the name of the selected object
        selectedObjectInput.text = playerController.selectedObject.name;

        virtualViewer.SpawnObject();    // Set up the virtual viewer for the selected object UI

        UpdateSelectedObjectPosition(); // Update position text
        UpdateSelectedObjectScale();    // Update scale text
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
            selectedObjectInput.text = "N/A";
            XText.text = "X: N/A";
            YText.text = "Y: N/A";
            ZText.text = "Z: N/A";
        }
    }

    public void IncreasePosition()      // For Position Plus button
    {
        if (playerController.selectedObject != null && xInputField != null && yInputField != null && zInputField != null)
        {
            float incrementX = float.Parse(xInputField.text);
            float incrementY = float.Parse(yInputField.text);
            float incrementZ = float.Parse(zInputField.text);

            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.x += incrementX;
            newPosition.y += incrementY;
            newPosition.z += incrementZ;

            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }

    public void DecreasePosition()      // For Position Plus button
    {
        if (playerController.selectedObject != null && xInputField != null && yInputField != null && zInputField != null)
        {
            float decrementX = float.Parse(xInputField.text);
            float decrementY = float.Parse(yInputField.text);
            float decrementZ = float.Parse(zInputField.text);

            Vector3 newPosition = playerController.selectedObject.transform.position;
            newPosition.x -= decrementX;
            newPosition.y -= decrementY;
            newPosition.z -= decrementZ;

            playerController.selectedObject.transform.position = newPosition;
            UpdateSelectedObjectPosition();
        }
    }


    public void UpdateSelectedObjectScale()
    {
        if (playerController.selectedObject != null)
        {
            // Get the selectedObject's scale
            Vector3 selectedObjectScale = playerController.selectedObject.transform.localScale;

            // Display the X, Y, and Z scales
            XScaleText.text = "X: " + selectedObjectScale.x.ToString("F2"); // Format to two decimal places
            YScaleText.text = "Y: " + selectedObjectScale.y.ToString("F2");
            ZScaleText.text = "Z: " + selectedObjectScale.z.ToString("F2");
        }
        else
        {
            // Handle the case when no object is selected
            XScaleText.text = "X: N/A";
            YScaleText.text = "Y: N/A";
            ZScaleText.text = "Z: N/A";
        }
    }

    public void IncreaseScale()
    {
        if (playerController.selectedObject != null && xScaleInputField != null && yScaleInputField != null && zScaleInputField != null)
        {
            float incrementX = float.Parse(xScaleInputField.text);
            float incrementY = float.Parse(yScaleInputField.text);
            float incrementZ = float.Parse(zScaleInputField.text);

            Vector3 newScale = playerController.selectedObject.transform.localScale;
            newScale.x += incrementX;
            newScale.y += incrementY;
            newScale.z += incrementZ;

            // Clamp the scale values to a minimum of 0.01
            newScale.x = Mathf.Max(newScale.x, 0.01f);
            newScale.y = Mathf.Max(newScale.y, 0.01f);
            newScale.z = Mathf.Max(newScale.z, 0.01f);

            playerController.selectedObject.transform.localScale = newScale;
            UpdateSelectedObjectScale();
        }
    }

    public void DecreaseScale()
    {
        if (playerController.selectedObject != null && xScaleInputField != null && yScaleInputField != null && zScaleInputField != null)
        {
            float decrementX = float.Parse(xScaleInputField.text);
            float decrementY = float.Parse(yScaleInputField.text);
            float decrementZ = float.Parse(zScaleInputField.text);

            Vector3 newScale = playerController.selectedObject.transform.localScale;
            newScale.x -= decrementX;
            newScale.y -= decrementY;
            newScale.z -= decrementZ;

            // Clamp the scale values to a minimum of 0.01
            newScale.x = Mathf.Max(newScale.x, 0.01f);
            newScale.y = Mathf.Max(newScale.y, 0.01f);
            newScale.z = Mathf.Max(newScale.z, 0.01f);

            playerController.selectedObject.transform.localScale = newScale;
            UpdateSelectedObjectScale();
        }
    }

    // Material replacement functions
    public void ReplaceMaterialRed()
    {
        ReplaceMaterial(redMaterial);
    }

    public void ReplaceMaterialOrange()
    {
        ReplaceMaterial(orangeMaterial);
    }

    public void ReplaceMaterialYellow()
    {
        ReplaceMaterial(yellowMaterial);
    }

    public void ReplaceMaterialGreen()
    {
        ReplaceMaterial(greenMaterial);
    }

    public void ReplaceMaterialBlue()
    {
        ReplaceMaterial(blueMaterial);
    }

    public void ReplaceMaterialPurple()
    {
        ReplaceMaterial(purpleMaterial);
    }
    
    public void ReplaceMaterialBlack()
    {
        ReplaceMaterial(blackMaterial);
    }

    public void ReplaceMaterialWhite()
    {
        ReplaceMaterial(whiteMaterial);
    }

    private void ReplaceMaterial(Material material)
    {
        if (playerController.selectedObject != null)
        {
            playerController.selectedObject.GetComponent<ObjectHandler>().NotLookingAtObject();
            playerController.selectedObject.GetComponent<ObjectHandler>().DeselectObject();

            Renderer renderer = playerController.selectedObject.GetComponent<Renderer>();

            if (renderer != null && renderer.materials.Length > 0)
            {
                Material[] materials = renderer.materials;
                materials[0] = material;
                renderer.materials = materials;
            }

            // Remove the third material from the MeshRenderer's materials array (The outline Fill)
            if (renderer != null && renderer.materials.Length > 2)
            {
                List<Material> updatedMaterials = new List<Material>(renderer.materials);
                updatedMaterials.RemoveAt(2); // Remove the third material
                renderer.materials = updatedMaterials.ToArray();
            }

            else
            {
                Debug.LogWarning("Selected object has no Renderer component or materials.");
            }

            NewSelectedObject();
        }

        else
        {
            Debug.LogWarning("No object selected.");
        }
    }

    // Function to toggle rotation constraints of the selected object
    public void ToggleRotationLock()
    {
        if (playerController.selectedObject != null)
        {
            Rigidbody selectedObjectRigidbody = playerController.selectedObject.GetComponent<Rigidbody>();

            // Check if the selected object has a Rigidbody component
            if (selectedObjectRigidbody != null)
            {
                // Toggle the rotation constraints
                if (selectedObjectRigidbody.constraints == RigidbodyConstraints.None)
                {
                    // If there are no constraints, lock all rotation axes
                    selectedObjectRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                }
                else
                {
                    // If rotation is already locked, remove the constraints
                    selectedObjectRigidbody.constraints = RigidbodyConstraints.None;
                }
            }
            else
            {
                Debug.LogWarning("Selected object does not have a Rigidbody component.");
            }
        }
        else
        {
            Debug.LogWarning("No object selected.");
        }
    }

    // Function to apply physics material to the selected object
    public void ApplyPhysicsMaterial(GameObject targetObject, PhysicMaterial physicsMaterial)
    {
        if (targetObject != null)
        {
            Collider[] colliders = targetObject.GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders)
            {
                collider.material = physicsMaterial;
            }
        }
        else
        {
            Debug.LogWarning("No object selected to apply the material to.");
        }
    }

    // Function to update the Dropdown options and selection based on the selectedObject's material
    public void UpdateDropdownOptionsAndSelection()
    {
        if (playerController.selectedObject != null)
        {
            // Clear existing Dropdown options
            materialDropdown.ClearOptions();

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            for (int i = 0; i < availableMaterials.Length; i++)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = availableMaterials[i].name;
                options.Add(option);
            }

            materialDropdown.options = options;

            // Set the Dropdown's value based on the selectedObject's current material
            PhysicMaterial currentMaterial = playerController.selectedObject.GetComponent<Collider>().material;
            int selectedIndex = System.Array.IndexOf(availableMaterials, currentMaterial);
            materialDropdown.value = selectedIndex;
        }
    }



    // Function to handle material selection changes in the Dropdown
    public void MaterialSelectionChanged(int selectedIndex)
    {
        if (playerController.selectedObject != null)
        {
            if (selectedIndex >= 0 && selectedIndex < availableMaterials.Length)
            {
                ApplyPhysicsMaterial(playerController.selectedObject, availableMaterials[selectedIndex]);
                
                PhysicMaterial currentMaterial = playerController.selectedObject.GetComponent<Collider>().material;

                physMaterialText.text = "Current material: " + currentMaterial.name;
            
            }
            else
            {
                Debug.LogWarning("Invalid material selection.");
            }
        }
        else
        {
            Debug.LogWarning("No object selected to apply the material to.");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // Function to toggle time pause
    public void ToggleTime()
    {
        // Toggle the timePaused variable
        timePaused = !timePaused;

        if (timePaused)
        {
            ShowInfoText("Time has paused!");
        }
        
        if (!timePaused)
        {
            ShowInfoText("Time resumes");
        }
    }

    private void TimeControl()        // This is a cheatsy way of making it seem like time is paused
    {
        // Iterate through all objects with ObjectHandler script in the scene
        ObjectHandler[] objectHandlers = FindObjectsOfType<ObjectHandler>();
        foreach (ObjectHandler objectHandler in objectHandlers)
        {
            Rigidbody rb = objectHandler.GetComponent<Rigidbody>();

            // Check if time is paused and toggle the isKinematic property accordingly
            if (rb != null)
            {
                rb.isKinematic = timePaused;
            }
        }
    }

    // Reset the rotation of the selectedObject to identity
    public void ResetSelectedObjectRotation()
    {
        if (playerController.selectedObject != null)
        {
            playerController.selectedObject.transform.rotation = Quaternion.identity;
        }
    }

    // Public function to toggle fullscreen mode
    public void ToggleFullscreen()
    {
        // Toggle fullscreen mode
        Screen.fullScreen = !Screen.fullScreen;
    }

    // Function to update and show text for a specified duration
    public void ShowInfoText(string newText)
    {
        // Update the text with the provided string
        infoText.text = newText;

        // Enable the Text component
        infoText.enabled = true;

        // Start a coroutine to disable the Text component after the specified duration
        StartCoroutine(DisableInfoTextAfterDelay());
    }

    // Coroutine to disable the Text component after a delay
    private IEnumerator DisableInfoTextAfterDelay()
    {
        yield return new WaitForSeconds(5.0f);

        // Disable the Text component
        infoText.enabled = false;
    }

    public void HandlingGridSnap()
    {
        playerController.selectedObject.GetComponent<ObjectHandler>().ToggleGridSnapping();

        if (playerController.selectedObject.GetComponent<ObjectHandler>().gridSnapping)
        {
            ShowInfoText("Object is snapping to grid");
        }

        if (!playerController.selectedObject.GetComponent<ObjectHandler>().gridSnapping)
        {
            ShowInfoText("Object is NOT snapping to grid");
        }
    }

    public void ToggleGravity()
    {
        if (playerController.selectedObject != null)
        {
            Rigidbody selectedObjectRigidbody = playerController.selectedObject.GetComponent<Rigidbody>();

            // Check if the selected object has a Rigidbody component
            if (selectedObjectRigidbody != null)
            {
                selectedObjectRigidbody.useGravity = !selectedObjectRigidbody.useGravity;

                if (selectedObjectRigidbody.useGravity)
                {
                    ShowInfoText("Gravity enabled for the selected object.");
                }
                else
                {
                    ShowInfoText("Gravity disabled for the selected object.");
                }
            }
            else
            {
                Debug.LogWarning("Selected object does not have a Rigidbody component.");
            }
        }
        else
        {
            Debug.LogWarning("No object selected.");
        }
    }
}
