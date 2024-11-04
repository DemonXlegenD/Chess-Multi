using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager;

    public void OnStartGame()
    {
        chessGameManager = Instantiate(chessGameManagerPrefab, null);
    }
}
