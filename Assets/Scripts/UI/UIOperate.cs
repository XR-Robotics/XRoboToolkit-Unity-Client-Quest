using System.Collections.Generic;
using System.Net;
using Robot;
using Robot.Conf;
// using Unity.XR.PICO.TOBSupport;
// using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIOperate : MonoBehaviour
{
    public Text SN;
    public Text LocalIP;
    public Text TargetIP;
    public Text TrackNum;
    public Toggle HeadTog;
    public Toggle ControllerTog;
    public Toggle HandTrackingTog;
    public Toggle SendTog;
    public Toggle AcontrolerTog;
    public Dropdown bodyModeDrop;
    public TcpHandler TcpHandler;
    public Text BodyInfo;
    public Toggle HighAccuracy;
    public Text Version;
    public Button ReconnectBtn;
    public Toggle NetshareTog;

    public GameObject Simulator;
    public GameObject CameraObj;
    public GameObject IpInputDialog;
    public GameObject ExtDevPanel;
    public InputAction SendDataAction;

    [Space(30)][Header("Refactoring")] public VideoSourceManager videoSource;
    public VideoSourceConfigManager sourceConfig => videoSource.videoSourceConfigManager;

    public Dropdown videoSourceDropdown;

    // Start is called before the first frame update
    private void Awake()
    {
#if UNITY_EDITOR
        if (Simulator != null)
        {
            Simulator.SetActive(true);
        }
#endif
        // ReconnectBtn.gameObject.SetActive(false);

        // bodyModeDrop.onValueChanged.AddListener(OnBodyModeDrop);
        HeadTog.onValueChanged.AddListener(OnHeadTog);
        ControllerTog.onValueChanged.AddListener(OnControllerTog);
        HandTrackingTog.onValueChanged.AddListener(OnHandTrackingTog);

        SendTog.onValueChanged.AddListener(OnSendTog);
        Version.text = "v: " + Application.version;
        // HighAccuracy.gameObject.SetActive(bodyModeDrop.value > 0);
        NetshareTog.onValueChanged.AddListener(OnNetShareTog);
        // HighAccuracy.onValueChanged.AddListener(OnHighAccuracy);
        ReconnectBtn.onClick.AddListener(OnReconnectBtn);
        //The shared network function is only available on B-end devices.
        NetshareTog.gameObject.SetActive(false);
        // Bypass getting sn via enterprise service to enable data transport
        SetDeviceSN("TestDevice");
        // bool intEnterprise = PXR_Enterprise.InitEnterpriseService();
        // Debug.Log("---InitEnterpriseService :" + intEnterprise);
        // PXR_Enterprise.BindEnterpriseService(OnBindEnterpriseService);

        // if (CameraObj != null)
        // {
        //     CameraObj.SetActive(false);
        // }

        AndroidProxy.CallBack += OnAndroidCallBack;
#if UNITY_EDITOR
        SetDeviceSN("TestDevice");
#endif
        // Refactoring
        sourceConfig.OnInitialized += OnSourceConfigOnOnInitialized;
        // Initialize video source configuration
        sourceConfig.Initialize();

        // Enable SendDataAction for Quest controllers
        if (SendDataAction != null)
        {
            SendDataAction.Enable();
            Debug.Log("SendDataAction enabled successfully");
        }
        else
        {
            Debug.LogWarning("SendDataAction.action is null - input action not properly configured");
        }
        
        // set FPS to 90
        OVRPlugin.systemDisplayFrequency = 90.0f;
    }

    private void OnSourceConfigOnOnInitialized()
    {
        // Update videoSourceDropdown options
        print("OnSourceConfigOnOnInitialized");
        videoSourceDropdown.ClearOptions();
        videoSourceDropdown.AddOptions(sourceConfig.GetVideoSourceNames());
    }

    private void OnAndroidCallBack(string key, string value)
    {
        if (key == "RequestPermissionsBack")
        {
            if (value == "0")
            {
                if (CameraObj != null)
                {
                    CameraObj.SetActive(true);
                }
            }
            else
            {
                Toast.Show("Permission denied!");
            }
        }
    }

    private void OnReconnectBtn()
    {
        TcpHandler.Reconnect();
    }

    public void TcpConnect(string ip)
    {
        TargetIP.text = "PC Service: " + ip;
        ReconnectBtn.gameObject.SetActive(true);
        TcpHandler.Connect(ip);
        ConnectSuccess();
    }

    public void ConnectSuccess()
    {
        TargetIP.text = "PC Service: " + TcpHandler.GetTargetIP;
    }

    private void OnBindEnterpriseService(bool bind)
    {
        Debug.Log("OnBindEnterpriseService " + bind);
        if (bind)
        {
            //The shared network function is only available on B-end devices.
            NetshareTog.gameObject.SetActive(true);
            // PXR_Enterprise.GetSwitchSystemFunctionStatus(SystemFunctionSwitchEnum.SFS_USB_TETHERING,
            //     (value) => { NetshareTog.SetIsOnWithoutNotify(value == 1); });
            //
            // string sn = PXR_Enterprise.StateGetDeviceInfo(SystemInfoEnum.EQUIPMENT_SN);
            // SetDeviceSN(sn);
        }
    }

    private void SetDeviceSN(string sn)
    {
        TcpHandler.SetDeviceSn(sn);
        Debug.Log("SN: " + sn);
        SN.text = "SN: " + sn;
    }

    private void OnNetShareTog(bool ison)
    {
        //     Debug.Log("OnNetShareTog:" + ison);
        //     if (ison)
        //         PXR_Enterprise.SwitchSystemFunction(SystemFunctionSwitchEnum.SFS_USB_TETHERING, SwitchEnum.S_ON);
        //     else
        //         PXR_Enterprise.SwitchSystemFunction(SystemFunctionSwitchEnum.SFS_USB_TETHERING, SwitchEnum.S_OFF);
        //
        //     PXR_Enterprise.GetSwitchSystemFunctionStatus(SystemFunctionSwitchEnum.SFS_USB_TETHERING,
        //         (value) => { Debug.Log("SFS_USB_TETHERING:" + value); });
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public void OnExtraDevBtn()
    {
        ExtDevPanel.SetActive(true);
    }

    public void OnWriteIpBtn()
    {
        IpInputDialog.SetActive(true);
    }

    private void OnBodyModeDrop(int index)
    {
        // TODO
        
        // TrackingData.TrackingType tType = (TrackingData.TrackingType)bodyModeDrop.value;
        // int res = 0;
        // bool support = false;

        // TODO: body tracking in Quest
        // MotionTrackerMode trackingMode = PXR_MotionTracking.GetMotionTrackerMode();
        // if (tType == TrackingData.TrackingType.Body)
        // {
        //     if (trackingMode != MotionTrackerMode.BodyTracking)
        //     {
        //         res = PXR_MotionTracking.CheckMotionTrackerModeAndNumber(MotionTrackerMode.BodyTracking,
        //             MotionTrackerNum.TWO);
        //     }
        //
        //     PXR_MotionTracking.GetBodyTrackingSupported(ref support);
        // }
        // else if (tType == TrackingData.TrackingType.Motion)
        // {
        //     if (trackingMode != MotionTrackerMode.MotionTracking)
        //     {
        //         res = PXR_MotionTracking.CheckMotionTrackerModeAndNumber(MotionTrackerMode.MotionTracking,
        //             MotionTrackerNum.ONE);
        //     }
        //
        //     support = true;
        // }

        // if (!support || res != 0)
        // {
        //     BodyInfo.text = "Tracker exception, please connect to calibrate tracker!";
        //     BodyInfo.color = Color.red;
        //
        //     bodyModeDrop.SetValueWithoutNotify(0);
        //     return;
        // }
        //
        // BodyInfo.color = Color.white;
        // BodyInfo.text = "Tracker detection is normal!";
        //
        // UpdateBodyTracking();
    }


    public void OnOpenCameraOperate()
    {
        if (CameraObj != null)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.Camera) &&
                Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                CameraObj.SetActive(!CameraObj.activeSelf);
            }
            else if (!CameraObj.activeSelf)
            {
                var permissionCallbacks = new PermissionCallbacks();
                permissionCallbacks.PermissionGranted += PermissionGranted;
                permissionCallbacks.PermissionDenied += PermissionDenied;

                string[] permissions = { Permission.Camera, Permission.Microphone };
                Permission.RequestUserPermissions(permissions, permissionCallbacks);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
        }
    }

    private void PermissionDenied(string obj)
    {
        Toast.Show("Permission denied!");
    }

    private void PermissionGranted(string obj)
    {
        if (CameraObj != null)
        {
            CameraObj.SetActive(true);
        }
    }

    private void RefreshLocalIP()
    {
        string localIP = Utils.GetLocalIPv4();
        LocalIP.text = localIP;
    }

    // Obtain the local IPv6 address
    private string GetLocalIPv6()
    {
        string localIP = "Not found";
        foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                localIP = ip.ToString();
                break;
            }
        }

        return localIP;
    }


    private void OnHeadTog(bool on)
    {
        TrackingData.SetHeadOn(on);
    }

    private void OnControllerTog(bool on)
    {
        TrackingData.SetControllerOn(on);

        // Disable on quest for now
        // // Ensure mutual exclusivity with HandTrackingTog
        // if (on && HandTrackingTog.isOn)
        // {
        //     HandTrackingTog.SetIsOnWithoutNotify(false);
        //     TrackingData.SetHandTrackingOn(false);
        // }
    }

    private void OnHandTrackingTog(bool on)
    {
        TrackingData.SetHandTrackingOn(on);

        // Disable on quest for now
        // // Ensure mutual exclusivity with ControllerTog
        // if (on && ControllerTog.isOn)
        // {
        //     ControllerTog.SetIsOnWithoutNotify(false);
        //     TrackingData.SetControllerOn(false);
        // }
    }

    private void OnSendTog(bool on)
    {
        TcpHandler.SendTrackingData = on;
        // Reset FPS
        if (!on)
        {
            FPSDisplay.Reset();
        }
    }

    private void OnHighAccuracy(bool on)
    {
        UpdateBodyTracking();
    }

    private void UpdateBodyTracking()
    {
        TrackingData.TrackingType tType = (TrackingData.TrackingType)bodyModeDrop.value;
        HighAccuracy.gameObject.SetActive(bodyModeDrop.value > 0);
        Debug.Log("UpdateBodyTracking " + tType);
        TrackNum.text = "";
        // TODO: Update for Quest
        // // Set bone length
        // BodyTrackingBoneLength boneLength = new BodyTrackingBoneLength();
        // if (bodyModeDrop.value <= 0)
        // {
        //     int ret = PXR_MotionTracking.StopBodyTracking();
        //     BodyInfo.text = "BodyTracking close";
        // }
        // else
        // {
        //     MotionTrackerConnectState state = new MotionTrackerConnectState();
        //     PXR_MotionTracking.GetMotionTrackerConnectStateWithSN(ref state);
        //     //  PXR_MotionTracking.GetMotionTrackerConnectStateWithSN(ref state);
        //     TrackNum.text = "TrackerNum:" + state.trackerSum;
        //
        //     if (tType == TrackingData.TrackingType.Body)
        //     {
        //         BodyTrackingMode mode = BodyTrackingMode.BTM_FULL_BODY_LOW;
        //         if (HighAccuracy.isOn)
        //         {
        //             mode = BodyTrackingMode.BTM_FULL_BODY_HIGH;
        //         }
        //
        //         // Enable full body motion capture default mode
        //         int ret = PXR_MotionTracking.StartBodyTracking(mode, boneLength);
        //         BodyInfo.text = "Start BodyTracking " + ret;
        //         Debug.Log(" UpdateBodyTracking :" + ret + " trackerSum:" + state.trackerSum);
        //     }
        //     else if (tType == TrackingData.TrackingType.Motion)
        //     {
        //         BodyInfo.text = "Start MotionTracking";
        //     }
        // }

        TrackingData.SetTrackingType(tType);
    }

    private float _lastTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (TcpHandler.State != SocketState.WORKING)
        {
            if (Time.time - _lastTime > 2)
            {
                _lastTime = Time.time;
                RefreshLocalIP();
            }
        }

        if (AcontrolerTog != null && AcontrolerTog.isOn)
        {
            // Use Input Actions only
            if (SendDataAction != null && SendDataAction.WasReleasedThisFrame())
            {
                SendTog.isOn = !SendTog.isOn;
                LogWindow.Info("Sending data: " + SendTog.isOn);
                Debug.Log("SendDataAction triggered - Sending data: " + SendTog.isOn);
            }
        }
    }

    public void OnQuitBtn()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        // Disable SendDataAction when object is destroyed
        if (SendDataAction != null)
        {
            SendDataAction.Disable();
        }
    }
}