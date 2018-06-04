using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
#if !UNITY_EDITOR

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net.Sockets;

public class NativeMethods
{
     [DllImport("kernel32")]
     public static extern void Sleep(uint dwMilliseconds);
}
#endif


public class Client : MonoBehaviour
{
     //public TrackingManager trackingManager;
     public StatusManager statusTextManager;

#if !UNITY_EDITOR
     private bool _useUWP = true;
     private Windows.Networking.Sockets.StreamSocket socket;
     private Task exchangeTask;
#endif

#if UNITY_EDITOR
    private bool _useUWP = false;
    System.Net.Sockets.TcpClient client;

     private Thread exchangeThread;
     System.Net.Sockets.NetworkStream stream;
#endif

     private Byte[] bytes = new Byte[256];
     public StreamWriter writer;
     public StreamReader reader;
     public int waitCycle = 50;
     public int cycleWaited = 50;
     public string host = null;
     public string port = null;

    private string received = null;
    public void Connect(string host, string port)
     {
          this.host = host;
          this.port = port;
          if (_useUWP)
          {
               ConnectUWP(host, port);
          }
          else
          {
               ConnectUnity(host, port);
          }
     }


#if UNITY_EDITOR
    private void ConnectUWP(string host, string port)
#else
     private async void ConnectUWP(string host, string port)
#endif
     {
#if UNITY_EDITOR
        errorStatus = "UWP TCP client used in Unity!";
#else
          try
          {
               if (exchangeTask != null) StopExchange();

               socket = new Windows.Networking.Sockets.StreamSocket();
               Windows.Networking.HostName serverHost = new Windows.Networking.HostName(host);
               await socket.ConnectAsync(serverHost, port);

               Stream streamOut = socket.OutputStream.AsStreamForWrite();
               writer = new StreamWriter(streamOut) {AutoFlush = true};
               

               Stream streamIn = socket.InputStream.AsStreamForRead();
               reader = new StreamReader(streamIn);

               RestartExchange();
               successStatus = "Connected!";
          }
          catch (Exception e)
          {
               errorStatus = e.ToString();
          }
#endif
     }

     private void ConnectUnity(string host, string port)
     {
#if !UNITY_EDITOR
          errorStatus = "Unity TCP client used in UWP!";
#else
        try
        {
            if (exchangeThread != null) StopExchange();
            try
            {
                //TODO:
                client = new TcpClient("DESKTOP-SBAH6K5", Int32.Parse(port));
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Int32.Parse(port));
                //client.Connect(endPoint);

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            RestartExchange();
            successStatus = "Connected!";
        }
        catch (Exception e)
        {
            errorStatus = e.ToString();
        }
#endif
     }

     private bool sendingData = false;
     private bool exchanging = false;
     private bool exchangeStopRequested = false;
     private string lastPacket = null;

     private string errorStatus = null;
     private string warningStatus = null;
     private string successStatus = null;
     private string unknownStatus = null;

     public void RestartExchange()
     {
#if UNITY_EDITOR
        if (exchangeThread != null) StopExchange();
        exchangeStopRequested = false;
        exchangeThread = new System.Threading.Thread(ExchangePackets);
        exchangeThread.Start();
#else
          if (exchangeTask != null) StopExchange();
          exchangeStopRequested = false;
          exchangeTask = Task.Run(() => ExchangePackets());
#endif
     }

     public void Update()
     {
          if (lastPacket != null)
          {
               ReportDataToTrackingManager(lastPacket);
          }

          if (errorStatus != null)
          {
               statusTextManager.SetError(errorStatus);
               errorStatus = null;
          }

          if (warningStatus != null)
          {
               statusTextManager.SetWarning(warningStatus);
               warningStatus = null;
          }

          if (successStatus != null)
          {
               statusTextManager.SetSuccess(successStatus);
               successStatus = null;
          }

          if (unknownStatus != null)
          {
               statusTextManager.SetUnknown(unknownStatus);
               unknownStatus = null;
          }
     }

     public void write(byte[] buffer, int offset, int count)
     {
          //while (sendingData)
          //{

          //}
          //while (cycleWaited < waitCycle)
          //{
          //     cycleWaited++;
          //}
          
          try
          {
               writer.BaseStream.Write(buffer, offset, count);
               writer.Flush();
          }
          catch (Exception ex)
          {
               Debug.Log(ex);
               Connect(host, port);
          }
#if !UNITY_EDITOR
          NativeMethods.Sleep(1000);
#endif
          sendingData = true;
     }
     
     public String receive(){
        received = "1 0 4 150.000000 200.000000 30.000000 46.023277 -74.989998 0.000000 1.570796 -99.989998 37.748737 0.000000 1.570796 -75.431366 74.989998 0.000000 1.570796";
        return received;
    }

     public void ExchangePackets()
     {
          while (!exchangeStopRequested)
          {
               if (writer == null || reader == null) continue;
               exchanging = true;

               //writer.Write("X\n");
               Debug.Log("Sent data!");
               //string received = null;

#if UNITY_EDITOR
            byte[] bytes = new byte[client.SendBufferSize];
            int recv = 0;
            while (true)
            {
                recv = stream.Read(bytes, 0, client.SendBufferSize);
                received += Encoding.UTF8.GetString(bytes, 0, recv);
                if (received.EndsWith("\n")) break;
            }
#else
               try
               {
                    received = reader.ReadLine();
               }
               catch (Exception ex)
               {
                    Debug.Log(ex); 
                    Connect(host, port);
               }
#endif

               lastPacket = received;
               Debug.Log("Read data: " + received);
               sendingData = false;
               exchanging = false;
          }
     }

     private void ReportDataToTrackingManager(string data)
     {
          if (data == null)
          {
               Debug.Log("Received a frame but data was null");
               return;
          }

          var parts = data.Split(';');
          foreach (var part in parts)
          {
               ReportStringToTrackingManager(part);
          }
     }

     private void ReportStringToTrackingManager(string rigidBodyString)
     {
          //    var parts = rigidBodyString.Split(':');
          //    var positionData = parts[1].Split(',');
          //    var rotationData = parts[2].Split(',');
          //
          //    int id = Int32.Parse(parts[0]);
          //    float x = float.Parse(positionData[0]);
          //    float y = float.Parse(positionData[1]);
          //    float z = float.Parse(positionData[2]);
          //    float qx = float.Parse(rotationData[0]);
          //    float qy = float.Parse(rotationData[1]);
          //    float qz = float.Parse(rotationData[2]);
          //    float qw = float.Parse(rotationData[3]);
          //
          //    Vector3 position = new Vector3(x, y, z);
          //    Quaternion rotation = new Quaternion(qx, qy, qz, qw);
          //
          //    trackingManager.UpdateRigidBodyData(id, position, rotation);
     }

     public void StopExchange()
     {
          exchangeStopRequested = true;

#if UNITY_EDITOR
        if (exchangeThread != null)
        {
            exchangeThread.Abort();
            stream.Close();
            client.Close();
            writer.Close();
            reader.Close();

            stream = null;
            exchangeThread = null;
        }
#else
          if (exchangeTask != null)
          {
               exchangeTask.Wait();
               socket.Dispose();
               writer.Dispose();
               reader.Dispose();

               socket = null;
               exchangeTask = null;
          }
#endif
          writer = null;
          reader = null;
     }

     public void OnDestroy()
     {
          StopExchange();
     }
}