using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;


public class Client : MonoBehaviour
{
    private string IpServer = string.Empty;
    private Guid id;
    public Guid Id { get { return id; } set { id = value; } }

    [SerializeField] public string Pseudo = "JEAN";
    [SerializeField] public int serverPort = 4269;
    [SerializeField] public int WaitBeforeStarting = 5;
    [SerializeField] BlackBoard Data;

    public string messageToSend = "Hello Server!";

    [SerializeField] private BlackBoard ActionBlackBoard;

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;
    private bool isListening = false;

    #region MonoBehaviours

    private void Start()
    {
        CreateData();
    }

    private void Update()
    {
        if(id == Guid.Empty)
        {
            SendPackageId();
        }
    }
    private void OnDestroy()
    {
        ClearData();
    }

    private void OnApplicationQuit()
    {
        CloseClient();
    }

    #endregion

    #region Setter

    public void SetId(Guid id)
    {
        this.Id = id;
    }

    public void SetClientIP(string ip_)
    {
        IpServer = ip_;
        Data.SetData(DataKey.SERVER_IP, IpServer);
    }

    #endregion

    #region Blackboard Data

    private void CreateData()
    {
        Data.AddData<Client>(DataKey.CLIENT, this);
        ActionBlackBoard.AddData<Action<Guid>>(DataKey.ACTION_SET_ID, SetId);
    }

    private void ClearData()
    {
        Data.ClearData(DataKey.CLIENT);
        ActionBlackBoard.ClearData(DataKey.ACTION_SET_ID);
    }

    #endregion

    #region Manager Client

    public void StartRunningClient()
    {
        clientReceiveThread = new Thread(ListenForData);
        clientReceiveThread.Start();
    }

    public void CloseClient()
    {
        isListening = false;

        if (stream != null)
        {
            stream.Close();
            stream = null;
        }

        if (client != null)
        {
            client.Close();
            client = null;
        }

        Debug.LogWarning("Client connection closed.");
    }

    #endregion

    #region Client Method

    public bool ConnectToServer()
    {
        try
        {
            client = new TcpClient(IpServer, serverPort);
            stream = client.GetStream();
            SendPackageId();
            isListening = true;

            StartRunningClient();

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
        byte[] buffer = new byte[1024];

        try
        {
            while (isListening && stream.CanRead)
            {
                if (stream.DataAvailable)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] incomingData = new byte[bytesRead];
                        Array.Copy(buffer, 0, incomingData, 0, bytesRead);

                        DataProcessing(incomingData);

                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("Server message received: " + serverMessage);
                    }
                    else
                    {
                        Debug.LogWarning("Server closed the connection.");
                        break;
                    }
                }
            }
        }
        catch (IOException ioException)
        {
            Debug.LogWarning("IOException during data reception: " + ioException.Message);
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException: " + socketException.Message);
        }
        finally
        {
            CloseClient();
        }
    }

    #endregion

    #region Data

    public void SendPackageId()
    {
        Header header = new Header(id, Pseudo, DateTime.Now, SendMethod.ONLY_CLIENT);
        IdRequest idRequest = new IdRequest(DataKey.ACTION_SET_ID);

        Package package = new Package(header, idRequest);

        SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    public void DataProcessing(byte[] _data)
    {
        try
        {
            Debug.Log("PROCESSING");
            Package package = DataSerialize.DeserializeFromBytes<Package>(_data);

            package.Data.CallAction(ActionBlackBoard, (IPlayerPseudo)package.Header, (ITimestamp)package.Header);
        }
        catch
        {
            Debug.LogWarning("Fail Process Data");
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

    #endregion
}

[Serializable]
public class IdRequest : Data
{
    public Guid Id;
    public IdRequest(DataKey _actionDataKey) : base(_actionDataKey) { }

    public IdRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.Id = (Guid)_info.GetValue("Id", typeof(Guid));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
        _info.AddValue("Id", Id);
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<Guid>>(ActionDataKey)?.Invoke(Id);
    }
}