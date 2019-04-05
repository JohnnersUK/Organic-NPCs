using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

public class NetworkIO : MonoBehaviour
{
    public static NetworkIO instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Save a network as a binary file
    public void SerializeObject<T>(string filename, T obj)
    {
        Stream stream = File.Open(filename, FileMode.Create);
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        binaryFormatter.Serialize(stream, obj);
        stream.Close();
    }

    // Load a network from a binary file
    public T DeSerializeObject<T>(string filename)
    {
        T objectToBeDeSerialized;

        Stream stream = File.Open(filename, FileMode.Open);
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        objectToBeDeSerialized = (T)binaryFormatter.Deserialize(stream);
        stream.Close();

        return objectToBeDeSerialized;
    }
}
