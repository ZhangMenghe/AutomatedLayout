using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
public class layoutScene : MonoBehaviour {
    private string filename;
    private int currentRecId = -1;
    private Transform floor;
    private List<float[]> recomParameters;
    private List<float[]> objectParams;
    private List<Transform> objects;
    private int objCount;
    private int schemeCount;
    
    public Transform wallPrefab;
    public Transform focalPrefab;
    public Transform obsPrefab;
    public Transform objPrefab;
    // Use this for initialization
    void Start () {
        filename = Application.dataPath + "/InputData/intermediate/recommendation.txt";
        recomParameters = new List<float[]>();
        objectParams = new List<float[]>();
        objects = new List<Transform>();
        InitiallayoutOnScreen();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void InitiallayoutOnScreen()
    {
        floor = GameObject.Find("Floor").transform;
        parser_resfile();
        //resize_room("RoomSize: 400 300");
    }
    public void ChangeRecommendation()
    {
        currentRecId = (currentRecId + 1) % schemeCount;
        for(int i=0;i<objCount;i++)
        {
            float[] param = recomParameters[schemeCount * i + currentRecId];
            float sx = objectParams[i][3], sy = objectParams[i][2], sz = objectParams[i][4];
            float cx = param[0], cy = sy, cz = param[1];
            float rot = param[2] / Mathf.PI * 180;
            if (objects.Count > i)
            {
                objects[i].SetPositionAndRotation(new Vector3(cx, cy, cz), Quaternion.Euler(new Vector3(.0f, rot, .0f)));
                return;
            }
            Transform obj = Instantiate(objPrefab, new Vector3(cx, cy, cz), Quaternion.Euler(new Vector3(.0f, rot, .0f)));
            obj.localScale = new Vector3(sx, sy, sz);
            objects.Add(obj);
        }
    }
    private float dist_of_points(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }
    private void draw_a_wall(float[] param)
    {
        float cx = (param[2] + param[4]) / 2;
        float cz = (param[3] + param[5]) / 2;
        float cy = 50;
        float rot;
        if (param[6] < 0)
            rot = -param[6];
        else
            rot = 90 - param[6];
        float sx = dist_of_points(param[2], param[3], param[4], param[5]);
        float sy = 100;
        float sz = 10;
        Transform wall = Instantiate(wallPrefab, new Vector3(cx, cy, cz), Quaternion.Euler(new Vector3(.0f, rot, .0f)));
        wall.localScale = new Vector3(sx, sy, sz);
    }
    // todo: test
    private void draw_a_obstacle(float[] param)
    {
        float cx = (param[0] + param[4]) / 2;
        float cz = (param[1] + param[5]) / 2; float cy = 50;
        float rot = -Mathf.Atan((param[7] - param[3]) / (param[6] - param[2])) * 180 / 3.14f;
       float  sx = dist_of_points(param[0], param[1], param[2], param[3]);
        float sz = dist_of_points(param[2], param[3], param[4], param[5]); float sy = 100;
        Transform obs = Instantiate(obsPrefab, new Vector3(cx, cy, cz), Quaternion.Euler(new Vector3(.0f, rot, .0f)));
        obs.localScale = new Vector3(sx, sy, sz);
    }
    private void draw_single_stuff(int cate, float[] param)
    {
        switch (cate)
        {
            case 0://wall
                draw_a_wall(param);
                break;
            case 2://focal point
                Instantiate(focalPrefab, new Vector3(param[1], .0f, param[2]), Quaternion.identity);
                break;
            case 3:
                draw_a_obstacle(param);
                break;
        }
    }
    private void resize_room(string roomStr)
    {
        string[] roomWords = roomStr.Split(' ');
        float width = float.Parse(roomWords[1]); float height = float.Parse(roomWords[2]);
        floor.localScale = new Vector3(width, floor.localScale.y, height);
    }

    private float[] getParametersFromWords(string[] words)
    {
        List<float> res = new List<float>();
        foreach (string sstr in words)
        {
            if (sstr[0] != 'R')
                res.Add(float.Parse(sstr));
        }
            
        return res.ToArray();
    }
    private void parser_resfile()
    {
        string[] contents = System.IO.File.ReadAllLines(filename);
        // tackle with roomsize
        resize_room(contents[0]);
        // tackle with other stuff
        int state = -1;

        for(int i=1; i< contents.Length; i++)
        {
            string[] parameters = contents[i].Split(' '); //Regex.Split(contents[i], "\t|\t");
            
            switch (parameters[0][0])
            {
                case 'W'://wall
                    state = 0;
                    break;
                case 'F'://furniture
                    state = 4;
                    break;
                case 'P'://focal point
                    state = 2;
                    break;
                case 'O'://obstacle
                    state = 3;
                    break;
                case 'R'://furniture pos recommendation
                    state = 1;
                    recomParameters.Add(getParametersFromWords(parameters));
                    break;
                default:
                    if (state == 4)
                        objectParams.Add(getParametersFromWords(parameters));
                    else
                        draw_single_stuff(state, getParametersFromWords(parameters));
                    break;
            }
        }
        objCount = objectParams.Count;
        schemeCount = recomParameters.Count/ objCount;

    }
}
