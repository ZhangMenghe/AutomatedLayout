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
            if (topCam.enabled)
            {
                topCam.enabled = false;
                fpsCam.enabled = true;
                fpsCam.GetComponent<fpsController>().onSwitchCam();
            }
            else
            {
                topCam.enabled = true;
                fpsCam.enabled = false;
                topCam.GetComponent<topCamView>().onSwitchCam();
            }
        }
    }

}
