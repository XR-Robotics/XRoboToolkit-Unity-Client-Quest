using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using LitJson;

namespace Robot.Quest
{
    public class QuestDataSource : MonoBehaviour
    {
        [Header("Controller Tracking Settings")]
        public bool enableControllerTracking = true;
        public float updateInterval = 0.1f; // Update every 100ms

        private float lastUpdateTime;
        private JsonData _controllerDataJson = new JsonData();
        private JsonData _leftControllerJson = new JsonData();
        private JsonData _rightControllerJson = new JsonData();

        void Start()
        {
            Debug.Log("QuestDataSource: Starting controller tracking");
            lastUpdateTime = Time.time;
        }

        void Update()
        {
            if (enableControllerTracking && Time.time - lastUpdateTime >= updateInterval)
            {
                JsonData controllerData = GetLeftRightControllerJsonData(0.0);
                PrintControllerPositionData(controllerData);
                lastUpdateTime = Time.time;
            }
        }

        private JsonData GetLeftRightControllerJsonData(double predictTime)
        {
            // Clear previous data
            _controllerDataJson.Clear();

            // Get left controller data
            InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (leftController.isValid)
            {
                // Get position and rotation
                leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition);
                leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation);

                GetControllerJsonData(leftController, ref _leftControllerJson);
                _controllerDataJson["left"] = _leftControllerJson;
                _controllerDataJson["left"]["pose"] = GetPoseStr(leftPosition, leftRotation);
                _controllerDataJson["left"]["position"] = GetPositionStr(leftPosition);
                _controllerDataJson["left"]["rotation"] = GetRotationStr(leftRotation);
            }

            // Get right controller data
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightController.isValid)
            {
                // Get position and rotation
                rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition);
                rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation);

                GetControllerJsonData(rightController, ref _rightControllerJson);
                _controllerDataJson["right"] = _rightControllerJson;
                _controllerDataJson["right"]["pose"] = GetPoseStr(rightPosition, rightRotation);
                _controllerDataJson["right"]["position"] = GetPositionStr(rightPosition);
                _controllerDataJson["right"]["rotation"] = GetRotationStr(rightRotation);
            }

            return _controllerDataJson;
        }

        private static void GetControllerJsonData(InputDevice controllerDevice, ref JsonData json)
        {
            // Clear previous data
            json.Clear();

            controllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var axis2D);
            controllerDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out var axisClick);
            controllerDevice.TryGetFeatureValue(CommonUsages.grip, out var grip);
            controllerDevice.TryGetFeatureValue(CommonUsages.trigger, out var trigger);
            controllerDevice.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryButton);
            controllerDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var secondaryButton);
            controllerDevice.TryGetFeatureValue(CommonUsages.menuButton, out var menuButton);

            json["axisX"] = axis2D.x;
            json["axisY"] = axis2D.y;
            json["axisClick"] = axisClick;
            json["grip"] = grip;
            json["trigger"] = trigger;
            json["primaryButton"] = primaryButton;
            json["secondaryButton"] = secondaryButton;
            json["menuButton"] = menuButton;
            json["deviceName"] = controllerDevice.name;
            json["isValid"] = controllerDevice.isValid;
        }

        private string GetPoseStr(Vector3 position, Quaternion rotation)
        {
            return $"{position.x:F4},{position.y:F4},{position.z:F4},{rotation.x:F4},{rotation.y:F4},{rotation.z:F4},{rotation.w:F4}";
        }

        private string GetPositionStr(Vector3 position)
        {
            return $"{position.x:F4},{position.y:F4},{position.z:F4}";
        }

        private string GetRotationStr(Quaternion rotation)
        {
            return $"{rotation.x:F4},{rotation.y:F4},{rotation.z:F4},{rotation.w:F4}";
        }

        private void PrintControllerPositionData(JsonData controllerData)
        {
            if (controllerData == null || controllerData.Count == 0)
            {
                Debug.LogWarning("QuestDataSource: No controller data available");
                return;
            }

            // Print left controller data
            if (controllerData.ContainsKey("left"))
            {
                var leftData = controllerData["left"];
                if (leftData.ContainsKey("position"))
                {
                    Debug.Log($"Left Controller Position: {leftData["position"]}");
                }
                if (leftData.ContainsKey("rotation"))
                {
                    Debug.Log($"Left Controller Rotation: {leftData["rotation"]}");
                }
                if (leftData.ContainsKey("pose"))
                {
                    Debug.Log($"Left Controller Pose: {leftData["pose"]}");
                }
            }

            // Print right controller data
            if (controllerData.ContainsKey("right"))
            {
                var rightData = controllerData["right"];
                if (rightData.ContainsKey("position"))
                {
                    Debug.Log($"Right Controller Position: {rightData["position"]}");
                }
                if (rightData.ContainsKey("rotation"))
                {
                    Debug.Log($"Right Controller Rotation: {rightData["rotation"]}");
                }
                if (rightData.ContainsKey("pose"))
                {
                    Debug.Log($"Right Controller Pose: {rightData["pose"]}");
                }
            }

            // Print button states for debugging
            PrintControllerButtonStates(controllerData);
        }

        private void PrintControllerButtonStates(JsonData controllerData)
        {
            // Print left controller buttons
            if (controllerData.ContainsKey("left"))
            {
                var leftData = controllerData["left"];
                if (leftData.ContainsKey("trigger") && (bool)leftData["trigger"])
                {
                    Debug.Log("Left Controller: Trigger pressed");
                }
                if (leftData.ContainsKey("grip") && (float)leftData["grip"] > 0.5f)
                {
                    Debug.Log($"Left Controller: Grip pressed ({leftData["grip"]})");
                }
                if (leftData.ContainsKey("primaryButton") && (bool)leftData["primaryButton"])
                {
                    Debug.Log("Left Controller: Primary button pressed");
                }
            }

            // Print right controller buttons
            if (controllerData.ContainsKey("right"))
            {
                var rightData = controllerData["right"];
                if (rightData.ContainsKey("trigger") && (bool)rightData["trigger"])
                {
                    Debug.Log("Right Controller: Trigger pressed");
                }
                if (rightData.ContainsKey("grip") && (float)rightData["grip"] > 0.5f)
                {
                    Debug.Log($"Right Controller: Grip pressed ({rightData["grip"]})");
                }
                if (rightData.ContainsKey("primaryButton") && (bool)rightData["primaryButton"])
                {
                    Debug.Log("Right Controller: Primary button pressed");
                }
            }
        }

        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.01f, interval); // Minimum 10ms update rate
        }

        public void ToggleControllerTracking()
        {
            enableControllerTracking = !enableControllerTracking;
            Debug.Log($"QuestDataSource: Controller tracking {(enableControllerTracking ? "enabled" : "disabled")}");
        }
    }
}