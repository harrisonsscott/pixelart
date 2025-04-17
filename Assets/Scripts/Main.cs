using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// place in the main image

// renders the main image along with camera movement for it

public class Main : MonoBehaviour {
    [HideInInspector] Button button;

    [Header("Shaders")]
    public ComputeShader generateShader; // converts the json file into an image
    public ComputeShader finishedShader; // colors squares that have been completed
    public ComputeShader textShader;
    public Material imageMaterial;

    [Header("Data")]
    int timer; // used for counting, increased by 1 each frame
    public bool usingGrid;
    [SerializeField] List<int> dataList;
    [SerializeField] List<bool> transparentList;
    [SerializeField] List<Vector4> colorsList;
    public List<int> amountList; // how many pixels of each color are in the image
    public List<int> amountFilledList; // how many pixels of each color have been filled in

    public int currentNumber;
    public int CurrentNumber {
        get {
            return currentNumber;
        }
        set {
            imageMaterial.SetFloat("_NumSelected", value);
            currentNumber = value;
        }
    }

    [Header("Textures")]
    public RenderTexture overlayTarget;
    public RenderTexture textTarget; // generated in textShader.compute and sampled in imageMat.shader
    public RenderTexture target;
    public Texture2DArray numbers; // 2d texture array with numbers 0-9

    public ImageData data;
    public Vector2 resolution; // resolution of the image rendered to the screen (not actual amount of pixels)

    [Header("Camera Movement")]
    public bool isDrawing; // determines whether the player is drawing or panning
    public int originalZoom;
    public Camera cam; // main camera
    public Vector2 size;
    public Vector3 dragStart;
    public RaycastHit hit;
    public Vector3 initialDiff; // difference between two fingers while pinching
    public float initialZoom;

    [Header("Raycasting")]
    public GraphicRaycaster mainCanvasGR;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    [Header("UI")]
    public UI classUI;

    private void Start() {
        timer = 1;

        if (classUI == null)
            classUI = FindAnyObjectByType<UI>();
    
        cam = Camera.main;
        button = gameObject.GetComponent<Button>();
        cam.orthographicSize = originalZoom;

        // add a button if it doesnt exist
        if (button == null){
            button = gameObject.AddComponent<Button>();
        }

        // a random image is selected from resources/data/
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>("data/");
        NewImage(textAssets[UnityEngine.Random.Range(0, textAssets.Length-1)].text);
        
        // load an already started file
        // if (File.Exists(Application.persistentDataPath + "/CherryPie.json")){
        //     string cherryPie = JsonUtility.ToJson(Load.LoadData("CherryPie"));
        //     NewImage(cherryPie);
        // } else {
        //     TextAsset cherryPie = Resources.Load<TextAsset>("data/CherryPie");
        //     NewImage(cherryPie.text);
        // }

        RenderImage();
        ChangeCurrentNumber(5);

        button.onClick.AddListener(() => {
            Vector2 pos = GetPosition(Input.mousePosition);
            if (GetNumber(pos) == CurrentNumber){
                Place(pos);
            }
            RenderImage();
            // Raycast();
        });

        usingGrid = false;
        CurrentNumber = 1;
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
        if (!IsDrawn(x, y)){
            classUI.ChangeProgress(currentNumber, (float)(amountFilledList[currentNumber]+1)/amountList[currentNumber]);
            amountFilledList[currentNumber] += 1;
        }
        data.solved[(int)(y * resolution.y + x)] = 1;
        RenderImage();
    }

    public void Place(Vector2 pos){
        Place((int)pos.x, (int)pos.y);
    }

    public bool IsDrawn(int x, int y){ // returns true if the selected pixel has been colored in
        int index = (int)(y * resolution.y + x);
        if (data.solved.Length < index){
            return false;
        }
        
        return data.solved[index] == 1;
    }

    public bool IsDrawn(Vector2 pos){
        return IsDrawn((int)pos.x, (int)pos.y);
    }

    public int GetNumber(int x, int y){ // returns the number of the selected pixel
        int index = (int)(y * resolution.y + x);
        if (dataList.Count < index){
            return -1;
        }

        return dataList[index];
    }

    public int GetNumber(Vector2 pos){
        return GetNumber((int)pos.x, (int)pos.y);
    }

    public void NewImage(string textData){
        data = JsonUtility.FromJson<ImageData>(textData);

        dataList = new List<int>();
        colorsList = new List<Vector4>();

        // decompress the data
        for (int i = 0; i < data.data.Length; i+=2){
            dataList.AddRange(Data.Decompress(data.data[i], data.data[i+1]));
        }

        // calculate how many pixels of each color there are
        for (int i = 0; i < dataList.Count; i++){
            int index = dataList[i];
            while (amountList.Count <= index){
                amountList.Add(0);
                amountFilledList.Add(0);
            }
            amountList[index]++;
        }

        data.keysUnpacked = new float[data.keys.Length * 4];

        // unpack the keys
        data = Data.UnpackColors(data);

        // extract the colors (ignores the transparent color)
        for (int i = 4; i < data.keysUnpacked.Length; i+=4){
            Vector4 col = new Vector4(0,0,0,0);

            for (int v = 0; v < 4; v++){
                col[v] = data.keysUnpacked[i + v];
            }        

            if (col[3] > 0){
                colorsList.Add(col);
                classUI.PlaceColor(col);
            }
        }

        classUI.SelectColor(1); // select the first color automatically
    }

    public RenderTexture RenderImage() // renders an image onto a material
    {
        target = new RenderTexture(data.size[0], data.size[1], 24)
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

        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * dataList.Count);
        dataBuffer.SetData(dataList.ToArray());

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keysUnpacked.Length);
        keyBuffer.SetData(data.keysUnpacked);

        ComputeBuffer finishedBuffer = new ComputeBuffer(1, sizeof(int) * data.solved.Length);
        finishedBuffer.SetData(data.solved);

        generateShader.SetTexture(0, "Result", target);
        generateShader.SetVector("Resolution", resolution);
        generateShader.SetBuffer(0, "data", dataBuffer);
        generateShader.SetBuffer(0, "keys", keyBuffer);
        generateShader.Dispatch(0, target.width / 8, target.height / 8, 1);

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
        imageMaterial.SetVector("_GridSize", resolution);
        imageMaterial.SetTexture("_TextData", textTarget);
        imageMaterial.SetFloat("_Grid", usingGrid == true ? 1 : 0);

        gameObject.GetComponent<RawImage>().material = imageMaterial;

        return target;
    }

    private void ChangeCurrentNumber(int number){ // changes the current number and updates the shader
        currentNumber = number;

        imageMaterial.SetFloat("_NumSelected", currentNumber);
    }

    private void Pan(){
        #if !UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)){
            Vector2 pos = GetPosition(Input.GetTouch(0).position);
            if (GetNumber(pos) == currentNumber && !IsDrawn(pos)){
                isDrawing = true;
                return;
            } else {
                isDrawing = false;
            }
            dragStart = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        }

        if (Input.GetMouseButton(0)){
            if (isDrawing){
                Vector2 pos = GetPosition(Input.GetTouch(0).position);
                if (GetNumber(pos) == currentNumber && !IsDrawn(pos)){
                    Place(pos);
                    return;
                }
            } else {
                Vector3 difference = dragStart - cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector2 size = new Vector2(originalZoom * (Screen.width/(float)Screen.height), originalZoom);

            
                cam.transform.position += difference;
                cam.transform.position = cam.transform.position.Clamp(new Vector3(-size.x, -size.y, 0), new Vector3(size.x, size.y,0));
                return;
            }
        }
        #endif
    }

    private void Zoom(){
        #if !UNITY_EDITOR
        // pinch to zoom on phone
        if (Input.touchCount >= 2){
            if (initialDiff == Vector3.zero){
                initialDiff = Input.GetTouch(1).position - Input.GetTouch(0).position;
                initialZoom = cam.orthographicSize;
            }
            Vector3 currentZoomDiff = Input.GetTouch(1).position - Input.GetTouch(0).position;
            float zoomDiff = currentZoomDiff.magnitude - initialDiff.magnitude;
            Debug.Log(zoomDiff);
            cam.orthographicSize = Mathf.Clamp(initialZoom - zoomDiff/200f, 1, 20);

            if (cam.orthographicSize < (originalZoom - 1) && !usingGrid){
                usingGrid = true;
                RenderImage();
            } else if (cam.orthographicSize > originalZoom && usingGrid){
                usingGrid = false;
                RenderImage();
            }
        } else {
            initialDiff = Vector3.zero;
        }
        #else
        // scroll to zoom on the computer
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0f){
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scrollDelta, 1, 20);

            if (cam.orthographicSize < (originalZoom - 1) && !usingGrid){
                usingGrid = true;
                RenderImage();
            } else if (cam.orthographicSize > originalZoom && usingGrid){
                usingGrid = false;
                RenderImage();
            }    
        }
        #endif
    }

    private void Update() {
        if (!UsingUI()){
            Pan();
            Zoom();
        }
    }

    private void FixedUpdate() {
        timer++;

        if (timer % 120 == 0){
            Load.SaveData(data, data.name);
        }
    }

    public bool UsingUI(){ // raycasts to see if the user is current using the ui
        m_PointerEventData = new PointerEventData(m_EventSystem){
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();

        mainCanvasGR.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            return true;
        }
        
        return false;
    }
}