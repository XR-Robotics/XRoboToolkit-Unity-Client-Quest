using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class ToggleCameraClippingPlane : MonoBehaviour
{
    [Header("Cameras to Adjust")]
    [SerializeField]
    private Camera firstCamera;

    [SerializeField] private Camera secondCamera;

    public Camera centerCamera;

    [Header("Clipping Plane Values")]
    [SerializeField]
    private float nearClipValueA = 0.1f;

    [SerializeField] private float nearClipValueB = 0.35f;

    [Header("RemoteCameraWindow")] public GameObject remoteCameraWindow;

    private bool wasButtonPressed = false;
    private bool useValueA = true; // Start with the first value

    void Update()
    {
        // Return if the remote camera window is not active
        if (!remoteCameraWindow.activeSelf) return;

        // Get the right controller directly using XRNode
        var rightControllerDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // Check if B button (secondaryButton) is pressed
        bool buttonValue = false;
        rightControllerDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonValue);

        // Trigger on button release (when button was pressed before but is not pressed now)
        if (!buttonValue && wasButtonPressed)
        {
            // Toggle between the two near clip values
            float newClipValue = useValueA ? nearClipValueB : nearClipValueA;
            useValueA = !useValueA;

            // Store original values for verification
            float firstCameraOriginal = firstCamera != null ? firstCamera.nearClipPlane : -1f;
            float secondCameraOriginal = secondCamera != null ? secondCamera.nearClipPlane : -1f;

            // // Apply to first camera
            // if (firstCamera != null)
            // {
            //     firstCamera.nearClipPlane = newClipValue;
            //     Debug.Log($"First camera '{firstCamera.name}' near clip plane changed from {firstCameraOriginal} to: {firstCamera.nearClipPlane} (expected: {newClipValue})");
            // }
            // else
            // {
            //     Debug.LogWarning("First camera is null!");
            // }
            //
            // // Apply to second camera
            // if (secondCamera != null)
            // {
            //     secondCamera.nearClipPlane = newClipValue;
            //     Debug.Log($"Second camera '{secondCamera.name}' near clip plane changed from {secondCameraOriginal} to: {secondCamera.nearClipPlane} (expected: {newClipValue})");
            // }
            // else
            // {
            //     Debug.LogWarning("Second camera is null!");
            // }
            //
            centerCamera.nearClipPlane = newClipValue;

            Debug.Log($"Both cameras near clip plane should now be: {newClipValue}");
        }

        // Update button state
        wasButtonPressed = buttonValue;
    }
}
