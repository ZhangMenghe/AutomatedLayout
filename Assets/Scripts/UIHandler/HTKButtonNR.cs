using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class HTKButtonNR : MonoBehaviour, IInputClickHandler
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
          if (ls.hasRecommendation)
          {
               ls.ChangeRecommendation();
               r_index.text = "Recommendation: " + ls.currentRecId;
          }
     }
}