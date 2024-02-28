using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using dm;
using System;

public class HandScript : MonoBehaviour
{
    // Start is called before the first frame update
    XRHandSubsystem m_Subsystem = null;

    public GameObject SetHandPose = null;
    void Start()
    {
        m_Subsystem =
            XRGeneralSettings.Instance?
                .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();
        if (m_Subsystem != null)
            m_Subsystem.updatedHands += OnHandUpdate;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnHandUpdate(XRHandSubsystem subsystem,
               XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
                   XRHandSubsystem.UpdateType updateType)
    {
        switch (updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
                // Update game logic that uses hand data
                break;
            case XRHandSubsystem.UpdateType.BeforeRender:
                // Update visual objects that use hand data
                XRHand left_hand = subsystem.leftHand;
                Action<XRHand> left_update = hand => UpdateLocation(0, hand);
                Action<XRHand> right_update = hand => UpdateLocation(1, hand);

                left_update(subsystem.leftHand);
                right_update(subsystem.rightHand);
                break;
        }
    }

    void UpdateLocation(int hand_type, XRHand hands)
    {
        //var wristIndex = XRHandJointID.MiddleProximal;

        // TODO(junlinp): it is better?
        var wristIndex = XRHandJointID.MiddleMetacarpal;

        if (hands.GetJoint(wristIndex).TryGetPose(out var wristJointPose))
        {
            if (SetHandPose != null) {
                Transform transform = SetHandPose.GetComponent<Transform>();
                transform.rotation = wristJointPose.rotation;
                transform.position = wristJointPose.position;
                Debug.LogFormat("SetHandPose {0}, {1}", transform.position, transform.rotation);
            }

            Vector3 position = wristJointPose.position;
            Quaternion rotation = wristJointPose.rotation;

            long current_timestamp = 0;
            double[] pose = new double[7];
            float[] device_pose = new float[7];

            DMInputApi.DMControllerPose(hand_type, ref current_timestamp, device_pose);
            if (current_timestamp > 0) {
                string DevicePoseString = string.Format("DevicePose xyz:{0},{1},{2}, wxyz:{3},{4},{5},{6}",
                    device_pose[0],
                    device_pose[1],
                    device_pose[2],
		            device_pose[3],
		            device_pose[4],
		            device_pose[5],
		            device_pose[6]
		        );

                string HandPoseString = string.Format("HandPose xyz:{0},{1},{2}, wxyz:{3},{4},{5},{6}",
                    position.x,
                    position.y,
                    position.z,
                    rotation.w,
                    rotation.x,
                    rotation.y,
                    rotation.z
		        );


                DMInputApi.FusionGetFusionResult(hand_type, ref current_timestamp, pose);

                Debug.LogFormat("{0},{1}", DevicePoseString, HandPoseString);

                //Debug.LogFormat("FusionGetFusionResult for HandType[{7}]: {0}, {1}, {2}, {3}, {4}, {5}, {6}", pose[0], pose[1], pose[2], pose[3], pose[4], pose[5], pose[6], hand_type);
                pose = new double[] {
                    position.x,
                    position.y,
                    position.z,
                    rotation.w,
                    rotation.x,
                    rotation.y,
                    rotation.z,
                };

                // TODO(junlinp):Test for no GestureTrakcing
                DMInputApi.FusionUpdateGestureTracking(hand_type,current_timestamp - 1, pose);
            }
        }
    }

    
}
