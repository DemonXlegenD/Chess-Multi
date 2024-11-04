using UnityEngine;
using System.Collections.Generic;

/*
 * This class computes AI move decision
 * ComputeMove method is called from ChessGameMananger during AI update turn
 */

public class ChessAI : MonoBehaviour
{
    #region Singleton
    static ChessAI instance = null;
    public static ChessAI Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessAI>();
            return instance;
        }
    }
    #endregion

    #region AI

    public Move ComputeMove()
    {
        // random AI move

        Move move;
        move.from = 0;
        move.to = 1;

        List<Move> moves = new List<Move>(); ;
        ChessGameManager.Instance.GetBoardState().GetValidMoves(EChessTeam.Black, moves);

        if (moves.Count > 0)
            move = moves[Random.Range(0, moves.Count - 1)];

        return move;
    }

    #endregion
}
