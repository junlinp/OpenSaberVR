using Unity.Mathematics;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Saber : MonoBehaviour
{
    public LayerMask layer;

    private Quaternion previous_quaternion;

    private Vector3 previousPos;
    private Vector3 posDelta;
    private Slice slicer;

    private float impactMagnifier = 120f;
    private float collisionForce = 0f;
    private float maxCollisionForce = 4000f;
    //private VRTK_ControllerReference controllerReference;
    public GameObject[] SaberMeshes;

    public Transform TipPoint;
    public Vector3 TipPrevPos;
    public Vector3 TipDelta;
    public quaternion targetRotation;

    public float ray_length = 5.0f;

    public UnityEvent slice_callback;

    public void SetSaberVisibility(bool x)
    {
        for (int i = 0; i < SaberMeshes.Length; i++)
        {
            //SaberMeshes[i].SetActive(x);
        }
    }
    private void Start()
    {
        slicer = GetComponentInChildren<Slice>(true);
        //var controllerEvent = GetComponentInChildren<VRTK_ControllerEvents>(true);
        /*
        if (controllerEvent != null && controllerEvent.gameObject != null)
        {
            controllerReference = VRTK_ControllerReference.GetControllerReference(controllerEvent.gameObject);
        }
        */
    }

    private void Pulse()
    {
        /*
        if (VRTK_ControllerReference.IsValid(controllerReference))
        {
            collisionForce = VRTK_DeviceFinder.GetControllerVelocity(controllerReference).magnitude * impactMagnifier;
            var hapticStrength = collisionForce / maxCollisionForce;
            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, hapticStrength, 0.5f, 0.01f);
        }
        else
        {
            var controllerEvent = GetComponentInChildren<VRTK_ControllerEvents>();
            if (controllerEvent != null && controllerEvent.gameObject != null)
            {
                controllerReference = VRTK_ControllerReference.GetControllerReference(controllerEvent.gameObject);
            }
        }
        */
    }

    void Update()
    {
        // prue rotation can't SliceObject....
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, ray_length, layer))
        {
            Debug.LogFormat("{0} Hit", layer.ToString());
            Quaternion delta_rotation = Quaternion.Inverse(previous_quaternion) * transform.rotation;
            float rad = 0.0f;
            Vector3 delta_angle_axis = Vector3.zero;
	        delta_rotation.ToAngleAxis(out rad, out delta_angle_axis);

            double rad_threshold = 30.0 / 180.0 * Math.PI;
            if (!string.IsNullOrWhiteSpace(hit.transform.tag) && hit.transform.CompareTag("CubeNonDirection"))
            {

                //if (Vector3.Angle(transform.position - previousPos, hit.transform.up) > 130 ||
                //    Vector3.Angle(transform.position - previousPos, hit.transform.right) > 130 ||
                //    Vector3.Angle(transform.position - previousPos, -hit.transform.up) > 130 ||
                //    Vector3.Angle(transform.position - previousPos, -hit.transform.right) > 130)
                //{
                //    SliceObject(hit.transform);
                //}

                Vector3 z_axis_forward = hit.transform.forward;
                double dot_value = Vector3.Dot(z_axis_forward, delta_angle_axis);
                Debug.LogFormat("Delta AngleAxis : {0}, Angle : {1}, Dot:{2}, z_axis_forward:{3}", delta_angle_axis, rad, dot_value, z_axis_forward);
                if ( Math.Abs(dot_value) < 0.2 && rad > rad_threshold) {
                    SliceObject(hit.transform);
		        }
            }
            else
            {
                // y-axis up
                Vector3 negetive_x_axis = hit.transform.right;
                double dot_value = Vector3.Dot(negetive_x_axis, delta_angle_axis); 
                //Debug.LogFormat("Delta AngleAxis : {0}, Angle : {1}, Dot:{2}, y_axis_up:{3}", delta_angle_axis, rad, dot_value, negetive_x_axis);
                if (dot_value > 1.0 / Math.Sqrt(2) && rad > rad_threshold) { 
                    SliceObject(hit.transform);
                }
                //if (Vector3.Angle(transform.position - previousPos, hit.transform.up) > 130)
                //{
                //    SliceObject(hit.transform);
                //}
            }
        }
        previousPos = transform.position;
        previous_quaternion = transform.rotation;

    }

    public void LateUpdate(){
        if(TipPrevPos!=TipPoint.transform.position){
            TipDelta = TipPrevPos-TipPoint.transform.position;
            targetRotation = Quaternion.LookRotation(slicer.transform.forward, TipDelta)*Quaternion.Euler(0, 0, 90);
            TipPrevPos = TipPoint.transform.position;
        }

        slicer.transform.rotation = Quaternion.Slerp(slicer.transform.rotation, targetRotation, Time.deltaTime*50);
    }

    private void SliceObject(Transform hittedObject)
    {
        var cutted = slicer.SliceObject(hittedObject.gameObject);
        var go = Instantiate(hittedObject.gameObject);

        go.GetComponent<CubeHandling>().enabled = false;
        go.GetComponentInChildren<BoxCollider>().enabled = false;
        go.layer = 0;

        foreach (var renderer in go.transform.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }

        foreach (var cut in cutted)
        {
            cut.transform.SetParent(go.transform);
            cut.AddComponent<BoxCollider>();
            var rigid = cut.AddComponent<Rigidbody>();
            rigid.useGravity = true;
        }

        go.transform.SetPositionAndRotation(hittedObject.position, hittedObject.rotation);

        Pulse();

        Destroy(hittedObject.gameObject);
        Destroy(go, 2f);

        slice_callback?.Invoke();
    }
}
