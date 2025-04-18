# Pixel Art!

This is a pretty common mobile game idea; coloring in images. I decided to do my own approach to it, where you start with an image that is turned into json, and then that json is turned back into a drawable image.

![img1]

## How it works
### To Json
In the *Generate* folder, you can find the c++ code for turning the image into json. It uses *[Opencv]* to loop over every pixel, and pixels with similar colors are grouped together. The threshold parameter can be used to specify how close in color pixels need to be for them to get grouped together.
Transparent pixels aren't suppose, so black pixels are used in their place.
The json file structure looks like this:
>{
   "data": [0, 2, 1, 1, 2, 1, 3, 1, 4... ],
    "invisPixels": 0,
    "keys": [
        &nbsp;&nbsp;&nbsp;"b00000", "613000", "615200", "096100", "003261", "2b0061", "610057", 
        &nbsp;&nbsp;&nbsp;"2a1404", "ff7e00", "ffd800", "18ff00", "0084ff", "7200ff", "ff00e4"
    ],
    "name": "debug",
    "size": [8, 8],
    "solved": [0, 0, 0, 0, 0, 0, 0, 0...],
    "tags": [
        &nbsp;&nbsp;&nbsp;"rainbow", "colorful", "pride", "pixelart", "spectrum"]
}

The first number in *data* represents the index of the color in keys, and the second number represents how many pixel that color repeats for. This repeats for the third and four number, and so on.
So 0, 2, 1, 1 means #b00000 (1st element of keys) for 2 pixels, and #613000 (2nd element of keys) for 1 pixel. The data goes from top left to bottom right. This repeats until every pixel has been covered.
*Solved* is from saving data. 0 represents a pixel that is uncolored, while 1 represents colored.

### From Json
In Unity, the json file is converted to the *ImageData* struct.
>[System.Serializable]
public class ImageData
{
    &nbsp;&nbsp;&nbsp;public string name;
    &nbsp;&nbsp;&nbsp;public ushort[] data;
    &nbsp;&nbsp;&nbsp;public string[] keys;
    &nbsp;&nbsp;&nbsp;public float[] keysUnpacked;
    &nbsp;&nbsp;&nbsp;public int[] size;
    &nbsp;&nbsp;&nbsp;public int[] solved;
}

The keys, which are hex value strings, are split into red, green, and blue channels each ranging from 0 to 1. The RGB channels are added to keysUnpacked, where they're are eventually packed into *Vector4* data types.
>for (int i = 4; i < data.keysUnpacked.Length; i+=4){
           &nbsp;&nbsp;&nbsp;Vector4 col = new Vector4(0,0,0,0);
            &nbsp;&nbsp;&nbsp;for (int v = 0; v < 4; v++){
               &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; col[v] = data.keysUnpacked[i + v];
       &nbsp;&nbsp;&nbsp;     } 
   &nbsp;&nbsp;&nbsp;         if (col[3] > 0){
      &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;          colorsList.Add(col);
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;        classUI.PlaceColor(col);
     &nbsp;&nbsp;&nbsp;       }
        }
 
 An addition alpha channel is added, and is set to 1 if the color is black (again, representing transparency), otherwise 0.

Data is decompressed with a very simple loop function.
>public static int[] Decompress(ushort number, ushort length) {
        &nbsp;&nbsp;&nbsp;List<int> list = new List<int>();
        &nbsp;&nbsp;&nbsp;for (int i = 0; i < length; i++){
            &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;list.Add(number);
        &nbsp;&nbsp;&nbsp;}
        &nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;return list.ToArray();
   }

### Back To Image
To get the data back to a drawable image, multiple shaders are used.

Firstly, the keys are converted into an image in *textShader.compute*, so that they can be sampled in the *imageMat.shader*.
> Result[id.xy] = float4(data[Resolution.x * Resolution.y - (id.y + 1) * Resolution.y + id.x] / 255.0 ,0,0,1);
> 
The result ends up looking like a very faint red image.
![img2]

The color can be converted back to a number in *imageMat.shader* with
> _NumIndex = tex2D(_TextData, index).r * 255.0;

Then the shader makes a black and white image and overlays what colors needs to be drawn on each pixel
>if (_NumIndex >= 10){
          &nbsp;&nbsp;&nbsp;           fixed4 col0 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv+float2(_Spacing, 0), 
          &nbsp;&nbsp;&nbsp;           int(_NumIndex / 10))); // first digit
               &nbsp;&nbsp;&nbsp;      fixed4 col1 = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv-float2(_Spacing, 0), 
           &nbsp;&nbsp;&nbsp;          _NumIndex - int(_NumIndex / 10) * 10 )); // second digit
               &nbsp;&nbsp;&nbsp;      col = col0 + col1;
                } else {
                 &nbsp;&nbsp;&nbsp;    col = UNITY_SAMPLE_TEX2DARRAY(_Numbers, float3(uv,_NumIndex));
                }
                
A 2d array with images of numbers 0-9 is sampled and sampled above.

![img3]
Using raycasts, the image can be drawn on.
![img4]

[Opencv]: https://opencv.org/
[img1]:https://media-hosting.imagekit.io/7800bb5f84cd421c/bananasRaw.png?Expires=1839559110&Key-Pair-Id=K2ZIVPTIP2VGHC&Signature=KIy1yYHo~--Cs4po~mF39WOl2M8qJU4kzHg70RqQf~OOTgYivXQ0z2UVSdBPEvWjGz4iwn4aUr6MhiHkjFGSy3LbZLLHO7ZmNKGTzK07TMr3rXoxaqt3mBMCSPB4~wWgJYOwqOYz15SUtyPoInN3Umxfq~Iv3w94k3xEGdIgxMDmH3lJit4tKzVK5sapLt6P89lBXzunAsus~agR9UbUxvCfiKLxyAEx58suJvDmoOnqx9KsN5ThFyPydAgmjl8Mlb~XMnxwkZmZy70x4GwxWu06vkpNdrxNXl7UQEbALpfYHz7hjOSFo8I6DOjBVANyVEC9RpAw~tgdZtUMFxdC4w__
[img2]: https://media-hosting.imagekit.io/eec360620b0647bf/bananasRed.png?Expires=1839558865&Key-Pair-Id=K2ZIVPTIP2VGHC&Signature=xMljEacqA2WpOKRmRteBwbeK3F0~s8ZATBGnpoTqn~AwvpV2KEbMZb9suHENxhfaUEZ9LaVjpzEkMBCCY-gt1oZoXRMF8xffUP04Xs95gurgkDLlazMzQLQmwhCwNOTrxvXu6v0WGoLFaVUxmYHmnE3Mhk9kG2G75Q6eBrPakh6f0rKpruxW2PrMZL-LkTGy~PWBwilrlR5m9Bp3YHsQElQtOJqdTY9DyyiMfH6wWXQDeBoQSzE5KYMg~Nf90vj9wFsoXVqgc35A9nLBI1eVSC3Up1S7B0osQ9I2KS2~EuD-r4G1XdwBBueyX4tjEXQM83n2iEl8h6pt48UcilsnZg__
[img3]: https://media-hosting.imagekit.io/76fecef650644e65/bananas.png?Expires=1839558865&Key-Pair-Id=K2ZIVPTIP2VGHC&Signature=iO4JR3h9jXwgsBceaF~8sjfTVEy7HDLMr4KOC5q8M2cZoWrbmBQv2SBEZS1DEAC7fr5J2Sg95IEjDF-0~-fJ9aW-g0Y0-U~QpFfHGhi0HO0aDgDfl9G9UQpZLyANpXlGNEICGuumml50ePSwfD5A~L55un9xPrwtkh4CHYHNuyS61KJXeu1xGdmm4C3CE~sTaQo4d0IFL-n03g7YpMNnCtoaZgvRzsbkGErFdAA-izrvUfOPPrTcx6b2Wu2QMPVyEykGWZ3WQpxhUdZhOeb4IEW-Npp5~aOfZFrtDvuFx-CkU-EK6ImTFrTIC6dL6UtZnqgy7oMdYwGmdtsnbk3uUA__
[img4]: https://media-hosting.imagekit.io/a1186242cfd740a4/img4.png?Expires=1839559203&Key-Pair-Id=K2ZIVPTIP2VGHC&Signature=dr~Uvvul30SrFuv1dfGC6~shLjjL7T6iUcL5t7mYFK0YYifbtSw2X9NolkFAmGX5Ld46OvN0wUcHhqC-HFzDzMbDDk8H5SenoS7PA5DaNvNIoS0ctfK-GsGkW5W9-Z7TNRY-shb0rtkamxVsRsBKuZp1COOJ~FGp8R0xZgGPRGVxmuNWY1HywaIG5-2tG7viQ~EIlMn5A0xf-2TW~YvMA1PEYAEFdzwegriT~6kv5GNx51gCx61vqMUgXA4tJ2-Y9lnZu-EhJaffFOnNoBHuicpF~Uk-kFW5TCauxnmQjSuRP4xZq4cLAfdYn7nBkRVjhnh~9fnW9Z5VIItfxnUgiA__

### Tags
The c++ part of the program also calls OpenAI's Api to automatically assign tags to each image for easier searching. Using the gpt-4o-mini-2024-07-18 model, it costs around $0.0012858 to assign tags to an image, or 778 images for $1. The gpt-4.1-nano-2025-04-14 model costs around $0.0000062 per image, or 161290 images for $1, but it much less accurate and almost always gives overly general tags, such as *pixel art* or *fantasy*, instead of ones like *banana* or *food* for the banana example.
