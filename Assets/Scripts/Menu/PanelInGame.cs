using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelInGame : MonoBehaviour
{
    [SerializeField] private RectTransform whiteTurnTextRect;
    [SerializeField] private RectTransform blackTurnTextRect;

    private void Start()
    {
        whiteTurnTextRect.gameObject.SetActive(true);
        blackTurnTextRect.gameObject.SetActive(false);
    }

    public void SwitchTurn()
    {
        whiteTurnTextRect.gameObject.SetActive(!whiteTurnTextRect.gameObject.activeSelf);
        blackTurnTextRect.gameObject.SetActive(!blackTurnTextRect.gameObject.activeSelf);
    }

    public void WhiteMove(bool isWhiteMove)
    {
        whiteTurnTextRect.gameObject.SetActive(isWhiteMove);
        blackTurnTextRect.gameObject.SetActive(!isWhiteMove);
    }
}
