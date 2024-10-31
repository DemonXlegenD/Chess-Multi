using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;


public class Client : MonoBehaviour
{
    private string IpServer = string.Empty;
    private uint id = 0;
    public uint Id { get { return id; } set { id = value; } }

    [SerializeField] public string Pseudo = "JEAN";
    [SerializeField] public int serverPort = 4269;
    [SerializeField] public int WaitBeforeStarting = 5;
    [SerializeField] BlackBoard Data;

    public string messageToSend = "Hello Server!";

    [SerializeField] private BlackBoard ActionBlackBoard;

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;
    private bool firstId = false;

    private void Start()
    {
        ActionBlackBoard.AddData<Action<uint>>(DataKey.ACTION_SET_ID, SetId);
    }

    void Update()
    {
    }

    public void SetId(uint id)
    {
        Debug.Log(id);
        this.id = id;
    }

    public void SetClientIP(string ip_)
    {
        IpServer = ip_;
        Data.SetData(DataKey.SERVER_IP, IpServer);
    }

    public void SendPackageId()
    {

        Header header = new Header(id, Pseudo, DateTime.Now, SendMethod.ONLY_CLIENT);

        IdRequest idRequest = new IdRequest(DataKey.ACTION_SET_ID);

        Package package = new Package(header, idRequest);

        Debug.Log(package);

        SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    public bool ConnectToServer()
    {
        try
        {
            client = new TcpClient(IpServer, serverPort);
            stream = client.GetStream();
            SendPackageId();

            clientReceiveThread = new Thread(new ThreadStart(ListenForData))
            {
                IsBackground = true
            };
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
            Package package = DataSerialize.DeserializeFromBytes<Package>(_data);

            package.Data.CallAction(ActionBlackBoard, (IPlayerPseudo)package.Header, (ITimestamp)package.Header);
        }
        catch
        {
            Debug.Log("ID");
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

[Serializable]
public class IdRequest : Data
{
    public uint Id = 0;
    public IdRequest(DataKey _actionDataKey) : base(_actionDataKey) { }

    public IdRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.Id = (uint)_info.GetValue("Id", typeof(uint));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
        _info.AddValue("Id", Id);
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<uint>>(ActionDataKey)?.Invoke(Id);
    }
}