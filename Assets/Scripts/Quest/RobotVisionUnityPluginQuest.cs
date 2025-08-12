using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Unity integration script for Robot Vision Unity Plugin Quest
/// This script handles communication with the Android native plugin for texture rendering
/// </summary>
public class RobotVisionUnityPluginQuest : MonoBehaviour
{
    [Header("Plugin Configuration")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private int width = 2560;
    [SerializeField] private int height = 720;
    [SerializeField] private int port = 12345;
    [SerializeField] private bool recordVideo = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Native plugin callback
    [System.Runtime.InteropServices.DllImport("NativeUnityRenderingCallback")]
    private static extern IntPtr GetRenderEventFunc();

    // Android Java plugin callback handler
    private class PluginInitializedCallback : AndroidJavaProxy
    {
        private Action<int> callback;

        public PluginInitializedCallback(Action<int> callback) : base(
            "com.xrobotoolkit.visionplugin.quest.RobotVisionUnityPluginQuest$OnInitializedListener")
        {
            this.callback = callback;
        }

        public void onInitialized(int textureId)
        {
            this.callback(textureId);
        }
    }

    private IntPtr nativeTexPtr;
    private bool nativeTexPtrSet;
    private AndroidJavaClass robotVisionPlugin;
    private CommandBuffer commandBuffer;
    private Texture2D externalTexture;

    void Start()
    {
        // InitializePlugin();
    }

    public void InitializeQuestPlugin(int width, int height, int port)
    {
        this.width = width;
        this.height = height;
        this.port = port;
        recordVideo = false;
        InitializePlugin();
    }

    private void InitializePlugin()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[RobotVisionPlugin] Initializing plugin with dimensions: {width}x{height}, port: {port}");

            // Initialize command buffer for native rendering
            commandBuffer = new CommandBuffer();
            commandBuffer.IssuePluginEvent(GetRenderEventFunc(), 1);

            if (Camera.main != null)
            {
                Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
            }
            else
            {
                Debug.LogWarning("[RobotVisionPlugin] Main camera not found, command buffer not added");
            }

            // Initialize Android Java plugin
            robotVisionPlugin = new AndroidJavaClass("com.xrobotoolkit.visionplugin.quest.RobotVisionUnityPluginQuest");

            // Create media decoder texture
            robotVisionPlugin.CallStatic(
                "CreateMediaDecoderTexture",
                width,
                height,
                port,
                recordVideo,
                new PluginInitializedCallback(OnTextureInitialized)
            );

            if (enableDebugLogs)
                Debug.Log("[RobotVisionPlugin] Plugin initialization requested");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RobotVisionPlugin] Failed to initialize plugin: {e.Message}");
        }
    }

    private void OnTextureInitialized(int textureId)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[RobotVisionPlugin] Texture initialized with ID: {textureId}");

            nativeTexPtr = (IntPtr)textureId;
            nativeTexPtrSet = true;

            StartCoroutine(SetupExternalTexture());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RobotVisionPlugin] Error during texture initialization: {e.Message}");
        }
    }

    private IEnumerator SetupExternalTexture()
    {
        // Wait a frame to ensure texture is fully initialized
        yield return new WaitForEndOfFrame();

        try
        {
            // Create external texture from native pointer
            externalTexture = Texture2D.CreateExternalTexture(
                width,
                height,
                TextureFormat.RGBA32,
                false,
                false,
                nativeTexPtr
            );

            if (externalTexture != null)
            {
                if (rawImage != null)
                {
                    rawImage.texture = externalTexture;
                    if (enableDebugLogs)
                        Debug.Log($"[RobotVisionPlugin] External texture assigned to UI element");
                }
                else
                {
                    Debug.LogWarning("[RobotVisionPlugin] RawImage component not assigned");
                }
            }
            else
            {
                Debug.LogError("[RobotVisionPlugin] Failed to create external texture");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RobotVisionPlugin] Error setting up external texture: {e.Message}");
        }
    }

    void Update()
    {
        // The native plugin handles automatic frame updates via SurfaceTexture callbacks
    }

    void OnDestroy()
    {
        CleanupPlugin();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Do nothing on pause, as the plugin handles its own state
    }

    public void CleanupPlugin()
    {
        try
        {
            if (robotVisionPlugin != null)
            {
                // Stop the decoder and cleanup resources
                robotVisionPlugin.CallStatic("StopDecoder");
                robotVisionPlugin.CallStatic("Release");

                if (enableDebugLogs)
                    Debug.Log("[RobotVisionPlugin] Plugin resources cleaned up");
            }

            if (commandBuffer != null && Camera.main != null)
            {
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
            }

            if (externalTexture != null)
            {
                DestroyImmediate(externalTexture);
                externalTexture = null;
            }

            nativeTexPtrSet = false;
            nativeTexPtr = IntPtr.Zero;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RobotVisionPlugin] Error during cleanup: {e.Message}");
        }
    }

    // Public methods for runtime configuration
    public void SetVideoPort(int newPort)
    {
        port = newPort;
        if (enableDebugLogs)
            Debug.Log($"[RobotVisionPlugin] Video port set to: {newPort}");
    }

    public void SetVideoDimensions(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
        if (enableDebugLogs)
            Debug.Log($"[RobotVisionPlugin] Video dimensions set to: {newWidth}x{newHeight}");
    }

    public void SetRecordingEnabled(bool enabled)
    {
        recordVideo = enabled;
        if (enableDebugLogs)
            Debug.Log($"[RobotVisionPlugin] Video recording enabled: {enabled}");
    }

    public bool IsPluginInitialized()
    {
        return nativeTexPtrSet && robotVisionPlugin != null;
    }
}
