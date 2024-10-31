using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;

public class TeamHandler : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI WhiteTeamPlayer;
    [SerializeField] TMPro.TextMeshProUGUI BlackTeamPlayer;
    [SerializeField] TMPro.TextMeshProUGUI SpectatorTeamPlayersOne;
    [SerializeField] TMPro.TextMeshProUGUI SpectatorTeamPlayersTwo;
    [SerializeField] BlackBoard Data;

    private List<string> spectatorList = new List<string>();

    void Start()
    {
        Data.AddData<bool>(DataKey.IS_WHITE, false);
        Data.AddData<bool>(DataKey.IS_BLACK, false);
        Data.AddData<bool>(DataKey.IS_SPECTATOR, false);

        UpdateSpectatorText();
    }

    public void AskServerToJoinWhite() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ALL_CLIENTS);

            TeamRequest team_request = new TeamRequest(Teams.TEAM_WHITE, JoinOrLeave.JOIN, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);


            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }   
    }

    public void AskServerToJoinBlack() 
    {
        JoinBlack(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
    }

    public void AskServerToLeaveWhite() 
    {
        LeaveWhite(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
    }

    public void AskServerToLeaveBlack() 
    {
        LeaveBlack(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
    }

    private void JoinWhite(string playerName)
    {
        LeaveCurrentTeam(); 
        if (!Data.GetValue<bool>(DataKey.IS_WHITE) && WhiteTeamPlayer.text == "")
        {
            Data.SetData(DataKey.IS_WHITE, true);
            WhiteTeamPlayer.text = playerName;
        }
    }

    private void JoinBlack(string playerName)
    {
        LeaveCurrentTeam(); 
        if (!Data.GetValue<bool>(DataKey.IS_BLACK) && BlackTeamPlayer.text == "")
        {
            Data.SetData(DataKey.IS_BLACK, true);
            BlackTeamPlayer.text = playerName; 
        }
    }

    private void LeaveWhite(string playerName)
    {
        if (Data.GetValue<bool>(DataKey.IS_WHITE))
        {
            Data.SetData(DataKey.IS_WHITE, false);
            WhiteTeamPlayer.text = "";
            JoinSpectator(playerName);
        }
    }

    private void LeaveBlack(string playerName)
    {
        if (Data.GetValue<bool>(DataKey.IS_BLACK))
        {
            Data.SetData(DataKey.IS_BLACK, false);
            BlackTeamPlayer.text = "";
            JoinSpectator(playerName);
        }
    }

    private void JoinSpectator(string playerName)
    {
        Debug.Log(playerName);
        LeaveCurrentTeam();
        if (!Data.GetValue<bool>(DataKey.IS_SPECTATOR))
        {
            Data.SetData(DataKey.IS_SPECTATOR, true);
            spectatorList.Add(playerName);
            UpdateSpectatorText();
        }
    }

    private void LeaveSpectator(string playerName)
    {
        if (Data.GetValue<bool>(DataKey.IS_SPECTATOR))
        {
            Data.SetData(DataKey.IS_SPECTATOR, false);
            spectatorList.Remove(playerName);
            UpdateSpectatorText();
        }
    }

    private void LeaveCurrentTeam()
    {
        LeaveWhite(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
        LeaveBlack(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
        LeaveSpectator(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
    }

    private void ResetTeams() 
    {
        WhiteTeamPlayer.text = "";
        BlackTeamPlayer.text = "";
        UpdateSpectatorText();
    }

    private void UpdateSpectatorText()
    {
        SpectatorTeamPlayersOne.text = "";
        SpectatorTeamPlayersTwo.text = "";

        for (int i = 0; i < spectatorList.Count; i++)
        {
            if (i % 2 == 0)
            {
                SpectatorTeamPlayersOne.text += spectatorList[i] + "\n";
            }
            else
            {
                SpectatorTeamPlayersTwo.text += spectatorList[i] + "\n";
            }
        }
    }
}


[Serializable]
public class TeamRequest : Data
{
    Teams Team;
    JoinOrLeave JoinOrLeave;

    public TeamRequest(Teams _team, JoinOrLeave _joinOrLeave, DataKey _actionDataKey) : base(_actionDataKey)
    {
        this.Team = _team;
        this.JoinOrLeave = _joinOrLeave;
    }

    public TeamRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.Team = (Teams)_info.GetValue("Team", typeof(Teams));
        this.JoinOrLeave = (JoinOrLeave)_info.GetValue("JoinOrLeave", typeof(JoinOrLeave));
    }

    public string TeamRequestProcess(string _pseudo, Teams _team, JoinOrLeave _joinOrLeave)
    {
        return "OUI";
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<string>>(ActionDataKey)?.Invoke(TeamRequestProcess(_dataPseudo.Pseudo, Team, JoinOrLeave));
    }
}

public enum Teams
{
    TEAM_WHITE,
    TEAM_BLACK,
}

public enum JoinOrLeave
{
    JOIN,
    LEAVE,
}