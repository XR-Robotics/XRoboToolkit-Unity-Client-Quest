using System;
using Robot;
using Robot.Network;
using Robot.V2.Network;
using UnityEngine;
using XRoboToolkit.Network;

namespace Network
{
    public static class NetworkCommand
    {
        public const string OPEN_CAMERA = "OPEN_CAMERA";
        public const string CLOSE_CAMERA = "CLOSE_CAMERA";
    }

    public class NetworkCommander : MonoBehaviour
    {
        private NetworkDataProcessor processor;
        private TcpManager tcpManager;

        private string logTag = "NetworkCommander";

        public static NetworkCommander Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public NetworkDataProcessor Processor
        {
            get { return processor; }
        }

        private void Start()
        {
            InitializeNetworkProcessor();
        }

        /// <summary>
        /// Initializes the network data processor and registers command handlers
        /// </summary>
        private void InitializeNetworkProcessor()
        {
            processor = new NetworkDataProcessor();

            // Register example command handlers
            processor.RegisterCommandHandler(NetworkCommand.OPEN_CAMERA, new OpenCameraHandler());
            processor.RegisterCommandHandler(NetworkCommand.CLOSE_CAMERA, new CloseCameraHandler());

            Debug.Log("Network data processor initialized with " + processor.GetRegisteredCommands().Length +
                      " handlers");
        }

        void OnDestroy()
        {
            // Clean up
            processor?.ClearAllHandlers();
        }

        #region Send Data

        /// <summary>
        /// Sends a network command with optional data payload to the connected client
        /// </summary>
        /// <param name="command">The command string to send</param>
        /// <param name="data">Optional byte array data payload</param>
        public void SendCommand(string command, byte[] data = null)
        {
            var protocol = new NetworkDataProtocol(command, data);
            var serializedData = NetworkDataProtocolSerializer.Serialize(protocol);
            // Send the packet using TcpManager
            TcpManager.Instance.ClientSend(serializedData);
        }

        /// <summary>
        /// Sends an OPEN_CAMERA command with camera configuration data
        /// </summary>
        /// <param name="data">Camera configuration data as byte array</param>
        public void OpenCamera(byte[] data)
        {
            LogWindow.Warn("Sending OPEN_CAMERA command.");
            SendCommand(NetworkCommand.OPEN_CAMERA, data);
        }

        /// <summary>
        /// Sends a CLOSE_CAMERA command to stop camera streaming
        /// </summary>
        public void CloseCamera()
        {
            LogWindow.Warn("Sending CLOSE_CAMERA command.");
            SendCommand(NetworkCommand.CLOSE_CAMERA, new byte[0]);
        }

        #endregion

        private class OpenCameraHandler : ICommandHandler
        {
            /// <summary>
            /// Handles OPEN_CAMERA command by deserializing camera configuration and initiating camera stream
            /// </summary>
            /// <param name="data">Serialized camera configuration data</param>
            public void HandleCommand(byte[] data)
            {
                Debug.Log("Handling OPEN_CAMERA command with data: " + BitConverter.ToString(data));
                // Add logic to open camera

                var cameraConfig = CameraRequestSerializer.Deserialize(data);
                Utils.WriteLog("OpenCameraHandler", $"Received camera config: {cameraConfig}");
                LogWindow.Warn($"Handling OPEN_CAMERA: {cameraConfig.ToString()}");

                // The stream only works for the VR headset
                if (cameraConfig.camera.Equals("VR"))
                {
                    // TODO: Support quest cameras in the future
                }
            }
        }

        private class CloseCameraHandler : ICommandHandler
        {
            /// <summary>
            /// Handles CLOSE_CAMERA command by stopping the camera preview
            /// </summary>
            /// <param name="data">Command data (typically empty for close commands)</param>
            public void HandleCommand(byte[] data)
            {
                Debug.Log("Handling CLOSE_CAMERA command with data: " + BitConverter.ToString(data));
                LogWindow.Warn("Handling CLOSE_CAMERA command");
                // Stop the camera preview
            }
        }
    }
}