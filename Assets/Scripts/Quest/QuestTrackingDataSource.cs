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

    public SkinnedMeshRenderer leftHandSkinMesh;
    public SkinnedMeshRenderer rightHandSkinMesh;

    public Controller leftController;
    public Controller rightController;
    public OVRControllerVisual leftControllerVisual;
    public OVRControllerVisual rightControllerVisual;

    public int GetActiveInputDevice()
    {
        // handtracking 2
        // controller 1
        // headset 0
        if (leftHandSkinMesh.isVisible || rightHandSkinMesh.isVisible)
        {
            return 2; // handtracking
        }
        else if (leftController.IsConnected || rightController.IsConnected)
        {
            return 1; // controller
        }
        else
        {
            return 0; // by default return headset
        }
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

        return Pose.identity;
    }

    public bool IsControllerActive(Handedness handedness)
    {
        if (handedness == Handedness.Left)
        {
            return leftController.IsPoseValid;
        }
        else
        {
            return rightController.IsPoseValid;
        }
    }

    public bool IsHandTrackingActive(Handedness handedness)
    {
        if (handedness == Handedness.Left)
        {
            return leftHandSkinMesh.isVisible;
        }
        else
        {
            return rightHandSkinMesh.isVisible;
        }
    }

    // public void GetJoints(Handedness handedness, ref Pose[] joints)
    // {
    //     // first is palm, second is wrist in visual
    //
    //     Hand hand = handedness == Handedness.Left ? _leftHand : _rightHand;
    //
    //     // wrist is the first joint in OpenXR and then Palm
    //     HandJointId wristJointId = HandJointId.HandWristRoot;
    //     HandJointId palmJointId = HandJointId.HandPalm;
    //
    //     Pose pose = Pose.identity;
    //
    //     // add wrist
    //     if (hand.GetJointPose(wristJointId, out pose))
    //     {
    //         joints[0] = pose;
    //     }
    //
    //     pose = Pose.identity;
    //
    //     // set palm
    //     if (hand.GetJointPose(palmJointId, out pose))
    //     {
    //         joints[1] = pose;
    //     }
    //
    //     pose = Pose.identity;
    //
    //     // add other joints 
    //     for (HandJointId i = wristJointId + 1; i < HandJointId.HandEnd; i++)
    //     {
    //         pose = Pose.identity;
    //         // set palm
    //         if (hand.GetJointPose(palmJointId, out pose))
    //         {
    //             joints[(int)i] = pose;
    //         }
    //     }
    // }
    public void GetJoints(Handedness handedness, ref Pose[] poses)
    {
        HandVisual handVisual = handedness == Handedness.Left ? leftHandVisual : rightHandVisual;
        var joints = handVisual.Joints;
        Assert.IsTrue(joints.Count == 26, "Expected 26 joints in the hand visual.");
        // first is palm, second is wrist in visual
        poses[0] = joints[1].GetPose(); // wrist
        poses[1] = joints[0].GetPose(); // palm

        for (var i = 2; i < joints.Count; i++)
        {
            poses[i] = joints[i].GetPose(); // other joints
        }
    }

}