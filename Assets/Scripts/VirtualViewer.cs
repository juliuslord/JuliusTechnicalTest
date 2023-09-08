using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualViewer : MonoBehaviour
{
    private PlayerController playerController; // Reference to the PlayerController script
    public Camera objectCamera; // Reference to the camera
    public float rotationSpeed = 20f; // Camera rotation speed
    public float distanceMultiplier = 2f; // Multiplier for camera distance based on object scale
    public GameObject spawnPositionObject; // Specify the spawn position GameObject

    private GameObject virtualObject;
    private bool isCameraActive = false;
    private bool objectIsSpawned = false;

    private void Start()
    {
        // Find the PlayerController script in the scene
        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null || objectCamera == null || spawnPositionObject == null)
        {
            Debug.LogError("Please assign the PlayerController, object camera, and spawn position object in the inspector.");
            enabled = false; // Disable the script if references are missing
            return;
        }
    }

    private void Update()
    {
        // Rotate the camera around the spawned object while keeping the same distance
        if (isCameraActive)
        {
            objectCamera.transform.RotateAround(virtualObject.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            objectCamera.transform.LookAt(virtualObject.transform);
        }

        if (playerController.selectedObject != null)
        {
            // Copy the scale from the selected object to the clone object
            virtualObject.transform.localScale = playerController.selectedObject.transform.localScale;
        }
    }

    public void SpawnObject()
    {
        // Check if the selected object has changed
        if (objectIsSpawned)
        {
            // Destroy the old spawned object if it exists
            if (virtualObject != null)
            {
                Destroy(virtualObject);
            }

            // Spawn a copy of the new selected object at the spawn position
            if (playerController.selectedObject != null)
            {
                SpawnLogic();
            }
            else
            {
                // Disable the camera if the selected object is null
                isCameraActive = false;
            }
        }

        else
        {
            SpawnLogic();
        }
    }

    private void SpawnLogic()
    {
        // Spawn a copy of the selected object at the spawn position
        virtualObject = Instantiate(playerController.selectedObject, spawnPositionObject.transform.position, Quaternion.identity);
        virtualObject.SetActive(true); // Ensure the object is active

        // I wanted to remove the outline from the virtual object, so it'd look nice
        // But the real selected object used the same material, so I couldnt edit it

        // Disable the Outline script on the spawned object
        Outline outline = virtualObject.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }                           // Destorying the outline script wasnt enough, so I removed the outline material

        // Remove the third material from the MeshRenderer's materials array (The outline Fill)
        MeshRenderer meshRenderer = virtualObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.materials.Length > 2)
        {
            List<Material> updatedMaterials = new List<Material>(meshRenderer.materials);
            updatedMaterials.RemoveAt(2); // Remove the third material
            meshRenderer.materials = updatedMaterials.ToArray();
        }
        // This worked


        // Calculate the initial camera distance based on object scale
        float objectScale = Mathf.Max(playerController.selectedObject.transform.lossyScale.x, playerController.selectedObject.transform.lossyScale.y, playerController.selectedObject.transform.lossyScale.z);
        float initialDistance = objectScale * distanceMultiplier;

        // Calculate the camera's position based on the spawn position and a side offset
        Vector3 sideOffset = spawnPositionObject.transform.right * -initialDistance; // Adjust the offset as needed
        objectCamera.transform.position = spawnPositionObject.transform.position + sideOffset;

        // Look at the spawned object
        objectCamera.transform.LookAt(virtualObject.transform);

        // Set the camera as active
        isCameraActive = true;

        // Set object as spawned
        objectIsSpawned = true;
    }
}
