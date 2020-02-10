using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class RectTransformReset : MonoBehaviour
{
    private Vector2 offsetMin;
    private Vector2 offsetMax;
    private RectTransform rectTransform;
    private int frameCounter = 0;

    public void Init(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
    {
        this.rectTransform = rectTransform;
        this.offsetMin = offsetMin;
        this.offsetMax = offsetMax;
    }


    void Update()
    {
        frameCounter++;
        if (frameCounter >= 1)
        {
            rectTransform.offsetMin = this.offsetMin;
            rectTransform.offsetMax = this.offsetMax;
            DestroyImmediate(this.gameObject, false);
        }
    }
}
