using System;
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
    [SerializeField] private BlackBoard ActionBlackBoard;
    private bool wantToInstantiate = false;
    [SerializeField] ChessGameManager chessGameManagerPrefab;
    private ChessGameManager chessGameManager = null;
    [SerializeField] private Camera mainCamera;

    private bool needToLeave = false;

    #region MonoBehaviors

    private void Start()
    {
        Application.runInBackground = true;
        blackBoard.AddData<GameManager>(DataKey.GAME_MANAGER, Instance);
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_LEAVE_ROOM, AskForLeaving);
    }

    private void Update()
    {
        if (wantToInstantiate && chessGameManager == null)
        {
            if (blackBoard.GetValue<bool>(DataKey.IS_BLACK)) {
                Debug.Log("IS BLACK");
                mainCamera.transform.position = BlackCamera.position;
                mainCamera.transform.rotation = BlackCamera.rotation;
            } else {
                Debug.Log("IS WHITE OR SPEC");
                mainCamera.transform.position = WhiteCamera.position;
                mainCamera.transform.rotation = WhiteCamera.rotation;
            }

            chessGameManager = Instantiate(chessGameManagerPrefab, null);
            wantToInstantiate = false;
        }

        if (needToLeave)
        {
            OnLeaveGame();
            needToLeave = false;
        }
    }

    private void OnDestroy()
    {
        blackBoard.ClearData(DataKey.GAME_MANAGER);
        ActionBlackBoard.ClearData(DataKey.ACTION_LEAVE_ROOM);
    }

    #endregion

    public void AskForLeaving()
    {
        Debug.Log("Ask for leaving");
        needToLeave = true;
    }

    public void OnStartGame()
    {
        wantToInstantiate = true;
    }

    public void OnResetGame()
    {
        chessGameManager.AskToReset();
    }

    public void OnLeaveGame()
    {
        Destroy(chessGameManager.gameObject);
    }
}
