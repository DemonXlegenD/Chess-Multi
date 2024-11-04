using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }
    #endregion

    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager;

    public void OnStartGame()
    {
        chessGameManager = Instantiate(chessGameManagerPrefab, null);
    }
}
