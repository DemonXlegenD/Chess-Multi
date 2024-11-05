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
    [SerializeField] private Camera camera;

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
            if (blackBoard.GetValue<bool>(DataKey.IS_BLACK)) {
                camera.transform.position = BlackCamera.position;
                camera.transform.rotation = BlackCamera.rotation;
            } else {
                camera.transform.position = WhiteCamera.position;
                camera.transform.rotation = WhiteCamera.rotation;
            }
            wantToInstantiate = false;
        }
    }
}
