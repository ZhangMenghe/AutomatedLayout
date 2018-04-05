using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class layoutController : MonoBehaviour {
    public Transform sceneLayouter;
	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {           
        if (Input.GetKeyDown(KeyCode.C))
            sceneLayouter.GetComponent<layoutScene>().ChangeRecommendation();
    }
}
