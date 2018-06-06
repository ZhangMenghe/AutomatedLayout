using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
public class mainMenuController : MonoBehaviour {
    public Canvas addonMenu;
    public Transform fpsCam;
    public Transform buttonParent;

    public bool onGenerating = false;
    public Transform loadingCubePrefab;
    public Transform layouterOjbect;

    private Transform loadingCubeIns;
    private List<float> nextButtonPos = new List<float>();
    private List<RectTransform> buttonList = new List<RectTransform>();
    private List<bool> isButtonMenuOn = new List<bool>();
    private List<Transform> MsgList = new List<Transform>();
    // Use this for initialization
    private layoutScene layoutScript;

    private GestureRecognizer recognizer;

    void Start () {
        recognizer = new GestureRecognizer();
        if (addonMenu.renderMode == RenderMode.WorldSpace)
        {
            addonMenu.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(.0f, 1.47f, 0.8f);
            addonMenu.GetComponent<RectTransform>().localScale = Vector3.one * 0.0005f;

            GetComponent<RectTransform>().anchoredPosition3D = new Vector3(.0f, 1.5f, 0.65f);
            GetComponent<RectTransform>().localScale = Vector3.one * 0.0005f;
        }
       
        addonMenu.enabled = false;
        layoutScript = layouterOjbect.GetComponent<layoutScene>();
        foreach (RectTransform child in buttonParent)
        {
            if (child.tag == "button")
            {
                buttonList.Add(child);
                nextButtonPos.Add(-1);
                isButtonMenuOn.Add(false);
            }
            else if (child.tag == "Msg")
            {
                child.GetComponent<Text>().enabled = false;
                MsgList.Add(child);
            }
               
        }
        recognizer.Tapped += (args) =>
        {
            gameObject.SendMessageUpwards("changeAlphaCanvas", SendMessageOptions.DontRequireReceiver);
        };
        
    }
	void changeAlphaCanvas()
    {
        GetComponent<CanvasGroup>().alpha = (GetComponent<CanvasGroup>().alpha == 1) ?0.1f:1.0f;
    }
    // Update is called once per frame
    void Update () {
        recognizer.StartCapturingGestures();
        for (int i=0; i< buttonList.Count; i++)
        {
            if (nextButtonPos[i] != -1)
            {
                if (buttonList[i].anchoredPosition.x != nextButtonPos[i])
                    buttonList[i].anchoredPosition = new Vector2(Mathf.Lerp(buttonList[i].anchoredPosition.x,
                                         nextButtonPos[i],
                                         0.05f), buttonList[i].anchoredPosition.y);
            }
        }
        if (onGenerating){
            loadingCubeIns.localPosition = fpsCam.localPosition +  fpsCam.forward * 10;
            loadingCubeIns.Rotate(Vector3.up , 100 * Time.deltaTime);
        }
	}
    public void addonButtonClicked()
    {
        nextButtonPos[0] = (nextButtonPos[0] == 120) ? 0 : 120;
        isButtonMenuOn[0] = !isButtonMenuOn[0];
        addonMenu.enabled = isButtonMenuOn[0];
    }
    public void processButtonClicked()
    {
        if (onGenerating)
            return;
        nextButtonPos[1] = 120;
        isButtonMenuOn[1] = !isButtonMenuOn[1];
        onGenerating = true;
        loadingCubeIns = Instantiate(loadingCubePrefab,
                                     fpsCam.localPosition+ new Vector3(.0f, .0f, 10.0f), 
                                     Quaternion.Euler(new Vector3(.0f,.0f,45)));
        MsgList[0].GetComponent<Text>().enabled = true;
        layoutScript.startToGenerate();
       
    }
}
