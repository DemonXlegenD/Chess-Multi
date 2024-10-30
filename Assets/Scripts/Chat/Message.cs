using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public enum SendTo
{
    ALL_CLIENTS,
    SPECTATOR,
    OPPONENT,
}

[Serializable]
public struct StructMessageChat : SMessageChat, ISerializable
{
    public SendTo sendTo;
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
    public StructMessageChat(string _pseudo, DateTime _dateTime, string _content, SendTo _sendTo)
    {
        Pseudo = _pseudo;   
        Timestamp = _dateTime;
        Content = _content;
        sendTo  = _sendTo;
    }

    public StructMessageChat(SerializationInfo _info, StreamingContext _ctxt)
    {
        this.sendTo = (SendTo)_info.GetValue("SendTo", typeof(SendTo)); 
        this.Pseudo = (string)_info.GetValue("Pseudo", typeof(string));
        this.Timestamp = (DateTime)_info.GetValue("Data", typeof(DateTime));
        this.Content = (string)_info.GetValue("Content", typeof(string));
    }

    public void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        _info.AddValue("SendTo", sendTo);
        _info.AddValue("Pseudo", Pseudo);
        _info.AddValue("Data", Timestamp);
        _info.AddValue("Content", Content);
    }

}

public interface SMessageChat
{
    public string Pseudo { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }
}

[Serializable]
public class ChatMessage : ISerializable
{
    public StructMessageChat TypeMessage;

    public ChatMessage(string _pseudo, DateTime _timestamp, string _content)
    {
        this.TypeMessage.Pseudo = _pseudo;
        this.TypeMessage.Timestamp = _timestamp;
        this.TypeMessage.Content = _content;
    }

    public ChatMessage(StructMessageChat _typeMessage)
    {
        this.TypeMessage = _typeMessage;
    }

    public string MessageFormat()
    {
        string hour_min_sec = TypeMessage.Timestamp.ToString("HH:mm:ss");
        return $"{TypeMessage.Pseudo} [{hour_min_sec}] : {TypeMessage.Content}";
    }

    #region Serialization & Deserialization

    public void GetObjectData(SerializationInfo _info, StreamingContext _ctxt)
    {
        TypeMessage.GetObjectData(_info, _ctxt);
    }

    public byte[] SerializeToBytes()
    {
        using (var memoryStream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, TypeMessage);
            return memoryStream.ToArray(); // Retourne un tableau d'octets
        }
    }

    public static T DeserializeFromBytes<T>(byte[] data)
    {
        using (var memoryStream = new MemoryStream(data))
        {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(memoryStream); // Reconstruit l'objet
        }
    }


    #endregion
}


