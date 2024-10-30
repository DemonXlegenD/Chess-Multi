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
    [SerializeField] TMPro.TMP_InputField NickName;
    [SerializeField] TMPro.TMP_InputField ConnectToIP;
    [SerializeField] TMPro.TextMeshProUGUI IP;

    private Server server = null;
    private Client client = null;
    Vector3 on = new Vector3(1, 1);
    Vector3 off = new Vector3(0, 0);

    void Start()
    {
        MainMenuStart.localScale = on;
        MainMenuJoinRoom.localScale = off;
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = off;
        MainMenuRoom.localScale = off;
        Chat.localScale = on;
        InGamePanel.localScale = off;
        Data.AddData<string>(DataKey.PLAYER_NICKNAME, NickName.text);
        Data.AddData<string>(DataKey.SERVER_IP, "0");
    }

    public void CreateRoom()
    {
        server = Instantiate(ServerPrefab);
        SucceedToConnect();
    }

    void UpdateIP()
    {
        Debug.Log(Data);
        IP.text = Data.GetValue<string>(DataKey.SERVER_IP);
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
            server.QuitServer();
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
        string ip_address = ConnectToIP.text;
        client.SetClientIP(ip_address); // Remove the last character '\0'

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
        MainMenuWaitForConnection.localScale = off;
        MainMenuConnectionFail.localScale = on;
    }

    public void SucceedToConnect()
    {
        Data.SetData(DataKey.PLAYER_NICKNAME, NickName.text);
        Invoke("UpdateIP", 1);

        MainMenuStart.localScale = off;
        MainMenuWaitForConnection.localScale = off;
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
