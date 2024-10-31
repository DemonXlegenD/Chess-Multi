using System;
using System.Runtime.Serialization;


#region Package
public struct Package : IHeader, IData, ISerializable
{
    public Header Header { get; set; }

    public Data Data { get; set; }

    public Package(Header _header, Data _data)
    {
        this.Header = _header;
        this.Data = _data;
    }

    public Package(SerializationInfo _info, StreamingContext _context)
    {
        this.Header = (Header)_info.GetValue("Header", typeof(Header)); ;
        this.Data = (Data)_info.GetValue("Data", typeof(Data));
    }

    public void GetObjectData(SerializationInfo _info, StreamingContext _context)
    {
        _info.AddValue("Header", Header);
        _info.AddValue("Data", Data);
    }
}

#endregion

#region Header

public enum SendMethod
{
    ONLY_SERVER,
    ONLY_CLIENT,
    ALL_CLIENTS,
    ONLY_SPECTATORS,
    ONLY_PLAYERS,
    OPPONENT,
}

public interface IIdClient
{
    public uint Id { get; set; }    
}

public interface IPlayerPseudo
{
    public string Pseudo { get; set; }
}

public interface ITimestamp
{
    public DateTime Timestamp { get; set; }
}

public interface ISendMethod
{
    public SendMethod SendMethod { get; set; }
}

public interface IHeader
{
    public Header Header { get; set; }
}

[Serializable]
public struct Header : IIdClient, IPlayerPseudo, ITimestamp, ISendMethod, ISerializable
{
    public uint Id { get; set; }
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }

    public SendMethod SendMethod { get; set; }

    public Header(uint _id, string _pseudo, DateTime _timestamp, SendMethod _sendMethod)
    {
        this.Id = _id;
        this.Pseudo = _pseudo;
        this.Timestamp = _timestamp;
        this.SendMethod = _sendMethod;
    }

    public Header(SerializationInfo _info, StreamingContext _context)
    {

        this.Id = (uint)_info.GetValue("Id", typeof(uint));
        this.Pseudo = (string)_info.GetValue("Pseudo", typeof(string));
        this.Timestamp = (DateTime)_info.GetValue("Timestamp", typeof(DateTime));
        this.SendMethod = (SendMethod)_info.GetValue("SendMethod", typeof(SendMethod));
    }

    public void GetObjectData(SerializationInfo _info, StreamingContext _context)
    {
        _info.AddValue("Id", Id);
        _info.AddValue("Pseudo", Pseudo);
        _info.AddValue("Timestamp", Timestamp);
        _info.AddValue("SendMethod", SendMethod);
    }
}

#endregion

#region Data
public interface IData
{
    public Data Data { get; set; }
}

public interface IAction
{
    public DataKey ActionDataKey { get; set; }
    public void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp);
}

public abstract class Data : IAction, ISerializable
{
    public DataKey ActionDataKey { get; set; }
    public abstract void CallAction(BlackBoard _actionBlackBoard, IPlayerPseudo _dataPseudo, ITimestamp _dataTimestamp);

    public Data()
    {
        this.ActionDataKey = DataKey.NONE;
    }

    public Data(DataKey _actionDataKey)
    {
        this.ActionDataKey = _actionDataKey;
    }

    public Data(SerializationInfo _info, StreamingContext _context)
    {
        this.ActionDataKey = (DataKey)_info.GetValue("ActionDataKey", typeof(DataKey));
    }

    public virtual void GetObjectData(SerializationInfo _info, StreamingContext _context)
    {
        _info.AddValue("ActionDataKey", ActionDataKey);
    }
}

#endregion