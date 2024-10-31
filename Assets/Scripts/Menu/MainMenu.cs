using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] TMPro.TMP_InputField NickName;
    [SerializeField] TMPro.TMP_InputField ConnectToIP;
    [SerializeField] TMPro.TextMeshProUGUI IP;

    private RectTransform currentMenu;
    private Server server = null;
    private Client client = null;
    Vector3 on = new Vector3(1, 1);
    Vector3 off = new Vector3(0, 0);

    void Start()
    {
        currentMenu = MainMenuStart;
        currentMenu.localScale = on;
        
        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = off;
        MainMenuRoom.localScale = off;
        Chat.localScale = on;
        InGamePanel.localScale = off;
        Data.AddData<string>(DataKey.PLAYER_NICKNAME, NickName.text);
        Data.AddData<string>(DataKey.SERVER_IP, "0");
        Data.AddData<bool>(DataKey.IS_HOST, false);
    }

    public void CreateRoom() 
    {
        server = Instantiate(ServerPrefab);
        Invoke("HostConnection", 1);
    }

    void UpdateIP()
    {
        Debug.Log(Data);
        IP.text = Data.GetValue<string>(DataKey.SERVER_IP);
    }

    public void JoinRoom() 
    {
        Data.SetData(DataKey.IS_HOST, false);

        ChangeMenu(MainMenuJoinRoom);
    }

    public void LeaveRoom() 
    {
        Destroy(client.gameObject);

        if (server != null)
        {
            server.QuitServer();
            Destroy(server.gameObject);
        }

        ChangeMenu(MainMenuStart);
    }

    public void StartGame() 
    {
        ChangeMenu(InGamePanel);
    }

    public void TryToConnect() 
    {
        ChangeMenu(MainMenuConnectionFail);

        ProcessConnectClient(ConnectToIP.text);    
    }

    public void HostConnection() 
    {
        ProcessConnectClient(Data.GetValue<string>(DataKey.SERVER_IP));
    }
    public void ProcessConnectClient(string ip) 
    {
        client = Instantiate(ClientPrefab);
        string ip_address = ip;
        client.SetClientIP(ip_address); // Remove the last character '\0'

        bool connectionSuccess = client.ConnectToServer();

        if (connectionSuccess)
        {
            SucceedToConnect(); 
        } else {
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

        ChangeMenu(MainMenuConnectionFail);
    }

    public void SucceedToConnect() 
    {
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.text);
        Invoke("UpdateIP", 1);

        ChangeMenu(MainMenuRoom);
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
