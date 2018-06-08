using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowUI : MonoBehaviour
{
     public GameObject cursor;
     public GameObject camera;

     public float followThreshold = 0.99f;
     public float smooth = 0.01f;
     public float angleMax = 0.8f;
     public float delta;
     public float angle;
     public bool follow = true;

     private Vector3 deltaLast;

     // Use this for initialization
     void Start()
     {
          delta = (this.transform.position - camera.transform.position).magnitude;
     }

     // Update is called once per frame
     void Update()
     {
          if (!follow)
          {
               delta = (this.transform.position - camera.transform.position).magnitude;
               return;
          }

          Vector3 p_camera = camera.transform.forward.normalized;
          Vector3 y_camera = new Vector3(p_camera.x, 0, p_camera.z);
          var delta_local = (this.transform.position - camera.transform.position).normalized;
          Vector3 y_delta = new Vector3(delta_local.x, 0, delta_local.z);
          float dp = Vector3.Dot(y_camera, y_delta);
          angle = Vector3.SignedAngle(y_delta, y_camera, Vector3.up);

          //Debug.Log(angle);
          //if (angle > 180)
          //{
          //     angle = angle - 360;
          //}
          if (dp < followThreshold)
          {
               angle *= smooth;
               if (angle > angleMax)
               {
                    angle = angleMax;
               }

               MyUtils.RotateAround(this.transform, camera.transform.position, Vector3.up, angle);
          }
          else
          {

               var pos = camera.transform.position;
               pos += delta * delta_local;
               pos.y = this.transform.position.y;
               this.transform.position = pos;
          }
     }
}