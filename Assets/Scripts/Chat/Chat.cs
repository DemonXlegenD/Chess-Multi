using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Serializable]
public class Chat : MonoBehaviour
{
    private int current = -1;
    [SerializeField] private RectTransform _content;
    public List<string> chat = new List<string>();
    [SerializeField] private TextMeshProUGUI TextMeshPro;

    public Action<string> action;
    
    public void AddMessage(string message)
    {
        chat.Add(message);
        current++;
        TextMeshPro.text = TextMeshPro.text + "\n" +message;

    }

    // Start is called before the first frame update
    void Start()
    {
        action = AddMessage;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
