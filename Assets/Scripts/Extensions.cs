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

    // other

    public static int ToInt(this string str){ // converts a string into an int
        int num;
        int.TryParse(str, out num);
        return num;
    }

    public static bool HasComponent <T>(this GameObject obj) where T:Component{
        return obj.GetComponent<T>() != null;
    }
}