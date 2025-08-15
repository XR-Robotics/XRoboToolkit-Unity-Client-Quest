using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaDecoderUDP
{
    private static AndroidJavaObject _javaObj = null;

    /// <summary>
    /// Gets or creates the Java MediaDecoderUDP object for native Android integration
    /// </summary>
    /// <returns>AndroidJavaObject instance of the UDP MediaDecoder</returns>
    private static AndroidJavaObject GetJavaObject()
    {
        if (_javaObj == null)
        {
            _javaObj = new AndroidJavaObject("com.xrobotoolkit.visionplugin.quest.MediaDecoderUDP");
        }

        return _javaObj;
    }

    /// <summary>
    /// Initializes the UDP MediaDecoder with Unity texture parameters
    /// </summary>
    /// <param name="unityTextureId">Unity texture identifier to render decoded video to</param>
    /// <param name="width">Width of the video texture</param>
    /// <param name="height">Height of the video texture</param>
    public static void initialize(int unityTextureId, int width, int height)
    {
        GetJavaObject().Call("initialize", unityTextureId, width, height);
    }

    /// <summary>
    /// Starts the UDP MediaDecoder server on the specified port
    /// </summary>
    /// <param name="port">UDP port number to listen for incoming video streams</param>
    /// <param name="record">Whether to record the incoming video stream</param>
    public static void startServer(int port, bool record)
    {
        GetJavaObject().Call("startServer", port, record);
    }

    /// <summary>
    /// Checks if a new frame has been received via UDP and is ready for processing
    /// </summary>
    /// <returns>True if a new frame is available, false otherwise</returns>
    public static bool isUpdateFrame()
    {
        return GetJavaObject().Call<bool>("isUpdateFrame");
    }

    /// <summary>
    /// Updates the Unity texture with the latest decoded frame from UDP stream
    /// </summary>
    public static void updateTexture()
    {
        GetJavaObject().Call("updateTexture");
    }

    /// <summary>
    /// Releases UDP MediaDecoder resources and cleans up native objects
    /// </summary>
    public static void release()
    {
        GetJavaObject().Call("release");
    }
}