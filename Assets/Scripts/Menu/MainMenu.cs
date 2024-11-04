using System;
using System.Runtime.Serialization;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] RectTransform MainMenuStart;
    [SerializeField] RectTransform MainMenuJoinRoom;
    [SerializeField] RectTransform MainMenuWaitForConnection;
    [SerializeField] RectTransform MainMenuConnectionFail;
    [SerializeField] RectTransform MainMenuRoom;
    [SerializeField] RectTransform Chat;
    [SerializeField] RectTransform InGamePanel;
    [SerializeField] Server ServerPrefab;
    [SerializeField] Client ClientPrefab;
    [SerializeField] BlackBoard Data;
    [SerializeField] BlackBoard ActionBlackBoard;
    [SerializeField] TMPro.TMP_InputField NickName;
    [SerializeField] TMPro.TMP_InputField ConnectToIP;
    [SerializeField] TMPro.TextMeshProUGUI IP;

    private RectTransform currentMenu;
    private Server server = null;
    private Client client = null;
    Vector3 on = new Vector3(1, 1);
    Vector3 off = new Vector3(0, 0);

    bool wantGamePanel = false;

    void Start()
    {
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_START_GAME_BY_HOST, StartGameAskByHost);
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_LEAVE_ROOM, LeaveRoom);

        currentMenu = MainMenuStart;
        currentMenu.localScale = on;

        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = off;
        MainMenuRoom.localScale = off;
        Chat.localScale = off;
        InGamePanel.localScale = off;

        Data.AddData<string>(DataKey.PLAYER_NICKNAME, NickName.text);
        Data.AddData<string>(DataKey.SERVER_IP, "0");
        Data.AddData<bool>(DataKey.IS_HOST, false);
    }

    private void Update()
    {
        if(currentMenu != InGamePanel && wantGamePanel)
        {
            ChangeMenu(InGamePanel);
            wantGamePanel = false;
        }
    }

    public void CreateRoom()
    {
        server = Instantiate(ServerPrefab);
        Invoke("HostConnection", 1);
    }

    public void JoinRoom()
    {
        Data.SetData(DataKey.IS_HOST, false);

        ChangeMenu(MainMenuJoinRoom);
    }

    public void LeaveRoom()
    {
        if (Data.GetValue<bool>(DataKey.IS_HOST) && server != null)
        {
            server.StopServer();
        }

        ChangeMenu(MainMenuStart);
        Chat.localScale = off;
    }

    public void StartGame()
    {
        if (Data.GetValue<bool>(DataKey.IS_HOST))
        {
            Client current_client = Data.GetValue<Client>(DataKey.CLIENT);


            Header header = new Header(current_client.Id, current_client.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

            ChessManagerRequest request = new ChessManagerRequest(DataKey.ACTION_START_GAME_BY_HOST);

            Package package = Package.CreatePackage(header, request);

            current_client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }
    }


    public void StartGameAskByHost()
    {
        Debug.Log("Start Game!!!!");
        Data.GetValue<GameManager>(DataKey.GAME_MANAGER).OnStartGame();
        wantGamePanel = true;
    }



    public void TryToConnect()
    {
        ChangeMenu(MainMenuConnectionFail);

        ProcessConnectClient(ConnectToIP.text);
    }

    public void HostConnection()
    {
        Data.SetData(DataKey.IS_HOST, true);
        ProcessConnectClient(Data.GetValue<string>(DataKey.SERVER_IP));
    }

    public void ProcessConnectClient(string _ip)
    {
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.text);
        client = Instantiate(ClientPrefab);
        client.SetClientIP(_ip);
        client.Pseudo = NickName.text;

        bool connectionSuccess = client.ConnectToServer();

        if (connectionSuccess)
        {
            SucceedToConnect();
        }
        else
        {
            FailToConnect();
        }
    }

    public bool SetUpClientConnection()
    {
        client = Instantiate(ClientPrefab);
        string ip_address = Data.GetValue<string>(DataKey.SERVER_IP);

        if (ip_address == "0")
        {
            ip_address = ConnectToIP.text;
        }
        client.SetClientIP(ip_address); // Remove the last character '\0'

        return client.ConnectToServer();
    }

    public void FailToConnect()
    {
        Data.SetData(DataKey.IS_HOST, false);
        client.CloseClient();
        Destroy(client.gameObject);

        ChangeMenu(MainMenuConnectionFail);
    }

    public void SucceedToConnect()
    {
        Invoke("UpdateIP", 1);

        ChangeMenu(MainMenuRoom);
        Chat.localScale = on;
    }

    void UpdateIP()
    {
        Debug.Log(Data);
        IP.text = Data.GetValue<string>(DataKey.SERVER_IP);
    }

    public void BackToMenuBecauseDoNotWantToJoinRoom()
    {
        ChangeMenu(MainMenuStart);
    }

    public void BackToMenuAfterFailToConnect()
    {
        ChangeMenu(MainMenuStart);
    }

    public void ChangeMenu(RectTransform menuToDisplay)
    {
        currentMenu.localScale = off;
        currentMenu = menuToDisplay;
        currentMenu.localScale = on;
    }
}

[Serializable]
public class ChessManagerRequest : Data
{
    public ChessManagerRequest(DataKey _actionDataKey) : base(_actionDataKey)
    {

    }
    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action>(ActionDataKey)?.Invoke();
    }



    public ChessManagerRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {

    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
    }
}