using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelLobby : MonoBehaviour
{
    private bool isHost = false;
    [SerializeField] private BlackBoard blackBoard;
    [SerializeField] private RectTransform startRectTransform;

    void Start()
    {
        startRectTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (blackBoard.GetValue<bool>(DataKey.IS_HOST) && !isHost)
        {
            startRectTransform.gameObject.SetActive(true);
            isHost = true;
        }
    }
}
