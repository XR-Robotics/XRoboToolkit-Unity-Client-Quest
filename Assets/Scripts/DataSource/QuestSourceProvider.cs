using UnityEngine;
using LitJson;
using Robot;
using System.Collections;

namespace Robot.DataSource
{
    public class QuestSourceProvider : MonoBehaviour
    {
        [Header("Tracking Settings")]
        [SerializeField] private bool enableHeadTracking = true;
        [SerializeField] private bool enableControllerTracking = true;
        [SerializeField] private bool enableHandTracking = true;
        [SerializeField] private bool enableBodyTracking = false;
        [SerializeField] private bool enableMotionTracking = false;

        [Header("Debug Settings")]
        [SerializeField] private bool printTrackingData = true;
        [SerializeField] private float printInterval = 1.0f; // Print every second

        private TrackingData trackingData;
        private JsonData totalData;
        private float lastPrintTime;

        void Start()
        {
            try
            {
                // Initialize tracking data
                trackingData = new TrackingData();
                totalData = new JsonData();

                // Wait a frame before configuring tracking to ensure XR is initialized
                StartCoroutine(InitializeTrackingDelayed());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize QuestSourceProvider: {e.Message}");
                enabled = false; // Disable the component if initialization fails
            }
        }

        private System.Collections.IEnumerator InitializeTrackingDelayed()
        {
            // Wait a few frames for XR subsystems to initialize
            yield return new WaitForSeconds(0.5f);

            try
            {
                // Configure tracking based on settings
                TrackingData.SetHeadOn(enableHeadTracking);
                TrackingData.SetControllerOn(enableControllerTracking);
                TrackingData.SetHandTrackingOn(enableHandTracking);

                if (enableBodyTracking)
                    TrackingData.SetTrackingType(TrackingData.TrackingType.Body);
                else if (enableMotionTracking)
                    TrackingData.SetTrackingType(TrackingData.TrackingType.Motion);
                else
                    TrackingData.SetTrackingType(TrackingData.TrackingType.None);

                Debug.Log("QuestSourceProvider initialized with tracking settings:");
                Debug.Log($"Head: {enableHeadTracking}, Controllers: {enableControllerTracking}, Hands: {enableHandTracking}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to configure tracking: {e.Message}");
            }
        }

        void Update()
        {
            // Make sure tracking data is initialized
            if (trackingData == null || totalData == null)
                return;

            try
            {
                // Get tracking data
                trackingData.Get(ref totalData);

                // Print tracking data at specified intervals
                if (printTrackingData && Time.time - lastPrintTime >= printInterval)
                {
                    PrintTrackingData();
                    lastPrintTime = Time.time;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Tracking data update failed: {e.Message}");
            }
        }

        private void PrintTrackingData()
        {
            Debug.Log("=== Quest Tracking Data ===");

            // Print general info
            if (totalData.ContainsKey("predictTime"))
                Debug.Log($"Predict Time: {totalData["predictTime"]} microseconds");

            if (totalData.ContainsKey("timeStampNs"))
                Debug.Log($"Timestamp: {totalData["timeStampNs"]} ns");

            if (totalData.ContainsKey("Input"))
                Debug.Log($"Active Input Device: {GetInputDeviceName((int)totalData["Input"])}");

            // Print app state
            if (totalData.ContainsKey("appState"))
            {
                var appState = totalData["appState"];
                if (appState.ContainsKey("focus"))
                    Debug.Log($"Application Focus: {appState["focus"]}");
            }

            // Print head tracking data
            if (totalData.ContainsKey("Head"))
            {
                PrintHeadTrackingData(totalData["Head"]);
            }

            // Print controller tracking data
            if (totalData.ContainsKey("Controller"))
            {
                PrintControllerTrackingData(totalData["Controller"]);
            }

            // Print hand tracking data
            if (totalData.ContainsKey("Hand"))
            {
                PrintHandTrackingData(totalData["Hand"]);
            }

            Debug.Log("=== End Tracking Data ===");
        }

        private void PrintHeadTrackingData(JsonData headData)
        {
            Debug.Log("--- Head Tracking ---");

            if (headData.ContainsKey("pose"))
            {
                string poseStr = (string)headData["pose"];
                var poseValues = poseStr.Split(',');
                if (poseValues.Length >= 7)
                {
                    Debug.Log($"Head Position: ({poseValues[0]}, {poseValues[1]}, {poseValues[2]})");
                    Debug.Log($"Head Rotation: ({poseValues[3]}, {poseValues[4]}, {poseValues[5]}, {poseValues[6]})");
                }
            }

            if (headData.ContainsKey("status"))
                Debug.Log($"Head Status: {headData["status"]}");
        }

        private void PrintControllerTrackingData(JsonData controllerData)
        {
            Debug.Log("--- Controller Tracking ---");

            // Left controller
            if (controllerData.ContainsKey("left"))
            {
                Debug.Log("Left Controller:");
                PrintSingleControllerData(controllerData["left"]);
            }

            // Right controller
            if (controllerData.ContainsKey("right"))
            {
                Debug.Log("Right Controller:");
                PrintSingleControllerData(controllerData["right"]);
            }
        }

        private void PrintSingleControllerData(JsonData controller)
        {
            if (controller.ContainsKey("pose"))
            {
                string poseStr = (string)controller["pose"];
                var poseValues = poseStr.Split(',');
                if (poseValues.Length >= 7)
                {
                    Debug.Log($"  Position: ({poseValues[0]}, {poseValues[1]}, {poseValues[2]})");
                    Debug.Log($"  Rotation: ({poseValues[3]}, {poseValues[4]}, {poseValues[5]}, {poseValues[6]})");
                }
            }

            // Print button states
            if (controller.ContainsKey("trigger"))
                Debug.Log($"  Trigger: {controller["trigger"]}");

            if (controller.ContainsKey("grip"))
                Debug.Log($"  Grip: {controller["grip"]}");

            if (controller.ContainsKey("axisX") && controller.ContainsKey("axisY"))
                Debug.Log($"  Thumbstick: ({controller["axisX"]}, {controller["axisY"]})");

            if (controller.ContainsKey("primaryButton"))
                Debug.Log($"  Primary Button: {controller["primaryButton"]}");

            if (controller.ContainsKey("secondaryButton"))
                Debug.Log($"  Secondary Button: {controller["secondaryButton"]}");

            if (controller.ContainsKey("menuButton"))
                Debug.Log($"  Menu Button: {controller["menuButton"]}");
        }

        private void PrintHandTrackingData(JsonData handData)
        {
            Debug.Log("--- Hand Tracking ---");

            // Left hand
            if (handData.ContainsKey("leftHand"))
            {
                Debug.Log("Left Hand:");
                PrintSingleHandData(handData["leftHand"]);
            }

            // Right hand
            if (handData.ContainsKey("rightHand"))
            {
                Debug.Log("Right Hand:");
                PrintSingleHandData(handData["rightHand"]);
            }
        }

        private void PrintSingleHandData(JsonData hand)
        {
            if (hand.ContainsKey("isActive"))
                Debug.Log($"  Is Active: {hand["isActive"]}");

            if (hand.ContainsKey("count"))
                Debug.Log($"  Joint Count: {hand["count"]}");

            if (hand.ContainsKey("scale"))
                Debug.Log($"  Hand Scale: {hand["scale"]}");

            // Print some key joint positions (wrist, thumb tip, index tip)
            if (hand.ContainsKey("HandJointLocations"))
            {
                var joints = hand["HandJointLocations"];
                if (joints.IsArray && joints.Count > 0)
                {
                    Debug.Log($"  Total Joints: {joints.Count}");

                    // Print first few joints as examples
                    int maxJointsToPrint = Mathf.Min(5, joints.Count);
                    for (int i = 0; i < maxJointsToPrint; i++)
                    {
                        var joint = joints[i];
                        if (joint.ContainsKey("p"))
                        {
                            string poseStr = (string)joint["p"];
                            var poseValues = poseStr.Split(',');
                            if (poseValues.Length >= 7)
                            {
                                Debug.Log($"    Joint {i} Position: ({poseValues[0]}, {poseValues[1]}, {poseValues[2]})");
                            }
                        }
                    }
                }
            }
        }

        private string GetInputDeviceName(int inputDevice)
        {
            switch (inputDevice)
            {
                case 0: return "Head";
                case 1: return "Controllers";
                case 2: return "Hand Tracking";
                default: return "Unknown";
            }
        }

        // Public methods to enable/disable tracking at runtime
        public void EnableHeadTracking(bool enable)
        {
            enableHeadTracking = enable;
            TrackingData.SetHeadOn(enable);
            Debug.Log($"Head tracking {(enable ? "enabled" : "disabled")}");
        }

        public void EnableControllerTracking(bool enable)
        {
            enableControllerTracking = enable;
            TrackingData.SetControllerOn(enable);
            Debug.Log($"Controller tracking {(enable ? "enabled" : "disabled")}");
        }

        public void EnableHandTracking(bool enable)
        {
            enableHandTracking = enable;
            TrackingData.SetHandTrackingOn(enable);
            Debug.Log($"Hand tracking {(enable ? "enabled" : "disabled")}");
        }

        public void SetPrintInterval(float interval)
        {
            printInterval = Mathf.Max(0.1f, interval);
            Debug.Log($"Print interval set to {printInterval} seconds");
        }

        public void TogglePrintTracking()
        {
            printTrackingData = !printTrackingData;
            Debug.Log($"Print tracking data {(printTrackingData ? "enabled" : "disabled")}");
        }

        // Get the current tracking data as JSON string
        public string GetTrackingDataJson()
        {
            return totalData.ToJson();
        }

        // Check if tracking is available
        public bool HasTracking()
        {
            return TrackingData.HasTracking;
        }
    }
}