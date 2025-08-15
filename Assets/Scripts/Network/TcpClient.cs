using UnityEngine;
using System;

namespace Robot.V2.Network
{
    public enum ClientStatus
    {
        None,
        Connected,
        Disconnected,
    }
    public class TcpClient : AndroidJavaProxy
    {
        private static TcpClient _callbackProxy = new TcpClient();

        public static TcpClient CallbackProxy => _callbackProxy;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<byte[], int> OnDataReceived;
        public event Action<string, Exception> OnError;

        private string logTag = "TcpClient(C#)";

        public static ClientStatus Status { get; private set; } = ClientStatus.None;

        public TcpClient() : base("com.xrobotoolkit.visionplugin.quest.TcpClient$ClientCallback")
        {

        }

        private static AndroidJavaObject _javaObj = null;

        /// <summary>
        /// Gets or creates the Java TcpClient object for native Android integration
        /// </summary>
        /// <returns>AndroidJavaObject instance of the TcpClient</returns>
        private static AndroidJavaObject GetJavaObject()
        {
            if (_javaObj == null)
            {
                _javaObj = new AndroidJavaObject("com.xrobotoolkit.visionplugin.quest.TcpClient");
            }

            return _javaObj;
        }

        /// <summary>
        /// Connects to a TCP server at the specified IP address and port
        /// </summary>
        /// <param name="ip">Server IP address to connect to</param>
        /// <param name="port">Server port to connect to</param>
        /// <param name="onConnected">Callback to execute when connection is established</param>
        public static void ConnectToServer(string ip, int port, Action onConnected)
        {
            LogWindow.Info($"Attempting to connect to server at {ip}:{port}");
            _callbackProxy.OnConnected = onConnected;
            GetJavaObject().Call("connectToServer", ip, port, _callbackProxy);
            Status = ClientStatus.Connected;
        }

        /// <summary>
        /// Sends byte data to the connected TCP server
        /// </summary>
        /// <param name="data">Byte array data to send</param>
        public static void Send(byte[] data)
        {
            GetJavaObject().Call("send", data);
        }

        /// <summary>
        /// Disconnects from the TCP server
        /// </summary>
        public static void Disconnect()
        {
            LogWindow.Info("Disconnecting from TCP server");
            GetJavaObject().Call("disconnect");
            Status = ClientStatus.Disconnected;
        }

        /// <summary>
        /// JNI callback method called when connection to server is established
        /// </summary>
        public void onConnected()
        {
            Utils.WriteLog(logTag, "Java TcpClient: Connected");
            LogWindow.Info("TCP Client successfully connected to server");
            Status = ClientStatus.Connected;
            OnConnected?.Invoke();
        }

        /// <summary>
        /// JNI callback method called when disconnected from server
        /// </summary>
        public void onDisconnected()
        {
            Utils.WriteLog(logTag, "Java TcpClient: Disconnected");
            LogWindow.Warn("TCP Client disconnected from server");
            Status = ClientStatus.Disconnected;
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// JNI callback method called when data is received from server
        /// </summary>
        /// <param name="data">Received data as byte array</param>
        /// <param name="length">Length of the received data</param>
        public void onDataReceived(byte[] data, int length)
        {
            Utils.WriteLog(logTag, $"Java TcpClient: Received data of length {length}");
            OnDataReceived?.Invoke(data, length);
        }

        /// <summary>
        /// JNI callback method called when an error occurs in the TCP client
        /// </summary>
        /// <param name="errorMessage">Error message from the Java layer</param>
        /// <param name="exception">Java exception object</param>
        public void onError(string errorMessage, AndroidJavaObject exception)
        {
            Utils.WriteLog(logTag, $"Java TcpClient: Error - {errorMessage} - {exception}");
            LogWindow.Error($"TCP Client error: {errorMessage}");
            OnError?.Invoke(errorMessage, new Exception($"{errorMessage} - {exception}"));
        }
    }
}