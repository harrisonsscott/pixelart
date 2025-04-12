using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

// loads json files from the data folder

/* 
The Resources folder in the editor only represents unmodified data
saved data is stored to Application.persistentDataPath

*/

public static class Load
{
    public static ImageData LoadData(string name)
    {
        string path = Application.persistentDataPath + "/" + name + ".data";
        if (File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ImageData data = formatter.Deserialize(stream) as ImageData;
            stream.Close();

            return data;
        } else {
            Debug.LogError("File " + name + ".data not found!");
            return null;
        }
    }

    // dont include the file ending for dest
    public static void SaveData(ImageData data, string dest){
        
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + dest + ".data";

        Debug.Log(path);
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }
}

public struct DataMix {
    public TextAsset textAsset;
    public ImageData imageData;
    public string name;

    public DataMix(TextAsset textAsset, ImageData imageData, string name){
        this.textAsset = textAsset;
        this.imageData = imageData;
        this.name = name;
    }
}

public enum SortMode {
    AlphabetAsc,
    AlphabetDes,
    Random
}