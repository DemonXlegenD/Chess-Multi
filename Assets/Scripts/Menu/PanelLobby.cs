using UnityEngine;

public class PanelLobby : MonoBehaviour
{
    [SerializeField] private BlackBoard blackBoard;
    [SerializeField] private RectTransform startRectTransform;

    #region MonoBehaviors

    void Start()
    {
        startRectTransform.gameObject.SetActive(false);
    }

    #endregion

    #region Display Methods

    public void DisplayStartButton()
    { 
            startRectTransform.gameObject.SetActive(true);
    }

    public void HideStartButton()
    {
        startRectTransform.gameObject.SetActive(false);
    }

    #endregion
}
