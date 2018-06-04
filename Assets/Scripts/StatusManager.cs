using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{

    public string lastError = null;
    public bool hasError = false;
    public bool hasWarning = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetError(string status)
    {
        hasWarning = false;
        hasError = true;
        lastError = status;
    }

    public void SetWarning(string status)
    {
        hasError = false;
        hasWarning = true;
        lastError = status;
    }

    public void SetSuccess(string status)
    {
        hasWarning = false;
        hasError = false;
        lastError = null;
    }

    public void SetUnknown(string status)
    {
        hasWarning = false;
        hasError = false;
        lastError = status;
    }

    public string GetLastError()
    {
        string ret = lastError;
        lastError = null;
        if(!hasError && !hasWarning)
        {
            ret = "";
        }
        hasError = false;
        hasWarning = false;
        return ret;
    }
}
