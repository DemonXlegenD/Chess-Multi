using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
public class Chat : MonoBehaviour
{
    private int current = 0;
    public List<string> chat = new List<string>();
    private CanvasRenderer canvas;
    [SerializeField] private TextMeshProUGUI TextMeshPro;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private BlackBoard ActionBlackBoard;

    public Action<string> action;
    
    public void AddMessage(string message)
    {
        Debug.Log(message);
        chat.Add(message);
 
        TextMeshPro.text += $"{message}\n";
    }

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<CanvasRenderer>();
        scrollRect = GetComponentInChildren<ScrollRect>();
        action = AddMessage;
        ActionBlackBoard.AddData<Action<string>>(DataKey.ACTION_CHAT, action);
    }

    // Update is called once per frame
    void Update()
    {
        if (current < chat.Count)
        {
            current++;
            Vector2 sizeDelta = contentRectTransform.sizeDelta;
            sizeDelta.y = TextMeshPro.preferredHeight;
            contentRectTransform.sizeDelta += sizeDelta;
            TextMeshPro.ForceMeshUpdate();
            contentRectTransform.ForceUpdateRectTransforms();
            
            if (scrollRect.verticalScrollbar.gameObject.activeSelf) {
                scrollRect.GraphicUpdateComplete();
                scrollRect.normalizedPosition = new Vector2(0, 0);
                scrollRect.GraphicUpdateComplete();
            }
        }
    }
}
