using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class Client : MonoBehaviour
{
    private string IpServer = string.Empty;
    [SerializeField] public string Pseudo = "JEAN";
    [SerializeField] public int serverPort = 4269;    
    [SerializeField] public int WaitBeforeStarting = 5;   
    [SerializeField] BlackBoard Data;
 
    public string messageToSend = "Hello Server!";

    [SerializeField] private BlackBoard ActionBlackBoard;

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    /*void Start()
    {
        Invoke("ConnectToServer", WaitBeforeStarting);
    }*/

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendMessageToServer(messageToSend);

            StructMessageChat chat = new StructMessageChat("Jean", DateTime.Now, "bouh", SendTo.ALL_CLIENTS, DataKey.ACTION_CHAT);

            byte[] buffer = DataSerialize.SerializeToBytes(chat);

            SendDataToServer(buffer);

            //Debug.Log(DataSerialize.DeserializeTypeFromBytes(buffer));

           // StructMessageChat oldChat = DataSerialize.DeserializeFromBytes<StructMessageChat>(buffer);
           // Debug.Log(oldChat.Pseudo);
        }
    }

    public void SetClientIP(string ip_) 
    {
        IpServer = ip_;
        Data.SetData(DataKey.SERVER_IP, IpServer);
    }


    public bool ConnectToServer()
    {
        try
        {
            client = new TcpClient(IpServer, serverPort);
            stream = client.GetStream();

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();

            return true;
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
            return false;
        }

    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                if (stream.DataAvailable)
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);

                        DataProcessing(incomingData);
                      
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("Server message received: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void DataProcessing(byte[] _data)
    {
        try
        {
            Debug.Log("PROCESSING");
            IAction action = DataSerialize.DeserializeFromBytes<IAction>(_data);
            action.CallAction(ActionBlackBoard);
        }
        catch
        {
            Debug.Log("Failed");
        }
    }

    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    public void SendDataToServer(byte[] data)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }
        stream.Write(data, 0, data.Length);
    }

    public void QuitClient() 
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }

    void OnApplicationQuit()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }
}
