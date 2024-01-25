﻿using Unity.Mathematics;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Saber : MonoBehaviour
{
    public LayerMask layer;
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


    public UnityEvent slice_callback;
    public float length = 1.0f;
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, length, layer))
        {
            if (!string.IsNullOrWhiteSpace(hit.transform.tag) && hit.transform.CompareTag("CubeNonDirection"))
            {
                if (Vector3.Angle(transform.position - previousPos, hit.transform.up) > 130 ||
                    Vector3.Angle(transform.position - previousPos, hit.transform.right) > 130 ||
                    Vector3.Angle(transform.position - previousPos, -hit.transform.up) > 130 ||
                    Vector3.Angle(transform.position - previousPos, -hit.transform.right) > 130)
                {
                    SliceObject(hit.transform);
                }
            }
            else
            {
                if (Vector3.Angle(transform.position - previousPos, hit.transform.up) > 130)
                {
                    SliceObject(hit.transform);
                }
            }
        }
        previousPos = transform.position;
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
