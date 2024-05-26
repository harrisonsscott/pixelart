using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    private Vector2 size;
    private RectTransform amount; // rect transform of the child
    private float progress; // from 0 to 1
    public float Progress {
        get {
            return progress;
        }
        set {
            Debug.Log(size.x);
            progress = Mathf.Clamp(value, 0, 1);
            amount.offsetMax = new Vector2(-progress*size.x, 0);
        }
    }

    void Start()
    {
        amount = transform.GetChild(0).GetComponent<RectTransform>();
        size = gameObject.GetComponent<RectTransform>().sizeDelta;

        Progress = 0.5f;
    }
}
