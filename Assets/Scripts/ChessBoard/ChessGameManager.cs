using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/*
 * This singleton manages the whole chess game
 *  - board data (see BoardState class)
 *  - piece models instantiation
 *  - player interactions (piece grab, drag and release)
 *  - AI update calls (see UpdateAITurn and ChessAI class)
 */


#region Enums
public enum EPieceType : uint
{
    Pawn = 0,
    King,
    Queen,
    Rook,
    Knight,
    Bishop,
    NbPieces,
    None
}

public enum EChessTeam
{
    White = 0,
    Black,
    None
}

public enum ETeamFlag : uint
{
    None = 1 << 0,
    Friend = 1 << 1,
    Enemy = 1 << 2
}
#endregion

#region Structs & Classes
public struct BoardSquare
{
    public EPieceType piece;
    public EChessTeam team;

    public BoardSquare(EPieceType p, EChessTeam t)
    {
        piece = p;
        team = t;
    }

    static public BoardSquare Empty()
    {
        BoardSquare res;
        res.piece = EPieceType.None;
        res.team = EChessTeam.None;
        return res;
    }
}

[Serializable]
public struct Move
{
    public int from;
    public int to;

    public override bool Equals(object o)
    {
        try
        {
            return (bool)(this == (Move)o);
        }
        catch
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return from + to;
    }

    public static bool operator ==(Move move1, Move move2)
    {
        return move1.from == move2.from && move1.to == move2.to;
    }

    public static bool operator !=(Move move1, Move move2)
    {
        return move1.from != move2.from || move1.to != move2.to;
    }
}

#endregion

public partial class ChessGameManager : MonoBehaviour
{

    #region Singleton
    static ChessGameManager instance = null;
    public static ChessGameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessGameManager>();
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private bool isAIEnabled = false;
    [SerializeField] private BlackBoard blackBoard;
    [SerializeField] private BlackBoard ActionBlackBoard;

    private ChessAI chessAI = null;
    private Transform boardTransform = null;
    private static int BOARD_SIZE = 8;
    private int pieceLayerMask;
    private int boardLayerMask;

    private Client currentClient;
    private Guid blackClientId = Guid.Empty;
    private Guid whiteClientId = Guid.Empty;
    private string whitePseudo = string.Empty;
    private string blackPseudo = string.Empty;

    private bool needToUpdatePieces = false;
    private bool isEndGame = false;
    private bool needResetGame = false;
    private MainMenu mainMenu = null;
    private PanelInGame panelInGame = null;
    private PanelEndGame panelEndGame = null;

    #region Chess Game Methods

    BoardState boardState = null;
    public BoardState GetBoardState() { return boardState; }

    EChessTeam teamTurn;

    private int positionQueen = -1;

    List<uint> scores;

    public delegate void PlayerTurnEvent(bool isWhiteMove);
    public event PlayerTurnEvent OnPlayerTurn = null;

    public delegate void ScoreUpdateEvent(uint whiteScore, uint blackScore);
    public event ScoreUpdateEvent OnScoreUpdated = null;


    public void ResetGame()
    {
        needResetGame = true;
    }

    public void EndGame()
    {
        isEndGame = true;
    }

    private void UpdateEndGame()
    {
        mainMenu.ShowEndGame();
        if (teamTurn == EChessTeam.White)
        {
            panelEndGame.AddWinner(whitePseudo);
        }
        else
        {
            panelEndGame.AddWinner(blackPseudo);
        }
        isEndGame = false;
        // increase score and reset board
        scores[(int)teamTurn]++;
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);

    }

    private void UpdateResetGame()
    {

        needResetGame = false;
        PrepareGame(false);
        // remove extra piece instances if pawn promotions occured
        teamPiecesArray[0].ClearPromotedPieces();
        teamPiecesArray[1].ClearPromotedPieces();

        mainMenu.ShowInGame();
    }

    public void PrepareGame(bool resetScore = true)
    {
        chessAI = ChessAI.Instance;

        // Start game
        boardState.Reset();

        teamTurn = EChessTeam.White;
        if (scores == null)
        {
            scores = new List<uint>();
            scores.Add(0);
            scores.Add(0);
        }
        if (resetScore)
        {
            scores.Clear();
            scores.Add(0);
            scores.Add(0);
        }
    }

    public void PlayTurn(Move move)
    {
        needToUpdatePieces = true;
        BoardState.EMoveResult result = boardState.PlayUnsafeMove(move);
        if (result == BoardState.EMoveResult.Promotion)
        {
            positionQueen = move.to;
        }
    }


    private void UpdateTeams()
    {
        EChessTeam otherTeam = (teamTurn == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
        if (boardState.DoesTeamLose(otherTeam))
        {
            if (blackBoard.GetValue<bool>(DataKey.IS_HOST)) 
            {
                AskToEnd();
            }
        }
        else
        {
            teamTurn = otherTeam;
        }
    }

    // used to instantiate newly promoted queen
    private void AddQueenAtPos(int pos)
    {
        teamPiecesArray[(int)teamTurn].AddPiece(EPieceType.Queen);
        GameObject[] crtTeamPrefabs = (teamTurn == EChessTeam.White) ? whitePiecesPrefab : blackPiecesPrefab;
        GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)EPieceType.Queen]);
        teamPiecesArray[(int)teamTurn].StorePiece(crtPiece, EPieceType.Queen);
        crtPiece.transform.position = GetWorldPos(pos);
    }

    public bool IsPlayerTurn()
    {
        return teamTurn == EChessTeam.White;
    }

    public BoardSquare GetSquare(int pos)
    {
        return boardState.squares[pos];
    }

    public uint GetScore(EChessTeam team)
    {
        return scores[(int)team];
    }

    private void UpdateBoardPiece(Transform pieceTransform, int destPos)
    {
        pieceTransform.position = GetWorldPos(destPos);
    }

    private Vector3 GetWorldPos(int pos)
    {
        Vector3 piecePos = boardTransform.position;
        piecePos.y += zOffset;
        piecePos.x = -widthOffset + pos % BOARD_SIZE;
        piecePos.z = -widthOffset + pos / BOARD_SIZE;

        return piecePos;
    }

    private int GetBoardPos(Vector3 worldPos)
    {
        int xPos = Mathf.FloorToInt(worldPos.x + widthOffset) % BOARD_SIZE;
        int zPos = Mathf.FloorToInt(worldPos.z + widthOffset);

        return xPos + zPos * BOARD_SIZE;
    }

    #endregion

    #region GameInfo

    private bool IsGameInfoReady()
    {
        if (blackClientId != Guid.Empty && whiteClientId != Guid.Empty && blackPseudo != string.Empty && whitePseudo != string.Empty)
        {
            return true;
        }
        return false;
    }

    private void AskGameInfo()
    {
        Header header = new Header(currentClient.Id, currentClient.Pseudo, DateTime.Now, SendMethod.ONLY_CLIENT);
        ChessInfoGameData data = new ChessInfoGameData(DataKey.ACTION_CHESS_GAME_INFO);

        Package package = Package.CreatePackage(header, data);

        currentClient.SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    private void SetGameInfo(Guid white_player_id, Guid black_player_id, string white_player_pseudo, string black_player_pseudo)
    {
        GetData();
        blackClientId = black_player_id;
        whiteClientId = white_player_id;
        whitePseudo = white_player_pseudo;
        blackPseudo = black_player_pseudo;    
    }

    private void AskToMove(Move move)
    {
        Header header = new Header(currentClient.Id, currentClient.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

        MoveData moveData = new MoveData(DataKey.ACTION_PLAY_MOVE);
        moveData.Move = move;

        Package package = Package.CreatePackage(header, moveData);

        currentClient.SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    public void AskToEnd()
    {
        Header header = new Header(currentClient.Id, currentClient.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

        SimpleAction end_game = new SimpleAction(DataKey.ACTION_END_GAME);

        Package package = Package.CreatePackage(header, end_game);

        currentClient.SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    public void AskToReset()
    {
        Header header = new Header(currentClient.Id, currentClient.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

        SimpleAction reset_game = new SimpleAction(DataKey.ACTION_RESET_GAME);

        Package package = Package.CreatePackage(header, reset_game);

        currentClient.SendDataToServer(DataSerialize.SerializeToBytes(package));
    }

    #endregion

    #region MonoBehaviour

    private TeamPieces[] teamPiecesArray = new TeamPieces[2];
    private float zOffset = 0.5f;
    private float widthOffset = 3.5f;

    void Start()
    {
        if (mainMenu == null) mainMenu = FindAnyObjectByType<MainMenu>();
        if (panelInGame == null) panelInGame = FindAnyObjectByType<PanelInGame>();
        if (panelEndGame == null) panelEndGame = FindAnyObjectByType<PanelEndGame>();

        OnPlayerTurn += panelInGame.WhiteMove;

        CreateData();
        GetData();

        AskGameInfo();

        pieceLayerMask = 1 << LayerMask.NameToLayer("Piece");
        boardLayerMask = 1 << LayerMask.NameToLayer("Board");

        boardTransform = GameObject.FindGameObjectWithTag("Board").transform;

        LoadPiecesPrefab();

        boardState = new BoardState();

        PrepareGame();

        teamPiecesArray[0] = null;
        teamPiecesArray[1] = null;

        CreatePieces();

        OnScoreUpdated += GUIManager.Instance.UpdateScore;

        if (OnPlayerTurn != null)
            OnPlayerTurn(teamTurn == EChessTeam.White);
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);
    }

    void Update()
    {
        if (IsGameInfoReady())
        {
            //Debug.LogWarning("CurrentId : " + currentClient.Id);
            if (teamTurn == EChessTeam.White)
            {
                if (currentClient.Id == whiteClientId)
                {
                    UpdatePlayerTurn();
                }
            }
            else if (teamTurn == EChessTeam.Black)
            {
                if (currentClient.Id == blackClientId)
                {
                    UpdatePlayerTurn();
                }
            }
            else if (isAIEnabled)
            {
                UpdateAITurn();
            }
            else
            {
                Watch();
            }

            if (needToUpdatePieces)
            {
  
                UpdatePieces();
                UpdateTeams();



                if (OnPlayerTurn != null)
                    OnPlayerTurn(teamTurn == EChessTeam.White);
                needToUpdatePieces = false;

            }

            if (isEndGame)
            {
                UpdateEndGame();
            }

            if (needResetGame)
            {
                UpdateResetGame();
                UpdatePieces();
            }
        }

    }

    private void OnDestroy()
    {
        ClearData();
    }

    #endregion

    #region Blackboard Data

    private void CreateData()
    {
        ActionBlackBoard.AddData<Action<Guid, Guid, string, string>>(DataKey.ACTION_CHESS_GAME_INFO, SetGameInfo);
        ActionBlackBoard.AddData<Action<Move>>(DataKey.ACTION_PLAY_MOVE, PlayTurn);
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_END_GAME, EndGame);
        ActionBlackBoard.AddData<Action>(DataKey.ACTION_RESET_GAME, ResetGame);
    }

    private void GetData()
    {
        currentClient = blackBoard.GetValue<Client>(DataKey.CLIENT);
    }

    private void ClearData()
    {
        ActionBlackBoard.ClearData(DataKey.ACTION_CHESS_GAME_INFO);
        ActionBlackBoard.ClearData(DataKey.ACTION_PLAY_MOVE);
        ActionBlackBoard.ClearData(DataKey.ACTION_END_GAME);
        ActionBlackBoard.ClearData(DataKey.ACTION_RESET_GAME);
    }

    #endregion

    #region Pieces

    GameObject[] whitePiecesPrefab = new GameObject[6];
    GameObject[] blackPiecesPrefab = new GameObject[6];

    void LoadPiecesPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhitePawn");
        whitePiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKing");
        whitePiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteQueen");
        whitePiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteRook");
        whitePiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKnight");
        whitePiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteBishop");
        whitePiecesPrefab[(uint)EPieceType.Bishop] = prefab;

        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackPawn");
        blackPiecesPrefab[(uint)EPieceType.Pawn] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKing");
        blackPiecesPrefab[(uint)EPieceType.King] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackQueen");
        blackPiecesPrefab[(uint)EPieceType.Queen] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackRook");
        blackPiecesPrefab[(uint)EPieceType.Rook] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKnight");
        blackPiecesPrefab[(uint)EPieceType.Knight] = prefab;
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackBishop");
        blackPiecesPrefab[(uint)EPieceType.Bishop] = prefab;
    }

    void CreatePieces()
    {
        // Instantiate all pieces according to board data
        if (teamPiecesArray[0] == null)
            teamPiecesArray[0] = new TeamPieces();
        if (teamPiecesArray[1] == null)
            teamPiecesArray[1] = new TeamPieces();

        GameObject[] crtTeamPrefabs = null;
        int crtPos = 0;
        foreach (BoardSquare square in boardState.squares)
        {
            crtTeamPrefabs = (square.team == EChessTeam.White) ? whitePiecesPrefab : blackPiecesPrefab;
            if (square.piece != EPieceType.None)
            {
                GameObject crtPiece = Instantiate(crtTeamPrefabs[(uint)square.piece]);
                teamPiecesArray[(int)square.team].StorePiece(crtPiece, square.piece);

                // set position
                Vector3 piecePos = boardTransform.position;
                piecePos.y += zOffset;
                piecePos.x = -widthOffset + crtPos % BOARD_SIZE;
                piecePos.z = -widthOffset + crtPos / BOARD_SIZE;
                crtPiece.transform.position = piecePos;
            }
            crtPos++;
        }
    }

    void UpdatePieces()
    {
        if (positionQueen != -1)
        {
            AddQueenAtPos(positionQueen);
            positionQueen = -1;
        }

        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < boardState.squares.Count; i++)
        {
            BoardSquare square = boardState.squares[i];
            if (square.team == EChessTeam.None)
                continue;

            int teamId = (int)square.team;
            EPieceType pieceType = square.piece;

            teamPiecesArray[teamId].SetPieceAtPos(pieceType, GetWorldPos(i));
        }
    }

    #endregion

    #region Gameplay

    Transform grabbed = null;
    float maxDistance = 100f;
    int startPos = 0;
    int destPos = 0;

    private void Watch()
    {

    }

    void UpdateAITurn()
    {
        Move move = chessAI.ComputeMove();
        PlayTurn(move);

        UpdatePieces();
    }

    void UpdatePlayerTurn()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbed)
                ComputeDrag();
            else
                ComputeGrab();
        }
        else if (grabbed != null)
        {
            // find matching square when releasing grabbed piece
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
            {
                grabbed.root.position = hit.transform.position + Vector3.up * zOffset;
            }

            destPos = GetBoardPos(grabbed.root.position);
            if (startPos != destPos)
            {
                Move move = new Move();
                move.from = startPos;
                move.to = destPos;

                if (boardState.IsValidMove(teamTurn, move))
                {
                    AskToMove(move);
                } else
                {
                    UpdatePieces();
                }
            }
            else
            {
                grabbed.root.position = GetWorldPos(startPos);
            }
            grabbed = null;
        }
    }

    void ComputeDrag()
    {
        // drag grabbed piece on board
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
        {
            grabbed.root.position = hit.point;
        }
    }

    void ComputeGrab()
    {
        // grab a new chess piece from board
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxDistance, pieceLayerMask))
        {
            grabbed = hit.transform;
            startPos = GetBoardPos(hit.transform.position);
        }
    }

    #endregion
}

[Serializable]
public class ChessInfoGameData : Data
{
    public Guid WhitePlayerId = Guid.Empty;

    public Guid BlackPlayerId = Guid.Empty;

    public string WhitePlayerPseudo = string.Empty;

    public string BlackPlayerPseudo = string.Empty;

    public ChessInfoGameData(DataKey _actionDataKey) : base(_actionDataKey)
    {

    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<Guid, Guid, string, string>>(ActionDataKey)?.Invoke(WhitePlayerId, BlackPlayerId, WhitePlayerPseudo, BlackPlayerPseudo);
    }

    public ChessInfoGameData(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        WhitePlayerId = (Guid)_info.GetValue("WhitePlayerId", typeof(Guid));
        BlackPlayerId = (Guid)_info.GetValue("BlackPlayerId", typeof(Guid));
        WhitePlayerPseudo = (string)_info.GetValue("WhitePlayerPseudo", typeof(string));
        BlackPlayerPseudo = (string)_info.GetValue("BlackPlayerPseudo", typeof(string));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);

        _info.AddValue("WhitePlayerId", WhitePlayerId);
        _info.AddValue("BlackPlayerId", BlackPlayerId);
        _info.AddValue("WhitePlayerPseudo", WhitePlayerPseudo);
        _info.AddValue("BlackPlayerPseudo", BlackPlayerPseudo);
    }
}

[Serializable]
public class MoveData : Data
{
    public Move Move;

    public MoveData(DataKey _actionDataKey) : base(_actionDataKey)
    {

    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        Debug.Log("Move");
        _actionBlackBoard.GetValue<Action<Move>>(ActionDataKey)?.Invoke(Move);
    }

    public MoveData(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        Move = (Move)_info.GetValue("Move", typeof(Move));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);

        _info.AddValue("Move", Move);
    }
}