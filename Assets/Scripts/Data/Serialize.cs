using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataSerialize 
{
    public static byte[] SerializeToBytes(object data)
    {
        using (var memoryStream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, data);
            return memoryStream.ToArray();
        }
    }

    public static Type DeserializeTypeFromBytes(byte[] data)
    {
        using (var memoryStream = new MemoryStream(data))
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(memoryStream).GetType();
        }
    }

    public static T DeserializeFromBytes<T>(byte[] data)
    {
        using (var memoryStream = new MemoryStream(data))
        {
            var formatter = new BinaryFormatter();
            object formatData = formatter.Deserialize(memoryStream);
            if(formatData is T typeData)
            return typeData;
        }
        return default;
    }
}
