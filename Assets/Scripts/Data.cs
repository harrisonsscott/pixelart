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
    public CompressedData[] data;
    public float[] keys;
    public int[] size;
    public int[] solved;
}

public class Data : MonoBehaviour
{
    private void Start() {
        
    }
}
