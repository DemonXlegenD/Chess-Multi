using System;
using UnityEngine;

[Serializable]
public class Typing : MonoBehaviour
{
    [SerializeField] private Chat chat;
    [SerializeField] private Client client;

    void Start()
    {

    }

    private void Update()
    {

    }

    public void SendMessageToChat(string _content)
    {
        if (_content != string.Empty)
        {
            client = FindAnyObjectByType<Client>();
            if (client != null)
            {
                Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

                ChatMessage chat_message = new ChatMessage(_content, DataKey.ACTION_CHAT);

                Package package = new Package(header, chat_message);


                client.SendDataToServer(DataSerialize.SerializeToBytes(package));
            }
        }

    }
}
