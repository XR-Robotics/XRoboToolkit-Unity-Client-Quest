using System;
using LitJson;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Unity.Collections;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Robot
{
    public class TrackingData
    {
        public static bool HeadOn { get; private set; }
        public static bool ControllerOn { get; private set; }
        public static bool HandTrackingOn { get; private set; }
        public static TrackingType TrackingTypeValue { get; private set; }

        private JsonData _motionTrackingJson = new JsonData();
        private JsonData _bodyTrackingJson = new JsonData();
        private JsonData _controllerDataJson = new JsonData();
        private JsonData _leftControllerJson = new JsonData();
        private JsonData _rightControllerJson = new JsonData();

        private JsonData _stateData = new JsonData();
        private JsonData _handData = new JsonData();
        private JsonData _leftHandData = new JsonData();
        private JsonData _rightHandData = new JsonData();

        public static void SetHeadOn(bool on)
        {
            HeadOn = on;
        }

        public static void SetControllerOn(bool on)
        {
            ControllerOn = on;
        }

        public static void SetHandTrackingOn(bool on)
        {
            HandTrackingOn = on;
        }

        public static void SetTrackingType(TrackingType trackingType)
        {
            TrackingTypeValue = trackingType;
        }

        public static bool HasTracking
        {
            get
            {
                return (HeadOn || ControllerOn || HandTrackingOn ||
                        TrackingTypeValue > TrackingType.None);
            }
        }

        public void Get(ref JsonData totalData)
        {
            try
            {
                //sensor
                double predictTime = Time.unscaledTimeAsDouble * 1000000; // Convert to microseconds
                totalData["predictTime"] = predictTime; //微秒，对应camera录制中帧插入的时间戳
                totalData["appState"] = _stateData;
                _stateData["focus"] = Application.isFocused;

                if (HeadOn)
                {
                    JsonData sensorJson = GetHeadTrackingData();
                    totalData["Head"] = sensorJson;
                }
                else
                {
                    if (totalData.ContainsKey("Head"))
                        totalData.Remove("Head");
                }

                if (ControllerOn)
                {
                    JsonData controller = GetLeftRightControllerJsonData(predictTime);
                    totalData["Controller"] = controller;
                }
                else
                {
                    if (totalData.ContainsKey("Controller"))
                        totalData.Remove("Controller");
                }

                if (HandTrackingOn)
                {
                    totalData["Hand"] = GetHandJsonData();
                }
                else
                {
                    if (totalData.ContainsKey("Hand"))
                        totalData.Remove("Hand");
                }

                // Remove PICO-specific tracking features for now
                // Body and Motion tracking would need to be implemented with OpenXR equivalents
                if (totalData.ContainsKey("Body"))
                    totalData.Remove("Body");
                if (totalData.ContainsKey("Motion"))
                    totalData.Remove("Motion");

                long nsTime = Utils.GetCurrentTimestamp();
                totalData["timeStampNs"] = nsTime;

                // For OpenXR, we determine active input device based on what's available
                int inputDevice = GetActiveInputDevice();
                totalData["Input"] = inputDevice;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TrackingData.Get() failed: {e.Message}");
                // Initialize minimal data structure to prevent further crashes
                if (!totalData.ContainsKey("predictTime"))
                    totalData["predictTime"] = Time.unscaledTimeAsDouble * 1000000;
                if (!totalData.ContainsKey("timeStampNs"))
                    totalData["timeStampNs"] = System.DateTime.UtcNow.Ticks;
                if (!totalData.ContainsKey("Input"))
                    totalData["Input"] = 0;
            }
        }

        private JsonData GetHeadTrackingData()
        {
            JsonData jsonData = new JsonData();

            try
            {
                // Get head tracking data from Unity XR
                InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if (headDevice.isValid)
                {
                    if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
                        headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                    {
                        jsonData["pose"] = GetPoseStr(position, rotation);
                        jsonData["status"] = 1;
                    }
                    else
                    {
                        jsonData["status"] = 0;
                    }
                }
                else
                {
                    jsonData["status"] = 0;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Head tracking failed: {e.Message}");
                jsonData["status"] = 0;
            }

            return jsonData;
        }

        private int GetActiveInputDevice()
        {
            try
            {
                // Check if hand tracking is available and active
                if (XRGeneralSettings.Instance != null &&
                    XRGeneralSettings.Instance.Manager != null &&
                    XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    var handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                    if (handSubsystem != null && handSubsystem.running)
                    {
                        // Check if either hand is actually tracked
                        if (handSubsystem.leftHand.isTracked || handSubsystem.rightHand.isTracked)
                        {
                            return 2; // HandTrackingActive equivalent
                        }
                    }
                }

                // Check if controllers are connected
                var leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                var rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                if ((leftController.isValid && leftController.characteristics.HasFlag(InputDeviceCharacteristics.Controller)) ||
                    (rightController.isValid && rightController.characteristics.HasFlag(InputDeviceCharacteristics.Controller)))
                {
                    return 1; // ControllerActive equivalent
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to get active input device: {e.Message}");
            }

            return 0; // HeadActive equivalent
        }


        // Remove PICO-specific tracking methods - these would need OpenXR equivalents
        // GetMotionTracking() and GetBodyTracking() methods removed
        // These features would require specific OpenXR extensions or alternative implementations


        private JsonData GetLeftRightControllerJsonData(double predictTime)
        {
            try
            {
                // Clear previous data
                _controllerDataJson = new JsonData();

                // Get left controller data
                InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                if (leftController.isValid && leftController.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
                {
                    // Get position and rotation
                    if (leftController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 leftPosition) &&
                        leftController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion leftRotation))
                    {
                        _leftControllerJson = new JsonData();
                        GetControllerJsonData(leftController, ref _leftControllerJson);
                        _controllerDataJson["left"] = _leftControllerJson;
                        _controllerDataJson["left"]["pose"] = GetPoseStr(leftPosition, leftRotation);
                    }
                }

                // Get right controller data
                InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                if (rightController.isValid && rightController.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
                {
                    // Get position and rotation
                    if (rightController.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 rightPosition) &&
                        rightController.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rightRotation))
                    {
                        _rightControllerJson = new JsonData();
                        GetControllerJsonData(rightController, ref _rightControllerJson);
                        _controllerDataJson["right"] = _rightControllerJson;
                        _controllerDataJson["right"]["pose"] = GetPoseStr(rightPosition, rightRotation);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Controller tracking failed: {e.Message}");
            }

            return _controllerDataJson;
        }

        private static void GetControllerJsonData(InputDevice controllerDevice, ref JsonData json)
        {
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
        }


        private JsonData GetHandJsonData()
        {
            try
            {
                // Clear previous data
                _handData = new JsonData();

                if (XRGeneralSettings.Instance == null ||
                    XRGeneralSettings.Instance.Manager == null ||
                    XRGeneralSettings.Instance.Manager.activeLoader == null)
                {
                    return _handData;
                }

                var handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                if (handSubsystem == null || !handSubsystem.running)
                {
                    return _handData;
                }

                // Get left hand data
                if (handSubsystem.leftHand.isTracked)
                {
                    _leftHandData = new JsonData();
                    GetXRHandTrackingData(handSubsystem.leftHand, ref _leftHandData);
                    _handData["leftHand"] = _leftHandData;
                }

                // Get right hand data
                if (handSubsystem.rightHand.isTracked)
                {
                    _rightHandData = new JsonData();
                    GetXRHandTrackingData(handSubsystem.rightHand, ref _rightHandData);
                    _handData["rightHand"] = _rightHandData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Hand tracking failed: {e.Message}");
            }

            return _handData;
        }

        private void GetXRHandTrackingData(XRHand hand, ref JsonData json)
        {
            try
            {
                json["isActive"] = hand.isTracked ? 1U : 0U;
                json["count"] = (uint)XRHandJointID.EndMarker;
                json["scale"] = 1.0f; // XR Hands doesn't provide scale directly

                JsonData jointLocationsJson = new JsonData();
                jointLocationsJson.SetJsonType(JsonType.Array);
                json["HandJointLocations"] = jointLocationsJson;

                // Iterate through all hand joints
                int jointIndex = 0;
                for (int i = 0; i < (int)XRHandJointID.EndMarker; i++)
                {
                    try
                    {
                        var jointID = XRHandJointIDUtility.FromIndex(i);
                        var joint = hand.GetJoint(jointID);

                        if (joint.TryGetPose(out Pose pose))
                        {
                            JsonData jointJson = new JsonData();
                            jointJson["p"] = GetPoseStr(pose.position, pose.rotation);

                            // XR Hands tracking state as status
                            uint status = 0;
                            if (joint.trackingState.HasFlag(XRHandJointTrackingState.Pose))
                                status |= 0x3F; // Position and orientation tracked and valid

                            jointJson["s"] = status;

                            // Try to get radius if available
                            if (joint.TryGetRadius(out float radius))
                                jointJson["r"] = radius;
                            else
                                jointJson["r"] = 0.01f; // Default radius

                            jointLocationsJson.Add(jointJson);
                            jointIndex++;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to get joint {i}: {e.Message}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Hand tracking data extraction failed: {e.Message}");
                json["isActive"] = 0U;
                json["count"] = 0U;
                json["scale"] = 1.0f;
                json["HandJointLocations"] = new JsonData();
            }
        }



        private string GetPoseStr(Vector3 position, Quaternion rotation)
        {
            return position.x.ToString("R") + "," + position.y.ToString("R") + "," + position.z.ToString("R") + "," +
                   rotation.x.ToString("R") + "," + rotation.y.ToString("R") + "," + rotation.z.ToString("R") + "," +
                   rotation.w.ToString("R");
        }

        public enum HandMode
        {
            Non = 0,
            Controller = 1,
            Hand = 2
        }

        public enum TrackingType
        {
            None = 0,
            // Body and Motion tracking would require OpenXR extensions
            // Keeping enum for compatibility but functionality removed
            Body = 1,
            Motion = 2
        }
    }
}