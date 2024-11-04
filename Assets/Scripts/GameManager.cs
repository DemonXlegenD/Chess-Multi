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

    [SerializeField] private BlackBoard blackBoard;
    private bool wantToInstantiate = false;
    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager = null;

    private void Start()
    {
        blackBoard.AddData<GameManager>(DataKey.GAME_MANAGER, Instance);
    }

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
