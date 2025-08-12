using System;
using LitJson;
using UnityEngine;
using UnityEngine.XR;
// using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Unity.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Input;
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

        private QuestTrackingDataSource _questTrackingDataSource;

        public QuestTrackingDataSource questTrackingDataSource
        {
            get
            {
                if (_questTrackingDataSource == null)
                {
                    _questTrackingDataSource = GameObject.FindObjectOfType<QuestTrackingDataSource>();
                }

                return _questTrackingDataSource;
            }
        }

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
                    GetHandJsonData();
                    totalData["Hand"] = _handData;
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
                int inputDevice = GetActiveInputDevice(); //1
                totalData["Input"] = inputDevice;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TrackingData.Get() failed: {e.StackTrace}");
                // Initialize minimal data structure to prevent further crashes
                if (!totalData.ContainsKey("predictTime"))
                    totalData["predictTime"] = Time.unscaledTimeAsDouble * 1000000;
                if (!totalData.ContainsKey("timeStampNs"))
                    totalData["timeStampNs"] = System.DateTime.UtcNow.Ticks;
                if (!totalData.ContainsKey("Input"))
                    totalData["Input"] = 0;
            }
        }

        void ConvertHandedness(ref Vector3 position, ref Quaternion rotation)
        {
            position.z *= -1;
            rotation.z *= -1;
            rotation.w *= -1;
        }

        private JsonData GetHeadTrackingData()
        {
            JsonData jsonData = new JsonData();

            var (valid, pose) = questTrackingDataSource.GetHeadsetPose();
            if (valid)
            {
                var position = pose.position;
                var rotation = pose.rotation;
                ConvertHandedness(ref position, ref rotation);
                jsonData["pose"] = GetPoseStr(position, rotation);
                jsonData["status"] = 3; // Match PICO
            }
            else
            {
                jsonData["status"] = 0;
            }

            return jsonData;
        }

        private int GetActiveInputDevice()
        {
            return questTrackingDataSource.GetActiveInputDevice();
        }


        // Remove PICO-specific tracking methods - these would need OpenXR equivalents
        // GetMotionTracking() and GetBodyTracking() methods removed
        // These features would require specific OpenXR extensions or alternative implementations

        private JsonData GetControllerJsonData(double predictTime, Handedness handedness)
        {
            InputDevice controller;
            var pose = Pose.identity;
            if (handedness == Handedness.Left)
            {
                controller = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                pose = questTrackingDataSource.GetControllerPose(Handedness.Left);
            }
            else
            {
                controller = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                pose = questTrackingDataSource.GetControllerPose(Handedness.Right);
            }
            
            Debug.Log("Handedness: " + handedness + ", Controller: " + controller.name + ", Pose: " + pose);

            var json = new JsonData();
            GetControllerJsonData(controller, ref json);

            var position = pose.position;
            var rotation = pose.rotation;
            ConvertHandedness(ref position, ref rotation);
            json["pose"] = GetPoseStr(position, rotation);
            return json;
        }

        private JsonData GetLeftRightControllerJsonData(double predictTime)
        {
            _leftControllerJson = GetControllerJsonData(predictTime, Handedness.Left);
            _controllerDataJson["left"] = _leftControllerJson;

            _rightControllerJson = GetControllerJsonData(predictTime, Handedness.Right);
            _controllerDataJson["right"] = _rightControllerJson;

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

            // controllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var position);
            // controllerDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation);

            // // Adjust to match the PICO coordinate system
            // position.z *= -1;
            // rotation.z *= -1;
            // rotation.w *= -1;

            json["axisX"] = axis2D.x;
            json["axisY"] = axis2D.y;
            json["axisClick"] = axisClick;
            json["grip"] = grip;
            json["trigger"] = trigger;
            json["primaryButton"] = primaryButton;
            json["secondaryButton"] = secondaryButton;
            json["menuButton"] = menuButton;
            // json["pose"] = GetPoseStr(position, rotation);
        }


        private void GetHandJsonData()
        {
            // Clear previous data
            _handData = new JsonData();

            _leftHandData = new JsonData();
            GetOVRHandTrackingData(Handedness.Left, ref _leftHandData);
            _handData["leftHand"] = _leftHandData;

            _rightHandData = new JsonData();
            GetOVRHandTrackingData(Handedness.Right, ref _rightHandData);
            _handData["rightHand"] = _rightHandData;
        }

        private void GetOVRHandTrackingData(Handedness handedness, ref JsonData json)
        {
            var status = questTrackingDataSource.IsHandTrackingActive(handedness);

            json["isActive"] = status ? 1U : 0U;
            json["count"] = 26; // Total number of bones
            json["scale"] = 1.0f;

            JsonData jointLocationsJson = new JsonData();
            jointLocationsJson.SetJsonType(JsonType.Array);
            json["HandJointLocations"] = jointLocationsJson;


            Pose[] joints = new Pose[26]; // to be global variable

            questTrackingDataSource.GetJoints(handedness, ref joints);

            foreach (var joint in joints)
            {
                var position = joint.position;
                var rotation = joint.rotation;

                ConvertHandedness(ref position, ref rotation);

                JsonData jointJson = new JsonData();

                jointJson["p"] = GetPoseStr(position, rotation);

                jointJson["s"] = status ? 1 : 0;

                // Set radius (can be obtained from skeleton or use default)
                jointJson["r"] = 0.0f;

                jointLocationsJson.Add(jointJson);
            }
        }

        // /// <summary>
        // /// Maps OVRPlugin hand status to OpenXR-compatible status flags
        // /// </summary>
        // private uint GetOVRTrackingStatus(OVRPlugin.HandStatus handStatus)
        // {
        //     uint status = 0;
        //
        //     // Bit layout (semantic mapping based on HandLocationStatus):
        //     // Bit 0: Position valid           => 0x01
        //     // Bit 1: Position tracked         => 0x02
        //     // Bit 2: Orientation valid        => 0x04
        //     // Bit 3: Orientation tracked      => 0x08
        //
        //     if (handStatus == OVRPlugin.HandStatus.HandTracked)
        //     {
        //         status |= 0x01; // Position valid
        //         status |= 0x02; // Position tracked
        //         status |= 0x04; // Orientation valid
        //         status |= 0x08; // Orientation tracked
        //     }
        //
        //     return status;
        // }

        // /// <summary>
        // /// Provides default radius values for OVRPlugin bone types
        // /// </summary>
        // private float GetDefaultJointRadiusForOVRBone(OVRPlugin.BoneId boneId)
        // {
        //     // Default radii based on typical hand joint sizes (in meters)
        //     switch (boneId)
        //     {
        //         case OVRPlugin.BoneId.Hand_WristRoot:
        //             return 0.025f;
        //
        //         // Thumb joints
        //         case OVRPlugin.BoneId.Hand_Thumb0:
        //         case OVRPlugin.BoneId.Hand_Thumb1:
        //         case OVRPlugin.BoneId.Hand_Thumb2:
        //         case OVRPlugin.BoneId.Hand_Thumb3:
        //             return 0.012f;
        //
        //         // Index finger joints
        //         case OVRPlugin.BoneId.Hand_Index1:
        //         case OVRPlugin.BoneId.Hand_Index2:
        //         case OVRPlugin.BoneId.Hand_Index3:
        //             return 0.008f;
        //
        //         // Middle finger joints
        //         case OVRPlugin.BoneId.Hand_Middle1:
        //         case OVRPlugin.BoneId.Hand_Middle2:
        //         case OVRPlugin.BoneId.Hand_Middle3:
        //             return 0.008f;
        //
        //         // Ring finger joints
        //         case OVRPlugin.BoneId.Hand_Ring1:
        //         case OVRPlugin.BoneId.Hand_Ring2:
        //         case OVRPlugin.BoneId.Hand_Ring3:
        //             return 0.007f;
        //
        //         // Little finger joints
        //         case OVRPlugin.BoneId.Hand_Pinky0:
        //         case OVRPlugin.BoneId.Hand_Pinky1:
        //         case OVRPlugin.BoneId.Hand_Pinky2:
        //         case OVRPlugin.BoneId.Hand_Pinky3:
        //             return 0.006f;
        //
        //         default:
        //             return 0.01f; // Generic default
        //     }
        // }

        // /// <summary>
        // /// Provides default radius values for different joint types based on OpenXR recommendations
        // /// </summary>
        // private float GetDefaultJointRadius(XRHandJointID jointID)
        // {
        //     // Default radii based on typical hand joint sizes (in meters)
        //     switch (jointID)
        //     {
        //         case XRHandJointID.Wrist:
        //             return 0.025f;
        //         case XRHandJointID.Palm:
        //             return 0.03f;
        //
        //         // Thumb joints
        //         case XRHandJointID.ThumbMetacarpal:
        //         case XRHandJointID.ThumbProximal:
        //         case XRHandJointID.ThumbDistal:
        //         case XRHandJointID.ThumbTip:
        //             return 0.012f;
        //
        //         // Index finger joints
        //         case XRHandJointID.IndexMetacarpal:
        //         case XRHandJointID.IndexProximal:
        //         case XRHandJointID.IndexIntermediate:
        //         case XRHandJointID.IndexDistal:
        //         case XRHandJointID.IndexTip:
        //             return 0.008f;
        //
        //         // Middle finger joints
        //         case XRHandJointID.MiddleMetacarpal:
        //         case XRHandJointID.MiddleProximal:
        //         case XRHandJointID.MiddleIntermediate:
        //         case XRHandJointID.MiddleDistal:
        //         case XRHandJointID.MiddleTip:
        //             return 0.008f;
        //
        //         // Ring finger joints
        //         case XRHandJointID.RingMetacarpal:
        //         case XRHandJointID.RingProximal:
        //         case XRHandJointID.RingIntermediate:
        //         case XRHandJointID.RingDistal:
        //         case XRHandJointID.RingTip:
        //             return 0.007f;
        //
        //         // Little finger joints
        //         case XRHandJointID.LittleMetacarpal:
        //         case XRHandJointID.LittleProximal:
        //         case XRHandJointID.LittleIntermediate:
        //         case XRHandJointID.LittleDistal:
        //         case XRHandJointID.LittleTip:
        //             return 0.006f;
        //
        //         default:
        //             return 0.01f; // Generic default
        //     }
        // }


        private static string GetPoseStr(Vector3 position, Quaternion rotation)
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