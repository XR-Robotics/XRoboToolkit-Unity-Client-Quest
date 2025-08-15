using System;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

namespace Robot.V2.Network
{
    public enum ServerStatus
    {
        None,
        Started,
        Stopped,
    }
    public class TcpServer : AndroidJavaProxy
    {

        private static TcpServer _callbackProxy = new TcpServer();

        public static TcpServer CallbackProxy => _callbackProxy;

        public event Action<int> OnServerStarted;
        public event Action OnClientDisconnected;
        public event Action<byte[], int> OnDataReceived;
        public event Action<string> OnError;
        public event Action OnServerStopped;

        private string logTag = "TcpServer(C#)";

        public static ServerStatus Status { get; private set; } = ServerStatus.None;

        public TcpServer() : base("com.xrobotoolkit.visionplugin.quest.TcpServer$ServerCallback")
        {

        }

        private static AndroidJavaObject _javaObj = null;

        /// <summary>
        /// Gets or creates the Java TcpServer object for native Android integration
        /// </summary>
        /// <returns>AndroidJavaObject instance of the TcpServer</returns>
        private static AndroidJavaObject GetJavaObject()
        {
            if (_javaObj == null)
            {
                _javaObj = new AndroidJavaObject("com.xrobotoolkit.visionplugin.quest.TcpServer");
            }

            return _javaObj;
        }

        /// <summary>
        /// Starts a TCP server on the specified port
        /// </summary>
        /// <param name="port">Port number to listen on</param>
        /// <param name="onServerStarted">Callback to execute when server is started</param>
        public static void StartTCPServer(int port, Action<int> onServerStarted)
        {
            LogWindow.Info($"Starting TCP server on port {port}");
            _callbackProxy.OnServerStarted = onServerStarted;

            GetJavaObject().Call("startTCPServer", port, _callbackProxy);
            Status = ServerStatus.Started;
        }

        /// <summary>
        /// Stops the TCP server
        /// </summary>
        public static void StopServer()
        {
            LogWindow.Info("Stopping TCP server");
            GetJavaObject().Call("stopServer");
            Status = ServerStatus.Stopped;
        }

        // JNI callback methods
        /// <summary>
        /// JNI callback method called when the server successfully starts
        /// </summary>
        /// <param name="port">Port number the server started on</param>
        public void onServerStarted(int port)
        {
            Utils.WriteLog(logTag, $"Server started on port: {port}");
            LogWindow.Info($"TCP Server successfully started on port {port}");
            OnServerStarted?.Invoke(port);
        }

        /// <summary>
        /// JNI callback method called when a client connects to the server
        /// </summary>
        /// <param name="socket">Java socket object representing the connected client</param>
        public void onClientConnected(AndroidJavaObject socket)
        {
            Utils.WriteLog(logTag, "Client connected: " + socket);
            LogWindow.Info("Client connected to TCP server");
        }

        /// <summary>
        /// JNI callback method called when a client disconnects from the server
        /// </summary>
        public void onClientDisconnected()
        {
            Utils.WriteLog(logTag, "Client disconnected");
            LogWindow.Warn("Client disconnected from TCP server");
            OnClientDisconnected?.Invoke();
        }

        /// <summary>
        /// JNI callback method called when data is received from a connected client
        /// </summary>
        /// <param name="data">Received data as byte array</param>
        /// <param name="length">Length of the received data</param>
        public void onDataReceived(byte[] data, int length)
        {
            Utils.WriteLog(logTag, "Received data of length: " + length);
            // decode
            var s = Encoding.UTF8.GetString(data);
            Utils.WriteLog(logTag, "Received data: " + s);
            OnDataReceived?.Invoke(data, length);
            Utils.WriteLog(logTag, "onDataReceived invoked");
        }

        /// <summary>
        /// JNI callback method called when an error occurs in the TCP server
        /// </summary>
        /// <param name="errorMessage">Error message from the Java layer</param>
        /// <param name="exception">Java exception object</param>
        public void onError(string errorMessage, AndroidJavaObject exception)
        {
            Utils.WriteLog(logTag, "Server error: " + errorMessage);
            LogWindow.Error($"TCP Server error: {errorMessage}");
            OnError?.Invoke($"{errorMessage} - {exception}");
        }

        /// <summary>
        /// JNI callback method called when the server is stopped
        /// </summary>
        public void onServerStopped()
        {
            Utils.WriteLog(logTag, "Server stopped");
            LogWindow.Info("TCP Server stopped");
            OnServerStopped?.Invoke();
        }

    }
}