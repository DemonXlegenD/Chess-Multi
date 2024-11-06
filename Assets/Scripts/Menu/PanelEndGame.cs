using TMPro;
using UnityEngine;

public class PanelEndGame : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winner;
    [SerializeField] RectTransform newGameBTN;
    [SerializeField]   BlackBoard BlackBoard;
    private bool needToShowButton = false;
    private bool needToHideButton = false;

    private void Start()
    {
        newGameBTN.gameObject.SetActive(false);
    }
    public void AddWinner(string winnerText)
    {
        winner.text = winnerText;
        needToShowButton = BlackBoard.GetValue<bool>(DataKey.IS_HOST) == true;
        needToHideButton = BlackBoard.GetValue<bool>(DataKey.IS_HOST) == false;
    }

    private void Update()
    {
        if (needToShowButton)
        {
            ShowNewGameButton();
            needToShowButton = false;
        }

        if (needToHideButton)
        {
            HideNewGameButton();
            needToHideButton = false;
        }
    }

    public void ShowNewGameButton()
    {
        newGameBTN.gameObject.SetActive(true);
    }

    public void HideNewGameButton()
    {
        newGameBTN.gameObject.SetActive(false);
    }
}