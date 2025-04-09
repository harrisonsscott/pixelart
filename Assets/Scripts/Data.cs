using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompressedData {
    public int number;
    public int length;
    public int[] Decompress() {
        List<int> list = new List<int>();

        for (int i = 0; i < length; i++){
            list.Add(number);
        }

        return list.ToArray();
    }
}

[System.Serializable]
public class ImageData
{
    public string name;
    public CompressedData[] data;
    public string[] keys; // hex values
    public float[] keysUnpacked;
    public int[] size;
    public int[] solved;
}

public class Data : MonoBehaviour
{
    private void Start() {
        Load.LoadJson(1, SortMode.Random);
    }
}
