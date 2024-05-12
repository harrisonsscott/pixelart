using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

// place in the main image

public class Image : MonoBehaviour {
    [HideInInspector] Button button;
    public ComputeShader computeShader; // converts the json file into an image
    public ComputeShader finishedShader; // colors squares that have been completed
    public Material imageMaterial;
    public TextAsset textAsset; // json data
    public bool usingGrid;

    public ImageData data;

    [Header("Camera Movement")]
    public int originalZoom;
    public Camera cam; // main camera
    public Vector2 size;
    public Vector3 dragStart;


    public RenderTexture overlayTarget;
    [SerializeField] List<int> dataList;
    [SerializeField] List<bool> transparentList;
    private void Start() {
        cam = Camera.main;
        button = gameObject.GetComponent<Button>();
        cam.orthographicSize = originalZoom;

        // add a button if it doesnt exist
        if (button == null){
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(() => {
            Vector2 pos = GetPosition(Input.mousePosition);
            Place(pos);
            RenderImage(textAsset.text);
        });

        usingGrid = false;
    }

    public Vector2 GetPosition(Vector3 mousePos){ // turns a mouse position into a position on the image ( (0,0) is the top left )
        Vector3 worldPosition = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        Debug.Log(worldPosition);
        return new Vector2(0,0);
    }

    public void Place(int x, int y){
        
    }

    public void Place(Vector2 pos){
        Place((int)pos.x, (int)pos.y);
    }

    public RenderTexture RenderImage(string textData) // renders an image onto a material
    {
        data = JsonUtility.FromJson<ImageData>(textData);

        // decompress the data

        dataList = new List<int>();

        for (int i = 0; i < data.data.Length; i++){
            dataList.AddRange(data.data[i].Decompress());
        }

        RenderTexture target = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        overlayTarget = new RenderTexture(data.size[0], data.size[1], 24)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };

        target.Create();

        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * dataList.Count);
        dataBuffer.SetData(dataList.ToArray());

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keys.Length);
        keyBuffer.SetData(data.keys);

        ComputeBuffer finishedBuffer = new ComputeBuffer(1, sizeof(int) * data.solved.Length);
        finishedBuffer.SetData(data.solved);

        computeShader.SetTexture(0, "Result", target);
        computeShader.SetVector("Resolution", new Vector2(data.size[0], data.size[1]));
        computeShader.SetBool("Grayscale", false);

        computeShader.SetBuffer(0, "data", dataBuffer);
        computeShader.SetBuffer(0, "keys", keyBuffer);

        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);

        finishedShader.SetTexture(0, "Result", overlayTarget);
        finishedShader.SetVector("Resolution", new Vector2(data.size[0], data.size[1]));

        finishedShader.SetBuffer(0, "data", dataBuffer);
        finishedShader.SetBuffer(0, "keys", keyBuffer);
        finishedShader.SetBuffer(0, "finished", finishedBuffer);

        finishedShader.Dispatch(0, overlayTarget.width / 8, overlayTarget.height / 8, 1);

        // material.mainTexture = target;
        imageMaterial.SetTexture("_MainTex", target);
        imageMaterial.SetTexture("_Overlay", overlayTarget);
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
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y, 1, 10);

        if (cam.orthographicSize < (originalZoom - 1) && !usingGrid){
            usingGrid = true;
            RenderImage(textAsset.text);
        } else if (cam.orthographicSize > originalZoom && usingGrid){
            usingGrid = false;
            RenderImage(textAsset.text);
        }
    }

    private void Update() {
        Pan();
        Zoom();
    }
}