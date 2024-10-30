using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.VersionControl;


public class Server : MonoBehaviour
{
    [SerializeField] private Chat chat;
    [SerializeField] public string IpV4;
    [SerializeField] public int serverPort = 4269;

    TcpListener server;
    Thread serverThread;

    private Dictionary<int, ClientInfo> clients = new Dictionary<int, ClientInfo>();
    private int clientCounter = 0;

    #region Monobehaviours
    void Start()
    {
        IpV4 = GetLocalIPAddress();
        serverThread = new Thread(new ThreadStart(SetupServer));
        serverThread.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BroadcastMessageToClients("Hello");
        }
        
    }
    #endregion

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void SetupServer()
    {
        try
        {
            IPAddress localAddr = IPAddress.Parse(IpV4);
            server = new TcpListener(localAddr, serverPort);
            server.Start();
            Debug.Log("Server started at " + IpV4 + ":" + serverPort);

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                int clientId = clientCounter++;
                var clientInfo = new ClientInfo
                {
                    Id = clientId,
                    TcpClient = client,
                    Stream = client.GetStream(),
                    ConnectionTimestamp = System.DateTime.Now.ToString()
                };

                clients.Add(clientId, clientInfo);
                BroadcastMessageToClients("Client " + clientId + " connected at " + clientInfo.ConnectionTimestamp);

                Thread clientThread = new Thread(() => HandleClient(clientInfo));
                clientThread.Start();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    private void DisplayMessage(string message)
    {
        chat.AddMessage(message);
    }

    private void HandleClient(ClientInfo clientInfo)
    {
        TcpClient client = clientInfo.TcpClient;
        NetworkStream stream = clientInfo.Stream;
        byte[] buffer = new byte[1024];
        string data;

        try
        {
            while ((stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                data = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                Debug.Log("SERVER : Received from client " + clientInfo.Id + ": " + data);

                //string broadcastMessage = "Client " + clientInfo.Id + ": " + data;
                //BroadcastMessageToClients(broadcastMessage);

                SendToData(buffer);

               
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Client " + clientInfo.Id + " disconnected: " + e);
        }
        finally
        {
            client.Close();
            clients.Remove(clientInfo.Id);
            Debug.Log("Client " + clientInfo.Id + " removed from the client list.");
        }
    }

    public void SendToData(byte[] _data)
    {
        ISendTo typeSendTo = DataSerialize.DeserializeFromBytes<ISendTo>(_data);

        switch (typeSendTo.SendTo)
        {
            case SendTo.OPPONENT:
                Debug.Log("Opponent");
                break;
            case SendTo.ALL_CLIENTS:
                Debug.Log("AllClient");
                BroadcastDataToAllClients(_data);
                break;
            case SendTo.SPECTATOR:
                Debug.Log("Spectator");
                break;
            default:
                Debug.Log("None");
                break;
        }
    }


    private void OnApplicationQuit()
    {
        foreach (var client in clients.Values)
        {
            client.Stream.Close();
            client.TcpClient.Close();
        }
        server.Stop();
        serverThread.Abort();
    }

    #region Basic Message
    public void SendMessageToClient(int clientId, string message)
    {
        if (clients.ContainsKey(clientId))
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            clients[clientId].Stream.Write(msg, 0, msg.Length);
        }
    }

    public void BroadcastMessageToClients(string message)
    {
        Debug.Log("Broadcast message: " + message);

        foreach (var client in clients.Values)
        {
            SendMessageToClient(client.Id, message);
        }
    }

    #endregion

    #region Data

    public void SendDataToClient(int _clientId, byte[] _data)
    {
        if (clients.ContainsKey(_clientId))
        {
            clients[_clientId].Stream.Write(_data, 0, _data.Length);
        }
    }

    public void BroadcastDataToAllClients(byte[] _data)
    {
        foreach (var client in clients.Values)
        {
            SendDataToClient(client.Id, _data);
        }
    }

    #endregion
}

public class ClientInfo
{
    public int Id { get; set; }
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public string ConnectionTimestamp { get; set; }
}
