using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Robot
{
    /// <summary>
    /// Manages XR initialization and helps prevent conflicts between different XR SDKs
    /// </summary>
    public class XRInitManager : MonoBehaviour
    {
        [Header("XR Settings")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private float initializationDelay = 1.0f;

        private bool _isInitialized = false;

        void Start()
        {
            if (initializeOnStart)
            {
                StartCoroutine(InitializeXRDelayed());
            }
        }

        private System.Collections.IEnumerator InitializeXRDelayed()
        {
            Debug.Log("XRInitManager: Starting XR initialization...");

            // Wait for the specified delay to ensure all systems are ready
            yield return new WaitForSeconds(initializationDelay);

            // try
            // {
                // Check if XR is already initialized
                if (XRGeneralSettings.Instance != null &&
                    XRGeneralSettings.Instance.Manager != null &&
                    XRGeneralSettings.Instance.Manager.isInitializationComplete)
                {
                    Debug.Log("XRInitManager: XR already initialized");
                    _isInitialized = true;
                }
                else
                {
                    Debug.Log("XRInitManager: Waiting for XR initialization...");

                    // Wait up to 10 seconds for XR to initialize
                    float waitTime = 0f;
                    while (!_isInitialized && waitTime < 10f)
                    {
                        if (XRGeneralSettings.Instance != null &&
                            XRGeneralSettings.Instance.Manager != null &&
                            XRGeneralSettings.Instance.Manager.isInitializationComplete)
                        {
                            _isInitialized = true;
                            Debug.Log("XRInitManager: XR initialization detected");
                        }

                        yield return new WaitForSeconds(0.1f);
                        waitTime += 0.1f;
                    }

                    if (!_isInitialized)
                    {
                        Debug.LogWarning("XRInitManager: XR initialization timed out");
                    }
                }

                // Log available XR devices
                LogXRDevices();

                // Log subsystems
                LogXRSubsystems();
            // }
            // catch (System.Exception e)
            // {
                // Debug.LogError($"XRInitManager: Error during XR initialization: {e.Message}");
            // }
        }

        private void LogXRDevices()
        {
            try
            {
                var devices = new System.Collections.Generic.List<InputDevice>();
                InputDevices.GetDevices(devices);

                Debug.Log($"XRInitManager: Found {devices.Count} XR input devices:");
                foreach (var device in devices)
                {
                    Debug.Log($"  - {device.name} ({device.characteristics})");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"XRInitManager: Failed to log XR devices: {e.Message}");
            }
        }

        private void LogXRSubsystems()
        {
            try
            {
                if (XRGeneralSettings.Instance?.Manager?.activeLoader != null)
                {
                    var loader = XRGeneralSettings.Instance.Manager.activeLoader;
                    Debug.Log($"XRInitManager: Active XR Loader: {loader.GetType().Name}");

                    // Check for hand tracking subsystem
                    var handSubsystem = loader.GetLoadedSubsystem<UnityEngine.XR.Hands.XRHandSubsystem>();
                    if (handSubsystem != null)
                    {
                        Debug.Log($"XRInitManager: Hand tracking subsystem found - Running: {handSubsystem.running}");
                    }
                    else
                    {
                        Debug.Log("XRInitManager: No hand tracking subsystem found");
                    }
                }
                else
                {
                    Debug.Log("XRInitManager: No active XR loader found");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"XRInitManager: Failed to log XR subsystems: {e.Message}");
            }
        }

        public bool IsXRInitialized => _isInitialized;

        /// <summary>
        /// Manually trigger XR initialization check
        /// </summary>
        public void CheckXRInitialization()
        {
            StartCoroutine(InitializeXRDelayed());
        }
    }
}
