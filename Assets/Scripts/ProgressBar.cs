using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Vector2 size;
    private RectTransform amount; // rect transform of the child
    [SerializeField] float prog; // from 0 to 1
    public float progress {
        get {
            return prog;
        }
        set {
            prog = Mathf.Clamp(value, 0, 1);
            amount.offsetMax = new Vector2(-size.x+prog*size.x, 0);
        }
    }
    private Color col;
    public Color color {
        get {
            return col;
        }
        set {
            col = value;
            amount.gameObject.GetComponent<Image>().color = value;
        }
    }

    void Awake()
    {
        amount = transform.GetChild(0).GetComponent<RectTransform>();
        size = gameObject.GetComponent<RectTransform>().sizeDelta;

        progress = 0f;
    }
}
