using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AndroidProxy : MonoBehaviour
{
    private Queue<string> _androidMassages = new Queue<string>();
    public static event Action<string, string> CallBack;

    /// <summary>
    /// Receives messages from Android native code and queues them for processing
    /// </summary>
    /// <param name="msg">Message string from Android, format: "key|value"</param>
    public void AndroidCall(string msg)
    {
        Debug.Log("AndroidCall:" + msg);
        _androidMassages.Enqueue(msg);
    }

    private void Update()
    {
        while (_androidMassages.Count > 0)
        {
            string msg = _androidMassages.Dequeue();
            if (CallBack != null)
            {
                string[] arr = msg.Split('|');
                if (arr.Length > 1)
                {
                    CallBack.Invoke(arr[0], arr[1]);
                }
            }
        }
    }
}