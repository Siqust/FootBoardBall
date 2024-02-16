using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsPlacement : MonoBehaviour
{
    public float radius = 1f;
    public float angle = 270f;
    public float start_angle = 0f;
    public float adjustment = 0.5f;
    public float addition_height = 1f;
    public float child_coef = 1f;
    private void Update()
    {
        float i = 4.5f;
        List<Transform> sorted_children = new List<Transform>();
        foreach (Transform child in transform)
        {
            sorted_children.Add(child);
        }
        sorted_children.Sort((x, y) => Convert.ToInt32(x.name).CompareTo(Convert.ToInt32(y.name)));
        foreach (Transform child in sorted_children)
        {

            var a = Mathf.Deg2Rad * (start_angle + angle / 2 - (angle / transform.childCount * i));
            child.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Cos(a) * (radius - 28 * (10 - transform.childCount)), Mathf.Sin(a) * (radius - 28 * (10 - transform.childCount)) * (adjustment / 100) + addition_height - child_coef * (10f - transform.childCount));
            //child.position = new Vector3(Mathf.Cos(180-(180/transform.childCount*i))*radius, Mathf.Sin(180 - (180 / transform.childCount * i))*radius, 0);
            i = (i + 1) % sorted_children.Count;
        }

        sorted_children.Sort((x, y) => x.position.y.CompareTo(y.position.y));
        for (int j = 0; j < transform.childCount; j++)
        {
            sorted_children[j].SetSiblingIndex(j);
        }
    }
}
