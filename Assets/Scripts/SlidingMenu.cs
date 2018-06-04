using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidingMenu : MonoBehaviour {
    //canvas
    public Canvas menuCanvas;

    //Slides Parent
    public Transform SlidesContent;

    //AutoLayouter
    public Transform layouterOjbect;
    //----------------private--------------------//

    private List<RectTransform> Slides= new List<RectTransform>();

    private float scrollStep;
    private float ButtonTransition = 0.1f;
    private int SlidesInView = 5;
    private Vector2 SlideSize = new Vector2(100,100);
    private Vector4 SlideMargin = new Vector4(10, 10, 10, 10);
    private float default_height = 300;
    private int objctNum;
    private int activeObjId = 0;
    private float layoutWidth;
    private float singleSlideWidth;
    private int lastActiveObjid = -1;
    private float ActiveOffsetTransition = 0.1f;

    private layoutScene layoutScript;
    // Use this for initialization
    void Start () {
        //setup layouter
        layoutScript = layouterOjbect.GetComponent<layoutScene>();

        //Initialize Slides List
        foreach (RectTransform child in SlidesContent)
            Slides.Add(child);
        objctNum = Slides.Count;
        layoutScript.initCateRecords(objctNum);
        scrollStep = 1 / (objctNum - 1);
        singleSlideWidth = SlideSize.x + SlideMargin.y + SlideMargin.w;
        SlidesContent.GetComponent<RectTransform>().sizeDelta = new Vector2(
            (objctNum + SlidesInView - 1) * singleSlideWidth,
            (SlideSize.y + SlideMargin.x + SlideMargin.z)
            );

        layoutWidth = SlidesContent.GetComponent<RectTransform>().rect.width;
        SlidesContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(layoutWidth/2, default_height);
        //Initilized each content
        for (int i=0; i<objctNum; i++)
            Slides[i].anchoredPosition = new Vector2(i* singleSlideWidth, .0f);
        updateCurrentFrame();
    }

    // Update is called once per frame
    void Update () {
        SlidesContent.GetComponent<RectTransform>().anchoredPosition
            = new Vector2(Mathf.Lerp(SlidesContent.localPosition.x, 
                                     layoutWidth / 2 - singleSlideWidth * activeObjId,
                                     ActiveOffsetTransition) ,default_height);
            //= new Vector2(layoutWidth / 2 - singleSlideWidth * activeObjId, default_height);
    }

    private void updateCurrentFrame()
    {      
        Slides[activeObjId].sizeDelta = SlideSize * 2;
        Slides[activeObjId].localPosition -= new Vector3(0, 0, 100);
        if (lastActiveObjid != -1){
            Slides[lastActiveObjid].sizeDelta = SlideSize;
            Slides[lastActiveObjid].localPosition += new Vector3(0, 0, 100);
        }   
    }
    public void nextButtonClicked()
    {
        lastActiveObjid = activeObjId;
        activeObjId = (activeObjId + 1) % objctNum;
        updateCurrentFrame();
    }
    public void preButtonClicked()
    {
        lastActiveObjid = activeObjId;
        activeObjId = (activeObjId - 1 + objctNum) % objctNum;
        updateCurrentFrame();
    }
    public void addupButtonClicked(){
        layoutScript.addCustomObj(activeObjId);
    }

}
