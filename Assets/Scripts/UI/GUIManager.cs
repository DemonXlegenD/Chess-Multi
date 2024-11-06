using UnityEngine;
using UnityEngine.UI;

/*
 * Simple GUI display : scores and team turn
 */

public class GUIManager : MonoBehaviour
{

    #region Singleton
    static GUIManager instance = null;
    public static GUIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GUIManager>();
            return instance;
        }
    }
    #endregion

    [SerializeField] private RectTransform InGamePanel;

    Transform whiteToMoveTr = null;
    Transform blackToMoveTr = null;
    Text whiteScoreText = null;
    Text blackScoreText = null;

    // Use this for initialization
    void Awake()
    {
        Application.runInBackground = true;
        whiteScoreText = InGamePanel.Find("WhiteScoreText").GetComponent<Text>();
        blackScoreText = InGamePanel.Find("BlackScoreText").GetComponent<Text>();
    }
	
    void DisplayTurn(bool isWhiteMove)
    {
        whiteToMoveTr.gameObject.SetActive(isWhiteMove);
        blackToMoveTr.gameObject.SetActive(!isWhiteMove);
    }

    public void UpdateScore(uint whiteScore, uint blackScore)
    {
        whiteScoreText.text = string.Format("White : {0}", whiteScore);
        blackScoreText.text = string.Format("Black : {0}", blackScore);
    }
}
