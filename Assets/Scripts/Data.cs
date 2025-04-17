using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageData
{
    public string name;
    public ushort[] data;
    public string[] keys; // hex values
    public float[] keysUnpacked;
    public int[] size;
    public int[] solved;
}

public static class Data {
    public static int[] Decompress(ushort number, ushort length) {
        List<int> list = new List<int>();

        for (int i = 0; i < length; i++){
            list.Add(number);
        }

        return list.ToArray();
    }
 }
