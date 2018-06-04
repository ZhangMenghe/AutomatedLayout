using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;
public class layoutScene : MonoBehaviour {
    private string recommendationFile;
    private string pythonProgramFile;
    private int currentRecId = -1;
    private Transform floor;
    private List<float[]> recomParameters;
    private List<float[]> objectParams;
    private List<Transform> objects;
    private int objCount;
    private int schemeCount;
    private Thread recogThread;
    private int HeaderSize = 12;
    private int TYPE_STRING = 7;

    public Transform wallPrefab;
    public Transform focalPrefab;
    public Transform obsPrefab;
    public Transform objPrefab;
    public List<int> customObjList = new List<int>();
    public String pythonEnv;
    public String IP_ADDR;
    public Client TcpClient;

    private List<int> cateObjRecords = new List<int>();
    private string[] receviedWords;
    private bool unProcess = true;
    void Awake()
    {
        recommendationFile = "E:/recommendation-nr.txt";
        //recommendationFile = Application.dataPath + "/InputData/intermediate/recommendation.txt";
        //pythonProgramFile = Directory.GetCurrentDirectory() + "/Auxiliary/SemanticRecog/depth2mask.py";
        //
        //recommendationFile = "ms-appx:///InputData/intermediate/recommendation.txt";
        //StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///xmlData/Test.xml"));
        ///Windows.Storage.ApplicationData.Current.LocalFolder;
        recomParameters = new List<float[]>();
        objectParams = new List<float[]>();
        objects = new List<Transform>();
        recogThread = null;
        pythonEnv = "C:/Users/menghe/Anaconda3/envs/mpytorch/python.exe";
        InitiallayoutOnScreen();
    }
    // Use this for initialization
    void Start () {
        //RecognizeFurniture();
        TcpClient.Connect(IP_ADDR, "9988");
    }
    void Update()
    {
        if(null != TcpClient.receive() && unProcess){
            receviedWords = TcpClient.receive().Split(' ');
            objCount = int.Parse(receviedWords[0]);
            for(int i=0; i<objCount; i++){
                float[] tmpObj = new float[5];
                for (int j = 1; j <= 5; j++)
                    tmpObj[j-1] = float.Parse(receviedWords[5 * i + j]);
                objectParams.Add(tmpObj);
            }

            int startId = 5 * objCount + 1;
            for(int i=startId,n=0; i< receviedWords.Length; i += 4,n++){
                float[] tmpObj = new float[4];
                for (int j = 0; j < 4; j++)
                    tmpObj[j] = float.Parse(receviedWords[startId + 4*n +j]);
                recomParameters.Add(tmpObj);
            }
            schemeCount = recomParameters.Count / objCount;
            unProcess = false;
        }      
    }
    public void initCateRecords(int cateNum)
    {
        for (int i = 0; i < cateNum; i++)
            cateObjRecords.Add(0);
    }
    /*public void RecognizeFurniture()
    {
        // if (recogThread.IsAlive)
        if(recogThread!=null)
            recogThread.Abort();
        recogThread = new Thread(process_existance);
        recogThread.Start();
    }*/

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
            float sx = objectParams[i][3], sy = objectParams[i][4], sz = objectParams[i][2];
            float cx = param[0], cy = sy/2, cz = param[1];
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

    public Vector2 get_roomSize(){
        return new Vector2(floor.localScale.x, floor.localScale.z);
    }

    private float dist_of_points(float x1, float y1, float x2, float y2){
        return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
    }

    private void draw_a_wall(float[] param)
    {
        float cx = (param[2] + param[4]) / 2;
        float cz = (param[3] + param[5]) / 2;
        float cy = 50;
        float rot;
        if (param[6] < 0)
            rot = -param[6]/Mathf.PI * 180;
        else
            rot = param[6] / Mathf.PI * 180;
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
            if (sstr.Length>0 && sstr[0] != 'R')
                res.Add(float.Parse(sstr));
        }
            
        return res.ToArray();
    }
    private void parser_resfile()
    {
        string[] contents = System.IO.File.ReadAllLines(recommendationFile);

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
        if(objCount != 0)
            schemeCount = recomParameters.Count/ objCount;
    }
    public void addCustomObj(int newObjId)
    {
        cateObjRecords[newObjId] += 1;
    }
    private void encode_and_send_input(String msg)
    {
        msg = "abc\n";
        byte[] contents = Encoding.ASCII.GetBytes(msg);
        byte[] sendMsy = new byte[HeaderSize + contents.Length];
        byte[] widthBuf = BitConverter.GetBytes(msg.Length);
        Buffer.BlockCopy(widthBuf, 0, sendMsy, 0, 4);
        sendMsy[8] = (byte)TYPE_STRING;
        contents.CopyTo(sendMsy, HeaderSize);

        //decoding :string someString = Encoding.ASCII.GetString(bytes);
        TcpClient.write(sendMsy, 0, sendMsy.Length);
    }
    public void startToGenerate()
    {
        String sendOutMsg = "";
        for (int i = 0; i < cateObjRecords.Count; i++)
            if (cateObjRecords[i] != 0){
                sendOutMsg += i.ToString() + " " + cateObjRecords[i].ToString();
                cateObjRecords[i] = 0;
            }
        encode_and_send_input(sendOutMsg);

    }
    /*private void process_existance()
    {
        UnityEngine.Debug.Log("start the thread");
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        try
        {
            Process python = new Process();
            python.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            python.StartInfo.CreateNoWindow = false;
            python.StartInfo.FileName = pythonEnv;
            python.StartInfo.Arguments = pythonProgramFile;
            python.Start();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }

        sw.Stop();
        UnityEngine.Debug.Log("Recognization complete! File is ready! Elapsed time: " + sw.ElapsedMilliseconds / 1000f);
    }*/
}
