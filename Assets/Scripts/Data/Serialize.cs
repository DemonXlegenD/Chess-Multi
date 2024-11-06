using System;
using System.Diagnostics;
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
            try
            {
                var formatter = new BinaryFormatter();
                object formatData = formatter.Deserialize(memoryStream);
                T result = (T)formatData;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        return default;
    }
}
