using UnityEngine;
using LitJson;

namespace Robot
{
    /// <summary>
    /// Test script for debugging hand tracking issues on Quest 3
    /// Attach this to a GameObject in your scene to test hand tracking functionality
    /// </summary>
    public class HandTrackingTester : MonoBehaviour
    {
        [Header("Testing Settings")]
        [SerializeField] private bool enableContinuousTesting = true;
        [SerializeField] private float testInterval = 2.0f;
        [SerializeField] private bool enableDetailedLogging = true;

        private TrackingData trackingData;
        private float lastTestTime;
        private int frameCount = 0;

        void Start()
        {
            trackingData = new TrackingData();
            Debug.Log("Hand Tracking Tester initialized");

            // Run initial diagnostic
            TrackingData.DiagnoseHandTracking();
        }

        void Update()
        {
            frameCount++;

            // Run continuous testing if enabled
            if (enableContinuousTesting && Time.time - lastTestTime > testInterval)
            {
                TestHandTracking();
                lastTestTime = Time.time;
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Hand Tracking Tester", new GUIStyle(GUI.skin.label) { fontSize = 16 });

            if (GUILayout.Button("Test Hand Tracking"))
            {
                TestHandTracking();
            }

            if (GUILayout.Button("Diagnose Hand Tracking"))
            {
                TrackingData.DiagnoseHandTracking();
            }

            if (GUILayout.Button("Restart Hand Tracking"))
            {
                bool success = TrackingData.RestartHandTracking();
                Debug.Log($"Hand tracking restart {(success ? "successful" : "failed")}");
            }

            if (GUILayout.Button("Toggle Hand Tracking"))
            {
                TrackingData.SetHandTrackingOn(!TrackingData.HandTrackingOn);
                Debug.Log($"Hand tracking toggled: {TrackingData.HandTrackingOn}");
            }

            // Display current status
            GUILayout.Space(10);
            GUILayout.Label($"Hand Tracking: {(TrackingData.HandTrackingOn ? "ON" : "OFF")}");
            GUILayout.Label($"Frame: {frameCount}");

            GUILayout.EndArea();
        }

        private void TestHandTracking()
        {
            Debug.Log("=== Testing Hand Tracking ===");

            try
            {
                // Enable hand tracking for testing
                TrackingData.SetHandTrackingOn(true);

                // Create test data structure
                JsonData testData = new JsonData();

                // Get tracking data
                trackingData.Get(ref testData);

                // Check if hand data was obtained
                if (testData.ContainsKey("Hand"))
                {
                    var handData = testData["Hand"];
                    bool hasLeftHand = handData.ContainsKey("leftHand");
                    bool hasRightHand = handData.ContainsKey("rightHand");

                    Debug.Log($"Hand tracking SUCCESS - Left: {hasLeftHand}, Right: {hasRightHand}");

                    if (enableDetailedLogging)
                    {
                        if (hasLeftHand)
                        {
                            var leftHand = handData["leftHand"];
                            Debug.Log($"Left hand active: {leftHand["isActive"]}, Joint count: {leftHand["count"]}");
                        }

                        if (hasRightHand)
                        {
                            var rightHand = handData["rightHand"];
                            Debug.Log($"Right hand active: {rightHand["isActive"]}, Joint count: {rightHand["count"]}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Hand tracking FAILED - No hand data in response");

                    // Log what we did get
                    Debug.Log($"Available keys in response: {string.Join(", ", GetJsonKeys(testData))}");
                }

                // Log input device status
                if (testData.ContainsKey("Input"))
                {
                    int inputDevice = (int)testData["Input"];
                    string deviceType = inputDevice switch
                    {
                        0 => "Head Tracking",
                        1 => "Controller",
                        2 => "Hand Tracking",
                        _ => $"Unknown ({inputDevice})"
                    };
                    Debug.Log($"Active input device: {deviceType}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Hand tracking test failed: {e.Message}");
            }

            Debug.Log("=== End Hand Tracking Test ===");
        }

        private string[] GetJsonKeys(JsonData jsonData)
        {
            var keys = new System.Collections.Generic.List<string>();
            if (jsonData != null && jsonData.IsObject)
            {
                foreach (string key in jsonData.Keys)
                {
                    keys.Add(key);
                }
            }
            return keys.ToArray();
        }
    }
}
