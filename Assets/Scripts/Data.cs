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

public class Data : MonoBehaviour
{
    private void Start() {
        Load.LoadJson(1, SortMode.Random);
    }
}
