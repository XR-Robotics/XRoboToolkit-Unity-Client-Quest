using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Robot
{
    public class Utils
    {
        /// <summary>
        /// Gets the current timestamp in microseconds since Unix epoch (January 1, 1970)
        /// </summary>
        /// <returns>Current timestamp in microseconds</returns>
        public static long GetCurrentTimestamp()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTimestamp = (long)((DateTime.UtcNow - epoch).TotalMilliseconds * 1000000);
            return currentTimestamp;
        }

        /// <summary>
        /// Gets the local IPv4 address of the machine
        /// </summary>
        /// <returns>Local IPv4 address as a string, or "Not found" if none available</returns>
        //Obtain the local IPv4 address
        public static string GetLocalIPv4()
        {
            string localIP = "Not found";
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }

            return localIP;
        }

        /// <summary>
        /// Check if the local port is occupied
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);

                listener.Start();

                listener.Stop();

                return true;
            }
            catch (SocketException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an automatically assigned available port from the system
        /// </summary>
        /// <returns>An available port number</returns>
        public static int GetAvailablePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0); // 传入 0 让系统自动分配可用端口
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        /// <summary>
        /// Gets camera intrinsics as a formatted string for external devices
        /// </summary>
        /// <param name="width">Camera width</param>
        /// <param name="height">Camera height</param>
        /// <returns>Camera intrinsics string in scientific notation format</returns>
        public static string GetCameraIntrinsicsStrE(int width, int height)
        {
            string cameraIntrinsics = "";
            // double[] intrinsics = PXR_Enterprise.GetCameraIntrinsicsfor4U(width, height, 76.35f, 61.05f);
            // for (int i = 0; i < intrinsics.Length; i++)
            // {
            //     if (i > 0)
            //     {
            //         cameraIntrinsics += ",";
            //     }
            //
            //     cameraIntrinsics += intrinsics[i].ToString("E16");
            // }

            return cameraIntrinsics;
        }

        /// <summary>
        /// Gets camera extrinsics (left and right camera parameters) as a formatted string
        /// </summary>
        /// <returns>Camera extrinsics string with left and right parameters separated by '|'</returns>
        public static string GetCameraExtrinsicsStrE()
        {
            // PXR_EnterprisePlugin.GetCameraExtrinsics(out var leftExtrinsics, out var rightExtrinsics);
            string cameraExtrinsics = "";
            //
            // //left
            // for (int i = 0; i < leftExtrinsics.Length; i++)
            // {
            //     if (i > 0)
            //     {
            //         cameraExtrinsics += ",";
            //     }
            //
            //     cameraExtrinsics += leftExtrinsics[i].ToString("E16");
            // }
            //
            //
            // cameraExtrinsics += "|";
            // //right
            // for (int i = 0; i < rightExtrinsics.Length; i++)
            // {
            //     if (i > 0)
            //     {
            //         cameraExtrinsics += ",";
            //     }
            //
            //     cameraExtrinsics += rightExtrinsics[i].ToString("E16");
            // }
            //
            return cameraExtrinsics;
        }

        /// <summary>
        /// Writes a formatted log message with tag and message content
        /// Uses colored output in Unity Editor and standard logcat format in builds
        /// </summary>
        /// <param name="tag">The tag to identify the log source</param>
        /// <param name="msg">The message content to log</param>
        public static void WriteLog(string tag, string msg)
        {
#if UNITY_EDITOR
            Debug.Log($"[XRoboToolkit]<color=red>{tag}</color>\t{msg}");
#else
            // Output to logcat
            Debug.Log($"[XRoboToolkit]\t{tag}\t{msg}");
#endif
        }
    }
}