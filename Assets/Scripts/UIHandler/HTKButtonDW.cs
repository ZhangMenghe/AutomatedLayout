using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class HTKButtonDW : MonoBehaviour, IInputClickHandler
{

     public layoutScene ls;
     public Text r_index;

     // Use this for initialization
     void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

     public void OnInputClicked(InputClickedEventData eventData)
     {
          ls.debug_wall = !ls.debug_wall;
          if (ls.debug_wall)
          {
               r_index.text = "Display Wall: On";
          }
          else
          {
               r_index.text = "Display Wall: Off";
          }
     }
}
