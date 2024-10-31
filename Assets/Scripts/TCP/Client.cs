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

    private void Start()
    {
        ActionBlackBoard.AddData<Action<Guid>>(DataKey.ACTION_SET_ID, SetId);
    }

    void Update()
    {
    }

    public void SetId(Guid id)
    {
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
            isListening = true;

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
            while (isListening && stream.CanRead)
            {
                if (stream.DataAvailable)
                {
                    int length;
                    try
                    {
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);

                            DataProcessing(incomingData);

                            string serverMessage = Encoding.UTF8.GetString(incomingData);
                            Debug.Log("Server message received: " + serverMessage);
                        }
                    }
                    catch (IOException ioException)
                    {
                        Debug.LogWarning("Stream read interrupted: " + ioException.Message);
                        break; // Sortir de la boucle si le stream est interrompu
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
        finally
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }

    public void DataProcessing(byte[] _data)
    {
        try
        {
            Debug.Log("PROCESSING");
            Package package = DataSerialize.DeserializeFromBytes<Package>(_data);

            package.Data.CallAction(ActionBlackBoard, package.Header, package.Header);
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

    public void StopListening()
    {
        isListening = false;
        //clientReceiveThread?.Join();
    }

    public void QuitClient()
    {
        StopListening();
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
    }

    void OnApplicationQuit()
    {
        StopListening();
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