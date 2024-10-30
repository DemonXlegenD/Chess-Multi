using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UDPExample : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;
    private string messageToSend = "Hello Server!";
    [SerializeField] public string IpV4;
    [SerializeField] public int serverPort = 4269;

    private void Start()
    {
        StartUDPClient(IpV4, serverPort);
    }

    private void StartUDPClient(string ipAddress, int port)
    {
        udpClient = new UdpClient();
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

        // Start receiving data asynchronously
        udpClient.BeginReceive(ReceiveData, null);

        // Send a message to the server
        SendData(messageToSend);
    }

    private void ReceiveData(IAsyncResult result)
    {
        byte[] receivedBytes = udpClient.EndReceive(result, ref remoteEndPoint);
        string receivedMessage = System.Text.Encoding.UTF8.GetString(receivedBytes);

        Debug.Log("Received from server: " + receivedMessage);

        // Continue receiving data asynchronously
        udpClient.BeginReceive(ReceiveData, null);
    }

    private void SendData(string message)
    {
        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(message);

        // Send the message to the server
        udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);

        Debug.Log("Sent to server: " + message);
    }
}