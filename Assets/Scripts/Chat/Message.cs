using System;
using System.Drawing;
using System.Runtime.Serialization;


[Serializable]
public class ChatMessage : Data
{
    public string Content { get; set; }
    public SerializableColor Color { get; set; } = SerializableColor.White;

    public ChatMessage(string _content) : base(DataKey.ACTION_CHAT)
    {
        this.Content = _content;
    }

    public ChatMessage(string _content, DataKey _actionDataKey) : base(_actionDataKey)
    {
        this.Content = _content;
    }

    public ChatMessage(string _content, SerializableColor _color) : base(DataKey.ACTION_CHAT)
    {
        this.Content = _content;
        this.Color = _color;
    }

    public ChatMessage(string _content, SerializableColor _color, DataKey _actionDataKey) : base(_actionDataKey)
    {
        this.Content = _content;
        this.Color = _color;
    }

    public ChatMessage(SerializationInfo _info, StreamingContext _ctxt) : base(_info, _ctxt)
    {
        this.Content = (string)_info.GetValue("Content", typeof(string));
        this.Color = (SerializableColor)_info.GetValue("Color", typeof(SerializableColor));
    }

    public override void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        base.GetObjectData(_info, _ctxt);
        _info.AddValue("Content", Content);
        _info.AddValue("Color", Color);
    }


    public string MessageFormat(string _pseudo, DateTime _timestamp)
    {
        string hour_min_sec = _timestamp.ToString("HH:mm:ss");
        return $"<color={Color.ToHex()}>{_pseudo}</color> <color=#FFFF00>[{hour_min_sec}]</color> : {Content}";
    }


    public override void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp)
    {
        _actionBlackBoard.GetValue<Action<string>>(ActionDataKey)?.Invoke(MessageFormat(_dataPseudo.Pseudo, _dataTimestamp.Timestamp));
    }
}

