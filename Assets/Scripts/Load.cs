using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

// loads json files from the data folder

/* 
The Resources folder in the editor only represents unmodified data
saved data is stored to Application.persistentDataPath

*/

public static class Load
{
    // loads a json file from the persistent data path
    public static ImageData LoadData(string name)
    {
        string path = Application.persistentDataPath + "/data/" + name + ".json";
        if (File.Exists(path)){
            string dataRaw = File.ReadAllText(path);
            ImageData data = JsonUtility.FromJson<ImageData>(dataRaw);

            return data;
        } else {
            Debug.LogError("File " + name + ".data not found!");
            return null;
        }
    }

    public static List<string> LoadAllDataNames()
    {
        string path = Application.persistentDataPath + "/data/";
        string[] filesRaw = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
        string[] files = new string[filesRaw.Length];

        for (int i = 0; i < filesRaw.Length; i++)
        {
            files[i] = System.IO.Path.GetFileNameWithoutExtension(filesRaw[i]);
        }

        return files.ToList();

    }

    public static List<ImageData> LoadAllData()
    {
        List<ImageData> files = new List<ImageData>();
        string path = Application.persistentDataPath + "/data/";
        string[] filesString = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (string file in filesString)
        {
            files.Add(JsonUtility.FromJson<ImageData>(File.ReadAllText(file)));
        }

        return files;
    }

    // saves a json file to the persistent data path, dont include the file ending in dest
    public static void SaveData(ImageData data, string dest){
        if (dest == ""){
            return;
        }
        string path = Application.persistentDataPath + "/data/" + dest + ".json";
        File.WriteAllText(path, JsonUtility.ToJson(data));
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