using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CompressedData {
    public int number;
    public int length;
    public bool transparent;

    public int[] DecompNumbers() {
        List<int> list = new List<int>();

        for (int i = 0; i < length; i++){
            list.Add(number);
        }

        return list.ToArray();
    }

    public bool[] DecompTransparent(){
        List<bool> list = new List<bool>();

        for (int i = 0; i < length; i++){
            list.Add(transparent);
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

public class Main : MonoBehaviour
{
    private void Start() {
        
    }
}
