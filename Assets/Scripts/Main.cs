// using Unity.XR.PICO.TOBSupport;
// using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class Main : MonoBehaviour
{
    private void Awake()
    {
        // DebugManager.instance.enableRuntimeUI = false;
        Application.logMessageReceived += OnLogMessageReceived;
        XRSettings.eyeTextureResolutionScale = 1.5f;
        // PXR_Manager.EnableVideoSeeThrough = true;
        //Closing the security fence is only effective on B-end devices.
        // PXR_Enterprise.SwitchSystemFunction(SystemFunctionSwitchEnum.SFS_SECURITY_ZONE_PERMANENTLY, SwitchEnum.S_OFF);
    }

    /// <summary>
    /// Handles log messages received from the Unity logging system
    /// Displays errors as toast messages and pushes all logs to the LogView
    /// </summary>
    /// <param name="condition">The log message</param>
    /// <param name="stackTrace">The stack trace if available</param>
    /// <param name="type">The type of log message (Error, Warning, Log, etc.)</param>
    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        LogView.Push(condition, stackTrace, type);
        if (type == LogType.Error)
        {
            Toast.Show(condition);
        }
    }

    private void OnEnable()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("OnEnable");
            // PXR_Enterprise.OpenVSTCamera();
        }
    }

    private void OnDisable()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("OnDisable");
            // PXR_Enterprise.CloseVSTCamera();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Debug.Log("OnApplicationPause " + pauseStatus);
        // if (pauseStatus)
        // {
        //     PXR_Enterprise.CloseVSTCamera();
        //     PXR_Enterprise.SwitchSystemFunction(SystemFunctionSwitchEnum.SFS_SECURITY_ZONE_PERMANENTLY,
        //         SwitchEnum.S_ON);
        // }
        // else
        // {
        //     PXR_Manager.EnableVideoSeeThrough = true;
        //     //Closing the security fence is only effective on B-end devices.
        //     PXR_Enterprise.SwitchSystemFunction(SystemFunctionSwitchEnum.SFS_SECURITY_ZONE_PERMANENTLY,
        //         SwitchEnum.S_OFF);
        //     bool openVstRes = PXR_Enterprise.OpenVSTCamera();
        //
        //     Debug.Log("openVstRes:" + openVstRes);
        // }
    }
}