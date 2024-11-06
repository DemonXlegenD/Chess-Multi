using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Chat : MonoBehaviour
{
    int current = 0;
    public List<string> chat = new List<string>();
    private CanvasRenderer canvas;
    [SerializeField] private TextMeshProUGUI TextMeshPro;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private BlackBoard ActionBlackBoard;

    #region MonoBehaviors
    void Start()
    {
        canvas = GetComponent<CanvasRenderer>();
        scrollRect = GetComponentInChildren<ScrollRect>();
        CreateData();
    }

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

    private void OnDestroy()
    {
        ClearData();
    }

    #endregion

    #region Blackboard Data

    private void CreateData()
    {
        ActionBlackBoard.AddData<Action<string>>(DataKey.ACTION_CHAT, AddMessage);
    }

    private void ClearData()
    {
        ActionBlackBoard.ClearData(DataKey.ACTION_CHAT);
    }

    #endregion

    #region Action

    public void AddMessage(string message)
    {
        chat.Add(message);
        TextMeshPro.text += $"{message}\n";
    }

    #endregion
}
