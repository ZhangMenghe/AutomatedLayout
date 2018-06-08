using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class HTKTButtonRR : MonoBehaviour, IInputClickHandler
{

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
          foreach (var slider in sliders)
          {
               slider.SetSliderValue(0.0f);
          }
     }
}