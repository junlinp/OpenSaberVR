/*
 * The speed calculation was taken from the project:
 * BeatSaver Viewer (https://github.com/supermedium/beatsaver-viewer) and ported to C#.
 * 
 * To be more precisly most of the code in the LateUpdate() method was ported to C# by me 
 * from their project.
 * 
 * Without that project this project won't exist, so thank you very much for releasing 
 * the source code under MIT license!
 */
using UnityEngine;
using System;
public class CubeHandling : MonoBehaviour
{
    public float AnticipationPosition;
    public float Speed;
    public double WarmUpPosition;
    public float rotation;
    public float target_x;
    public float target_y;
    
    void LateUpdate()
    {
        //Debug.Log("Z : " + transform.position.z + " anticipationPosition : " + AnticipationPosition);

        if (transform.position.z > AnticipationPosition)
        {
            Vector3 direction = (new Vector3(target_x, target_y, AnticipationPosition) - transform.position).normalized;
            
            //var newPositionZ = transform.position.z - BeatsConstants.WARM_UP_SPEED_METER_PER_SECONDS * Time.deltaTime;
            var new_position = transform.position + direction * BeatsConstants.WARM_UP_SPEED_METER_PER_SECONDS * Time.deltaTime / -direction.z;

            //Debug.LogFormat("Z: {0}, NewPositionZ : {1}",transform.position.z, newPositionZ);
            // Warm up / warp in.
            if (new_position.z > AnticipationPosition)
            {

                //transform.position = new Vector3(transform.position.x,transform.position.y, newPositionZ);
                transform.position = new_position;
            }
            else
            {
                //transform.position = new Vector3(transform.position.x, transform.position.y, AnticipationPosition);
                transform.position = new Vector3(target_x, target_y, AnticipationPosition);
                transform.Rotate(transform.forward, rotation);
            }
            //Debug.LogFormat("AfterSet Z: {0} to {1}", transform.position.z, newPositionZ);
        }
        else
        {
            
            // Standard moving.
            transform.position -= transform.forward * BeatsConstants.ANTICIPATION_SPEED_METER_PER_SECONDS * Time.deltaTime;           
        }

        if (transform.position.z < -10)
        {
            Destroy(gameObject);
        }
        
            
        
    }
}
