using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

public class PlaneDetectionToggle : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public Button toggleButton;
    public TMP_Text buttonText; // Use TextMeshPro for button text

    private bool planesVisible = true;

    void Start()
    {
        if (!planeManager || !toggleButton || !buttonText)
        {
            Debug.LogError("PlaneDetectionToggle: Missing references in Inspector!");
            return;
        }

        toggleButton.onClick.AddListener(TogglePlaneDetection);
        UpdateButtonText(); // Set initial button text
    }

    void TogglePlaneDetection()
    {
        if (planeManager == null) return;

        planesVisible = !planesVisible;
        planeManager.enabled = planesVisible;

        foreach (var plane in planeManager.trackables)
        {
            if (plane != null)
                plane.gameObject.SetActive(planesVisible);
        }

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        buttonText.text = planesVisible ? "Disable Plane Detection" : "Enable Plane Detection";
    }
}
