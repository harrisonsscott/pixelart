using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// loads json files from the data folder

public static class Load
{
    public static List<TextAsset> textAssets;
    public static List<ImageData> imageDatas;
    public static List<string> names;

    private static void Init(){
        imageDatas = new List<ImageData>();
        names = new List<string>();

        textAssets = Resources.LoadAll<TextAsset>("data").ToList();

        foreach (TextAsset textAsset in textAssets){
            ImageData data = JsonUtility.FromJson<ImageData>(textAsset.text);

            imageDatas.Add(data);
            names.Add(data.name);

        }
    }

    public static void Sort(SortMode sortMode){
        // group lists together
        List<DataMix> mix = new List<DataMix>();

        for (int i = 0; i < textAssets.Count;i++){
            mix.Add(new DataMix(
                textAssets[i],
                imageDatas[i],
                names[i]
            ));
        }

        switch(sortMode){
            case SortMode.AlphabetAsc:
                mix.Sort((x, y) => x.imageData.name.CompareTo(y.imageData.name));
                break;
            case SortMode.AlphabetDes:
                mix.Sort((x, y) => x.imageData.name.CompareTo(y.imageData.name));
                mix = mix.Flip();
                break;
            case SortMode.Random:
                break;
        }

        // unpack the lists

        for (int i = 0; i < mix.Count; i++){
            textAssets[i] = mix[i].textAsset;
            imageDatas[i] = mix[i].imageData;
            names[i] = mix[i].name;
        }
    }

    public static string LoadJson(string name)
    {
        if (textAssets == null){
            Init();
        }

        TextAsset textAsset = Resources.Load<TextAsset>(name);
        return textAsset.text;
    }

    public static List<string> LoadJson(int amount, SortMode sortMode){
        if (textAssets == null){
           Init();
        }
        
        if (amount > textAssets.Count){
            Debug.LogError("Count exceeds array size!");
            return new List<string>();
        }

        Sort(SortMode.AlphabetDes);

        foreach(var name in names){
            Debug.Log(name);
        }

        List<string> files = new List<string>();

        foreach(TextAsset asset in textAssets.GetRange(0, amount)){
            files.Add(asset.text);
        }

        return files;
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