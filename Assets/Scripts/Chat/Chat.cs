using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Serializable]
public class Chat : MonoBehaviour
{
    private int current = 0;
    [SerializeField] private RectTransform _content;
    public List<string> chat = new List<string>();
    [SerializeField] private TextMeshProUGUI TextMeshPro;
    [SerializeField] private RectTransform scrollRectTransform;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private BlackBoard ActionBlackBoard;

    public Action<string> action;
    
    public void AddMessage(string message)
    {
        Debug.Log(message);
        chat.Add(message);
 
        TextMeshPro.text += $"{message}\n";

        Vector2 sizeDelta = contentRectTransform.sizeDelta;
        sizeDelta.y = TextMeshPro.preferredHeight;
        contentRectTransform.sizeDelta = sizeDelta;
    }

    // Start is called before the first frame update
    void Start()
    {
        action = AddMessage;
        ActionBlackBoard.AddData<Action<string>>(DataKey.ACTION_CHAT, action);
    }

    // Update is called once per frame
    void Update()
    {
        if (current < chat.Count)
        {
            current++;
            TextMeshPro.ForceMeshUpdate();
            contentRectTransform.ForceUpdateRectTransforms();
            scrollRectTransform.ForceUpdateRectTransforms();
        }
    }
}
