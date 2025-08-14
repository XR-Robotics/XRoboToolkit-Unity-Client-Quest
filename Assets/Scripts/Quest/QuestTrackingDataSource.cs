using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.Input.Visuals;
using UnityEngine;
using UnityEngine.Assertions;

public class QuestTrackingDataSource : MonoBehaviour
{
    public Hmd headset;
    public HandVisual leftHandVisual;
    public HandVisual rightHandVisual;
    
    public List<Transform> leftHandJoints;
    public List<Transform> rightHandJoints;

    public Controller leftController;
    public Controller rightController;
    public OVRControllerVisual leftControllerVisual;
    public OVRControllerVisual rightControllerVisual;

    public int GetActiveInputDevice()
    {
        // handtracking 2
        // controller 1
        // headset 0
        if (leftHandVisual.IsVisible || rightHandVisual.IsVisible)
        {
            return 2; // handtracking
        }

        if (leftController.IsConnected || rightController.IsConnected)
        {
            return 1; // controller
        }

        return 0; // by default return headset
    }

    public (bool, Pose) GetHeadsetPose()
    {
        if (headset.TryGetRootPose(out Pose pose))
        {
            return (true, pose);
        }

        return (false, Pose.identity);
    }

    public Pose GetControllerPose(Handedness handedness)
    {
        if (handedness == Handedness.Left)
        {
            var pos = leftControllerVisual.transform.position;
            var rot = leftControllerVisual.transform.rotation;
            return new Pose(pos, rot);
        }
        else
        {
            var pos = rightControllerVisual.transform.position;
            var rot = rightControllerVisual.transform.rotation;
            return new Pose(pos, rot);
        }
    }

    public bool IsControllerActive(Handedness handedness)
    {
        if (handedness == Handedness.Left)
        {
            return leftController.IsPoseValid;
        }

        return rightController.IsPoseValid;
    }

    public bool IsHandTrackingActive(Handedness handedness)
    {
        if (handedness == Handedness.Left)
        {
            return leftHandVisual.IsVisible;
        }

        return rightHandVisual.IsVisible;
    }

    public void GetJoints(Handedness handedness, ref Pose[] poses)
    {
        var joints = handedness == Handedness.Left ? leftHandJoints : rightHandJoints;

        for (var i = 0; i < joints.Count; i++)
        {
            poses[i] = joints[i].GetPose();
        }
    }
}