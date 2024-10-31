using System;
using System.Runtime.Serialization;

[Serializable]
public class ChatMessage : Data
{
    public string Content { get; set; }

    public ChatMessage(string _content, DataKey _actionDataKey) : base(_actionDataKey)
    {
        this.Content = _content;
    }


    public ChatMessage(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.Content = (string)_info.GetValue("Content", typeof(string));
    }

    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<string>>(ActionDataKey)?.Invoke(MessageFormat(_dataPseudo.Pseudo, _dataTimestamp.Timestamp));
    }

    public string MessageFormat(string _pseudo, DateTime _timestamp)
    {
        string hour_min_sec = _timestamp.ToString("HH:mm:ss");
        return $"{_pseudo} [{hour_min_sec}] : {Content}";
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
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



