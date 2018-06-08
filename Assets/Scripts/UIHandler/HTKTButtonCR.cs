using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;

public class HTKTButtonCR : MonoBehaviour, IInputClickHandler
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
          foreach (var go in ls.objects)
          {
               Destroy(go.gameObject);
          }
          ls.objects.Clear();
          ls.hasRecommendation = false;
          r_index.text = "Recommendation: N/A";
     }
}