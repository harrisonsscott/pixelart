using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // the transforms of the gameObjects that actually hold the elements
    public Transform proContainer;
    public Transform newContainer;
    public Transform popularContainer;

    public ComputeShader generateShader; // GenerateShaderPreview.compute
    public RenderTexture target;
    public Material material;
    private TextAsset[] textAssets;

    ImageData getData(int index){
        return JsonUtility.FromJson<ImageData>(textAssets[index].text);
    }

    void Start()
    {
        textAssets = Resources.LoadAll<TextAsset>("data/");
        ImageData data = getData(0);

        List<int> dataList = new List<int>();

        for (int i = 0; i < data.data.Length; i+=2){
            dataList.AddRange(Data.Decompress(data.data[i], data.data[i+1]));
        }

        data.keysUnpacked = new float[data.keys.Length * 4];

        for (int i = 0; i < data.keys.Length; i++){
            string key = data.keys[i];
            Debug.Log(key);
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

        target = new RenderTexture(32, 32, 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        
        
        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * dataList.Count);
        dataBuffer.SetData(dataList.ToArray());

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keysUnpacked.Length);
        keyBuffer.SetData(data.keysUnpacked);

        ComputeBuffer finishedBuffer = new ComputeBuffer(1, sizeof(int) * data.solved.Length);
        finishedBuffer.SetData(data.solved);

        generateShader.SetTexture(0, "Result", target);
        generateShader.SetVector("Resolution", new Vector2(32, 32));
        generateShader.SetBuffer(0, "data", dataBuffer);
        generateShader.SetBuffer(0, "keys", keyBuffer);
        generateShader.SetBuffer(0, "finished", finishedBuffer);
        generateShader.Dispatch(0, target.width / 8, target.height / 8, 1);

        GameObject element = proContainer.GetChild(0).gameObject;
        element.GetComponent<RawImage>().texture = target;
    }
}
