using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class Server : MonoBehaviour
{
    [SerializeField] private Chat chat;
    [SerializeField] BlackBoard Data;
    [SerializeField] public string IpV4;
    [SerializeField] public int serverPort = 4269;

    private static Guid Id = Guid.Empty;
    private static string Name = string.Empty;

    private Guid WhitePlayerID = Guid.Empty;
    private Guid BlackPlayerID = Guid.Empty;

    public Guid GetWhitePlayerID() { return WhitePlayerID; }
    public Guid GetBlackPlayerID() { return BlackPlayerID; }

    TcpListener server;
    Thread serverThread;

    private Dictionary<Guid, ClientInfo> clients = new Dictionary<Guid, ClientInfo>();

    #region Monobehaviours
    void Start()
    {
        Data.AddData<Server>(DataKey.SERVER, this);
        serverThread = new Thread(new ThreadStart(SetupServer));
        serverThread.Start();
    }
    #endregion

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Data.SetData(DataKey.SERVER_IP, ip.ToString());
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void SetupServer()
    {
        try
        {
            Id = Guid.NewGuid();
            IpV4 = GetLocalIPAddress();
            IPAddress localAddr = IPAddress.Parse(IpV4);
            server = new TcpListener(localAddr, serverPort);
            server.Start();

            Name = $"Server-{Id}-{IpV4}-{serverPort}";
            Debug.Log("Server started : " + Name);

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Guid clientId = Guid.NewGuid();
                var clientInfo = new ClientInfo
                {
                    Id = clientId,
                    TcpClient = client,
                    Stream = client.GetStream(),
                    ConnectionTimestamp = System.DateTime.Now.ToString()
                };

                clients.Add(clientId, clientInfo);

                SendDataToAllClients(ServerConsole.Log($"Client {clientId} connected at {clientInfo.ConnectionTimestamp}"));

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

                SendToData(buffer, clientInfo.Id);
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
            SendDataToAllClients(ServerConsole.Log($"Client {clientInfo.Id} left the party"));
        }
    }

    public void SendToData(byte[] _data, Guid _clientId)
    {
        Package package = DataSerialize.DeserializeFromBytes<Package>(_data);

        switch (package.Header.SendMethod)
        {
            case SendMethod.OPPONENT:
                Debug.Log("Opponent");
                break;
            case SendMethod.ONLY_CLIENT:
                if (package.Data is IdRequest id_request)
                {
                    Debug.Log(_clientId);
                    Header header = package.Header;
                    header.Id = _clientId;
                    package.Header = header;
                    id_request.Id = _clientId;
                }

                if (package.Data is ChessInfoGameData chess_info_game_data)
                {
                    chess_info_game_data.BlackPlayerId = BlackPlayerID;
                    chess_info_game_data.WhitePlayerId = WhitePlayerID;
                }

                _data = DataSerialize.SerializeToBytes(package);
                SendDataToClient(_clientId, _data);
                Debug.Log("Only client");
                break;
            case SendMethod.ALL_CLIENTS:
                Debug.Log("AllClient");
                SendDataToAllClients(_data);
                break;
            case SendMethod.ONLY_SERVER:
                Debug.Log("ONLY_SERVER");
                if (package.Data is TeamRequest team_request)
                {
                    if (team_request.RequestJoinOrLeave == JoinOrLeave.JOIN)
                    {
                        if (team_request.RequestTeam == Teams.TEAM_WHITE && WhitePlayerID == Guid.Empty && BlackPlayerID != _clientId)
                        {
                            WhitePlayerID = _clientId;
                            SendDataToAllClients(_data);
                        }
                        else if (team_request.RequestTeam == Teams.TEAM_BLACK && BlackPlayerID == Guid.Empty && WhitePlayerID != _clientId)
                        {
                            BlackPlayerID = _clientId;
                            SendDataToAllClients(_data);
                        }
                    }
                    else if (team_request.RequestJoinOrLeave == JoinOrLeave.LEAVE)
                    {
                        if (team_request.RequestTeam == Teams.TEAM_WHITE && WhitePlayerID == _clientId)
                        {
                            Debug.Log("leave");
                            WhitePlayerID = Guid.Empty;
                            SendDataToAllClients(_data);
                        }
                        else if (team_request.RequestTeam == Teams.TEAM_BLACK && BlackPlayerID == _clientId)
                        {
                            BlackPlayerID = Guid.Empty;
                            SendDataToAllClients(_data);
                        }
                    }
                }
                break;
            case SendMethod.ONLY_SPECTATORS:
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

    public void QuitServer()
    {
        //BroadcastMessageToClients(broadcastMessage); LEAVE ROOM TO MAIN MENUE
        foreach (var client in clients.Values)
        {
            client.Stream.Close();
            client.TcpClient.Close();
        }
        server.Stop();
        serverThread.Abort();
    }

    #region Basic Message
    public void SendMessageToClient(Guid clientId, string message)
    {
        if (clients.ContainsKey(clientId))
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            clients[clientId].Stream.Write(msg, 0, msg.Length);
        }
    }

    #endregion

    #region Data

    public void SendDataToClient(Guid _clientId, byte[] _data)
    {
        if (clients.ContainsKey(_clientId))
        {
            clients[_clientId].Stream.Write(_data, 0, _data.Length);
        }
    }

    public void SendDataToAllClients(byte[] _data)
    {
        foreach (var client in clients.Values)
        {
            SendDataToClient(client.Id, _data);
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

    public class ServerConsole
    {
        static Package package = new Package(new Header(Id, Name, DateTime.Now, SendMethod.ALL_CLIENTS), new ChatMessage(string.Empty, SerializableColor.Red, DataKey.ACTION_CHAT));

        public static byte[] Log(string _message)
        {
            ChatMessage chat_message = (ChatMessage)package.Data;
            chat_message.Content = _message;

            return DataSerialize.SerializeToBytes(package);
        }

    }
}

public class ClientInfo
{
    public Guid Id { get; set; }
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public string ConnectionTimestamp { get; set; }
}
