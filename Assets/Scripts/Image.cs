using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// place in the main image

public class Image : MonoBehaviour {
    [HideInInspector] Button button;
    public ComputeShader computeShader; // converts the json file into an image
    public ComputeShader finishedShader; // colors squares that have been completed
    public ComputeShader textShader;
    public Material imageMaterial;
    public TextAsset textAsset; // json data
    public bool usingGrid;
    public RenderTexture overlayTarget;
    public RenderTexture textTarget;

    [SerializeField] List<int> dataList;
    [SerializeField] List<bool> transparentList;

    public Texture2DArray numbers;

    public ImageData data;
    public int[] solved;
    public Vector2 resolution; // resolution of the image rendered to the screen (not actual amount of pixels)

    [Header("Camera Movement")]
    public int originalZoom;
    public Camera cam; // main camera
    public Vector2 size;
    public Vector3 dragStart;


    private void Start() {
        cam = Camera.main;
        button = gameObject.GetComponent<Button>();
        cam.orthographicSize = originalZoom;

        // add a button if it doesnt exist
        if (button == null){
            button = gameObject.AddComponent<Button>();
        }

        NewImage(textAsset.text);
        RenderImage();

        button.onClick.AddListener(() => {
            Vector2 pos = GetPosition(Input.mousePosition);
            Place(pos);
            RenderImage();
        });

        usingGrid = false;
    }

    public Vector2 GetPosition(Vector3 mousePos){ // turns a mouse position into a position on the image ( (0,0) is the top left )
        Vector2 worldPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        Vector2 gridPos = (worldPosition * 50) / (new Vector2(512.0f, 512.0f) / resolution);
        
        // set (0,0) to the top left
        gridPos.x += resolution.x / 2.0f;
        gridPos.y -= (resolution.y / 2.0f) - 1;
        
        gridPos.x = Mathf.Floor(gridPos.x);
        gridPos.y = -Mathf.Floor(gridPos.y);

        return gridPos;
    }

    public void Place(int x, int y){
        solved[(int)(y * resolution.y + x)] = 1;
    }

    public void Place(Vector2 pos){
        Place((int)pos.x, (int)pos.y);
    }

    public void NewImage(string textData){
        data = JsonUtility.FromJson<ImageData>(textData);

        dataList = new List<int>();
        solved = data.solved;

        for (int i = 0; i < data.data.Length; i++){
            dataList.AddRange(data.data[i].Decompress());
        }

    }

    public RenderTexture RenderImage() // renders an image onto a material
    {
        RenderTexture target = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        textTarget = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        overlayTarget = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        resolution = new Vector2(data.size[0], data.size[1]);

        target.Create();
        overlayTarget.Create();

        Debug.Log(dataList);
        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * dataList.Count);
        dataBuffer.SetData(dataList.ToArray());

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keys.Length);
        keyBuffer.SetData(data.keys);

        ComputeBuffer finishedBuffer = new ComputeBuffer(1, sizeof(int) * data.solved.Length);
        finishedBuffer.SetData(solved);

        computeShader.SetTexture(0, "Result", target);
        computeShader.SetVector("Resolution", resolution);
        computeShader.SetBuffer(0, "data", dataBuffer);
        computeShader.SetBuffer(0, "keys", keyBuffer);
        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);

        finishedShader.SetTexture(0, "Result", overlayTarget);
        finishedShader.SetVector("Resolution", resolution);
        finishedShader.SetBuffer(0, "data", dataBuffer);
        finishedShader.SetBuffer(0, "keys", keyBuffer);
        finishedShader.SetBuffer(0, "finished", finishedBuffer);
        finishedShader.Dispatch(0, overlayTarget.width / 8, overlayTarget.height / 8, 1);

        textShader.SetTexture(0, "Result", textTarget);
        textShader.SetVector("Resolution", resolution);
        textShader.SetBuffer(0, "data", dataBuffer);
        textShader.Dispatch(0, textTarget.width / 8, textTarget.height / 8, 1);

        // material.mainTexture = target;
        imageMaterial.SetTexture("_MainTex", target);
        imageMaterial.SetTexture("_Overlay", overlayTarget);
        imageMaterial.SetFloatArray("_GridSize", resolution.ToArray());
        imageMaterial.SetTexture("_TextData", textTarget);
        imageMaterial.SetFloat("_Grid", usingGrid == true ? 1 : 0);

        gameObject.GetComponent<RawImage>().material = imageMaterial;

        return target;
    }

    private void Pan(){
        if (Input.GetMouseButtonDown(0)){
            dragStart = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0)){
            Vector3 difference = dragStart - cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 size = new Vector2(originalZoom * (Screen.width/(float)Screen.height), originalZoom);

            cam.transform.position += difference;
            cam.transform.position = cam.transform.position.Clamp(new Vector3(-size.x, -size.y, 0), new Vector3(size.x, size.y,0));
        }
    }

    private void Zoom(){
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y, 1, 20);

        if (cam.orthographicSize < (originalZoom - 1) && !usingGrid){
            usingGrid = true;
            RenderImage();
        } else if (cam.orthographicSize > originalZoom && usingGrid){
            usingGrid = false;
            RenderImage();
        }
    }

    private void Update() {
        Pan();
        Zoom();
    }
}