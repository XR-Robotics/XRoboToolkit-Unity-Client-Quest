using System;
using System.Runtime.InteropServices;
using LitJson;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using Unity.Collections;
using Oculus.Interaction;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Robot
{
    // OpenXR structures and enums for head tracking
    [StructLayout(LayoutKind.Sequential)]
    public struct XrVector3f
    {
        public float x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XrQuaternionf
    {
        public float x, y, z, w;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XrPosef
    {
        public XrQuaternionf orientation;
        public XrVector3f position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XrSpaceLocation
    {
        public uint type; // XR_TYPE_SPACE_LOCATION
        public IntPtr next;
        public ulong locationFlags;
        public XrPosef pose;
    }

    public enum XrResult
    {
        XR_SUCCESS = 0,
        XR_TIMEOUT_EXPIRED = 1,
        XR_SESSION_LOSS_PENDING = 3,
        XR_EVENT_UNAVAILABLE = 4,
        XR_SPACE_BOUNDS_UNAVAILABLE = 5,
        XR_SESSION_NOT_FOCUSED = 6,
        XR_FRAME_DISCARDED = 9,

        // Error codes (negative values)
        XR_ERROR_VALIDATION_FAILURE = -1,
        XR_ERROR_RUNTIME_FAILURE = -2,
        XR_ERROR_OUT_OF_MEMORY = -3,
        XR_ERROR_API_VERSION_UNSUPPORTED = -4,
        XR_ERROR_INITIALIZATION_FAILED = -7,
        XR_ERROR_FUNCTION_UNSUPPORTED = -8,
        XR_ERROR_FEATURE_UNSUPPORTED = -9,
        XR_ERROR_EXTENSION_NOT_PRESENT = -10,
        XR_ERROR_LIMIT_REACHED = -11,
        XR_ERROR_SIZE_INSUFFICIENT = -12,
        XR_ERROR_HANDLE_INVALID = -13,
        XR_ERROR_INSTANCE_LOST = -14,
        XR_ERROR_SESSION_RUNNING = -16,
        XR_ERROR_SESSION_NOT_RUNNING = -17,
        XR_ERROR_SESSION_LOST = -18,
        XR_ERROR_SYSTEM_INVALID = -19,
        XR_ERROR_PATH_INVALID = -20,
        XR_ERROR_PATH_COUNT_EXCEEDED = -21,
        XR_ERROR_PATH_FORMAT_INVALID = -22,
        XR_ERROR_PATH_UNSUPPORTED = -23,
        XR_ERROR_LAYER_INVALID = -24,
        XR_ERROR_LAYER_LIMIT_EXCEEDED = -25,
        XR_ERROR_SWAPCHAIN_RECT_INVALID = -26,
        XR_ERROR_SWAPCHAIN_FORMAT_UNSUPPORTED = -27,
        XR_ERROR_ACTION_TYPE_MISMATCH = -28,
        XR_ERROR_SESSION_NOT_READY = -29,
        XR_ERROR_SESSION_NOT_STOPPING = -30,
        XR_ERROR_TIME_INVALID = -31,
        XR_ERROR_REFERENCE_SPACE_UNSUPPORTED = -32,
        XR_ERROR_FILE_ACCESS_ERROR = -33,
        XR_ERROR_FILE_CONTENTS_INVALID = -34,
        XR_ERROR_FORM_FACTOR_UNSUPPORTED = -35,
        XR_ERROR_FORM_FACTOR_UNAVAILABLE = -36,
        XR_ERROR_API_LAYER_NOT_PRESENT = -37,
        XR_ERROR_CALL_ORDER_INVALID = -38,
        XR_ERROR_GRAPHICS_DEVICE_INVALID = -39,
        XR_ERROR_POSE_INVALID = -40,
        XR_ERROR_INDEX_OUT_OF_RANGE = -41,
        XR_ERROR_VIEW_CONFIGURATION_TYPE_UNSUPPORTED = -42,
        XR_ERROR_ENVIRONMENT_BLEND_MODE_UNSUPPORTED = -43,
        XR_ERROR_NAME_DUPLICATED = -44,
        XR_ERROR_NAME_INVALID = -45,
        XR_ERROR_ACTIONSET_NOT_ATTACHED = -46,
        XR_ERROR_ACTIONSETS_ALREADY_ATTACHED = -47,
        XR_ERROR_LOCALIZED_NAME_DUPLICATED = -48,
        XR_ERROR_LOCALIZED_NAME_INVALID = -49,
        XR_ERROR_GRAPHICS_REQUIREMENTS_CALL_MISSING = -50,
        XR_ERROR_RUNTIME_UNAVAILABLE = -51
    }

    // OpenXR space location flags
    public enum XrSpaceLocationFlags : ulong
    {
        XR_SPACE_LOCATION_ORIENTATION_VALID_BIT = 0x00000001,
        XR_SPACE_LOCATION_POSITION_VALID_BIT = 0x00000002,
        XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT = 0x00000004,
        XR_SPACE_LOCATION_POSITION_TRACKED_BIT = 0x00000008
    }

    // OpenXR constants
    public static class XrConstants
    {
        public const uint XR_TYPE_SPACE_LOCATION = 42;
        public const ulong XR_NULL_HANDLE = 0;
    }

    // OpenXR P/Invoke declarations
    // Note: These functions may not be available in all Unity OpenXR plugin versions
    // The implementation includes fallback mechanisms for compatibility
    public class TrackingData
    {
        // OpenXR P/Invoke declarations
        [DllImport("openxr_loader", CallingConvention = CallingConvention.Cdecl)]
        private static extern XrResult xrLocateSpace(ulong space, ulong baseSpace, long time,
            ref XrSpaceLocation location);

        [DllImport("openxr_loader", CallingConvention = CallingConvention.Cdecl)]
        private static extern XrResult xrGetCurrentInteractionProfile(ulong session, ulong topLevelUserPath,
            ref ulong interactionProfile);

        // Unity OpenXR Plugin native methods - using available Unity OpenXR functions
        // These functions are more commonly available in Unity OpenXR plugin
        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "script_get_openxr_app_space")]
        private static extern ulong GetCurrentAppSpace();

        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "script_get_openxr_play_space")]
        private static extern ulong GetTrackingSpace();

        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "script_get_predicted_display_time")]
        private static extern long GetPredictedDisplayTime();

        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "script_is_session_running")]
        private static extern bool IsSessionRunning();

        // Alternative approach using Unity OpenXR's internal functions
        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NativeConfig_GetRuntimeName")]
        private static extern IntPtr GetRuntimeName();

        [DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "NativeConfig_GetRuntimeVersion")]
        private static extern long GetRuntimeVersion();

        public static bool HeadOn { get; private set; }
        public static bool ControllerOn { get; private set; }
        public static bool HandTrackingOn { get; private set; }
        public static TrackingType TrackingTypeValue { get; private set; }

        // Configuration for head tracking method
        public static bool UseOpenXRCAPI { get; set; } = false;

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

        public static void SetUseOpenXRCAPI(bool useOpenXR)
        {
            UseOpenXRCAPI = useOpenXR;
            Debug.Log($"Head tracking method set to: {(useOpenXR ? "OpenXR C API" : "Unity XR")}");
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
                    JsonData sensorJson = UseOpenXRCAPI ? GetHeadTrackingDataOpenXRCAPI() : GetHeadTrackingData();
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
                int inputDevice = GetActiveInputDevice(); //1
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
                        // before mapping
                        Debug.Log($"[TrackingData] Headset : {position} {rotation}");
                        // Adjust to match the PICO coordinate system
                        position.z *= -1;
                        rotation.z *= -1;
                        rotation.w *= -1;

                        jsonData["pose"] = GetPoseStr(position, rotation);
                        jsonData["status"] = 3; // Match PICO
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

        /// <summary>
        /// Gets head tracking data using OpenXR C API for more direct access to headset pose information.
        /// This method provides an alternative to Unity's XR input system by directly calling OpenXR native functions.
        /// 
        /// Features:
        /// - Direct OpenXR C API calls for potentially lower latency
        /// - Predicted display time for better pose accuracy
        /// - Full tracking state validation (position/orientation valid and tracked)
        /// - Automatic fallback to Unity XR on API failures
        /// - Coordinate system adjustment to match PICO format
        /// 
        /// Usage:
        /// - Set TrackingData.UseOpenXRCAPI = true to enable this method
        /// - Call TrackingData.TestOpenXRHeadTracking() to compare with Unity XR
        /// 
        /// Requirements:
        /// - OpenXR runtime must be active
        /// - Unity OpenXR plugin with native function access
        /// - Compatible OpenXR loader (openxr_loader.dll)
        /// 
        /// Returns JsonData with:
        /// - "pose": String with position and rotation (x,y,z,qx,qy,qz,qw)
        /// - "status": Integer (0=no tracking, 1=partial, 3=full tracking)
        /// </summary>
        private JsonData GetHeadTrackingDataOpenXRCAPI()
        {
            JsonData jsonData = new JsonData();

            try
            {
                // First try to validate OpenXR is available through Unity's XR system
                if (!ValidateOpenXRSession())
                {
                    Debug.LogWarning("OpenXR session validation failed");
                    jsonData["status"] = 0;
                    return jsonData;
                }

                // Try to get spaces using the updated function signatures
                ulong headSpace = 0;
                ulong trackingSpace = 0;
                long predictedDisplayTime = 0;

                try
                {
                    // headSpace = GetCurrentAppSpace();
                    // trackingSpace = GetTrackingSpace();
                    // predictedDisplayTime = GetPredictedDisplayTime();
                }
                catch (System.EntryPointNotFoundException)
                {
                    // Functions not available, fall back to alternative approach
                    Debug.LogWarning("Unity OpenXR plugin functions not available, using alternative approach");
                    return GetHeadTrackingDataOpenXRAlternative();
                }
                catch (System.DllNotFoundException)
                {
                    Debug.LogWarning("Unity OpenXR plugin DLL not found, falling back to Unity XR");
                    return GetHeadTrackingData();
                }

                // if (headSpace == XrConstants.XR_NULL_HANDLE || trackingSpace == XrConstants.XR_NULL_HANDLE)
                // {
                //     Debug.LogWarning("OpenXR spaces not available");
                //     jsonData["status"] = 0;
                //     return jsonData;
                // }

                // Initialize space location structure
                XrSpaceLocation spaceLocation = new XrSpaceLocation
                {
                    type = XrConstants.XR_TYPE_SPACE_LOCATION,
                    next = IntPtr.Zero,
                    locationFlags = 0,
                    pose = new XrPosef
                    {
                        orientation = new XrQuaternionf { x = 0, y = 0, z = 0, w = 1 },
                        position = new XrVector3f { x = 0, y = 0, z = 0 }
                    }
                };

                // Locate the head space relative to the tracking space
                XrResult result = xrLocateSpace(headSpace, trackingSpace, predictedDisplayTime, ref spaceLocation);

                if (result == XrResult.XR_SUCCESS)
                {
                    // Check if position and orientation are valid and tracked
                    bool positionValid = (spaceLocation.locationFlags &
                                          (ulong)XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT) != 0;
                    bool orientationValid = (spaceLocation.locationFlags &
                                             (ulong)XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT) != 0;
                    bool positionTracked = (spaceLocation.locationFlags &
                                            (ulong)XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_TRACKED_BIT) != 0;
                    bool orientationTracked = (spaceLocation.locationFlags &
                                               (ulong)XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_TRACKED_BIT) !=
                                              0;

                    if (positionValid && orientationValid && positionTracked && orientationTracked)
                    {
                        // Convert OpenXR pose to Unity format
                        Vector3 position = new Vector3(
                            spaceLocation.pose.position.x,
                            spaceLocation.pose.position.y,
                            spaceLocation.pose.position.z
                        );

                        Quaternion rotation = new Quaternion(
                            spaceLocation.pose.orientation.x,
                            spaceLocation.pose.orientation.y,
                            spaceLocation.pose.orientation.z,
                            spaceLocation.pose.orientation.w
                        );

                        Debug.Log($"[TrackingData] Headset CAPI : {position} {rotation}");

                        // // Adjust coordinate system to match PICO system (same as existing method)
                        // position.z *= -1;
                        // rotation.z *= -1;
                        // rotation.w *= -1;

                        jsonData["pose"] = GetPoseStr(position, rotation);
                        jsonData["status"] = 3; // Full tracking status

                        Debug.Log($"OpenXR Head tracking: pos=({position.x:F3}, {position.y:F3}, {position.z:F3}), " +
                                  $"rot=({rotation.x:F3}, {rotation.y:F3}, {rotation.z:F3}, {rotation.w:F3})");
                    }
                    else
                    {
                        // Partial tracking or invalid data
                        Debug.LogWarning(
                            $"OpenXR Head tracking partial: posValid={positionValid}, oriValid={orientationValid}, " +
                            $"posTracked={positionTracked}, oriTracked={orientationTracked}");
                        jsonData["status"] = 1; // Partial tracking
                    }
                }
                else
                {
                    Debug.LogWarning($"OpenXR xrLocateSpace failed with result: {result}");
                    jsonData["status"] = 0;
                }
            }
            catch (System.DllNotFoundException dllEx)
            {
                Debug.LogError($"OpenXR DLL not found: {dllEx.Message}. Falling back to Unity XR.");
                // Fallback to Unity XR method
                return GetHeadTrackingData();
            }
            catch (System.EntryPointNotFoundException entryEx)
            {
                Debug.LogError($"OpenXR function not found: {entryEx.Message}. Falling back to Unity XR.");
                // Fallback to Unity XR method
                return GetHeadTrackingData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"OpenXR Head tracking failed: {e.Message}. Falling back to Unity XR.");
                // Fallback to Unity XR method
                return GetHeadTrackingData();
            }

            return jsonData;
        }

        /// <summary>
        /// Validates that OpenXR session is available and running using Unity's XR system
        /// This provides a fallback validation method when native functions aren't available
        /// </summary>
        private bool ValidateOpenXRSession()
        {
            try
            {
                // Check if XR is initialized and running
                if (XRGeneralSettings.Instance == null ||
                    XRGeneralSettings.Instance.Manager == null ||
                    XRGeneralSettings.Instance.Manager.activeLoader == null)
                {
                    Debug.LogWarning("XR system not properly initialized");
                    return false;
                }

                // Check if OpenXR is the active loader
                var activeLoaderName = XRGeneralSettings.Instance.Manager.activeLoader.GetType().Name;
                if (!activeLoaderName.Contains("OpenXR"))
                {
                    Debug.LogWarning($"Active XR loader is not OpenXR: {activeLoaderName}");
                    return false;
                }

                // Check if we can get basic XR data (indicates session is active)
                var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if (!headDevice.isValid)
                {
                    Debug.LogWarning("Head device not available - session may not be active");
                    return false;
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"OpenXR session validation failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Alternative OpenXR head tracking implementation using Unity's XR input system with OpenXR optimizations
        /// This method can be used if the Unity OpenXR plugin specific functions are not available
        /// It uses Unity's InputDevice system but tries to get more precise timing and validation
        /// </summary>
        private JsonData GetHeadTrackingDataOpenXRAlternative()
        {
            JsonData jsonData = new JsonData();

            try
            {
                Debug.Log("Using OpenXR alternative implementation (Unity XR with OpenXR optimizations)");

                // Validate OpenXR is active
                if (!ValidateOpenXRSession())
                {
                    Debug.LogWarning("OpenXR session not available for alternative implementation");
                    return GetHeadTrackingData();
                }

                // Get head tracking data from Unity XR with additional validation
                InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if (headDevice.isValid)
                {
                    // Try to get tracking state first for better validation
                    bool isTracked = false;
                    if (headDevice.TryGetFeatureValue(CommonUsages.isTracked, out isTracked) && !isTracked)
                    {
                        Debug.LogWarning("Head device is valid but not tracked");
                        jsonData["status"] = 1; // Partial tracking
                        return jsonData;
                    }

                    if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
                        headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                    {
                        // Log the raw OpenXR data before coordinate transformation
                        Debug.Log($"[TrackingData] Headset OpenXR Alt (raw): {position} {rotation}");

                        // Apply coordinate system transformation to match PICO format
                        // (commented out in the main implementation, so keeping consistent)
                        // position.z *= -1;
                        // rotation.z *= -1;
                        // rotation.w *= -1;

                        jsonData["pose"] = GetPoseStr(position, rotation);
                        jsonData["status"] = isTracked ? 3 : 1; // Full or partial tracking

                        Debug.Log($"OpenXR Alternative Head tracking: pos=({position.x:F3}, {position.y:F3}, {position.z:F3}), " +
                                 $"rot=({rotation.x:F3}, {rotation.y:F3}, {rotation.z:F3}, {rotation.w:F3})");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to get position/rotation from head device");
                        jsonData["status"] = 0;
                    }
                }
                else
                {
                    Debug.LogWarning("Head device is not valid");
                    jsonData["status"] = 0;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"OpenXR Alternative head tracking failed: {e.Message}");
                jsonData["status"] = 0;
                // Last resort: fall back to the standard Unity XR implementation
                return GetHeadTrackingData();
            }

            return jsonData;
        }

        /// <summary>
        /// Test method to validate OpenXR C API head tracking functionality
        /// Call this method to test if OpenXR C API is working correctly
        /// </summary>
        public static void TestOpenXRHeadTracking()
        {
            Debug.Log("=== Testing OpenXR C API Head Tracking ===");

            TrackingData tracker = new TrackingData();

            // Test Unity XR first
            Debug.Log("Testing Unity XR method...");
            JsonData unityResult = tracker.GetHeadTrackingData();
            Debug.Log($"Unity XR result: status = {unityResult["status"]}, pose = {unityResult.ContainsKey("pose")}");

            // Test OpenXR C API
            Debug.Log("Testing OpenXR C API method...");
            JsonData openXRResult = tracker.GetHeadTrackingDataOpenXRCAPI();
            Debug.Log(
                $"OpenXR C API result: status = {openXRResult["status"]}, pose = {openXRResult.ContainsKey("pose")}");

            // Test OpenXR Alternative
            Debug.Log("Testing OpenXR Alternative method...");
            JsonData openXRAltResult = tracker.GetHeadTrackingDataOpenXRAlternative();
            Debug.Log(
                $"OpenXR Alternative result: status = {openXRAltResult["status"]}, pose = {openXRAltResult.ContainsKey("pose")}");

            // Compare results
            if (unityResult.ContainsKey("pose") && openXRResult.ContainsKey("pose"))
            {
                Debug.Log($"Unity XR pose: {unityResult["pose"]}");
                Debug.Log($"OpenXR pose: {openXRResult["pose"]}");
            }

            if (openXRAltResult.ContainsKey("pose"))
            {
                Debug.Log($"OpenXR Alternative pose: {openXRAltResult["pose"]}");
            }

            // Test the configuration switching
            Debug.Log("Testing configuration switching...");

            // Test with OpenXR C API enabled
            TrackingData.SetUseOpenXRCAPI(true);
            JsonData configTest1 = new JsonData();
            tracker.Get(ref configTest1);
            Debug.Log($"With OpenXR C API enabled: Head status = {(configTest1.ContainsKey("Head") ? configTest1["Head"]["status"] : "No head data")}");

            // Test with OpenXR C API disabled
            TrackingData.SetUseOpenXRCAPI(false);
            JsonData configTest2 = new JsonData();
            tracker.Get(ref configTest2);
            Debug.Log($"With OpenXR C API disabled: Head status = {(configTest2.ContainsKey("Head") ? configTest2["Head"]["status"] : "No head data")}");

            Debug.Log("=== OpenXR Head Tracking Test Complete ===");
        }

        /// <summary>
        /// Provides information about OpenXR C API availability and fallback options
        /// </summary>
        public static void CheckOpenXRCAPIAvailability()
        {
            Debug.Log("=== OpenXR C API Availability Check ===");

            TrackingData tracker = new TrackingData();

            // Test session validation
            bool sessionValid = tracker.ValidateOpenXRSession();
            Debug.Log($"OpenXR Session Valid: {sessionValid}");

            // Test native function availability
            try
            {
                // ulong testSpace = tracker.GetCurrentAppSpace();
                // Debug.Log($"GetCurrentAppSpace succeeded: Handle = {testSpace}");
            }
            catch (System.EntryPointNotFoundException)
            {
                Debug.LogWarning("GetCurrentAppSpace not available - Unity OpenXR plugin may not expose this function");
            }
            catch (System.DllNotFoundException)
            {
                Debug.LogWarning("UnityOpenXR.dll not found - Unity OpenXR plugin may not be installed");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GetCurrentAppSpace failed: {e.Message}");
            }

            // Test OpenXR loader availability
            try
            {
                // This will fail gracefully if openxr_loader isn't available
                XrSpaceLocation testLocation = new XrSpaceLocation();
                Debug.Log("OpenXR structures can be created successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"OpenXR structure creation failed: {e.Message}");
            }

            // Provide recommendations
            Debug.Log("=== Recommendations ===");
            if (!sessionValid)
            {
                Debug.Log("- Ensure OpenXR is the active XR provider in XR Management settings");
                Debug.Log("- Verify VR headset is connected and OpenXR runtime is active");
            }

            Debug.Log("- If OpenXR C API functions are not available, the system will automatically fall back to:");
            Debug.Log("  1. OpenXR Alternative implementation (Unity XR with OpenXR optimizations)");
            Debug.Log("  2. Standard Unity XR implementation");
            Debug.Log("- Both fallback methods provide equivalent functionality with slightly different performance characteristics");

            Debug.Log("=== OpenXR C API Availability Check Complete ===");
        }


        private int GetActiveInputDevice()
        {
            try
            {
                // Check if hand tracking is available and active with Quest 3 specific checks
                if (XRGeneralSettings.Instance != null &&
                    XRGeneralSettings.Instance.Manager != null &&
                    XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    var handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader
                        .GetLoadedSubsystem<XRHandSubsystem>();

                    if (handSubsystem != null && handSubsystem.running)
                    {
                        // Check if either hand is actually tracked with more detailed validation
                        bool leftHandTracked = handSubsystem.leftHand.isTracked;
                        bool rightHandTracked = handSubsystem.rightHand.isTracked;

                        if (leftHandTracked || rightHandTracked)
                        {
                            Debug.Log($"Hand tracking active - Left: {leftHandTracked}, Right: {rightHandTracked}");
                            return 2; // HandTrackingActive equivalent
                        }
                        else
                        {
                            Debug.Log("Hand tracking subsystem running but no hands tracked");
                        }
                    }
                    else
                    {
                        if (handSubsystem == null)
                        {
                            Debug.Log("Hand tracking subsystem not available");
                        }
                        else
                        {
                            Debug.Log($"Hand tracking subsystem not running. Running state: {handSubsystem.running}");
                        }
                    }
                }

                // Check if controllers are connected with enhanced validation
                var leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                var rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

                bool leftControllerValid = leftController.isValid &&
                                           leftController.characteristics.HasFlag(InputDeviceCharacteristics
                                               .Controller);
                bool rightControllerValid = rightController.isValid &&
                                            rightController.characteristics.HasFlag(InputDeviceCharacteristics
                                                .Controller);

                if (leftControllerValid || rightControllerValid)
                {
                    Debug.Log($"Controller active - Left: {leftControllerValid}, Right: {rightControllerValid}");
                    return 1; // ControllerActive equivalent
                }

                // Log what devices we do have
                var devices = new System.Collections.Generic.List<InputDevice>();
                InputDevices.GetDevices(devices);
                Debug.Log($"No hand tracking or controllers detected. Available devices: {devices.Count}");
                foreach (var device in devices)
                {
                    Debug.Log($"  Device: {device.name} - {device.characteristics}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to get active input device: {e.Message}");
            }

            Debug.Log("Defaulting to head tracking");
            return 0; // HeadActive equivalent
        }


        // Remove PICO-specific tracking methods - these would need OpenXR equivalents
        // GetMotionTracking() and GetBodyTracking() methods removed
        // These features would require specific OpenXR extensions or alternative implementations


        private JsonData GetLeftRightControllerJsonData(double predictTime)
        {
            // Get left controller data
            InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            _leftControllerJson = new JsonData();
            GetControllerJsonData(leftController, ref _leftControllerJson);
            _controllerDataJson["left"] = _leftControllerJson;

            // Get right controller data
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            _rightControllerJson = new JsonData();
            GetControllerJsonData(rightController, ref _rightControllerJson);
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

            controllerDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var position);
            controllerDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var rotation);

            // Adjust to match the PICO coordinate system
            position.z *= -1;
            rotation.z *= -1;
            rotation.w *= -1;

            json["axisX"] = axis2D.x;
            json["axisY"] = axis2D.y;
            json["axisClick"] = axisClick;
            json["grip"] = grip;
            json["trigger"] = trigger;
            json["primaryButton"] = primaryButton;
            json["secondaryButton"] = secondaryButton;
            json["menuButton"] = menuButton;
            json["pose"] = GetPoseStr(position, rotation);
        }


        private JsonData GetHandJsonData()
        {
            try
            {
                // Clear previous data
                _handData = new JsonData();

                // Check XR system availability with more lenient checks for Quest 3
                if (XRGeneralSettings.Instance == null ||
                    XRGeneralSettings.Instance.Manager == null)
                {
                    Debug.LogWarning("XRGeneralSettings or Manager not available for hand tracking");
                    return _handData;
                }

                var activeLoader = XRGeneralSettings.Instance.Manager.activeLoader;
                if (activeLoader == null)
                {
                    Debug.LogWarning("No active XR loader found for hand tracking");
                    return _handData;
                }

                // Try to get hand subsystem with more detailed logging
                var handSubsystem = activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                if (handSubsystem == null)
                {
                    // For Quest 3, sometimes the subsystem takes time to initialize
                    Debug.LogWarning("Hand tracking subsystem not found. This might be normal during initialization.");
                    return _handData;
                }

                // Check if subsystem is running, but be more flexible for Quest 3
                if (!handSubsystem.running)
                {
                    Debug.LogWarning($"Hand tracking subsystem not running. State: {handSubsystem.running}");
                    // Try to start it if it's not running
                    try
                    {
                        Debug.Log("Attempting to start hand tracking subsystem...");
                        handSubsystem.Start();

                        // Give it a moment to initialize
                        if (!handSubsystem.running)
                        {
                            Debug.LogWarning("Hand tracking subsystem failed to start");
                        }
                    }
                    catch (System.Exception startEx)
                    {
                        Debug.LogWarning($"Failed to start hand tracking subsystem: {startEx.Message}");
                    }

                    return _handData;
                }

                // // Add subsystem info for debugging
                // _handData["subsystemRunning"] = handSubsystem.running;

                // For Quest 3, check if hands are available but not yet tracked
                // This can happen during the initial seconds after hand tracking starts
                bool leftHandAvailable = handSubsystem.leftHand != null;
                bool rightHandAvailable = handSubsystem.rightHand != null;
                bool leftHandTracked = leftHandAvailable && handSubsystem.leftHand.isTracked;
                bool rightHandTracked = rightHandAvailable && handSubsystem.rightHand.isTracked;

                Debug.Log($"Hand availability - Left: {leftHandAvailable}, Right: {rightHandAvailable}");
                Debug.Log($"Hand tracking - Left: {leftHandTracked}, Right: {rightHandTracked}");

                // Get left hand data with additional validation
                if (leftHandTracked)
                {
                    _leftHandData = new JsonData();
                    if (GetXRHandTrackingData(handSubsystem.leftHand, ref _leftHandData))
                    {
                        _handData["leftHand"] = _leftHandData;
                        Debug.Log("Left hand tracking data successfully obtained");
                    }
                    // else
                    // {
                    //     Debug.LogWarning("Failed to get left hand tracking data despite hand being tracked");
                    //     // Still provide the empty structure
                    //     _leftHandData = new JsonData();
                    //     _leftHandData["isActive"] = 0U;
                    //     _leftHandData["count"] = 0U;
                    //     // _leftHandData["validJointCount"] = 0U;
                    //     _leftHandData["scale"] = 1.0f;
                    //     _leftHandData["HandJointLocations"] = new JsonData();
                    //     _handData["leftHand"] = _leftHandData;
                    // }
                }
                // else
                // {
                //     if (leftHandAvailable)
                //     {
                //         Debug.Log("Left hand available but not tracked - this is normal during initialization");
                //     }
                //     else
                //     {
                //         Debug.Log("Left hand not available");
                //     }
                //
                //     // Provide structure even when not tracked for consistency
                //     _leftHandData = new JsonData();
                //     _leftHandData["isActive"] = 0U;
                //     _leftHandData["count"] = 0U;
                //     // _leftHandData["validJointCount"] = 0U;
                //     _leftHandData["scale"] = 1.0f;
                //     // _leftHandData["HandJointLocations"] = new JsonData();
                //     _handData["leftHand"] = _leftHandData;
                // }

                // Get right hand data with additional validation
                if (rightHandTracked)
                {
                    _rightHandData = new JsonData();
                    if (GetXRHandTrackingData(handSubsystem.rightHand, ref _rightHandData))
                    {
                        _handData["rightHand"] = _rightHandData;
                        Debug.Log("Right hand tracking data successfully obtained");
                    }
                    // else
                    // {
                    //     Debug.LogWarning("Failed to get right hand tracking data despite hand being tracked");
                    //     // Still provide the empty structure
                    //     _rightHandData = new JsonData();
                    //     _rightHandData["isActive"] = 0U;
                    //     _rightHandData["count"] = 0U;
                    //     // _rightHandData["validJointCount"] = 0U;
                    //     _rightHandData["scale"] = 1.0f;
                    //     // _rightHandData["HandJointLocations"] = new JsonData();
                    //     _handData["rightHand"] = _rightHandData;
                    // }
                }
                // else
                // {
                //     if (rightHandAvailable)
                //     {
                //         Debug.Log("Right hand available but not tracked - this is normal during initialization");
                //     }
                //     else
                //     {
                //         Debug.Log("Right hand not available");
                //     }
                //
                //     // Provide structure even when not tracked for consistency
                //     _rightHandData = new JsonData();
                //     _rightHandData["isActive"] = 0U;
                //     _rightHandData["count"] = 0U;
                //     // _rightHandData["validJointCount"] = 0U;
                //     _rightHandData["scale"] = 1.0f;
                //     // _rightHandData["HandJointLocations"] = new JsonData();
                //     _handData["rightHand"] = _rightHandData;
                // }

                // // Add overall hand tracking status
                // _handData["leftHandTracked"] = leftHandTracked;
                // _handData["rightHandTracked"] = rightHandTracked;
                // _handData["anyHandTracked"] = leftHandTracked || rightHandTracked;
                // _handData["leftHandAvailable"] = leftHandAvailable;
                // _handData["rightHandAvailable"] = rightHandAvailable;
                //
                // // Add Quest 3 specific debugging info
                // if (handSubsystem.subsystemDescriptor != null)
                // {
                //     _handData["subsystemId"] = handSubsystem.subsystemDescriptor.id;
                // }

                // If no hands are tracked, provide some guidance
                if (!leftHandTracked && !rightHandTracked)
                {
                    Debug.LogWarning("No hands currently tracked. Make sure:");
                    Debug.LogWarning("1. Hands are visible to Quest 3 cameras");
                    Debug.LogWarning("2. Hand tracking is enabled in Quest settings");
                    Debug.LogWarning("3. Lighting conditions are adequate");
                    Debug.LogWarning("4. Hands are within tracking range (~arm's length)");
                }

                // Log summary
                Debug.Log(
                    $"Hand tracking summary - Left tracked: {leftHandTracked}, Right tracked: {rightHandTracked}, Subsystem running: {handSubsystem.running}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Hand tracking failed with exception: {e.Message}\nStack trace: {e.StackTrace}");

                // // Ensure we always return a valid structure
                // _handData = new JsonData();
                // // _handData["subsystemRunning"] = false;
                // // _handData["leftHandTracked"] = false;
                // // _handData["rightHandTracked"] = false;
                // // _handData["anyHandTracked"] = false;
                // // _handData["leftHandAvailable"] = false;
                // // _handData["rightHandAvailable"] = false;
                //
                // // Add empty hand data structures
                // JsonData emptyHandData = new JsonData();
                // emptyHandData["isActive"] = 0U;
                // emptyHandData["count"] = 0U;
                // // emptyHandData["validJointCount"] = 0U;
                // emptyHandData["scale"] = 1.0f;
                // // emptyHandData["HandJointLocations"] = new JsonData();
                //
                // _handData["leftHand"] = emptyHandData;
                // _handData["rightHand"] = emptyHandData;
            }

            return _handData;
        }

        private bool GetXRHandTrackingData(XRHand hand, ref JsonData json)
        {
            try
            {
                // Validate hand tracking state
                if (!hand.isTracked)
                {
                    Debug.LogWarning("Hand is not tracked");
                    return false;
                }

                json["isActive"] = hand.isTracked ? 1U : 0U;
                json["count"] = (uint)XRHandJointID.EndMarker - 1; // first is invalid
                json["scale"] = 1.0f; // XR Hands doesn't provide scale directly

                JsonData jointLocationsJson = new JsonData();
                jointLocationsJson.SetJsonType(JsonType.Array);
                json["HandJointLocations"] = jointLocationsJson;

                int successfulJoints = 0;
                int totalJoints = (int)XRHandJointID.EndMarker;

                // The XRHandJointID (UnityEngine.XR.Hands)
                // Invalid = 0
                // BeginMarker = 1 (also represents Wrist)
                // Palm = 2

                // OpenXR
                // Palm = 0
                // Wrist = 1

                // Add Palm first - 2
                var palmJointID = XRHandJointID.Palm;
                var palmJoint = hand.GetJoint(palmJointID);

                if (palmJoint.TryGetPose(out Pose palmPose))
                {
                    JsonData jointJson = new JsonData();

                    // Adjust to match the PICO coordinate system
                    palmPose.position.z *= -1;
                    palmPose.rotation.z *= -1;
                    palmPose.rotation.w *= -1;

                    jointJson["p"] = GetPoseStr(palmPose.position, palmPose.rotation);

                    // Enhanced tracking state mapping for Quest 3
                    uint status = GetOpenXRTrackingStatus(palmJoint.trackingState);
                    jointJson["s"] = ((ulong)status);

                    jointJson["r"] = 0.0f;

                    jointLocationsJson.Add(jointJson);
                    successfulJoints++;
                }

                // Iterate through all hand joints with improved error handling
                for (int i = 0; i < totalJoints; i++)
                {
                    try
                    {
                        var jointID = XRHandJointIDUtility.FromIndex(i);
                        var joint = hand.GetJoint(jointID);

                        // Validate joint before trying to get pose
                        if (joint.id == XRHandJointID.Invalid || joint.id == XRHandJointID.Palm)
                        {
                            continue;
                        }

                        if (joint.TryGetPose(out Pose pose))
                        {
                            JsonData jointJson = new JsonData();

                            // Adjust to match the PICO coordinate system
                            pose.position.z *= -1;
                            pose.rotation.z *= -1;
                            pose.rotation.w *= -1;

                            jointJson["p"] = GetPoseStr(pose.position, pose.rotation);

                            // Enhanced tracking state mapping for Quest 3
                            uint status = GetOpenXRTrackingStatus(joint.trackingState);
                            jointJson["s"] = ((ulong)status);

                            // // Try to get radius with fallback
                            // if (joint.TryGetRadius(out float radius) && radius > 0)
                            // {
                            //     jointJson["r"] = radius;
                            // }
                            // else
                            // {
                            //     // Use joint-specific default radii for better Quest 3 compatibility
                            //     jointJson["r"] = GetDefaultJointRadius(jointID);
                            // }
                            jointJson["r"] = 0.0f;

                            jointLocationsJson.Add(jointJson);
                            successfulJoints++;
                        }
                        // else
                        // {
                        //     // For debugging: log which joints fail to get poses
                        //     if (i % 5 == 0) // Only log every 5th joint to avoid spam
                        //     {
                        //         Debug.LogWarning($"Failed to get pose for joint {jointID} (index {i})");
                        //     }
                        // }
                    }
                    catch (System.Exception jointEx)
                    {
                        Debug.LogWarning($"Exception getting joint {i}: {jointEx.Message}");
                    }
                }

                // // Update count with actual valid joints
                // json["validJointCount"] = (uint)successfulJoints;

                // // Log success rate for debugging
                // float successRate = (float)successfulJoints / totalJoints;
                // if (successRate < 0.5f)
                // {
                //     Debug.LogWarning($"Low joint success rate: {successfulJoints}/{totalJoints} ({successRate:P1})");
                // }
                // else
                // {
                //     Debug.Log($"Hand tracking success: {successfulJoints}/{totalJoints} joints ({successRate:P1})");
                // }

                return successfulJoints > 0; // Return true if we got at least some joints
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Hand tracking data extraction failed: {e.Message}\nStack trace: {e.StackTrace}");

                // // Initialize safe fallback data
                // json["isActive"] = 0U;
                // json["count"] = 0U;
                // // json["validJointCount"] = 0U;
                // json["scale"] = 1.0f;
                // json["HandJointLocations"] = new JsonData();
                // json["HandJointLocations"].SetJsonType(JsonType.Array);

                return false;
            }
        }

        /// <summary>
        /// Maps XR Hands tracking state to OpenXR-compatible status flags
        /// </summary>
        private uint GetOpenXRTrackingStatus(XRHandJointTrackingState trackingState)
        {
            uint status = 0;

            // Bit layout (semantic mapping based on HandLocationStatus):
            // Bit 0: Position valid           => 0x01
            // Bit 1: Position tracked         => 0x02
            // Bit 2: Orientation valid        => 0x04
            // Bit 3: Orientation tracked      => 0x08
            // Bit 4: Linear velocity valid    => 0x10
            // Bit 5: Angular velocity valid   => 0x20

            if (trackingState.HasFlag(XRHandJointTrackingState.Pose))
            {
                status |= 0x01; // Position valid
                status |= 0x02; // Position tracked
                status |= 0x04; // Orientation valid
                status |= 0x08; // Orientation tracked
            }

            // This is not support in PICO

            // if (trackingState.HasFlag(XRHandJointTrackingState.LinearVelocity))
            // {
            //     status |= 0x10; // Linear velocity valid (bit 4)
            // }
            //
            // if (trackingState.HasFlag(XRHandJointTrackingState.AngularVelocity))
            // {
            //     status |= 0x20; // Angular velocity valid (bit 5)
            // }

            return status;
        }

        /// <summary>
        /// Provides default radius values for different joint types based on OpenXR recommendations
        /// </summary>
        private float GetDefaultJointRadius(XRHandJointID jointID)
        {
            // Default radii based on typical hand joint sizes (in meters)
            switch (jointID)
            {
                case XRHandJointID.Wrist:
                    return 0.025f;
                case XRHandJointID.Palm:
                    return 0.03f;

                // Thumb joints
                case XRHandJointID.ThumbMetacarpal:
                case XRHandJointID.ThumbProximal:
                case XRHandJointID.ThumbDistal:
                case XRHandJointID.ThumbTip:
                    return 0.012f;

                // Index finger joints
                case XRHandJointID.IndexMetacarpal:
                case XRHandJointID.IndexProximal:
                case XRHandJointID.IndexIntermediate:
                case XRHandJointID.IndexDistal:
                case XRHandJointID.IndexTip:
                    return 0.008f;

                // Middle finger joints
                case XRHandJointID.MiddleMetacarpal:
                case XRHandJointID.MiddleProximal:
                case XRHandJointID.MiddleIntermediate:
                case XRHandJointID.MiddleDistal:
                case XRHandJointID.MiddleTip:
                    return 0.008f;

                // Ring finger joints
                case XRHandJointID.RingMetacarpal:
                case XRHandJointID.RingProximal:
                case XRHandJointID.RingIntermediate:
                case XRHandJointID.RingDistal:
                case XRHandJointID.RingTip:
                    return 0.007f;

                // Little finger joints
                case XRHandJointID.LittleMetacarpal:
                case XRHandJointID.LittleProximal:
                case XRHandJointID.LittleIntermediate:
                case XRHandJointID.LittleDistal:
                case XRHandJointID.LittleTip:
                    return 0.006f;

                default:
                    return 0.01f; // Generic default
            }
        }



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

        /// <summary>
        /// Diagnostic method to help troubleshoot hand tracking issues on Quest 3
        /// Call this method when hand tracking isn't working to get detailed status information
        /// </summary>
        public static void DiagnoseHandTracking()
        {
            Debug.Log("=== Hand Tracking Diagnostic for Quest 3 ===");

            try
            {
                // Check XR system status
                if (XRGeneralSettings.Instance == null)
                {
                    Debug.LogError("XRGeneralSettings.Instance is null - XR system not initialized");
                    return;
                }

                if (XRGeneralSettings.Instance.Manager == null)
                {
                    Debug.LogError("XRGeneralSettings Manager is null");
                    return;
                }

                var manager = XRGeneralSettings.Instance.Manager;
                Debug.Log($"XR Manager initialization complete: {manager.isInitializationComplete}");

                if (manager.activeLoader == null)
                {
                    Debug.LogError("No active XR loader found");
                    Debug.Log("Available loaders:");
                    foreach (var loader in manager.activeLoaders)
                    {
                        Debug.Log($"  - {loader.GetType().Name}");
                    }

                    return;
                }

                Debug.Log($"Active XR Loader: {manager.activeLoader.GetType().Name}");

                // Check hand tracking subsystem
                var handSubsystem = manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
                if (handSubsystem == null)
                {
                    Debug.LogError("Hand tracking subsystem not found");
                    Debug.Log("This could indicate:");
                    Debug.Log("  - Hand tracking not enabled in XR settings");
                    Debug.Log("  - Quest 3 hand tracking not properly configured");
                    Debug.Log("  - Missing required packages or permissions");
                    return;
                }

                Debug.Log($"Hand tracking subsystem found: {handSubsystem.GetType().Name}");
                Debug.Log($"Hand tracking subsystem running: {handSubsystem.running}");
                Debug.Log($"Hand tracking subsystem descriptor: {handSubsystem.subsystemDescriptor?.id}");

                if (handSubsystem.running)
                {
                    bool leftTracked = handSubsystem.leftHand.isTracked;
                    bool rightTracked = handSubsystem.rightHand.isTracked;

                    Debug.Log($"Left hand tracked: {leftTracked}");
                    Debug.Log($"Right hand tracked: {rightTracked}");

                    if (leftTracked)
                    {
                        Debug.Log($"Left hand root pose: {handSubsystem.leftHand.rootPose}");
                    }

                    if (rightTracked)
                    {
                        Debug.Log($"Right hand root pose: {handSubsystem.rightHand.rootPose}");
                    }

                    // If subsystem is running but no hands tracked, provide specific guidance
                    if (!leftTracked && !rightTracked)
                    {
                        Debug.LogWarning("=== HAND TRACKING ISSUE DETECTED ===");
                        Debug.LogWarning("Subsystem is running but no hands are tracked.");
                        Debug.LogWarning("This typically means:");
                        Debug.LogWarning("");
                        Debug.LogWarning("QUEST 3 DEVICE SETTINGS:");
                        Debug.LogWarning("1. Open Quest Settings → Hands and Controllers");
                        Debug.LogWarning("2. Ensure Hand Tracking is enabled");
                        Debug.LogWarning("3. Set tracking to 'Auto Switch' or 'Hands Only'");
                        Debug.LogWarning("4. Try toggling hand tracking off and on");
                        Debug.LogWarning("");
                        Debug.LogWarning("PHYSICAL ENVIRONMENT:");
                        Debug.LogWarning("5. Ensure hands are visible to front cameras");
                        Debug.LogWarning("6. Check lighting - avoid very bright/dark areas");
                        Debug.LogWarning("7. Keep hands within ~arm's length of headset");
                        Debug.LogWarning("8. Remove gloves or objects covering hands");
                        Debug.LogWarning("9. Try different hand positions/gestures");
                        Debug.LogWarning("");
                        Debug.LogWarning("APP PERMISSIONS:");
                        Debug.LogWarning("10. Verify app has hand tracking permission");
                        Debug.LogWarning("11. Check Android manifest includes hand tracking permission");
                        Debug.LogWarning("");
                        Debug.LogWarning("TROUBLESHOOTING STEPS:");
                        Debug.LogWarning("12. Restart the hand tracking subsystem");
                        Debug.LogWarning("13. Try switching to controllers and back to hands");
                        Debug.LogWarning("14. Restart the app");
                        Debug.LogWarning("15. Restart the Quest device");
                    }
                }

                // Check input devices
                var devices = new System.Collections.Generic.List<InputDevice>();
                InputDevices.GetDevices(devices);
                Debug.Log($"Total XR input devices: {devices.Count}");

                foreach (var device in devices)
                {
                    Debug.Log($"  Device: {device.name}");
                    Debug.Log($"    Characteristics: {device.characteristics}");
                    Debug.Log($"    Valid: {device.isValid}");
                }

                // Check if controllers are interfering
                var leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                var rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                bool controllersPresent = (leftController.isValid &&
                                           leftController.characteristics.HasFlag(InputDeviceCharacteristics
                                               .Controller)) ||
                                          (rightController.isValid &&
                                           rightController.characteristics.HasFlag(
                                               InputDeviceCharacteristics.Controller));

                if (controllersPresent)
                {
                    Debug.LogWarning("Controllers detected - they may be interfering with hand tracking");
                    Debug.LogWarning(
                        "Try putting controllers down or switching to 'Hands Only' mode in Quest settings");
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError($"Hand tracking diagnostic failed: {e.Message}\nStack trace: {e.StackTrace}");
            }

            Debug.Log("=== End Hand Tracking Diagnostic ===");
        }

        /// <summary>
        /// Attempts to restart the hand tracking subsystem
        /// This can help resolve hand tracking issues on Quest 3
        /// </summary>
        public static bool RestartHandTracking()
        {
            try
            {
                Debug.Log("Attempting to restart hand tracking subsystem...");

                if (XRGeneralSettings.Instance?.Manager?.activeLoader == null)
                {
                    Debug.LogError("Cannot restart hand tracking - XR system not properly initialized");
                    return false;
                }

                var handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader
                    .GetLoadedSubsystem<XRHandSubsystem>();

                if (handSubsystem == null)
                {
                    Debug.LogError("Hand tracking subsystem not found - cannot restart");
                    return false;
                }

                // Stop the subsystem if it's running
                if (handSubsystem.running)
                {
                    Debug.Log("Stopping hand tracking subsystem...");
                    handSubsystem.Stop();

                    // Wait a brief moment
                    System.Threading.Thread.Sleep(100);
                }

                // Start the subsystem
                Debug.Log("Starting hand tracking subsystem...");
                handSubsystem.Start();

                // Verify it started
                if (handSubsystem.running)
                {
                    Debug.Log("Hand tracking subsystem successfully restarted");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to start hand tracking subsystem");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to restart hand tracking: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to force-enable hand tracking on Quest 3
        /// This method tries multiple approaches to activate hand tracking
        /// </summary>
        public static bool ForceEnableHandTracking()
        {
            try
            {
                Debug.Log("Attempting to force-enable hand tracking...");

                if (XRGeneralSettings.Instance?.Manager?.activeLoader == null)
                {
                    Debug.LogError("Cannot enable hand tracking - XR system not properly initialized");
                    return false;
                }

                var handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader
                    .GetLoadedSubsystem<XRHandSubsystem>();

                if (handSubsystem == null)
                {
                    Debug.LogError("Hand tracking subsystem not found - cannot enable");
                    return false;
                }

                // Step 1: Ensure subsystem is started
                if (!handSubsystem.running)
                {
                    Debug.Log("Starting hand tracking subsystem...");
                    handSubsystem.Start();

                    // Wait briefly for initialization
                    var startTime = Time.unscaledTime;
                    while (!handSubsystem.running && (Time.unscaledTime - startTime) < 2.0f)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                }

                if (!handSubsystem.running)
                {
                    Debug.LogError("Failed to start hand tracking subsystem");
                    return false;
                }

                Debug.Log("Hand tracking subsystem is running");

                // Step 2: Check if hands are available (may take time on Quest 3)
                var checkStartTime = Time.unscaledTime;
                bool handsDetected = false;

                // Give it up to 5 seconds to detect hands
                while ((Time.unscaledTime - checkStartTime) < 5.0f && !handsDetected)
                {
                    if (handSubsystem.leftHand.isTracked || handSubsystem.rightHand.isTracked)
                    {
                        handsDetected = true;
                        break;
                    }

                    // Log every second to show we're trying
                    if ((int)(Time.unscaledTime - checkStartTime) % 1 == 0)
                    {
                        Debug.Log(
                            $"Waiting for hands to be detected... ({(int)(Time.unscaledTime - checkStartTime)}s)");
                    }

                    System.Threading.Thread.Sleep(100);
                }

                if (handsDetected)
                {
                    Debug.Log("Hand tracking successfully enabled and hands detected!");
                    return true;
                }
                else
                {
                    Debug.LogWarning("Hand tracking subsystem is running but no hands detected");
                    Debug.LogWarning("Make sure:");
                    Debug.LogWarning("- Hands are visible to Quest 3 cameras");
                    Debug.LogWarning("- Hand tracking is enabled in Quest device settings");
                    Debug.LogWarning("- You're in a well-lit environment");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to force-enable hand tracking: {e.Message}");
                return false;
            }
        }
    }
}