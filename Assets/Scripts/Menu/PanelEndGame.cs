using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;

public class PanelEndGame : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winner;

    public void AddWinner(string winnerText)
    {
        winner.text = winnerText;
    }
}