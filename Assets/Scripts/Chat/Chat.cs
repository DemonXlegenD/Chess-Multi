using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour
{
    private int current = -1;
    [SerializeField] private RectTransform _content;
    public List<string> chat = new List<string>();
    private List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();  
    [SerializeField] private TextMeshProUGUI TextMeshProPrefab;

    public void AddMessage(string message)
    {
        chat.Add(message);
        current++;
       // TextMeshProUGUI textMeshPro = Instantiate<TextMeshProUGUI>(TextMeshProPrefab, _content);
        //textMeshPro.text = $"[{hourMinuteSecond}] : {message}";
        //texts.Add(textMeshPro);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (texts.Count == current)
        {
            TextMeshProUGUI textMeshPro = Instantiate<TextMeshProUGUI>(TextMeshProPrefab, _content);
            string hourMinuteSecond = DateTime.Now.ToString("HH:mm:ss");
            textMeshPro.text = $"[{hourMinuteSecond}] : {chat[current]}";
            texts.Add(textMeshPro);

        }
    }
}
