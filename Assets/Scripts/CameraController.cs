using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera activeCamera;

    void Start()
    {
        // Initialize the active camera (can be main or secondary)
        activeCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Ensure active camera is properly referenced
        if (activeCamera != null)
        {
            Vector3 cameraEuler = activeCamera.transform.eulerAngles;
            cameraEuler.x = Mathf.Clamp(cameraEuler.x, 0f, 75f); // Restrict downward view
            activeCamera.transform.eulerAngles = cameraEuler;
        }
    }

    // Call this function from PlaneObjectSpawner when switching cameras
    public void SetActiveCamera(Camera newCamera)
    {
        activeCamera = newCamera;
    }
}
