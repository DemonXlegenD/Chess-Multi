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
    [SerializeField] private BlackBoard ActionBlackBoard;

    private bool shouldUpdate = false;
    private List<string> spectatorList = new List<string>();
    private string whitePlayer = "";
    private string blackPlayer = "";
    public Action<TeamRequestResult> action;

    void Start()
    {
        Data.AddData<bool>(DataKey.IS_WHITE, false);
        Data.AddData<bool>(DataKey.IS_BLACK, false);
        Data.AddData<bool>(DataKey.IS_SPECTATOR, false);

        action = TeamRequestServerAnswer;
        ActionBlackBoard.AddData<Action<TeamRequestResult>>(DataKey.ACTION_TEAM_REQUEST, action);

        UpdateSpectatorText();
    }

    void Update() 
    {
        if (shouldUpdate) 
        {
            UpdateSpectatorText();
            shouldUpdate = false;
        }

    }

    public void TeamRequestServerAnswer(TeamRequestResult _TeamRequestResult)
    {
        string _pseudo = _TeamRequestResult.Pseudo;
        Teams _team = _TeamRequestResult.Team;
        JoinOrLeave _joinOrLeave = _TeamRequestResult.JoinOrLeave;

        Debug.Log(_pseudo + " " + _team + " " + _joinOrLeave);

        if (_joinOrLeave == JoinOrLeave.JOIN) 
        {
            if (_team == Teams.TEAM_WHITE) 
            {
                JoinWhite(_pseudo);   
            } else if (_team == Teams.TEAM_BLACK) 
            {
                JoinBlack(_pseudo);
            }
        }
        else if (_joinOrLeave == JoinOrLeave.LEAVE) 
        {
            if (_team == Teams.TEAM_WHITE) 
            {
                LeaveWhite(_pseudo);
            } else if (_team == Teams.TEAM_BLACK) 
            {
                LeaveBlack(_pseudo);
            }
        }
    }

    public void AskServerToJoinWhite() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            TeamRequest team_request = new TeamRequest(Teams.TEAM_WHITE, JoinOrLeave.JOIN, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);

            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }   
    }

    public void AskServerToJoinBlack() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            TeamRequest team_request = new TeamRequest(Teams.TEAM_BLACK, JoinOrLeave.JOIN, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);

            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }  
    }

    public void AskServerToLeaveWhite() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            TeamRequest team_request = new TeamRequest(Teams.TEAM_WHITE, JoinOrLeave.LEAVE, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);

            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }  
    }

    public void AskServerToLeaveBlack() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            TeamRequest team_request = new TeamRequest(Teams.TEAM_BLACK, JoinOrLeave.LEAVE, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);

            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }  
    }

    private void JoinWhite(string playerName)
    {
        LeaveSpectator(playerName);
        whitePlayer = playerName;
        shouldUpdate = true;
    }

    private void JoinBlack(string playerName)
    {
        LeaveSpectator(playerName);
        blackPlayer = playerName; 
        shouldUpdate = true;
    }

    private void LeaveWhite(string playerName)
    {
        whitePlayer = "";
        JoinSpectator(playerName);
        shouldUpdate = true;
    }

    private void LeaveBlack(string playerName)
    {
        blackPlayer = "";
        JoinSpectator(playerName);
        shouldUpdate = true;
    }

    private void JoinSpectator(string playerName)
    {
        spectatorList.Add(playerName);
    }

    private void LeaveSpectator(string playerName)
    {
        spectatorList.Remove(playerName);
    }

    private void LeaveCurrentTeam()
    {
        LeaveWhite(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
        LeaveBlack(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
        LeaveSpectator(Data.GetValue<string>(DataKey.PLAYER_NICKNAME));
    }

    private void ResetTeams() 
    {
        whitePlayer = "";
        blackPlayer = "";
    }

    private void UpdateSpectatorText()
    {
        string t1 = "";
        string t2 = "";

        for (int i = 0; i < spectatorList.Count; i++)
        {
            if (i % 2 == 0)
            {
                t1 += spectatorList[i] + "\n";
            }
            else
            {
                t2 += spectatorList[i] + "\n";
            }
        }

        WhiteTeamPlayer.text = whitePlayer;
        BlackTeamPlayer.text = blackPlayer;

        SpectatorTeamPlayersOne.text = t1;
        SpectatorTeamPlayersTwo.text = t2;
    }
}


[Serializable]
public class TeamRequest : Data
{
    public Teams RequestTeam;
    public JoinOrLeave RequestJoinOrLeave;

    public TeamRequest(Teams _team, JoinOrLeave _joinOrLeave, DataKey _actionDataKey) : base(_actionDataKey)
    {
        this.RequestTeam = _team;
        this.RequestJoinOrLeave = _joinOrLeave;
    }

    public TeamRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.RequestTeam = (Teams)_info.GetValue("RequestTeam", typeof(Teams));
        this.RequestJoinOrLeave = (JoinOrLeave)_info.GetValue("RequestJoinOrLeave", typeof(JoinOrLeave));
    }

    public TeamRequestResult TeamRequestProcess(string _pseudo, Teams _team, JoinOrLeave _joinOrLeave)
    {
        return new TeamRequestResult(_pseudo, _team, _joinOrLeave);
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
        _info.AddValue("RequestTeam", RequestTeam);
        _info.AddValue("RequestJoinOrLeave", RequestJoinOrLeave);
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<TeamRequestResult>>(ActionDataKey)?.Invoke(TeamRequestProcess(_dataPseudo.Pseudo, RequestTeam, RequestJoinOrLeave));
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

public struct TeamRequestResult
{
    public string Pseudo { get; }
    public Teams Team { get; }
    public JoinOrLeave JoinOrLeave { get; }

    public TeamRequestResult(string pseudo, Teams team, JoinOrLeave joinOrLeave)
    {
        Pseudo = pseudo;
        Team = team;
        JoinOrLeave = joinOrLeave;
    }
}