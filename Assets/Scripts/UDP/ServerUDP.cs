using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ServerUDP : MonoBehaviour
{
    private UdpClient udpServer;
    private IPEndPoint remoteEndPoint;
    [SerializeField] public int serverPort = 4269;
    Thread serverThread;

    private void Start()
    {
        serverThread = new Thread(new ThreadStart(StartUDPServer));
        serverThread.Start();
    }

    private void StartUDPServer()
    {
        udpServer = new UdpClient(serverPort);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, serverPort);

        Debug.Log("Server started. Waiting for messages...");

        udpServer.BeginReceive(ReceiveData, null);
        
    }

    private void ReceiveData(IAsyncResult result)
    {
        byte[] receivedBytes = udpServer.EndReceive(result, ref remoteEndPoint);
        string receivedMessage = System.Text.Encoding.UTF8.GetString(receivedBytes);

        Debug.Log("Received from client: " + receivedMessage);

        udpServer.BeginReceive(ReceiveData, null);
    }

    private void SendData(string message, IPEndPoint endPoint)
    {
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(message);

        // Send the message to the client
        udpServer.Send(sendBytes, sendBytes.Length, endPoint);

        Debug.Log("Sent to client: " + message);
    }
}
