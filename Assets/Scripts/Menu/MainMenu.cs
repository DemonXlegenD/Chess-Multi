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
    [SerializeField] GameObject ServerPrefab;
    [SerializeField] GameObject ClientPrefab;
    [SerializeField] BlackBoard Data;
    [SerializeField] GameObject NickName;
    [SerializeField] GameObject ConnectToIP;
    [SerializeField] GameObject IP;

    private GameObject server = null;
    private GameObject client = null;
    Vector3 on = new Vector3(1, 1);
    Vector3 off = new Vector3(0, 0);

    void Start()
    {
        MainMenuStart.localScale = on;
        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = off;
        MainMenuRoom.localScale = off;
        Chat.localScale = off;
        InGamePanel.localScale = off;
        Data.AddData<string>(DataKey.PLAYER_NICKNAME, NickName.GetComponent<TMPro.TextMeshProUGUI>().text);
        Data.AddData<string>(DataKey.SERVER_IP, "0");
    }

    public void CreateRoom() 
    {
        server = Instantiate(ServerPrefab);
        SucceedToConnect(); 
    }

    void UpdateIP()
    {
        IP.GetComponent<TMPro.TextMeshProUGUI>().text = Data.GetValue<string>(DataKey.SERVER_IP);
    }

    public void JoinRoom() 
    {
        MainMenuStart.localScale = off;
        MainMenuJoinRoom.localScale = on;
    }

    public void LeaveRoom() 
    {
        if (server != null) 
        {
            server.GetComponent<Server>().QuitServer();
            Destroy(server);
        }
        MainMenuRoom.localScale = off;
        MainMenuStart.localScale = on;
    }

    public void StartGame() 
    {
        MainMenuRoom.localScale = off;
        InGamePanel.localScale = on;
    }

    public void TryToConnect() 
    {
        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = on;
        client = Instantiate(ClientPrefab);
        client.GetComponent<Client>().SetClientIP(ConnectToIP.GetComponent<TMPro.TextMeshProUGUI>().text);
        bool connectionSuccess = client.GetComponent<Client>().ConnectToServer();
        if (connectionSuccess) 
        {
            SucceedToConnect(); 
        } else {
            FailToConnect();
        }
    }

    public void FailToConnect() 
    {
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = on;
    }

    public void SucceedToConnect() 
    {
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.GetComponent<TMPro.TextMeshProUGUI>().text);
        Invoke("UpdateIP", 1);

        MainMenuStart.localScale = off;
        MainMenuRoom.localScale = on;
    }
    public void BackToMenuBecauseDoNotWantToJoinRoom() 
    {
        MainMenuJoinRoom.localScale = off;
        MainMenuStart.localScale = on;
    }

    public void BackToMenuAfterFailToConnect() 
    {
        MainMenuConnectionFail.localScale = off;
        MainMenuStart.localScale = on;
    }
}
