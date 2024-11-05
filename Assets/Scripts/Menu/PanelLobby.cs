using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelLobby : MonoBehaviour
{
    [SerializeField] private BlackBoard blackBoard;
    [SerializeField] private RectTransform startRectTransform;

    void Start()
    {
        startRectTransform.gameObject.SetActive(false);
    }

    public void DisplayStartButton()
    { 
            startRectTransform.gameObject.SetActive(true);
    }

    public void HideStartButton()
    {
        startRectTransform.gameObject.SetActive(false);
    }
}
