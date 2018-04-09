using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class topCamView : MonoBehaviour {
    // Use this for initialization
    public Transform perPrefab;
    public Transform fpsCam;
    public Transform sceneLayouter;

    private Transform person;
    private float speed = 0.1f;
    private Vector2 rotation = Vector2.zero;
    private float cameraSensitivity = 30;
    private float personHeight;
    void Start () {
        person = Instantiate(perPrefab);
        personHeight = fpsCam.GetComponent<fpsController>().personHeight;
        Vector2 roomSize = sceneLayouter.GetComponent<layoutScene>().get_roomSize();
        transform.localPosition = new Vector3(.0f, Mathf.Max(roomSize.x,roomSize.y)/2+100, .0f);
    }
	public void onSwitchCam()
    {
        person.SetPositionAndRotation(fpsCam.position, fpsCam.rotation);
    }
    
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            person.Translate(person.forward * speed);

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            person.Translate(-person.forward * speed);

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            person.Translate(-person.right * speed);

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            person.Translate(person.right * speed);
        person.position = new Vector3(person.position.x, personHeight, person.position.z);
    }
    private void LateUpdate()
    {
        rotation.x += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotation.y += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotation.y = Mathf.Clamp(rotation.y, -90, 90);

        person.localRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
        person.localRotation *= Quaternion.AngleAxis(rotation.y, Vector3.left);

        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;
    }
    public Transform getPersonTransform()
    {
        return person;
    }
}
