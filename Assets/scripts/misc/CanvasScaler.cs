using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScaler : MonoBehaviour
{
    void Update()
    {
        var Rect = GetComponent<RectTransform>();
        Vector3 scale = new Vector3(Display.main.systemWidth / 2340f, Display.main.systemHeight / 1080f, 1);
        Rect.localScale = scale;
    }
}
