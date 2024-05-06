using System.IO;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ImageData
{
    public int[] data;
    public float[] keys;
}

public class ComputeShaderScript : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture target;
    public TextAsset textAsset;
    public ImageData data;
    public Material material;

    private void Load(string textData){
        data = JsonUtility.FromJson<ImageData>(textData);

        target = new RenderTexture(64, 64, 24)
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
        material.SetTexture("_MainTex", target);
    }

    private void Start() {
        Load(textAsset.text);
    }
}
