using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class UI : MonoBehaviour
{
    // in the bottom of the ui, a long list of all the colors are laid out and are able to be selected
    public GameObject colorReference; // make sure this has the RawImage, and Button components and a child with TMP_TEXT
    public GameObject colorIndicator; // thin strip that indicates the current color
    public Transform bottom; // bottom of the UI where you can select the colors
    public Transform colorContent; // where all the colorReference clones are placed
    public List<Color> colorList; // list of all the current colors
    public Image image;

    void Awake()
    {
        colorList = new List<Color>();
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
        colorList = new List<Color>();
        for (int i = 0; i < colorContent.childCount; i++){
            Destroy(colorContent.GetChild(i).gameObject);
        }
    }

    public void PlaceColor(Color color){ // clones colorReference and places it in colorContent
        if (colorContent.childCount == 0){
            ChangeColor(color);
        }

        colorList.Add(color);

        GameObject clone = Instantiate(colorReference, colorContent);
        clone.GetComponent<RawImage>().color = color;
        clone.transform.GetChild(0).GetComponent<TMP_Text>().text = colorContent.childCount + "";

        int index = colorContent.childCount;

        // change the color indicator's color when clicked
        clone.GetComponent<Button>().onClick.AddListener(() => {
            colorIndicator.GetComponent<RawImage>().color = colorList[index-1];
            image.CurrentNumber = index;
        });
    }

    public void PlaceColor(string hex){
        PlaceColor(hex.ToRGB());
    }

    public void PlaceColor(Vector4 color){
        PlaceColor(color.ToColor());
    }
}
