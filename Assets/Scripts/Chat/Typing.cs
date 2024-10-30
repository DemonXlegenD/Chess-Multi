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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Type");
            SendMessageToChat("Jean", "Salut Monsieur", SendTo.ALL_CLIENTS);
        }*/
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
