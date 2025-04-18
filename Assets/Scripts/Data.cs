using System;
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
    public string[] tags;
}

public static class Data {
    public static int[] Decompress(ushort number, ushort length) {
        List<int> list = new List<int>();

        for (int i = 0; i < length; i++){
            list.Add(number);
        }

        return list.ToArray();
    }

    // converts compressed data.keys into readable colors in data.keysUnpacked
    public static ImageData UnpackColors(ImageData data){
        for (int i = 0; i < data.keys.Length; i++){
            string key = data.keys[i];
            string r = key.Substring(0, 2);
            string g = key.Substring(2, 2);
            string b = key.Substring(4, 2);

            int ri = Convert.ToInt16(r, 16);
            int gi = Convert.ToInt16(g, 16);
            int bi = Convert.ToInt16(b, 16);
            data.keysUnpacked[i * 4] = ri / 255f;
            data.keysUnpacked[i * 4 + 1] = gi / 255f;
            data.keysUnpacked[i * 4 + 2] = bi / 255f;
            data.keysUnpacked[i * 4 + 3] = (ri+gi+bi == 0) ? 0f : 1f; 
        }
        return data;
    }
 }
