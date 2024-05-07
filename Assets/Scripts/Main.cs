using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageData
{
    public int[] data;
    public float[] keys;
    public int[] size;
}

public class Main : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material imageMaterial;
    public TextAsset textAsset;

    public RenderTexture RenderImage(string textData) // renders an image onto a material
    {
        ImageData data = JsonUtility.FromJson<ImageData>(textData);

        RenderTexture target = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };
        target.Create();

        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * data.data.Length);
        dataBuffer.SetData(data.data);

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keys.Length);
        keyBuffer.SetData(data.keys);

        computeShader.SetTexture(0, "Result", target);
        computeShader.SetVector("Resolution", new Vector2(target.width, target.height));

        computeShader.SetBuffer(0, "data", dataBuffer);
        computeShader.SetBuffer(0, "keys", keyBuffer);

        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);
        
        // material.mainTexture = target;
        imageMaterial.SetTexture("_MainTex", target);

        return target;
    }

    private void Start() {
        RenderImage(textAsset.text);
    }
}
