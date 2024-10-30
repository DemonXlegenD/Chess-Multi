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

public interface IAction<T>
{
    public Action<T> Action { get; set; }

}

public interface ICallAction
{
    public void CallAction();
}

public interface IStructData : ISendTo, ISerializable, IAction<string>, ICallAction { }


[Serializable]
public struct StructMessageChat : SMessageChat, IStructData
{
    public Action<string> Action { get; set; }
    public SendTo SendTo { get; set; }
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
    public StructMessageChat(string _pseudo, DateTime _dateTime, string _content, SendTo _sendTo)
    {
        Pseudo = _pseudo;
        Timestamp = _dateTime;
        Content = _content;
        SendTo = _sendTo;
        Action = null;
    }

    public StructMessageChat(string _pseudo, DateTime _dateTime, string _content, SendTo _sendTo, Action<string> _action)
    {
        Pseudo = _pseudo;
        Timestamp = _dateTime;
        Content = _content;
        SendTo = _sendTo;
        Action = _action;
    }

    public StructMessageChat(SerializationInfo _info, StreamingContext _ctxt)
    {
        this.SendTo = (SendTo)_info.GetValue("SendTo", typeof(SendTo));
        this.Pseudo = (string)_info.GetValue("Pseudo", typeof(string));
        this.Timestamp = (DateTime)_info.GetValue("Data", typeof(DateTime));
        this.Content = (string)_info.GetValue("Content", typeof(string));
        this.Action = (Action<string>)_info.GetValue("Action", typeof(Action<string>));
    }

    public void CallAction()
    {
        Action?.Invoke(MessageFormat());
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
        _info.AddValue("Action", Action);
    }



}

public interface SMessageChat
{
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
}


