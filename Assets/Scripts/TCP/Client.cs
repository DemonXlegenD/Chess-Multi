using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using UnityEditor.VersionControl;


public class Client : MonoBehaviour
{
    [SerializeField] public string IpV4;
    [SerializeField] public int serverPort = 4269;    
    [SerializeField] public int WaitBeforeStarting = 5;    
    public string messageToSend = "Hello Server!";

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    void Start()
    {
        Invoke("ConnectToServer", WaitBeforeStarting);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendMessageToServer(messageToSend);

            ChatMessage chat = new ChatMessage("Jean", DateTime.Now, "bouh");

            byte[] buffer = chat.SerializeToBytes();
            Debug.Log(buffer);

            ChatMessage oldChat = new ChatMessage(ChatMessage.DeserializeFromBytes<StructMessageChat>(buffer));
            Debug.Log(oldChat.TypeMessage.Pseudo);
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient(IpV4, serverPort);
            stream = client.GetStream();

            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
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
