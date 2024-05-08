using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

// place in the main image

public class Image : MonoBehaviour {
    [HideInInspector] Button button;
    public ComputeShader computeShader;
    public Material imageMaterial;
    public TextAsset textAsset;

    ImageData data;

    [Header("Camera Movement")]
    public Camera cam;
    public Vector3 dragStart;

    private void Start() {
        cam = Camera.main;
        button = gameObject.GetComponent<Button>();

        // add a button if it doesnt exist
        if (button == null){
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(() => {
            RenderImage(textAsset.text);
            Debug.Log("i");
        });
    }

    public RenderTexture RenderImage(string textData) // renders an image onto a material
    {
        data = JsonUtility.FromJson<ImageData>(textData);

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
        computeShader.SetBool("Grayscale", false);

        computeShader.SetBuffer(0, "data", dataBuffer);
        computeShader.SetBuffer(0, "keys", keyBuffer);

        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);
        
        // material.mainTexture = target;
        imageMaterial.SetTexture("_MainTex", target);
        gameObject.GetComponent<RawImage>().material = imageMaterial;

        RenderText();

        return target;
    }

    private void RenderText() // renders a number onto the image's pixels
    {
        Vector2 imageSize = new Vector2(data.size[0], data.size[1]);
    }

    private void Pan(){
        if (Input.GetMouseButtonDown(0)){
            dragStart = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0)){
            Vector3 difference = dragStart - cam.ScreenToWorldPoint(Input.mousePosition);

            cam.transform.position += difference;
        }
    }

    private void Update() {
        Pan();
    }
}