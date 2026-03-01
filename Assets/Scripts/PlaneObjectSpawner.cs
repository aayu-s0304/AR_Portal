using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

public class PlaneObjectSpawner : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject doorPrefab;
    public GameObject terrainPrefab;
    public Button spawnButton;
    public Button collapseButton;

    public Camera mainCamera;
    public Camera secondaryCamera;

    public Material newSkyboxMaterial;

    private GameObject spawnedDoor;
    private GameObject spawnedTerrain;
    private bool terrainSpawned = false;

    public CameraController cameraController;  // Reference to CameraController

    void Start()
    {
        spawnButton.onClick.AddListener(SpawnDoor);
        collapseButton.onClick.AddListener(CollapseDoor);
        spawnButton.gameObject.SetActive(true);
        collapseButton.gameObject.SetActive(false);

        if (mainCamera == null || secondaryCamera == null)
        {
            Debug.LogError("Main or Secondary Camera is missing!");
            return;
        }

        EnsureCameraComponents(mainCamera.gameObject);
        EnsureCameraComponents(secondaryCamera.gameObject);

        secondaryCamera.enabled = false; // Start with secondary camera off
    }

    void EnsureCameraComponents(GameObject cameraObj)
    {
        if (!cameraObj.GetComponent<Rigidbody>())
        {
            Rigidbody rb = cameraObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (!cameraObj.GetComponent<BoxCollider>())
        {
            BoxCollider collider = cameraObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        if (spawnedDoor == null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinBounds))
            {
                spawnButton.gameObject.SetActive(true);
            }
        }
    }

    void SpawnDoor()
    {
        if (spawnedDoor == null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;

                // Spawn door **1.5 meters in front of the user** at the detected plane height
                Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;
                spawnPosition.y = hitPose.position.y; // Ensure the door spawns at the detected plane's height

                spawnedDoor = Instantiate(doorPrefab, spawnPosition, doorPrefab.transform.rotation);

                spawnButton.gameObject.SetActive(false);
                collapseButton.gameObject.SetActive(true);

                BoxCollider doorCollider = spawnedDoor.GetComponent<BoxCollider>();
                if (doorCollider == null)
                {
                    doorCollider = spawnedDoor.AddComponent<BoxCollider>();
                }
                doorCollider.isTrigger = true;
            }
            else
            {
                Debug.Log("No plane detected to place the door.");
            }
        }
    }


    void CollapseDoor()
    {
        if (spawnedDoor != null)
        {
            Destroy(spawnedDoor);
            spawnedDoor = null;
            collapseButton.gameObject.SetActive(false);
            spawnButton.gameObject.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == spawnedDoor)
        {
            if (!terrainSpawned)
            {
                StartCoroutine(SpawnTerrainWithDelay());
            }
            else
            {
                RemoveTerrain();
            }

            SwitchCamera();
        }
    }

    IEnumerator SpawnTerrainWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        SpawnTerrain();
    }

    void SpawnTerrain()
    {
        if (spawnedTerrain == null)
        {
            // Spawn terrain at fixed position
            Vector3 terrainPosition = new Vector3(-41.5f, 0f, -37.1f);

            spawnedTerrain = Instantiate(terrainPrefab, terrainPosition, Quaternion.identity);
            terrainSpawned = true;
            RenderSettings.skybox = newSkyboxMaterial;

            // Move the user to the center of the new terrain
            StartCoroutine(MoveUserToTerrainCenter());
        }
    }

    IEnumerator MoveUserToTerrainCenter()
    {
        yield return new WaitForSeconds(0.5f);

        if (spawnedTerrain != null)
        {
            // Assuming the terrain has a collider or terrain component to define its size
            Vector3 terrainSize = GetTerrainSize(spawnedTerrain);
            Vector3 terrainCenter = spawnedTerrain.transform.position;
            terrainCenter.z += terrainSize.z / 2f; // Center on terrain

            terrainCenter.y = mainCamera.transform.position.y; // Keep the user's height the same

            mainCamera.transform.position = terrainCenter;
        }
    }

    Vector3 GetTerrainSize(GameObject terrainObj)
    {
        Terrain terrain = terrainObj.GetComponent<Terrain>();
        if (terrain != null)
        {
            return terrain.terrainData.size; // Use Unity Terrain size
        }

        Collider terrainCollider = terrainObj.GetComponent<Collider>();
        if (terrainCollider != null)
        {
            return terrainCollider.bounds.size; // Use Collider size if available
        }

        Debug.LogWarning("No Terrain or Collider found on terrainPrefab. Defaulting to (10,1,10).");
        return new Vector3(10, 1, 10); // Default size if no valid component found
    }

    void RemoveTerrain()
    {
        if (spawnedTerrain != null)
        {
            Destroy(spawnedTerrain, 0.5f);
            spawnedTerrain = null;
            terrainSpawned = false;
        }
    }

    void SwitchCamera()
    {
        if (mainCamera != null && secondaryCamera != null)
        {
            bool isMainActive = mainCamera.enabled;

            mainCamera.enabled = !isMainActive;
            secondaryCamera.enabled = isMainActive;

            // Pass the active camera to CameraController
            if (cameraController != null)
            {
                cameraController.SetActiveCamera(isMainActive ? secondaryCamera : mainCamera);
            }

            mainCamera.gameObject.SetActive(true);
            secondaryCamera.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Cameras are missing! Assign them in the Inspector.");
        }
    }
}
