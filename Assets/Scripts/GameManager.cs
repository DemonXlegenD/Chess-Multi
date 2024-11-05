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

    [SerializeField] private Transform WhiteCamera;
    [SerializeField] private Transform BlackCamera;
    [SerializeField] private BlackBoard blackBoard;
    private bool wantToInstantiate = false;
    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager = null;
    [SerializeField] private Camera mainCamera;

    #region MonoBehaviors

    private void Start()
    {
        blackBoard.AddData<GameManager>(DataKey.GAME_MANAGER, Instance);
    }

    private void Update()
    {
        if (wantToInstantiate && chessGameManager == null)
        {
            chessGameManager = Instantiate(chessGameManagerPrefab, null);
            if (blackBoard.GetValue<bool>(DataKey.IS_BLACK)) {
                mainCamera.transform.position = BlackCamera.position;
                mainCamera.transform.rotation = BlackCamera.rotation;
            } else {
                mainCamera.transform.position = WhiteCamera.position;
                mainCamera.transform.rotation = WhiteCamera.rotation;
            }
            wantToInstantiate = false;
        }
    }

    private void OnDestroy()
    {
        blackBoard.ClearData(DataKey.GAME_MANAGER);
    }

    #endregion

    public void OnStartGame()
    {
        wantToInstantiate = true;
    }

    public void OnEndGame()
    {
        Destroy(chessGameManager);
    }
}
