using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpsController : MonoBehaviour {
    private float speed = 0.1f;
    private Vector2 rotation = Vector2.zero;
    private float cameraSensitivity = 30;
    private bool firstTime = true;
    public Camera topCam;
    public float personHeight = 0.8f;

    // Use this for initialization
    void Start () {
        transform.SetPositionAndRotation(new Vector3(.0f, personHeight, .0f), Quaternion.identity);
	}

    void OnEnable()
    {
        if (firstTime)
        {
            firstTime = false;
            return;
        }
        Transform person = topCam.GetComponent<topCamView>().getPersonTransform();
        transform.SetPositionAndRotation(person.position, person.rotation);
    }
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            transform.Translate(Camera.main.transform.forward * speed);            

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            transform.Translate(-Camera.main.transform.forward * speed);

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            transform.Translate(-Camera.main.transform.right * speed);

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            transform.Translate(Camera.main.transform.right * speed);
        transform.position = new Vector3(transform.position.x, personHeight, transform.position.z);
    }
    private void LateUpdate()
    {
        rotation.x += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotation.y += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotation.y = Mathf.Clamp(rotation.y, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotation.y, Vector3.left);

        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;
    }
}
