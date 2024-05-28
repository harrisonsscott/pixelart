using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// loads json files from the data folder

public static class Load
{
    public static List<TextAsset> textAssets;
    public static string LoadJson(string name)
    {
        if (textAssets == null){
            textAssets = Resources.LoadAll<TextAsset>("").ToList();
        }

        TextAsset textAsset = Resources.Load<TextAsset>(name);
        return textAsset.text;
    }

    public static List<string> LoadJson(int amount, SortMode sortMode){
        if (textAssets == null){
            textAssets = Resources.LoadAll<TextAsset>("").ToList();

            foreach(var t in textAssets){
                Debug.Log("a - " + t);
            }
        }

        Debug.Log(amount + " - " + textAssets.Count);
        
        if (amount > textAssets.Count){
            Debug.LogError("Count exceeds array size!");
            return new List<string>();
        }

        List<string> files = new List<string>();

        foreach(TextAsset asset in textAssets.GetRange(0, amount)){
            files.Add(asset.text);
            Debug.Log(asset.text);
        }

        return files;
    }
}

public enum SortMode {
    AlphabetFirst,
    AlphabetLast,
    Random
}