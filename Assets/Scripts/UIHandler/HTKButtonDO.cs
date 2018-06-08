using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class HTKButtonDO : MonoBehaviour, IInputClickHandler
{

     public layoutScene ls;
     public Text r_index;

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
          ls.debug_obs = !ls.debug_obs;
          if (ls.debug_obs)
          {
               r_index.text = "Display Obstacle: On";
          }
          else
          {
               r_index.text = "Display Obstacle: Off";
          }
     }
}
