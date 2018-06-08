using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;
using UnityEditor;

public class layoutScene : MonoBehaviour
{
     private string recommendationFile;
     private string pythonProgramFile;
     public int currentRecId = -1;
     private Transform floor;
     private List<float[]> recomParameters;
     private List<float[]> objectParams;
     public List<Transform> objects;
     private int objCount;
     private int schemeCount;
     private Thread recogThread;
     private int HeaderSize = 12;
     private int TYPE_STRING = 7;
     private float ground_yoffset = -0.0f;

     private List<Transform> walls;
     private List<Transform> obstacles;

     public bool debug_wall = false;
     public bool debug_obs = false;

     private bool pre_dw = true;
     private bool pre_do = true;

     public Transform wallPrefab;
     public Transform focalPrefab;

     public Transform obsPrefab;

     public bool hasRecommendation = false;

     public int index = 0;
     //public Transform objPrefab;
     public List<int> customObjList = new List<int>();
     public String pythonEnv;
     public String IP_ADDR;
     public Client TcpClient;

     private List<int> cateObjRecords = new List<int>();
     private string[] receviedWords;
     private bool unProcess = true;

     private List<Vector3> initialRotation =
          new List<Vector3>
          {
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 0),
               new Vector3(-90, 180, 0),
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 0),
               new Vector3(0, 0, 90)
          };

     public List<Transform> objPrefabList;

     void Awake()
     {
          //recommendationFile = "E:/recommendation-nr.txt";
          //recommendationFile = Application.dataPath + "/InputData/intermediate/recommendation.txt";
          //pythonProgramFile = Directory.GetCurrentDirectory() + "/Auxiliary/SemanticRecog/depth2mask.py";
          //
          //recommendationFile = "ms-appx:///InputData/intermediate/recommendation.txt";
          //StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///xmlData/Test.xml"));
          ///Windows.Storage.ApplicationData.Current.LocalFolder;
          recomParameters = new List<float[]>();
          objectParams = new List<float[]>();
          objects = new List<Transform>();
          walls = new List<Transform>();
          obstacles = new List<Transform>();
          recogThread = null;
          pythonEnv = "C:/Users/menghe/Anaconda3/envs/mpytorch/python.exe";
          InitiallayoutOnScreen();
     }

     // Use this for initialization
     void Start()
     {
          //RecognizeFurniture();
          //TcpClient.Connect(IP_ADDR, "9988");
          var curr = objPrefabList[1];

          /* for (int i = 0; i < objPrefabList.Count; i++)
           {
                var curr = objPrefabList[i];
                if (curr == null)
                {
                     continue;
                }

                Texture2D tex = null;
                while (tex == null)
                {
                     tex = AssetPreview.GetAssetPreview(curr.gameObject);
                 }
                byte[] bytes = tex.EncodeToPNG();


                // For testing purposes, also write to a file in the project folder
                File.WriteAllBytes(Application.dataPath + "/../prefab"+i+".png", bytes);

           }*/
     }

     void Update()
     {
          if (debug_wall)
          {
               if (!pre_dw)
               {
                    pre_dw = true;
                    foreach (var w in walls)
                    {
                         if (w != null)
                              w.gameObject.SetActive(true);
                    }
               }
          }
          else
          {
               if (pre_dw)
               {
                    pre_dw = false;
                    foreach (var w in walls)
                    {
                         if (w != null)
                              w.gameObject.SetActive(false);
                    }
               }
          }

          if (debug_obs)
          {
               if (!pre_do)
               {
                    pre_do = true;
                    foreach (var w in obstacles)
                    {
                         if (w != null)
                              w.gameObject.SetActive(true);
                    }
               }
          }
          else
          {
               if (pre_do)
               {
                    pre_do = false;
                    foreach (var w in obstacles)
                    {
                         if (w != null)
                              w.gameObject.SetActive(false);
                    }
               }
          }

          //todo: this is debug only!! dummy to add a bed only
          string received = TcpClient.receive(index);
          //string received = TcpClient.lastPacket;
          if (null != received && received.Length > 0 && !hasRecommendation)
          {
               recomParameters.Clear();
               objectParams.Clear();
               receviedWords = received.Split(' ');
               TcpClient.lastPacket = "";
               objCount = int.Parse(receviedWords[0]);
               if (objCount < 1)
                    return;
               for (int i = 0; i < objCount; i++)
               {
                    float[] tmpObj = new float[5];
                    for (int j = 1; j <= 5; j++)
                         tmpObj[j - 1] = float.Parse(receviedWords[5 * i + j]);
                    objectParams.Add(tmpObj);
               }

               int startId = 5 * objCount + 1;
               for (int i = startId, n = 0; i < receviedWords.Length; i += 4, n++)
               {
                    float[] tmpObj = new float[4];
                    for (int j = 0; j < 4; j++)
                         tmpObj[j] = float.Parse(receviedWords[startId + 4 * n + j]);
                    recomParameters.Add(tmpObj);
               }

               schemeCount = recomParameters.Count / objCount;
               hasRecommendation = true;
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
          var floor_p = floor.position;
          floor_p.y += ground_yoffset;
          floor.position = floor_p;
          parser_resfile();
          //resize_room("RoomSize: 400 300");
     }

     //param: id, cate, width, height, zheight
     private void draw_a_furniture(float[] param)
     {

     }

     public void ChangeRecommendation()
     {
          currentRecId = (currentRecId + 1) % schemeCount;
          for (int i = 0; i < objCount; i++)
          {
               int cate = (int) objectParams[i][1];
               Transform objPrefab = objPrefabList[cate];
               float[] param = recomParameters[schemeCount * i + currentRecId];
               float sx = objectParams[i][2], sy = objectParams[i][4], sz = objectParams[i][3];

               BoxCollider box = objPrefab.GetComponent<BoxCollider>();
               float box_x, box_y, box_z, box_cy;
               if (Mathf.Abs(initialRotation[cate].x % 360) == 90)
               {
                    box_x = box.size.x;
                    box_y = box.size.z;
                    box_z = box.size.y;
                    box_cy = box.center.z;
               }
               else if (Mathf.Abs(initialRotation[cate].y % 360) == 90)
               {
                    box_x = box.size.z;
                    box_y = box.size.y;
                    box_z = box.size.x;
                    box_cy = box.center.y;
               }
               else if (Mathf.Abs(initialRotation[cate].z % 360) == 90)
               {
                    box_x = box.size.y;
                    box_y = box.size.z;
                    box_z = box.size.x;
                    box_cy = box.center.z;
               }
               else
               {
                    box_x = box.size.x;
                    box_y = box.size.y;
                    box_z = box.size.z;
                    box_cy = box.center.y;
               }

               // Quaternion rotation = Quaternion.Euler(initialRotation[cate]);
               // Matrix4x4 m = Matrix4x4.Rotate(rotation);
               // new Vector3 test = m.MultiplyPoint3x4(new Vector);
               sx /= 100 * box_x;
               sy /= 100 * box_y;
               sz /= 100 * box_z;

               float cx = param[0], cy = sy * (box_y + box_cy) / 2, cz = param[1];

               float roty = initialRotation[cate].y + param[3] / Mathf.PI * 180;


               if (objects.Count > i)
               {
                    objects[i].SetPositionAndRotation(new Vector3(cx / 100, cy + ground_yoffset, cz / 100),
                         Quaternion.Euler(new Vector3(initialRotation[cate].x, roty, initialRotation[cate].z)));
                    continue;
               }


               Transform obj = Instantiate(objPrefabList[cate], new Vector3(cx / 100, cy + ground_yoffset, cz / 100),
                    Quaternion.Euler(new Vector3(initialRotation[cate].x, roty, initialRotation[cate].z)));


               obj.localScale = new Vector3(sx, sy, sz);
               objects.Add(obj);
          }
     }

     public Vector2 get_roomSize()
     {
          return new Vector2(floor.localScale.x, floor.localScale.z);
     }

     private float dist_of_points(float x1, float y1, float x2, float y2)
     {
          return Mathf.Sqrt(Mathf.Pow((x2 - x1), 2) + Mathf.Pow((y2 - y1), 2));
     }

     private void draw_a_wall(float[] param)
     {
          float cx = (param[2] + param[4]) / 2;
          float cz = (param[3] + param[5]) / 2;
          float rot;
          if (param[6] < 0)
               rot = -param[6] / Mathf.PI * 180;
          else
               rot = param[6] / Mathf.PI * 180;
          float sx = dist_of_points(param[2], param[3], param[4], param[5]) / 100;
          float sy = 3;
          float sz = 0.1f;
          Transform wall = Instantiate(wallPrefab, new Vector3(cx / 100, sy / 2 + ground_yoffset, cz / 100),
               Quaternion.Euler(new Vector3(.0f, rot, .0f)));
          wall.localScale = new Vector3(sx, sy, sz);
          walls.Add(wall);
     }

     // todo: test
     private void draw_a_obstacle(float[] param)
     {
          float cx = (param[0] + param[4]) / 2;
          float cz = (param[1] + param[5]) / 2;
          float cy = 50;
          float rot = -Mathf.Atan((param[7] - param[3]) / (param[6] - param[2])) * 180 / 3.14f;
          float sx = dist_of_points(param[0], param[1], param[2], param[3]);
          float sz = dist_of_points(param[2], param[3], param[4], param[5]);
          float sy = 100;
          Transform obs = Instantiate(obsPrefab, new Vector3(cx, cy, cz), Quaternion.Euler(new Vector3(.0f, rot, .0f)));
          obs.localScale = new Vector3(sx, sy, sz);
          obstacles.Add(obs);
     }

     private void draw_single_stuff(int cate, float[] param)
     {
          switch (cate)
          {
               case 0: //wall
                    draw_a_wall(param);
                    break;
               case 2: //focal point
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
          float width = float.Parse(roomWords[1]);
          float height = float.Parse(roomWords[2]);
          floor.localScale = new Vector3(width / 100, floor.localScale.y, height / 100);
     }

     private float[] getParametersFromWords(string[] words)
     {
          List<float> res = new List<float>();
          foreach (string sstr in words)
          {
               if (sstr.Length > 0 && sstr[0] != 'R')
                    res.Add(float.Parse(sstr));
          }

          return res.ToArray();
     }

     private string[] debug_emulator_wall()
     {
          string[] contents = new string[6];
          contents[0] = "RoomSize: 400 300";
          contents[1] = "WALL_Id zheight vertices zrotation";
          contents[2] = "0 20 -200.000000 150.000000 200.000000 150.000000 0";
          contents[3] = "1 20 -200.000000 -150.000000 -200.000000 150.000000 1.5708";
          contents[4] = "2 20 200.000000 -150.000000 200.000000 150.000000 1.5708";
          contents[5] = "3 20 -200.000000 -150.000000 200.000000 -150.000000 0";
          return contents;
     }

     private void parser_resfile()
     {
          // string[] contents = System.IO.File.ReadAllLines(recommendationFile);
          string[] contents = debug_emulator_wall();
          // tackle with roomsize
          resize_room(contents[0]);
          // tackle with other stuff
          int state = -1;

          for (int i = 1; i < contents.Length; i++)
          {
               string[] parameters = contents[i].Split(' '); //Regex.Split(contents[i], "\t|\t");

               switch (parameters[0][0])
               {
                    case 'W': //wall
                         state = 0;
                         break;
                    case 'F': //furniture
                         state = 4;
                         break;
                    case 'P': //focal point
                         state = 2;
                         break;
                    case 'O': //obstacle
                         state = 3;
                         break;
                    case 'R': //furniture pos recommendation
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
          if (objCount != 0)
               schemeCount = recomParameters.Count / objCount;
     }

     public void addCustomObj(int newObjId)
     {
          cateObjRecords[newObjId] += 1;
     }

     public void encode_and_send_input(String msg)
     {
          byte[] contents = Encoding.ASCII.GetBytes(msg);
          byte[] sendMsy = new byte[HeaderSize + contents.Length];
          byte[] widthBuf = BitConverter.GetBytes(msg.Length);
          Buffer.BlockCopy(widthBuf, 0, sendMsy, 0, 4);
          sendMsy[8] = (byte) TYPE_STRING;
          contents.CopyTo(sendMsy, HeaderSize);

          //decoding :string someString = Encoding.ASCII.GetString(bytes);
          UnityEngine.Debug.Log(msg);
          //TcpClient.write(sendMsy, 0, sendMsy.Length);
          TcpClient.doRead = true;
     }

     public void startToGenerate()
     {
          String sendOutMsg = "";
          //todo:this is for debug only!!
          addCustomObj(4);

          for (int i = 0; i < cateObjRecords.Count; i++)
               if (cateObjRecords[i] != 0)
               {
                    sendOutMsg += i.ToString() + " " + cateObjRecords[i].ToString() + ",";
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