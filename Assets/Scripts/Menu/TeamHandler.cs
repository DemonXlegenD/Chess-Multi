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

    private Guid WhiteTeamPlayerID = Guid.Empty;
    private Guid BlackTeamPlayerID = Guid.Empty;

    #region MonoBehaviors

    void Start()
    {
        CreateData();
        ResetTeams();
    }

    void Update() 
    {
        if (shouldUpdate) 
        {
            UpdateText();
            shouldUpdate = false;
        }

    }

    private void OnDestroy()
    {
        
    }

    #endregion

    #region Blackboard Data

    private void CreateData()
    {
        Data.AddData<bool>(DataKey.IS_WHITE, false);
        Data.AddData<bool>(DataKey.IS_BLACK, false);
        Data.AddData<bool>(DataKey.IS_SPECTATOR, false);

        ActionBlackBoard.AddData<Action<TeamRequestResult>>(DataKey.ACTION_TEAM_REQUEST, TeamRequestServerAnswer);
        ActionBlackBoard.AddData<Action<Guid, Guid>>(DataKey.ACTION_TEAM_INFO, SetTeamInfo);
    }

    private void ClearData()
    {
        Data.ClearData(DataKey.IS_WHITE);
        Data.ClearData(DataKey.IS_BLACK);
        Data.ClearData(DataKey.IS_SPECTATOR);

        ActionBlackBoard.ClearData(DataKey.ACTION_TEAM_REQUEST);
    }

    #endregion

    #region Teams

    public void ResetTeams() 
    {
        whitePlayer = "";
        blackPlayer = "";
        spectatorList = new List<string>();
        shouldUpdate = true;
    }

    public void TeamRequestServerAnswer(TeamRequestResult _TeamRequestResult)
    {
        string _pseudo = _TeamRequestResult.Pseudo;
        Guid _playerID = _TeamRequestResult.PlayerID;
        Teams _team = _TeamRequestResult.Team;
        JoinOrLeave _joinOrLeave = _TeamRequestResult.JoinOrLeave;

        Debug.Log(_playerID + " " + _pseudo + " " + _team + " " + _joinOrLeave);

        if (_joinOrLeave == JoinOrLeave.JOIN) 
        {
            if (_team == Teams.TEAM_WHITE) 
            {
                WhiteTeamPlayerID = _playerID;
                JoinWhite(_pseudo);   
            } else if (_team == Teams.TEAM_BLACK) 
            {
                BlackTeamPlayerID = _playerID;
                JoinBlack(_pseudo);
            }
        }
        else if (_joinOrLeave == JoinOrLeave.LEAVE) 
        {
            if (_team == Teams.TEAM_WHITE) 
            {
                WhiteTeamPlayerID = _playerID;
                LeaveWhite(_pseudo);
            } else if (_team == Teams.TEAM_BLACK) 
            {
                BlackTeamPlayerID = _playerID;
                LeaveBlack(_pseudo);
            }
        }

        UpdateTeams();
    }

    #endregion

    #region Info

    private void SetTeamInfo(Guid white_player_id, Guid black_player_id)
    {
        WhiteTeamPlayerID = white_player_id;
        BlackTeamPlayerID = black_player_id;
        UpdateTeams();
    }

    private void AskTeamInfo()
    {
        Client currentClient = FindAnyObjectByType<Client>();
        if (currentClient != null)
        {
            Header header = new Header(currentClient.Id, currentClient.Pseudo, DateTime.Now, SendMethod.ONLY_CLIENT);
            ChessInfoGameData data = new ChessInfoGameData(DataKey.ACTION_TEAM_INFO);

            Package package = Package.CreatePackage(header, data);

            currentClient.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }  
    }

    public void AskServerToJoinWhite() 
    {
        Client client = FindAnyObjectByType<Client>();
        if (client != null)
        {
            Header header = new Header(client.Id, client.Pseudo, DateTime.Now, SendMethod.ONLY_SERVER);

            TeamRequest team_request = new TeamRequest(Guid.Empty, Teams.TEAM_WHITE, JoinOrLeave.JOIN, DataKey.ACTION_TEAM_REQUEST);

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

            TeamRequest team_request = new TeamRequest(Guid.Empty, Teams.TEAM_BLACK, JoinOrLeave.JOIN, DataKey.ACTION_TEAM_REQUEST);

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

            TeamRequest team_request = new TeamRequest(Guid.Empty, Teams.TEAM_WHITE, JoinOrLeave.LEAVE, DataKey.ACTION_TEAM_REQUEST);

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

            TeamRequest team_request = new TeamRequest(Guid.Empty, Teams.TEAM_BLACK, JoinOrLeave.LEAVE, DataKey.ACTION_TEAM_REQUEST);

            Package package = new Package(header, team_request);

            client.SendDataToServer(DataSerialize.SerializeToBytes(package));
        }  
    }

    #endregion

    #region Actions

    public void JoinWhite(string playerName)
    {
        LeaveSpectator(playerName);
        whitePlayer = playerName;
        shouldUpdate = true;
    }

    public void JoinBlack(string playerName)
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

    #endregion

    #region UI

    private void UpdateTeams() 
    {
        if (Data.GetValue<Client>(DataKey.CLIENT) != null) 
        {
            if (WhiteTeamPlayerID == Data.GetValue<Client>(DataKey.CLIENT).Id)
            {
                Debug.Log("IS WHITE ut");
                Data.SetData(DataKey.IS_WHITE, true);
                Data.SetData(DataKey.IS_SPECTATOR, false);
                Data.SetData(DataKey.IS_BLACK, false);
            } else if (BlackTeamPlayerID == Data.GetValue<Client>(DataKey.CLIENT).Id)
            {
                Debug.Log("IS BLACK ut");
                Data.SetData(DataKey.IS_BLACK, true);
                Data.SetData(DataKey.IS_SPECTATOR, false);
                Data.SetData(DataKey.IS_WHITE, false);
            } else {
                Debug.Log("===");
                Debug.Log(Data.GetValue<Client>(DataKey.CLIENT).Id);
                Debug.Log(WhiteTeamPlayerID);
                Debug.Log(BlackTeamPlayerID);
                Debug.Log("===");
                Data.SetData(DataKey.IS_WHITE, false);
                Data.SetData(DataKey.IS_BLACK, false);
                Data.SetData(DataKey.IS_SPECTATOR, true);
            }
        }
    }
    private void UpdateText()
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

    #endregion

}


[Serializable]
public class TeamRequest : Data
{
    public Guid PlayerID;
    public Teams RequestTeam;
    public JoinOrLeave RequestJoinOrLeave;

    public TeamRequest(Guid _playerID, Teams _team, JoinOrLeave _joinOrLeave, DataKey _actionDataKey) : base(_actionDataKey)
    {   
        this.PlayerID = _playerID;
        this.RequestTeam = _team;
        this.RequestJoinOrLeave = _joinOrLeave;
    }

    public TeamRequest(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.PlayerID = (Guid)_info.GetValue("PlayerID", typeof(Guid));
        this.RequestTeam = (Teams)_info.GetValue("RequestTeam", typeof(Teams));
        this.RequestJoinOrLeave = (JoinOrLeave)_info.GetValue("RequestJoinOrLeave", typeof(JoinOrLeave));
    }

    public TeamRequestResult TeamRequestProcess(string _pseudo, Guid _playerID, Teams _team, JoinOrLeave _joinOrLeave)
    {
        return new TeamRequestResult(_pseudo, _playerID, _team, _joinOrLeave);
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
        _info.AddValue("PlayerID", PlayerID);
        _info.AddValue("RequestTeam", RequestTeam);
        _info.AddValue("RequestJoinOrLeave", RequestJoinOrLeave);
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<TeamRequestResult>>(ActionDataKey)?.Invoke(TeamRequestProcess(_dataPseudo.Pseudo, PlayerID, RequestTeam, RequestJoinOrLeave));
    }
}


#region Enums & Structs
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
    public Guid PlayerID { get; }
    public Teams Team { get; }
    public JoinOrLeave JoinOrLeave { get; }

    public TeamRequestResult(string pseudo, Guid playerID, Teams team, JoinOrLeave joinOrLeave)
    {
        Pseudo = pseudo;
        PlayerID = playerID;
        Team = team;
        JoinOrLeave = joinOrLeave;
    }
}

#endregion