using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR; // Required for XR input
using System.IO; // Required for file operations

public class SetLERE : MonoBehaviour
{

    public GameObject CanvLE;
    public GameObject CanvRE;
    public RawImage RemoteCameraImage;
    public Material matLE;
    public Material matRE;

    private float visibleRatio = 0.555f;
    private float contentRatio = 1.8f;

    private InputDevice rightController;
    private bool isAdjusting = false;
    private bool triggerPressedLastFrame = false;
    
    [Space(30)]
    [Header("Status")]
    public Text StatusText;

    private string configFilePath;

    // Define a simple class for our configuration data
    [System.Serializable]
    public class RatioConfig
    {
        public float visibleRatio;
        public float contentRatio;
    }

    void Awake()
    {
        // Set the path for the config file
        // Application.persistentDataPath is a good place for config files on Android/Pico
        configFilePath = Path.Combine(Application.persistentDataPath, "lere_config.json");
        LoadConfig(); // Load config when the script awakes
    }

    void Start()
    {
        InitializeRenderSettings();
        TryGetRightController();
    }

    void Update()
    {
        // Try to get the controller if not already found
        if (!rightController.isValid)
        {
            TryGetRightController();
        }

        // Only proceed if the controller is valid
        if (rightController.isValid)
        {
            HandleControllerInput();
        }
        else
        {
            // Fallback for debugging or if controller not found (optional: keep keyboard input)
            HandleKeyboardInput();
        }
    }

    private void InitializeRenderSettings()
    {
        if ((!CanvLE.activeSelf) || (!CanvRE.activeSelf))
        {
            CanvLE.SetActive(true);
            CanvRE.SetActive(true);
        }

        if (RemoteCameraImage.texture != null)
        {
            matLE.SetTexture("_mainRT", RemoteCameraImage.texture);
            matRE.SetTexture("_mainRT", RemoteCameraImage.texture);
        }
        else
        {
            Debug.LogWarning("RemoteCameraImage.texture is null. Ensure it's assigned and active.");
        }

        matLE.SetInt("_isLE", 1);
        matRE.SetInt("_isLE", 0);

        ApplyRatiosToMaterials();
        Debug.Log($"Initial Ratios - visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
    }

    private void TryGetRightController()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count > 0)
        {
            rightController = devices[0];
            Debug.Log($"Right controller found: {rightController.name}");
        }
    }

    private void HandleControllerInput()
    {
        bool currentGripState = false;
        if (rightController.TryGetFeatureValue(CommonUsages.gripButton, out currentGripState) &&
            currentGripState)
        {
            if (!triggerPressedLastFrame)
            {
                // Trigger was just pressed
                isAdjusting = !isAdjusting; // Toggle adjustment mode
                if (!isAdjusting)
                {
                    SaveConfig(); // Save when exiting adjustment mode
                }

                Debug.Log($"Adjustment mode toggled: {isAdjusting}");
            }

            triggerPressedLastFrame = true;
        }
        else
        {
            triggerPressedLastFrame = false;
        }
        
        // Update status
        StatusText.text = $"Adjusting: {isAdjusting}";

        if (isAdjusting)
        {
            Vector2 joystickValue;
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue))
            {
                // Adjust visibleRatio with Y-axis of joystick
                if (Mathf.Abs(joystickValue.y) > 0.1f) // Dead zone
                {
                    visibleRatio += joystickValue.y * Time.deltaTime * 0.1f; // Adjust sensitivity
                    ApplyRatiosToMaterials();
                    Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
                }

                // Adjust contentRatio with X-axis of joystick
                if (Mathf.Abs(joystickValue.x) > 0.1f) // Dead zone
                {
                    contentRatio += joystickValue.x * Time.deltaTime * 0.1f; // Adjust sensitivity
                    ApplyRatiosToMaterials();
                    Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
                }
            }
        }
    }

    private void ApplyRatiosToMaterials()
    {
        matLE.SetFloat("_visibleRatio", visibleRatio);
        matRE.SetFloat("_visibleRatio", visibleRatio);
        matLE.SetFloat("_contentRatio", contentRatio);
        matRE.SetFloat("_contentRatio", contentRatio);
    }

    // Optional: Keep keyboard input for development/debugging
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            visibleRatio += 0.005f;
            ApplyRatiosToMaterials();
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            visibleRatio -= 0.005f;
            ApplyRatiosToMaterials();
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            contentRatio += 0.005f;
            ApplyRatiosToMaterials();
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            contentRatio -= 0.005f;
            ApplyRatiosToMaterials();
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }
    }

    private void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            try
            {
                string json = File.ReadAllText(configFilePath);
                RatioConfig config = JsonUtility.FromJson<RatioConfig>(json);
                visibleRatio = config.visibleRatio;
                contentRatio = config.contentRatio;
                Debug.Log(
                    $"Config loaded from {configFilePath}. visibleRatio: {visibleRatio}, contentRatio: {contentRatio}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading config file: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"Config file not found at {configFilePath}. Using default values.");
            // Optionally save default values if no config exists
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            RatioConfig config = new RatioConfig
            {
                visibleRatio = visibleRatio,
                contentRatio = contentRatio
            };
            string json = JsonUtility.ToJson(config, true); // true for pretty print
            File.WriteAllText(configFilePath, json);
            Debug.Log($"Config saved to {configFilePath}. visibleRatio: {visibleRatio}, contentRatio: {contentRatio}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving config file: {e.Message}");
        }
    }
}