using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchCam : MonoBehaviour {
    public Camera topCam;
    public Camera fpsCam;
    // Use this for initialization
    void Start () {
        topCam.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.V))
        {
            topCam.enabled = !topCam.enabled;
            fpsCam.enabled = !fpsCam.enabled;
        }
    }

}
