using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SetLERE : MonoBehaviour
{

    public GameObject CanvCE;
    public RemoteCameraWindow remoteCameraWindow;
    public Material matCE;

    //private float visibleRatio = 0.75f;
    //private float contentRatio = 0.88f;
    private float visibleRatio = 0.555f;
    private float contentRatio = 1.8f;
    private float heightCompressionFactor = 1.333333f; // 4:3 aspect ratio

    public void UpdateParameters(float visible, float content, float heightCompression)
    {
        // Adjust the ratios based on the height compression factor
        visibleRatio = visible;
        contentRatio = content;
        heightCompressionFactor = heightCompression;

        // Log the updated values
        Debug.Log(
            $"Updated Ratios - visible: {visibleRatio}, content: {contentRatio}, heightCompression: {heightCompressionFactor}");
    }

    public void ResetCanvases()
    {
        CanvCE.SetActive(false);
    }

    void Update()
    {
        if (!CanvCE.activeSelf)
        {
            // CanvLE.SetActive(true);
            // CanvRE.SetActive(true);
            CanvCE.SetActive(true);
            var texture = remoteCameraWindow.Texture;
            if (texture != null)
            {
                matCE.SetTexture("_mainRT", texture);
                matCE.SetFloat("_visibleRatio", visibleRatio);
                matCE.SetFloat("_contentRatio", contentRatio);
                matCE.SetFloat("_heightCompressionFactor", heightCompressionFactor);
            }
        }
    }
}
