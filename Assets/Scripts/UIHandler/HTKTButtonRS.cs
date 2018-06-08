using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class HTKTButtonRS : MonoBehaviour, IInputClickHandler
{
     public layoutScene ls;
     public List<SliderGestureControl> sliders;

     //public Text r_index;

     // Use this for initialization
     void Start()
     {

     }

     // Update is called once per frame
     void Update()
     {

     }

     public void OnInputClicked(InputClickedEventData eventData)
     {
          string sendOutMsg = "";
          for (int i = 0; i < sliders.Count; i++)
          {
               int val = (int) Math.Round(sliders[i].SliderValue, MidpointRounding.AwayFromZero)  ;
                    if (val != 0)
                    {
                         sendOutMsg += i.ToString() + " " + val.ToString() + ",";
                         //cateObjRecords[i] = 0;
                    }

               ls.encode_and_send_input(sendOutMsg);
          }

          if (sliders[0].SliderValue > 1.0)
          {
               ls.index = 1;
          }
          else
          {
               ls.index = 0;
          }

          ls.hasRecommendation = false;
     }
}
