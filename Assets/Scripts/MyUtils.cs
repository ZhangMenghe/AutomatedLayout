using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
public class MyUtils : Singleton<MyUtils>
{

     public static void RotateAround(Transform t, Vector3 point, Vector3 axis, float angle)
     {
          Vector3 vector = t.position;
          Quaternion rotation = Quaternion.AngleAxis(angle, axis);
          Vector3 vector2 = vector - point;
          vector2 = rotation * vector2;
          vector = point + vector2;
          t.position = vector;
          t.Rotate(axis, angle);
     }
}
