using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UI : MonoBehaviour
{
    // in the bottom of the ui, a long list of all the colors are laid out and are able to be selected
    public GameObject colorReference; // make sure this has the RawImage, and Button components and a child with TMP_TEXT
    public GameObject colorIndicator; // thin strip that indicates the current color
    public Transform bottom; // bottom of the UI where you can select the colors
    public Transform colorContent; // where all the colorReference clones are placed

    void Awake()
    {
        ClearColors();
    }

    public void ChangeColor(Color color){ // changes the color indicator
        colorIndicator.GetComponent<RawImage>().color = color;
    }

    public void ChangeColor(string hex){
        ChangeColor(hex.ToRGB());
    }

    public void ChangeColor(Vector4 color){
        ChangeColor(color.ToColor());
    }

    public void ClearColors(){ // removes all the children in colorContent
        for (int i = 0; i < colorContent.childCount; i++){
            Destroy(colorContent.GetChild(i).gameObject);
        }
    }

    public void PlaceColor(Color color){ // clones colorReference and places it in colorContent
        if (colorContent.childCount == 0){
            ChangeColor(color);
        }
        GameObject clone = Instantiate(colorReference, colorContent);
        clone.GetComponent<RawImage>().color = color;
        clone.transform.GetChild(0).GetComponent<TMP_Text>().text = colorContent.childCount + "";
    }

    public void PlaceColor(string hex){
        PlaceColor(hex.ToRGB());
    }

    public void PlaceColor(Vector4 color){
        PlaceColor(color.ToColor());
    }
}
