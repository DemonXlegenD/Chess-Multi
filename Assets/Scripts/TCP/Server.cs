using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;


public class Server : MonoBehaviour
{
    [SerializeField] private Chat chat;
    [SerializeField] BlackBoard Data;
    [SerializeField] public string IpV4;
    [SerializeField] public int serverPort = 4269;

    private bool isServerRunning = false;

    private static Guid Id = Guid.Empty;
    private static string Name = string.Empty;

    private Guid WhitePlayerID = Guid.Empty;
    private Guid BlackPlayerID = Guid.Empty;

    public Guid GetWhitePlayerID() { return WhitePlayerID; }
    public Guid GetBlackPlayerID() { return BlackPlayerID; }
    private string WhitePlayerNickname = "";
    private string BlackPlayerNickname = "";

    public string GetWhitePlayerNickname() { return WhitePlayerNickname; }
    public string GetBlackPlayerNickname() { return BlackPlayerNickname; }

    TcpListener server;
    Thread serverThread;

    private Dictionary<Guid, ClientInfo> clients = new Dictionary<Guid, ClientInfo>();

    #region Monobehaviours
    void Start()
    {
        Data.AddData<Server>(DataKey.SERVER, this);
        StartServer();
    }


    private void OnApplicationQuit()
    {
        StopServer();
    }

    #endregion

    #region Server Method

    public void StartServer()
    {
        serverThread = new Thread(SetupServer); 
        serverThread.Start();
    }


    public void StopServer()
    {
        isServerRunning = false;

        SendDataToAllClients(ServerAction.Log($"Server is shutting down"));
        SendDataToAllClients(ServerAction.DoAction(DataKey.ACTION_LEAVE_ROOM));

        Data.ClearData(DataKey.SERVER);

        foreach (var client_info in clients.Values)
        {
            try
            {
                client_info.Stream?.Close();
                client_info.TcpClient?.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Erreur lors de la fermeture de la connexion pour le client {client_info.Id}: {ex.Message}");
            }
        }

        clients.Clear();

        if (server != null)
        {
            server.Stop(); // Close the TCPListener
        }
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Join(); // Waiting for the end of the thread
        }
    }

    private void SetupServer()
    {
        try
        {
            Id = Guid.NewGuid();
            IpV4 = GetLocalIPAddress(); // Get IP that clients are going to use to join
            IPAddress localAddr = IPAddress.Parse(IpV4);
            server = new TcpListener(localAddr, serverPort); // Start the TCP listener
            server.Start();

            Name = $"Server";

            isServerRunning = true;

            while (isServerRunning)
            {
                //Check Clients Waiting To Connect
                if (server.Pending())
                {
                    TcpClient client = server.AcceptTcpClient(); // Accept clients

                    Guid clientId = Guid.NewGuid(); // Create a new ID
                    var clientInfo = new ClientInfo // Create a new client struct
                    {
                        Id = clientId,
                        pseudo = string.Empty,
                        TcpClient = client,
                        Stream = client.GetStream(),
                        ConnectionTimestamp = System.DateTime.Now.ToString()
                    };

                    clients.Add(clientId, clientInfo); // Add it to the client list

//SendDataToAllClients(ServerAction.Log($"Client {clientId} connected at {clientInfo.ConnectionTimestamp}")); // Tell everyone that a new client logged in

                    Thread clientThread = new Thread(() => HandleClient(clientInfo)); // Start the thread that listen for this client msg
                    clientThread.Start();
                }

            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
            Debug.Log("Server stopped.");
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
                Data.SetData(DataKey.SERVER_IP, ip.ToString());
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void HandleClient(ClientInfo clientInfo)
    {
        TcpClient client = clientInfo.TcpClient;
        NetworkStream stream = clientInfo.Stream;
        byte[] buffer = new byte[1024];
        string data;

        try
        {
            while (isServerRunning && stream.CanRead)
            {
                int bytesRead;
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException e)
                {
                    Debug.Log("Client " + (clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id) + " disconnected (IOException): " + e.Message);
                    break;
                }
                catch (ObjectDisposedException)
                {
                    Debug.Log("Client " + (clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id) + " disconnected: Stream closed.");
                    break;
                }

                if (bytesRead == 0) // 0 bytes read means the client has disconnected
                {
                    Debug.Log("Client " + (clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id) + " disconnected (0 bytes read).");
                    break;
                }

                data = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd('\0');
                Debug.Log("SERVER : Received from client " + (clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id) + ": " + data);

                DataProcessing(buffer, clientInfo.Id); // Process incoming data
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Client " + clientInfo.Id + " disconnected (SocketException): " + e.Message);
        }
        finally
        {
            // Close and Clean the client
            client.Close();
            clients.Remove(clientInfo.Id);
            Debug.Log("Client " + clientInfo.Id + " removed from the client list.");

            if (isServerRunning)
            {
                SendDataToAllClients(ServerAction.Log($"Client {(clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id)} left the party")); // Send to everyone that the server is closing so they have to close too
            }
        }
    }

    #region Data

    public void DataProcessing(byte[] _data, Guid _clientId) // Process incoming data
    {
        Package package = DataSerialize.DeserializeFromBytes<Package>(_data); // Deserialize incoming data From Bytes

        if (clients[_clientId].pseudo == string.Empty)
        {
            ClientInfo clientInfo = clients[_clientId];
            clientInfo.pseudo = package.Header.Pseudo;
            SendDataToAllClients(ServerAction.Log($"Client {(clientInfo.pseudo != string.Empty ? clientInfo.pseudo : clientInfo.Id)} connected at {clientInfo.ConnectionTimestamp}")); // Tell everyone that a new client logged in
        }

        switch (package.Header.SendMethod)
        {
            case SendMethod.OPPONENT: // If the package is for the opponent
                Debug.Log("Opponent"); // Not handle yet
                break;
            case SendMethod.ONLY_CLIENT: // If the package is for himself
                Package new_package = new Package();
                if (package.Data is IdRequest id_request) // If it's a request for the id (first package exchanged when client logged in)
                {
                    Header header = package.Header;
                    header.Id = _clientId;
                    package.Header = header;

                    id_request.Id = _clientId; // Set the client ID in the package 
                    Debug.LogWarning("Debug ID : " + id_request.Id);
                    new_package = Package.CreatePackage(header, id_request);
                }
                else if (package.Data is ChessInfoGameData chess_info_game_data) // If client ask for the game info
                {
                    chess_info_game_data.BlackPlayerId = BlackPlayerID;
                    chess_info_game_data.WhitePlayerId = WhitePlayerID;

                    chess_info_game_data.WhitePlayerPseudo = WhitePlayerNickname;
                    chess_info_game_data.BlackPlayerPseudo = BlackPlayerNickname;

                    new_package = Package.CreatePackage(package.Header, chess_info_game_data);
                }
                else
                {
                    new_package = package;  
                }

                SendDataToClient(_clientId, DataSerialize.SerializeToBytes(new_package)); // Send it back
                Debug.Log("Only client");
                break;
            case SendMethod.ALL_CLIENTS: // If the package is for everyone
                Debug.Log("AllClient");
                SendDataToAllClients(_data); // Send it to everyone
                break;
            case SendMethod.ONLY_SERVER: // If it's for the server 
                Debug.Log("ONLY_SERVER");
                if (package.Data is TeamRequest team_request) // Team request is a special resquest sent when a client ask to join a team (black or white)
                {
                    HandleTeamRequest(package, team_request, _clientId, _data); // Handle leaving or joining a team
                } else if (package.Data is ChessManagerRequest chess_manager_request) // If the host ask to start the game
                {
                    if(WhitePlayerID != Guid.Empty && BlackPlayerID != Guid.Empty) 
                    {
                        SendDataToAllClients(_data); // Send to everyone that the game has started
                    }
                }
                else if (package.Data is RoomInfoData room_info) // If the client ask for the black team player's name or white team player's name
                {
                    room_info.BlackPlayerNickname = BlackPlayerNickname;
                    room_info.WhitePlayerNickname = WhitePlayerNickname;

                    _data = DataSerialize.SerializeToBytes(package); 

                    SendDataToClient(_clientId, _data); // Send it back to the client that was asking
                }
                break;
            case SendMethod.ONLY_SPECTATORS: // If it's for the spectators
                Debug.Log("Spectator");
                break;
            default: 
                Debug.Log("None");
                break;
        }
    }

    private void HandleTeamRequest(Package _package, TeamRequest team_request, Guid _clientId, byte[] _data) // Handle leaving or joining a team
    {
        if (team_request.RequestJoinOrLeave == JoinOrLeave.JOIN) // If the client ask to join a team
        {
            if (team_request.RequestTeam == Teams.TEAM_WHITE && WhitePlayerID == Guid.Empty && BlackPlayerID != _clientId)
            {
                WhitePlayerID = _clientId; // Save the white player's ID
                team_request.PlayerID = WhitePlayerID; // Send his ID 
                WhitePlayerNickname = _package.Header.Pseudo; // Send his nickname

                _data = DataSerialize.SerializeToBytes(_package);
                SendDataToAllClients(_data); // Tell everyone that this player join white team
            }
            else if (team_request.RequestTeam == Teams.TEAM_BLACK && BlackPlayerID == Guid.Empty && WhitePlayerID != _clientId)
            {
                BlackPlayerID = _clientId;
                team_request.PlayerID = BlackPlayerID;
                BlackPlayerNickname = _package.Header.Pseudo;
                _data = DataSerialize.SerializeToBytes(_package);
                SendDataToAllClients(_data);
            }
        }
        else if (team_request.RequestJoinOrLeave == JoinOrLeave.LEAVE) // If the client ask to leave a team
        {
            if (team_request.RequestTeam == Teams.TEAM_WHITE && WhitePlayerID == _clientId)
            {
                WhitePlayerID = Guid.Empty; // Reset the white player's ID
                WhitePlayerNickname = "";
                team_request.PlayerID = WhitePlayerID;
                
                _data = DataSerialize.SerializeToBytes(_package);
                SendDataToAllClients(_data);
            }
            else if (team_request.RequestTeam == Teams.TEAM_BLACK && BlackPlayerID == _clientId)
            {
                BlackPlayerID = Guid.Empty;
                BlackPlayerNickname = "";
                team_request.PlayerID = BlackPlayerID;
                _data = DataSerialize.SerializeToBytes(_package);
                SendDataToAllClients(_data);
            }
        }
    }
    #endregion

    #region Basic Message
    public void SendMessageToClient(Guid clientId, string message)
    {
        if (clients.ContainsKey(clientId))
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);
            clients[clientId].Stream.Write(msg, 0, msg.Length);
        }
    }

    public void SendDataToClient(Guid _clientId, byte[] _data)
    {
        if (clients.ContainsKey(_clientId))
        {
            var client = clients[_clientId];
            if (client.Stream != null && client.Stream.CanWrite)
            {
                try
                {
                    client.Stream.Write(_data, 0, _data.Length);
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.LogError($"Erreur : Le flux pour le client {_clientId} est ferm�.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Erreur d'�criture : {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Le flux du client {_clientId} est d�j� ferm� ou inaccessible.");
            }
        }
    }

    public void SendDataToAllClients(byte[] _data)
    {
        foreach (var client in clients.Values)
        {
            SendDataToClient(client.Id, _data);
        }
    }

    #endregion

    public class ServerAction 
    {
        static Package package = new Package(new Header(Id, Name, DateTime.Now, SendMethod.ALL_CLIENTS));

        public static byte[] Log(string _message) // Used to send chat messages from server
        {
            ChatMessage chat_message = new ChatMessage(_message, SerializableColor.Red, DataKey.ACTION_CHAT);
            package.Data = chat_message;

            return DataSerialize.SerializeToBytes(package);
        }

        public static byte[] DoAction(DataKey data_key) // Used to order clients to do things (leave, start game, etc)
        {
            SimpleAction simple_action = new SimpleAction(data_key);
            package.Data = simple_action;

            return DataSerialize.SerializeToBytes(package);
        }
    }
}

[Serializable]
public class SimpleAction : Data // Used to order clients to do simple things (call actions without args)
{
    public SimpleAction(DataKey _actionDataKey) : base(_actionDataKey)
    {

    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action>(ActionDataKey)?.Invoke();
    }

    public SimpleAction(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {

    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
    }
}

public class ClientInfo // Struct to save clients data
{
    public Guid Id { get; set; }

    public string pseudo { get; set; }
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public string ConnectionTimestamp { get; set; }
}
