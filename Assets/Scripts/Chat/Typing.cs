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
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Type");
            SendMessageToChat("Jean", "Salut Monsieur", SendTo.ALL_CLIENTS);
        }
    }

    public void SendMessageToChat(string pseudo, string contenu, SendTo _sendTo)
    {
        StructMessageChat chat_message = new StructMessageChat(pseudo, DateTime.Now, contenu, _sendTo, DataKey.ACTION_CHAT);

        client.SendDataToServer(DataSerialize.SerializeToBytes(chat_message));
    }
}
