using System;
using System.Runtime.Serialization;

public enum SendTo
{
    ALL_CLIENTS,
    SPECTATOR,
    OPPONENT,
}

public interface ISendTo
{
    public SendTo SendTo { get; set; }
}

public interface IAction
{
    public DataKey ActionDataKey { get; set; }
    public void CallAction(BlackBoard _actionBlackBoard);
}


public interface IStructData : ISendTo, ISerializable, IAction { }


[Serializable]
public struct StructMessageChat : SMessageChat, IStructData
{
    public SendTo SendTo { get; set; }
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }

    public DataKey ActionDataKey { get; set; }
    public StructMessageChat(string _pseudo, DateTime _dateTime, string _content, SendTo _sendTo, DataKey _actionDataKey)
    {
        Pseudo = _pseudo;
        Timestamp = _dateTime;
        Content = _content;
        SendTo = _sendTo;
        ActionDataKey = _actionDataKey;
    }


    public StructMessageChat(SerializationInfo _info, StreamingContext _ctxt)
    {
        this.SendTo = (SendTo)_info.GetValue("SendTo", typeof(SendTo));
        this.Pseudo = (string)_info.GetValue("Pseudo", typeof(string));
        this.Timestamp = (DateTime)_info.GetValue("Data", typeof(DateTime));
        this.Content = (string)_info.GetValue("Content", typeof(string));
        this.ActionDataKey = (DataKey)_info.GetValue("ActionDataKey", typeof(DataKey));
    }

    public void CallAction(BlackBoard _actionBlackBoard)
    {
        _actionBlackBoard.GetValue<Action<string>>(ActionDataKey)?.Invoke(MessageFormat());
    }

    public string MessageFormat()
    {
        string hour_min_sec = Timestamp.ToString("HH:mm:ss");
        return $"{Pseudo} [{hour_min_sec}] : {Content}";
    }

    public void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        _info.AddValue("SendTo", SendTo);
        _info.AddValue("Pseudo", Pseudo);
        _info.AddValue("Data", Timestamp);
        _info.AddValue("Content", Content);
        _info.AddValue("ActionDataKey", ActionDataKey);
    }



}

public interface SMessageChat
{
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
}


