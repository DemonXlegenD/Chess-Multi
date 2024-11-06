using System;
using System.Runtime.Serialization;
using UnityEngine;

/*
 * This class handle every change in the UI
 */

public class MainMenu : MonoBehaviour
{
    [SerializeField] RectTransform MainMenuStart;
    [SerializeField] RectTransform MainMenuJoinRoom;
    [SerializeField] RectTransform MainMenuEndGame;
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

    private TeamHandler teamHandler;

    private RectTransform toChangePanel; // Next panel to display
    private RectTransform currentMenu;
    
    private Server server = null;
    private Client client = null;

    Vector3 on = new Vector3(1, 1); // Display panel
    Vector3 off = new Vector3(0, 0); // Do not display panel

    bool wantGamePanel = false;
    bool needChangePanel = false;

    #region MonoBehaviors

    void Start()
    {
        teamHandler = GetComponent<TeamHandler>();

        currentMenu = MainMenuStart;
        currentMenu.localScale = on;

        MainMenuEndGame.localScale = off;
        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = off;
        MainMenuRoom.localScale = off;
        Chat.localScale = off;
        InGamePanel.localScale = off;

        CreateData();
    }

    private void Update()
    {
        // To avoid threading error whith UI :
        if(currentMenu != InGamePanel && wantGamePanel) // Displaying menu actualisation
        {
            ChangeMenu(InGamePanel);
            wantGamePanel = false;
        }

        if(toChangePanel != currentMenu && needChangePanel) // Displaying menu actualisation
        {
            Debug.LogWarning("Leave the room");
            LeaveRoom();
            needChangePanel = false;
        }
    }

    private void OnDestroy()
    {
        ClearData(); // Clear blackboard's data
    }

    #endregion

    #region Blackboard Data

    public void CreateData()
    {
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_START_GAME_BY_HOST, StartGameAskByHost); // Set action to start the game
        ActionBlackBoard.AddData<Action<string, string>>(DataKey.ACTION_ROOM_INFO, SetRoomInfo); // Set action to get teams player's ID

        Data.AddData<string>(DataKey.PLAYER_NICKNAME, NickName.text);
        Data.AddData<string>(DataKey.SERVER_IP, "0");
        Data.AddData<bool>(DataKey.IS_HOST, false);
    }

    public void ClearData()
    {
        ActionBlackBoard.ClearData(DataKey.ACTION_START_GAME_BY_HOST);
        ActionBlackBoard.ClearData(DataKey.ACTION_LEAVE_ROOM);
        ActionBlackBoard.ClearData(DataKey.ACTION_ROOM_INFO);

        Data.ClearData(DataKey.PLAYER_NICKNAME);
        Data.ClearData(DataKey.SERVER_IP);
        Data.ClearData(DataKey.IS_HOST);
    }

    #endregion

    #region Panel

    public void ChangeMenu(RectTransform menuToDisplay)
    {
        currentMenu.localScale = off;
        currentMenu = menuToDisplay;
        currentMenu.localScale = on;
    }

    public void ShowEndGame()
    {
        ChangeMenu(MainMenuEndGame);
    }

    public void ShowInGame()
    {
        ChangeMenu(InGamePanel);
    }

    public void BackToMenuBecauseDoNotWantToJoinRoom()
    {
        ChangeMenu(MainMenuStart);
    }

    public void BackToMenuAfterFailToConnect()
    {
        ChangeMenu(MainMenuStart);
    }

    #endregion

    #region Room Service

    #region Actions
    public void CreateRoom()
    {
        server = Instantiate(ServerPrefab); // Become host
        Invoke("HostConnection", 0.1f);
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
            server.StopServer(); // Stop server if is host
            if (server) Destroy(server.gameObject);
        }
        
        if(client) Destroy(client.gameObject);

        ChangeMenu(MainMenuStart);
        Chat.localScale = off;
    }

    public void StartGame() // Send a package to the server that ask him if we can start the game (and start it if we can)
    {
        if (Data.GetValue<bool>(DataKey.IS_HOST))
        {
            Client current_client = Data.GetValue<Client>(DataKey.CLIENT);

            Header header = new Header(current_client.Id, current_client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            ChessManagerRequest request = new ChessManagerRequest(DataKey.ACTION_START_GAME_BY_HOST); 

            Package package = Package.CreatePackage(header, request);

            current_client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }
    }

    #endregion

    #region Connections

    public void TryToConnect()
    {
        ChangeMenu(MainMenuWaitForConnection);
        ProcessConnectClient(ConnectToIP.text);
    }

    public void HostConnection()
    {
        Data.SetData(DataKey.IS_HOST, true);
        ProcessConnectClient(Data.GetValue<string>(DataKey.SERVER_IP));
    }

    public void ProcessConnectClient(string _ip) // Instanciate client and try to connect
    {
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.text);
        client = Instantiate(ClientPrefab);
        client.SetClientIP(_ip);
        string name = NickName.text == string.Empty ? "Player" : NickName.text;
        client.Pseudo = name;

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

    public void FailToConnect()
    {
        Data.SetData(DataKey.IS_HOST, false);
        client.CloseClient();
        Destroy(client.gameObject);

        ChangeMenu(MainMenuConnectionFail);
    }

    public void SucceedToConnect()
    {
        Invoke("UpdateIP", 0.1f);
        teamHandler.ResetTeams();
        ChangeMenu(MainMenuRoom);
        Chat.localScale = on;
        Invoke("AskRoomInfo", 0.1f);
    }

    #endregion

    #region Info

    private void AskRoomInfo()
    {
        if (!Data.GetValue<bool>(DataKey.IS_HOST))
        {
            Client current_client = Data.GetValue<Client>(DataKey.CLIENT);
            Header header = new Header(current_client.Id, current_client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);
            RoomInfoData data = new RoomInfoData(DataKey.ACTION_ROOM_INFO);

            Package package = Package.CreatePackage(header, data);

            current_client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }
    }
    private void SetRoomInfo(string white_player_nickname, string black_player_nickname)
    {
        teamHandler.JoinWhite(white_player_nickname);
        teamHandler.JoinBlack(black_player_nickname);
    }

    public void AskForLeaving()
    {
        needChangePanel = true;
        toChangePanel = MainMenuStart;
    }

    public void StartGameAskByHost()
    {
        Data.GetValue<GameManager>(DataKey.GAME_MANAGER).OnStartGame();
        wantGamePanel = true;
    }

    void UpdateIP()
    {
        IP.text = Data.GetValue<string>(DataKey.SERVER_IP);
    }

    #endregion

    #endregion
}

[Serializable]
public class RoomInfoData : Data
{
    public string WhitePlayerNickname = "";

    public string BlackPlayerNickname = "";

    public RoomInfoData(DataKey _actionDataKey) : base(_actionDataKey)
    {

    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<string, string>>(ActionDataKey)?.Invoke(WhitePlayerNickname, BlackPlayerNickname);
    }

    public RoomInfoData(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        WhitePlayerNickname = (string)_info.GetValue("WhitePlayerNickname", typeof(string));
        BlackPlayerNickname = (string)_info.GetValue("BlackPlayerNickname", typeof(string));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);

        _info.AddValue("WhitePlayerNickname", WhitePlayerNickname);
        _info.AddValue("BlackPlayerNickname", BlackPlayerNickname);
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