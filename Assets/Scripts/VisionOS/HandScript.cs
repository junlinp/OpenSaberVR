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

    private double accumulated_left_time_ms = 0.0;
    private double accumulated_right_time_ms = 0.0;

    private double update_gesture_per_ms = 10000;

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };
    
    Queue queue = new Queue();

    public GameObject SetHandPose = null;
    public GameObject SetConvertHandPose = null;

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

        if (hand_type == 0) {
            accumulated_left_time_ms += Time.deltaTime * 1000;
	    }

        if (hand_type == 1) {
            accumulated_right_time_ms += Time.deltaTime * 1000;
	    }

        //var wristIndex = XRHandJointID.MiddleProximal;

        // TODO(junlinp): it is better?
        var wristIndex = XRHandJointID.MiddleMetacarpal;

        if (hands.GetJoint(wristIndex).TryGetPose(out var wristJointPose))
        {
            
            Vector3 position = wristJointPose.position;
            Quaternion rotation = wristJointPose.rotation;
            if (SetHandPose != null && hand_type == 0) {
                Transform transform = SetHandPose.GetComponent<Transform>();
                transform.rotation = wristJointPose.rotation;
                transform.position = wristJointPose.position;
                Debug.LogFormat("SetHandPose {0}, {1}", transform.position, transform.rotation);
            }

            //
            // Quaternion T_hand_imu_rotation = new Quaternion(-0.207398f, 0.119288f, -0.947998f, -0.628531f);
            // Vector3 T_hand_imu_position = new Vector3(-0.0557426f, -0.0100484f, 0.138848f);
            //
            // Quaternion T_hand_imu_rotation = new Quaternion(-0.132137f, 0.0938899f, -0.79426f, -0.455216f);
            // Vector3 T_hand_imu_position = new Vector3(0.0560049f, -0.0217981f, 0.0409697f);

            // Quaternion T_hand_imu_rotation = new Quaternion(-0.0651637f, 0.271091f, -0.943049f, -0.639748f);
            // Vector3 T_hand_imu_position = new Vector3(0.0100402f, -0.0512941f, 0.074441f);
            //
            // { t:[   0.01005 - 0.0513511  0.0743921], q:[-0.065242  0.271152 - 0.943005 - 0.639705] }
            //
            double theta = -110.0 / 180.0 * 3.1415926;
            Quaternion T_hand_imu_rotation = new Quaternion(0.0f, 0.0f, (float)Math.Sin(theta / 2.0), (float)Math.Cos(theta / 2.0));
            Vector3 T_hand_imu_position = new Vector3(0.015f, -0.03f, 0.11f);

            Quaternion T_w_i_rotation = rotation * T_hand_imu_rotation;
            Vector3 T_w_i_position = rotation * T_hand_imu_position + position;


            if (SetConvertHandPose != null && hand_type == 0)
            {
                Transform transform = SetConvertHandPose.GetComponent<Transform>();
                transform.rotation = T_w_i_rotation;
                transform.position = T_w_i_position;
            }
            if (hand_type == 0 && accumulated_left_time_ms <= update_gesture_per_ms)
            {
                return;
            }

            if (hand_type == 1 && accumulated_right_time_ms <= update_gesture_per_ms)
            {
                return;
            }


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

                /*
                DMInputApi.CPose pose1 = new DMInputApi.CPose();
                Pair<DMInputApi.CPose, DMInputApi.CPose> pair;
                queue.Enqueue(pair);
                
                while(queue.Count > 3) {
                    queue.Dequeue();
		        }

                if (queue.Count == 3) { 

		        }
                */
                /*
                /* 
                 * extrinsic parameters
		         * { t: [-0.0557426 -0.0100484   0.138848], q: [-0.207398  0.119288 -0.947998 -0.628531] }
                 * { t: [ 0.0560049 -0.0217981  0.0409697], q: [-0.132137 0.0938899  -0.79426 -0.455216] }
                 * { t: [ 0.0148509 -0.0391234  0.0645446], q: [-0.0723103    0.25409  -0.947413  -0.640077] }
                 * { t: [ 0.0100402 -0.0512941   0.074441], q: [-0.0651637   0.271091  -0.943049  -0.639748] }
                 * { t: [ 0.0100543 -0.0513216  0.0744136], q: [0.0510247 -0.211947  0.737016 -0.639752] }
                 *
		        */


                pose = new double[] {
                    T_w_i_position.x,
                    T_w_i_position.y,
                    T_w_i_position.z,
                    T_w_i_rotation.w,
                    T_w_i_rotation.x,
                    T_w_i_rotation.y,
                    T_w_i_rotation.z,
                };
                // TODO(junlinp):Test for no GestureTrakcing
                DMInputApi.FusionUpdateGestureTracking(hand_type,current_timestamp - 1, pose);

                if (hand_type == 0)
                {
                    accumulated_left_time_ms = 0.0;
                }

                if (hand_type == 1) {
                    accumulated_right_time_ms = 0.0f;
                }

            }
        }
}


}
