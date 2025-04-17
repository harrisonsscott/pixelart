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
    public Transform continuePlayingContainer;
    public GameObject previewReference; // a 256x256 gameobject with a raw image 
    public ComputeShader generateShader; // GenerateShaderPreview.compute
    public RenderTexture target;
    public Material material;
    private TextAsset[] textAssets;

    ImageData getData(int index){
        return JsonUtility.FromJson<ImageData>(textAssets[index].text);
    }

    Texture generateImage(ImageData data){
        List<int> dataList = new List<int>();

        for (int i = 0; i < data.data.Length; i+=2){
            dataList.AddRange(Data.Decompress(data.data[i], data.data[i+1]));
        }

        data.keysUnpacked = new float[data.keys.Length * 4];

        data = Data.UnpackColors(data);

        target = new RenderTexture(data.size[0], data.size[1], 24)
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
        generateShader.SetVector("Resolution", new Vector2(data.size[0], data.size[1]));
        generateShader.SetBuffer(0, "data", dataBuffer);
        generateShader.SetBuffer(0, "keys", keyBuffer);
        generateShader.SetBuffer(0, "finished", finishedBuffer);
        generateShader.Dispatch(0, target.width / 8, target.height / 8, 1);
        
        return target;
    }

    void Start()
    {
        // load the menu
        textAssets = Resources.LoadAll<TextAsset>("data/");
        for (int i = 0; i < textAssets.Length; i++)
        {
            ImageData data = getData(i);

            GameObject element = Instantiate(previewReference);
            element.transform.parent = proContainer;
            element.GetComponent<RawImage>().texture = generateImage(data);
        }

        // load continue playing
        List<ImageData> files = Load.LoadAllData();
        for (int i = 0; i < files.Count; i++)
        {
            GameObject element = Instantiate(previewReference);
            element.transform.parent = continuePlayingContainer;
            element.GetComponent<RawImage>().texture = generateImage(files[i]);
        }
    }
}
