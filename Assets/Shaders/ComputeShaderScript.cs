using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderScript : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture target;
    // Start is called before the first frame update
    private void OnRenderImage(RenderTexture src, RenderTexture dest){
        target = new RenderTexture(256, 256, 24)
        {
            enableRandomWrite = true
        };
        target.Create();

        computeShader.SetTexture(0, "Result", target);
        computeShader.SetVector("Resolution", new Vector2(target.width, target.height));
        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);

        Graphics.Blit(target, dest);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
