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

    private bool wantToInstantiate = false;
    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager;

    public void OnStartGame()
    {
        wantToInstantiate = true;
    }

    private void Update()
    {
        if (wantToInstantiate && chessGameManager == null)
        {
            chessGameManager = Instantiate(chessGameManagerPrefab, null);
            wantToInstantiate = false;
        }
    }
}
