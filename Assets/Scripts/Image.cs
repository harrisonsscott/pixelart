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
    public Camera cam;
    public Vector2 size;
    public Vector3 dragStart;


    public RenderTexture overlayTarget;
    private void Start() {
        cam = Camera.main;
        button = gameObject.GetComponent<Button>();
        cam.orthographicSize = originalZoom;

        // add a button if it doesnt exist
        if (button == null){
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(() => {
            RenderImage(textAsset.text);
        });

        usingGrid = false;
    }

    public RenderTexture RenderImage(string textData) // renders an image onto a material
    {
        data = JsonUtility.FromJson<ImageData>(textData);

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

        ComputeBuffer dataBuffer = new ComputeBuffer(1, sizeof(int) * data.data.Length);
        dataBuffer.SetData(data.data);

        ComputeBuffer keyBuffer = new ComputeBuffer(1, sizeof(float) * data.keys.Length);
        keyBuffer.SetData(data.keys);

        computeShader.SetTexture(0, "Result", target);
        computeShader.SetVector("Resolution", new Vector2(data.size[0], data.size[1]));
        computeShader.SetBool("Grayscale", false);

        computeShader.SetBuffer(0, "data", dataBuffer);
        computeShader.SetBuffer(0, "keys", keyBuffer);

        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);

        finishedShader.SetTexture(0, "Result", overlayTarget);
        
        // material.mainTexture = target;
        imageMaterial.SetTexture("_MainTex", target);
        imageMaterial.SetFloat("_Grid", usingGrid == true ? 1 : 0);
        imageMaterial.SetFloatArray("_Solved", data.solved);

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