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
    [SerializeField] BlackBoard Data;
    [SerializeField] GameObject NickName;
    [SerializeField] GameObject IP;

    private GameObject server = null;
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
    }

    public void CreateRoom() 
    {
        MainMenuStart.localScale = off;
        MainMenuRoom.localScale = on;
        
        server = Instantiate(ServerPrefab);

        Data.AddData<GameObject>(DataKey.SERVER, server);
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.GetComponent<TMPro.TextMeshProUGUI>().text);

        IP.GetComponent<TMPro.TextMeshProUGUI>().text = Data.GetValue<GameObject>(DataKey.SERVER).GetComponent<Server>().IpV4;
        
        Debug.Log(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
        Debug.Log(Data.GetValue<GameObject>(DataKey.SERVER).GetComponent<Server>().IpV4);
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
    }

    public void FailToConnect() 
    {
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = on;
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
