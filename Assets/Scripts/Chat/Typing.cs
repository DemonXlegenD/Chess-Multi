using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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

    public void SendMessageToChat(string contenu)
    {
        client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            StructMessageChat chat_message = new StructMessageChat(client.Pseudo, DateTime.Now, contenu, SendTo.ALL_CLIENTS, DataKey.ACTION_CHAT);

            client.SendDataToServer(DataSerialize.SerializeToBytes(chat_message));
        }
    }
}
