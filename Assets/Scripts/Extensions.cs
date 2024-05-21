using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// extension functions
public class Func {
    public static Color HexToRGB(string hex){ // converts a hex code into an RGB value, ex "#ff0500" -> (255, 5 ,0)
        int shift = 0;

        if (hex.Substring(0, 1) == "#"){
            shift = 1; // ignore the hashtag if it exists
        }

        string r = hex.Substring(0 + shift, 2);
        string g = hex.Substring(2 + shift, 2);
        string b = hex.Substring(4 + shift, 2);
        
        return new Color(
            int.Parse(r, NumberStyles.HexNumber) / 255f,
            int.Parse(g, NumberStyles.HexNumber) / 255f,
            int.Parse(b, NumberStyles.HexNumber) / 255f);

    }

    public static string RGBToHex(Color rgb){
        return ColorUtility.ToHtmlStringRGBA(rgb);
    }
}

public static class Extensions {
    // color functions

    public static string ToHex (this Color color){ // converts a color object into a hex string
        return Func.RGBToHex(color);
    }

    public static Color ToRGB (this string str){ // converts a hex string into a color object
        return Func.HexToRGB(str);
    }

    public static int[] ToRGBInt (this string str){
        Color color = Func.HexToRGB(str);
        return new int[3] {(int)color.r, (int)color.g, (int)color.b};
    }

    public static Color ToColor (this Vector4 v){
        return new Color(v[0], v[1], v[2], v[3]);
    }

    // vectors

    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max){
        float clampedX = Mathf.Clamp(vector.x, min.x, max.x);
        float clampedY = Mathf.Clamp(vector.y, min.y, max.y);
        float clampedZ = Mathf.Clamp(vector.z, min.z, max.z);
        return new Vector3(clampedX, clampedY, clampedZ);
    }

    public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max){
        float clampedX = Mathf.Clamp(vector.x, min.x, max.x);
        float clampedY = Mathf.Clamp(vector.y, min.y, max.y);
        return new Vector2(clampedX, clampedY);
    }

    public static float[] ToArray(this Vector2 vec){
        return new float[2]{vec.x, vec.y};
    }

    public static float[] ToArray(this Vector3 vec){
        return new float[3]{vec.x, vec.y, vec.z};
    }

    // other

    public static int ToInt(this string str){ // converts a string into an int
        int num;
        int.TryParse(str, out num);
        return num;
    }

    public static bool HasComponent <T>(this GameObject obj) where T:Component{
        return obj.GetComponent<T>() != null;
    }

    public static List<float> ToFloat(this List<int> list){
        List<float> floatList = new List<float>(list.Count);

        foreach (int value in list)
        {
            float floatValue = (float)value;
            floatList.Add(floatValue);
        }

        return floatList;
    }
}